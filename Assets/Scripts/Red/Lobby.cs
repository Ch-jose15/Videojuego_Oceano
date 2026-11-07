using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lobby: partida por IP en red local o por código online con Relay
public class Lobby : MonoBehaviour
{
    public TMP_InputField campoIP;
    public TMP_InputField campoCodigo;
    public TMP_Text textoEstado;
    public string escenaJuego = "Prueba_Cooperativo";
    public string ipPorDefecto = "127.0.0.1";
    public ushort puerto = 7777;

    bool serviciosListos;

    // Muestra la IP de este PC y rellena el campo con la última IP usada
    void Start()
    {
        textoEstado.text = "Tu IP: " + ObtenerIPLocal();
        campoIP.text = PlayerPrefs.GetString("UltimaIP", ipPorDefecto);
        NetworkManager.Singleton.OnClientDisconnectCallback += AlDesconectar;
    }

    // Crea la partida en red local y lleva a todos a la escena de juego
    public void IniciarHost()
    {
        UnityTransport transporte = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transporte.SetConnectionData("0.0.0.0", puerto);

        if (NetworkManager.Singleton.StartHost())
            NetworkManager.Singleton.SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
        else
            textoEstado.text = "No se pudo crear la partida";
    }

    // Se conecta por IP en red local
    public void IniciarCliente()
    {
        UnityTransport transporte = NetworkManager.Singleton.GetComponent<UnityTransport>();
        string ip = string.IsNullOrWhiteSpace(campoIP.text) ? ipPorDefecto : campoIP.text.Trim();
        PlayerPrefs.SetString("UltimaIP", ip);
        transporte.SetConnectionData(ip, puerto);

        textoEstado.text = "Conectando a " + ip + "...";
        NetworkManager.Singleton.StartClient();
    }

    // Inicia los servicios de Unity e inicia sesión anónima, una sola vez
    async Task PrepararServicios()
    {
        if (serviciosListos) return;

        InitializationOptions opciones = new InitializationOptions();
        opciones.SetProfile("Jugador" + Random.Range(1000, 9999));
        await UnityServices.InitializeAsync(opciones);

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        serviciosListos = true;
    }

    // Crea una partida online y muestra el código para el otro jugador
    public async void CrearPartidaOnline()
    {
        try
        {
            textoEstado.text = "Creando partida online...";
            await PrepararServicios();

            SessionOptions opciones = new SessionOptions { MaxPlayers = 2 }.WithRelayNetwork();
            ISession sesion = await MultiplayerService.Instance.CreateSessionAsync(opciones);

            GUIUtility.systemCopyBuffer = sesion.Code;
            textoEstado.text = "Código: " + sesion.Code + " (copiado)\nEsperando al otro jugador...";
            NetworkManager.Singleton.OnClientConnectedCallback += AlConectarseJugador;
        }
        catch (System.Exception e)
        {
            textoEstado.text = "Error al crear: " + e.Message;
            Debug.LogException(e);
        }
    }

    // Se une a una partida online con el código del host
    public async void UnirseConCodigo()
    {
        string codigo = campoCodigo.text.Trim().ToUpper();
        if (string.IsNullOrEmpty(codigo))
        {
            textoEstado.text = "Escribe un código";
            return;
        }

        try
        {
            textoEstado.text = "Uniéndose a " + codigo + "...";
            await PrepararServicios();
            await MultiplayerService.Instance.JoinSessionByCodeAsync(codigo);
            textoEstado.text = "Conectado. Esperando al host...";
        }
        catch (System.Exception e)
        {
            textoEstado.text = "Error al unirse: " + e.Message;
            Debug.LogException(e);
        }
    }

    // Cuando entra el segundo jugador, el host lleva a los dos a la escena de juego
    void AlConectarseJugador(ulong idCliente)
    {
        if (!NetworkManager.Singleton.IsHost) return;
        if (NetworkManager.Singleton.ConnectedClients.Count < 2) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= AlConectarseJugador;
        NetworkManager.Singleton.SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
    }

    // Avisa si la conexión falla o se corta
    void AlDesconectar(ulong idCliente)
    {
        if (textoEstado != null)
            textoEstado.text = "Conexión fallida o cerrada";
    }

    // Busca la IP de este PC en la red local
    string ObtenerIPLocal()
    {
        foreach (IPAddress ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip.ToString();
        }
        return "desconocida";
    }
}