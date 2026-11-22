// Enemigo marino: deambula, persigue al jugador más cercano, le hace daño al tocarlo y muere sin vida. En red solo piensa el host y EnemigoRed lo sincroniza.
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
    public float intervaloBuscarJugador = 0.25f;
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
    private EnemigoRed red;
    private float tiempoProximaBusqueda = 0f;

    // Configura el Rigidbody y empieza con la vida llena
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = dragAgua;

        red = GetComponent<EnemigoRed>();
        if (vidaJugador != null) jugador = vidaJugador.transform;

        vidaActual = vidaMaxima;
    }

    // En red, solo el host mueve y decide; los clientes solo ven lo que les llega
    private bool PuedePensar()
    {
        return red == null || !red.IsSpawned || red.IsServer;
    }

    // Busca al jugador vivo más cercano (en solitario solo hay uno)
    private void BuscarJugadorCercano()
    {
        if (Time.time < tiempoProximaBusqueda) return;
        tiempoProximaBusqueda = Time.time + intervaloBuscarJugador;

        VidaJugador masCercano = null;
        float mejorDistancia = float.MaxValue;

        foreach (VidaJugador vida in FindObjectsByType<VidaJugador>(FindObjectsSortMode.None))
        {
            float distancia = Vector3.Distance(transform.position, vida.transform.position);
            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                masCercano = vida;
            }
        }

        vidaJugador = masCercano;
        jugador = masCercano != null ? masCercano.transform : null;
    }

    // Hace daño al jugador: en red se lo manda a su dueño, en solitario se lo resta directamente
    private void GolpearJugador()
    {
        CombateRed combate = vidaJugador.GetComponent<CombateRed>();
        if (combate != null && combate.IsSpawned) combate.AplicarDaño(dañoPorGolpe);
        else vidaJugador.RecibirDaño(dañoPorGolpe);
    }

    // Guarda el punto de aparición y elige el primer destino
    void Start()
    {
        puntoInicio = transform.position;
        ElegirNuevoDestino();
    }

    // Empieza a escuchar la muerte del jugador
    void OnEnable()
    {
        MuerteJugador.AlMorirJugador += OlvidarJugador;
    }

    // Deja de escuchar la muerte del jugador
    void OnDisable()
    {
        MuerteJugador.AlMorirJugador -= OlvidarJugador;
    }

    // Deja de perseguir al jugador y vuelve a deambular
    private void OlvidarJugador()
    {
        if (!persiguiendo) return;

        persiguiendo = false;
        ElegirNuevoDestino();
    }

    // Decide si persigue o deja de perseguir al jugador más cercano y le golpea si está cerca
    void Update()
    {
        if (muerto || !PuedePensar()) return;
        if (!JugadorLocal.EnRed() && (EstadoJuego.MenuAbierto || EstadoJuego.JugadorMuerto)) return;

        if (!persiguiendo || jugador == null) BuscarJugadorCercano();
        if (jugador == null) return;

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
            GolpearJugador();
            tiempoProximoGolpe = Time.time + cooldownGolpe;
        }
    }

    // Mueve al enemigo persiguiendo o deambulando en cada paso de física
    void FixedUpdate()
    {
        if (muerto || !PuedePensar()) return;

        if (persiguiendo && jugador != null) Perseguir();
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

    // Resta vida al enemigo y lo mata si llega a cero; en red, el cliente se lo pide al host
    public void RecibirDaño(float cantidad)
    {
        if (cantidad <= 0f || muerto) return;

        if (!PuedePensar())
        {
            red.PedirDañoRpc(cantidad);
            return;
        }

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        if (vidaActual <= 0f) Morir();
    }

    // Mata al enemigo y lo elimina tras un instante (en red lo retira el host para todos)
    private void Morir()
    {
        MostrarMuerte();

        if (red != null && red.IsSpawned) red.MarcarMuerto(duracionMuerte);
        else Destroy(gameObject, duracionMuerte);
    }

    // Para al enemigo, le quita el collider y lo pinta de colorAlMorir
    public void MostrarMuerte()
    {
        muerto = true;

        if (!rb.isKinematic) rb.linearVelocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = colorAlMorir;
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