using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

// Lobby: crea partida como host o se une a una por IP
public class Lobby : MonoBehaviour
{
    public TMP_InputField campoIP;
    public TMP_Text textoEstado;
    public string escenaJuego = "Prueba_Cooperativo";
    public ushort puerto = 7777;
    public string ipPorDefecto = "127.0.0.1";

    // Muestra la IP de este PC para saber cuál escribir en el otro
    void Start()
    {
        textoEstado.text = "Tu IP: " + ObtenerIPLocal();
        campoIP.text = PlayerPrefs.GetString("UltimaIP", ipPorDefecto);
        NetworkManager.Singleton.OnClientDisconnectCallback += AlDesconectar;
    }

    // Crea la partida y lleva a todos a la escena de juego
    public void IniciarHost()
    {
        UnityTransport transporte = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transporte.SetConnectionData("0.0.0.0", puerto);

        if (NetworkManager.Singleton.StartHost())
        {
            NetworkManager.Singleton.SceneManager.LoadScene(escenaJuego, LoadSceneMode.Single);
        }
        else
        {
            textoEstado.text = "No se pudo crear la partida";
        }
    }

    // Se conecta a la IP escrita en el campo
    public void IniciarCliente()
    {
        UnityTransport transporte = NetworkManager.Singleton.GetComponent<UnityTransport>();
        string ip = string.IsNullOrWhiteSpace(campoIP.text) ? ipPorDefecto : campoIP.text.Trim();
        PlayerPrefs.SetString("UltimaIP", ip);
        transporte.SetConnectionData(ip, puerto);

        textoEstado.text = "Conectando a " + ip + "...";
        NetworkManager.Singleton.StartClient();
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