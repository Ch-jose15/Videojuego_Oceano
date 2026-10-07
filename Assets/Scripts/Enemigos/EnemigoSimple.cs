// Enemigo marino: deambula, persigue al jugador si se acerca, le hace daño al tocarlo y muere cuando se queda sin vida.
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemigoSimple : MonoBehaviour
{
    [Header("Referencias")]
    public VidaJugador vidaJugador;

    [Header("Movimiento (en metros por segundo)")]
    public float velocidadDeambular = 1.5f;
    public float velocidadPersecucion = 3.5f;
    public float radioDeambular = 8f;
    public float tiempoCambioDestino = 5f;
    public float dragAgua = 2f;

    [Header("Detección")]
    public float radioDeteccion = 5f;
    public float radioPerderJugador = 8f;

    [Header("Ataque")]
    public float distanciaAtaque = 1.8f;
    public float dañoPorGolpe = 10f;
    public float cooldownGolpe = 1.5f;

    [Header("Vida")]
    public float vidaMaxima = 30f;
    public float vidaActual;
    public Color colorAlMorir = Color.white;
    public float duracionMuerte = 0.15f;

    private Rigidbody rb;
    private Transform jugador;
    private Vector3 puntoInicio;
    private Vector3 destinoDeambular;
    private float tiempoParaCambiarDestino;
    private bool persiguiendo = false;
    private float tiempoProximoGolpe = 0f;
    private bool muerto = false;

    // Configura el Rigidbody, busca al jugador y empieza con la vida llena
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = dragAgua;

        if (vidaJugador == null) vidaJugador = FindFirstObjectByType<VidaJugador>();
        if (vidaJugador != null) jugador = vidaJugador.transform;

        vidaActual = vidaMaxima;
    }

    // Guarda el punto de aparición y elige el primer destino
    void Start()
    {
        puntoInicio = transform.position;
        ElegirNuevoDestino();
    }

    // Decide si persigue o deja de perseguir al jugador y le golpea si está cerca
    void Update()
    {
        if (jugador == null || EstadoJuego.MenuAbierto || muerto) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (!persiguiendo && distancia <= radioDeteccion)
        {
            persiguiendo = true;
        }
        else if (persiguiendo && distancia >= radioPerderJugador)
        {
            persiguiendo = false;
            ElegirNuevoDestino();
        }

        if (distancia <= distanciaAtaque && Time.time >= tiempoProximoGolpe)
        {
            vidaJugador.RecibirDaño(dañoPorGolpe);
            tiempoProximoGolpe = Time.time + cooldownGolpe;
        }
    }

    // Mueve al enemigo persiguiendo o deambulando en cada paso de física
    void FixedUpdate()
    {
        if (muerto) return;

        if (persiguiendo) Perseguir();
        else Deambular();
    }

    // Nada hacia el jugador sin empujarlo cuando ya está pegado a él
    private void Perseguir()
    {
        if (Vector3.Distance(transform.position, jugador.position) < distanciaAtaque * 0.8f) return;

        MoverHacia(jugador.position, velocidadPersecucion);
    }

    // Nada hacia su destino y elige otro al llegar o si tarda demasiado
    private void Deambular()
    {
        tiempoParaCambiarDestino -= Time.fixedDeltaTime;
        if (Vector3.Distance(transform.position, destinoDeambular) < 1f || tiempoParaCambiarDestino <= 0f) ElegirNuevoDestino();

        MoverHacia(destinoDeambular, velocidadDeambular);
    }

    // Elige un punto aleatorio alrededor del punto de aparición, a la misma altura
    private void ElegirNuevoDestino()
    {
        Vector2 circulo = Random.insideUnitCircle * radioDeambular;
        destinoDeambular = puntoInicio + new Vector3(circulo.x, 0f, circulo.y);
        tiempoParaCambiarDestino = tiempoCambioDestino;
    }

    // Empuja al enemigo hacia un destino a la velocidad indicada en metros por segundo
    private void MoverHacia(Vector3 destino, float velocidad)
    {
        Vector3 direccion = (destino - transform.position).normalized;
        rb.AddForce(direccion * velocidad * dragAgua, ForceMode.Acceleration);
    }

    // Resta vida al enemigo y lo mata si llega a cero
    public void RecibirDaño(float cantidad)
    {
        if (cantidad <= 0f || muerto) return;

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        if (vidaActual <= 0f) Morir();
    }

    // Para al enemigo, lo pinta de colorAlMorir y lo destruye tras un instante
    private void Morir()
    {
        muerto = true;

        rb.linearVelocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = colorAlMorir;

        Destroy(gameObject, duracionMuerte);
    }

    // Corrige en el Inspector la distancia de perder al jugador para que sea mayor que la de detección
    void OnValidate()
    {
        if (radioPerderJugador <= radioDeteccion) radioPerderJugador = radioDeteccion + 1f;
    }

    // Dibuja en la vista Scene los radios de detección, pérdida y ataque
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, radioPerderJugador);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
    }
}
