using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lobby: partida en solitario o cooperativo online con código (Relay de Unity)
public class Lobby : MonoBehaviour
{
    public TMP_InputField campoCodigo;
    public TMP_Text textoEstado;
    public string escenaJuego = "Prueba_Cooperativo";
    public ushort puerto = 7777;

    static bool serviciosListos;

    // Limpia el texto de estado y escucha si la conexión se corta
    void Start()
    {
        if (textoEstado != null) textoEstado.text = "";
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.OnClientDisconnectCallback += AlDesconectar;
    }

    // Deja de escuchar la red al salir del Lobby
    void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientDisconnectCallback -= AlDesconectar;
        NetworkManager.Singleton.OnClientConnectedCallback -= AlConectarseJugador;
    }

    // Partida en solitario: solo escucha en este ordenador, así que nadie puede unirse
    public void JugarSolo()
    {
        UnityTransport transporte = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transporte.SetConnectionData("127.0.0.1", puerto);

        if (NetworkManager.Singleton.StartHost())
            NetworkManager.Singleton.SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
        else
            Escribir("No se pudo crear la partida");
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
            Escribir("Creando partida online...");
            await PrepararServicios();

            SessionOptions opciones = new SessionOptions { MaxPlayers = 2 }.WithRelayNetwork();
            ISession sesion = await MultiplayerService.Instance.CreateSessionAsync(opciones);

            GUIUtility.systemCopyBuffer = sesion.Code;
            Escribir("Código: " + sesion.Code + " (copiado)\nEsperando al otro jugador...");
            NetworkManager.Singleton.OnClientConnectedCallback += AlConectarseJugador;
        }
        catch (System.Exception e)
        {
            Escribir("Error al crear: " + e.Message);
            Debug.LogException(e);
        }
    }

    // Se une a una partida online con el código del host
    public async void UnirseConCodigo()
    {
        string codigo = campoCodigo != null ? campoCodigo.text.Trim().ToUpper() : "";
        if (string.IsNullOrEmpty(codigo))
        {
            Escribir("Escribe un código");
            return;
        }

        try
        {
            Escribir("Uniéndose a " + codigo + "...");
            await PrepararServicios();
            await MultiplayerService.Instance.JoinSessionByCodeAsync(codigo);
            Escribir("Conectado. Esperando al host...");
        }
        catch (System.Exception e)
        {
            Escribir("Error al unirse: " + e.Message);
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
        Escribir("Conexión fallida o cerrada");
    }

    // Escribe un mensaje en el texto de estado, si existe
    void Escribir(string mensaje)
    {
        if (textoEstado != null) textoEstado.text = mensaje;
    }
}