using UnityEngine;

//Enemigo marino simple (prototipo).
//Si toca al jugador (distanciaAtaque) le quita vida, con un tiempo de espera entre golpes.
[RequireComponent(typeof(Rigidbody))]
public class EnemigoSimple : MonoBehaviour
{
    [Header("Referencias")]
    public VidaJugador vidaJugador; //Si está vacío, se busca solo al empezar

    [Header("Movimiento (en metros por segundo)")]
    public float velocidadDeambular = 1.5f;
    public float velocidadPersecucion = 3.5f;
    public float radioDeambular = 8f;
    public float tiempoCambioDestino = 5f;
    public float dragAgua = 2f;

    [Header("Detección")]
    public float radioDeteccion = 12f;
    public float radioPerderJugador = 18f;

    [Header("Ataque")]
    public float distanciaAtaque = 1.8f;
    public float dañoPorGolpe = 10f;
    public float cooldownGolpe = 1.5f;

    private Rigidbody rb;
    private Transform jugador;
    private Vector3 puntoInicio;
    private Vector3 destinoDeambular;
    private float tiempoParaCambiarDestino;
    private bool persiguiendo = false;
    private float tiempoProximoGolpe = 0f;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; //Bajo el agua flota, no cae
        rb.linearDamping = dragAgua;

        if (vidaJugador == null) vidaJugador = FindFirstObjectByType<VidaJugador>();
        if (vidaJugador != null) jugador = vidaJugador.transform;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Start()
    {
        puntoInicio = transform.position;
        ElegirNuevoDestino();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        //Con un menú abierto el juego está en pausa: el enemigo no golpea
        if (jugador == null || EstadoJuego.MenuAbierto) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        //Empezar/dejar de perseguir según la distancia
        if (distancia <= radioDeteccion) persiguiendo = true;
        else if (distancia >= radioPerderJugador) persiguiendo = false;

        //Golpear si está lo bastante cerca y ya pasó el cooldown
        if (distancia <= distanciaAtaque && Time.time >= tiempoProximoGolpe)
        {
            vidaJugador.RecibirDaño(dañoPorGolpe);
            tiempoProximoGolpe = Time.time + cooldownGolpe;
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void FixedUpdate()
    {
        if (persiguiendo) Perseguir();
        else Deambular();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Perseguir()
    {
        //Si ya estamos pegados al jugador no seguimos empujándolo
        if (Vector3.Distance(transform.position, jugador.position) < distanciaAtaque * 0.8f) return;

        MoverHacia(jugador.position, velocidadPersecucion);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Deambular()
    {
        //Cambiamos de destino si hemos llegado o si llevamos mucho rato (por si nos atascamos contra algo)
        tiempoParaCambiarDestino -= Time.fixedDeltaTime;
        if (Vector3.Distance(transform.position, destinoDeambular) < 1f || tiempoParaCambiarDestino <= 0f) ElegirNuevoDestino();

        MoverHacia(destinoDeambular, velocidadDeambular);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Punto aleatorio alrededor del punto de inicio, a la misma altura (así no se mete en el suelo ni sale del agua)
    private void ElegirNuevoDestino()
    {
        Vector2 circulo = Random.insideUnitCircle * radioDeambular;
        destinoDeambular = puntoInicio + new Vector3(circulo.x, 0f, circulo.y);
        tiempoParaCambiarDestino = tiempoCambioDestino;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Con drag, la velocidad máxima que se alcanza es aceleración / drag.
    //Por eso multiplicamos por dragAgua: así "velocidad" son metros por segundo reales
    private void MoverHacia(Vector3 destino, float velocidad)
    {
        Vector3 direccion = (destino - transform.position).normalized;
        rb.AddForce(direccion * velocidad * dragAgua, ForceMode.Acceleration);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Dibuja los radios en la vista Scene al seleccionar el enemigo (ayuda para ajustar valores)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}
