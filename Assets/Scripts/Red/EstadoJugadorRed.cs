// Estado del jugador en red: enseña la linterna al compañero, sincroniza si está caído, mueve la cámara de espectador, recibe la tarjeta de revivir y lanza el Game Over cuando caen todos.
using Unity.Netcode;
using UnityEngine;

public class EstadoJugadorRed : NetworkBehaviour
{
    [Header("Linterna (la copia que ve el compañero)")]
    public Linterna prefabLinterna;
    public Transform puntoManoIzquierda;

    [Header("Cámara de espectador")]
    public float distanciaEspectador = 4f;
    public float alturaEspectador = 1.5f;
    public float suavizadoEspectador = 5f;

    [Header("Game Over")]
    public float esperaGameOver = 4f;
    public string escenaLobby = "Lobby";

    private readonly NetworkVariable<bool> tieneLinterna = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> linternaEncendida = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> caido = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private static bool gameOverEnviado = false;

    private Transform camara;
    private Transform padreCamara;
    private Vector3 posicionCamara;
    private Quaternion rotacionCamara;
    private Linterna linternaVisual;
    private bool espectando = false;
    private bool revivido = false;
    private float proximaComprobacion = 0f;

    /*========================================================================================*/

    public bool Revivido { get { return revivido; } }

    /*========================================================================================*/

    public override void OnNetworkSpawn()
    {
        Camera cam = GetComponentInChildren<Camera>(true);
        if (cam != null) camara = cam.transform;
        if (puntoManoIzquierda == null && camara != null) puntoManoIzquierda = camara.Find("PuntoManoIzquierda");

        tieneLinterna.OnValueChanged += AlCambiarLinterna;
        linternaEncendida.OnValueChanged += AlCambiarLinterna;
        caido.OnValueChanged += AlCambiarCaido;

        if (IsServer) gameOverEnviado = false;

        if (!IsOwner)
        {
            ActualizarLinternaVisual();
            AplicarCaido(caido.Value);
        }
    }

    /*========================================================================================*/

    public override void OnNetworkDespawn()
    {
        tieneLinterna.OnValueChanged -= AlCambiarLinterna;
        linternaEncendida.OnValueChanged -= AlCambiarLinterna;
        caido.OnValueChanged -= AlCambiarCaido;
    }

    /*========================================================================================*/

    private void Update()
    {
        if (!IsSpawned) return;

        if (IsOwner) PublicarLinterna();
        if (IsServer && IsOwner) ComprobarGameOver();
    }

    /*========================================================================================*/

    private void LateUpdate()
    {
        if (!espectando || camara == null) return;

        EstadoJugadorRed objetivo = CompañeroVivo();
        if (objetivo == null) return;

        Transform t = objetivo.transform;
        Vector3 destino = t.position - t.forward * distanciaEspectador + Vector3.up * alturaEspectador;
        camara.position = Vector3.Lerp(camara.position, destino, 1f - Mathf.Exp(-suavizadoEspectador * Time.deltaTime));
        camara.LookAt(t.position + Vector3.up * 0.5f);
    }

    /*========================================================================================*/
    /* LINTERNA                                                                               */
    /*========================================================================================*/

    private void PublicarLinterna()
    {
        Linterna linterna = puntoManoIzquierda != null ? puntoManoIzquierda.GetComponentInChildren<Linterna>() : null;
        bool tiene = linterna != null;
        bool encendida = tiene && linterna.EstaEncendida();

        if (tieneLinterna.Value != tiene) tieneLinterna.Value = tiene;
        if (linternaEncendida.Value != encendida) linternaEncendida.Value = encendida;
    }

    /*========================================================================================*/

    private void AlCambiarLinterna(bool anterior, bool actual)
    {
        if (!IsOwner) ActualizarLinternaVisual();
    }

    /*========================================================================================*/

    private void ActualizarLinternaVisual()
    {
        if (tieneLinterna.Value && linternaVisual == null && prefabLinterna != null && puntoManoIzquierda != null)
        {
            linternaVisual = Instantiate(prefabLinterna, puntoManoIzquierda);
            linternaVisual.transform.localPosition = Vector3.zero;
            linternaVisual.transform.localRotation = Quaternion.identity;
            linternaVisual.name = "LinternaCompañero";
            linternaVisual.enabled = false;
        }
        else if (!tieneLinterna.Value && linternaVisual != null)
        {
            Destroy(linternaVisual.gameObject);
            linternaVisual = null;
        }

        if (linternaVisual != null && linternaVisual.luz != null) linternaVisual.luz.enabled = linternaEncendida.Value;
    }

    /*========================================================================================*/
    /* CAÍDO Y ESPECTADOR                                                                     */
    /*========================================================================================*/

    public void MarcarCaido(bool valor)
    {
        if (!IsOwner) return;

        caido.Value = valor;
        AplicarCaido(valor);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        if (valor && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = valor;
    }

    /*========================================================================================*/

    private void AlCambiarCaido(bool anterior, bool actual)
    {
        AplicarCaido(actual);
    }

    /*========================================================================================*/

    private void AplicarCaido(bool estaCaido)
    {
        PropietarioRed propietario = GetComponent<PropietarioRed>();
        if (propietario != null && propietario.cuerpo != null)
        {
            foreach (Renderer r in propietario.cuerpo.GetComponentsInChildren<Renderer>(true))
                r.enabled = !estaCaido;
        }

        foreach (Collider c in GetComponents<Collider>())
            c.enabled = !estaCaido;
    }

    /*========================================================================================*/

    public void EntrarEspectador()
    {
        if (espectando || camara == null) return;

        padreCamara = camara.parent;
        posicionCamara = camara.localPosition;
        rotacionCamara = camara.localRotation;
        camara.SetParent(null);
        espectando = true;
    }

    /*========================================================================================*/

    public void SalirEspectador()
    {
        if (!espectando || camara == null) return;

        espectando = false;
        camara.SetParent(padreCamara);
        camara.localPosition = posicionCamara;
        camara.localRotation = rotacionCamara;
    }

    /*========================================================================================*/
    /* REVIVIR                                                                                */
    /*========================================================================================*/

    public void PedirRevivir()
    {
        RevivirRpc();
    }

    /*========================================================================================*/

    [Rpc(SendTo.Owner)]
    private void RevivirRpc()
    {
        revivido = true;
    }

    /*========================================================================================*/

    public void LimpiarRevivido()
    {
        revivido = false;
    }

    /*========================================================================================*/
    /* GAME OVER                                                                              */
    /*========================================================================================*/

    private void ComprobarGameOver()
    {
        if (gameOverEnviado || Time.time < proximaComprobacion) return;
        proximaComprobacion = Time.time + 0.5f;

        EstadoJugadorRed[] todos = FindObjectsByType<EstadoJugadorRed>(FindObjectsSortMode.None);
        if (todos.Length == 0) return;

        foreach (EstadoJugadorRed estado in todos)
        {
            if (!estado.IsSpawned || !estado.caido.Value) return;
        }

        gameOverEnviado = true;
        GameOverRpc();
    }

    /*========================================================================================*/

    [Rpc(SendTo.Everyone)]
    private void GameOverRpc()
    {
        FinPartida.Lanzar(esperaGameOver, escenaLobby);
    }

    /*========================================================================================*/
    /* AYUDAS PARA OTROS SCRIPTS                                                              */
    /*========================================================================================*/

    public static bool JugadorCaido(Component componente)
    {
        if (componente == null) return false;

        EstadoJugadorRed estado = componente.GetComponentInParent<EstadoJugadorRed>();
        return estado != null && estado.IsSpawned && estado.caido.Value;
    }

    /*========================================================================================*/

    public static bool HayCompañero()
    {
        int jugadores = 0;
        foreach (EstadoJugadorRed estado in FindObjectsByType<EstadoJugadorRed>(FindObjectsSortMode.None))
        {
            if (estado.IsSpawned) jugadores++;
        }
        return jugadores >= 2;
    }

    /*========================================================================================*/

    public static EstadoJugadorRed CompañeroCaido()
    {
        foreach (EstadoJugadorRed estado in FindObjectsByType<EstadoJugadorRed>(FindObjectsSortMode.None))
        {
            if (estado.IsSpawned && !estado.IsOwner && estado.caido.Value) return estado;
        }
        return null;
    }

    /*========================================================================================*/

    private static EstadoJugadorRed CompañeroVivo()
    {
        foreach (EstadoJugadorRed estado in FindObjectsByType<EstadoJugadorRed>(FindObjectsSortMode.None))
        {
            if (estado.IsSpawned && !estado.IsOwner && !estado.caido.Value) return estado;
        }
        return null;
    }

    /*========================================================================================*/

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        gameOverEnviado = false;
    }
}