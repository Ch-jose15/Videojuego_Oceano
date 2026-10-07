// Dispara proyectiles con el clic izquierdo hacia donde mira la cámara, solo si el disparo se ha desbloqueado en la tienda.
using UnityEngine;

public class DisparoJugador : MonoBehaviour
{
    [Header("Referencias")]
    public Transform camara;
    public Proyectil prefabProyectil;

    [Header("Estado")]
    public bool disparoDesbloqueado = false;

    [Header("Ajustes del disparo")]
    public float velocidadProyectil = 25f;
    public float tiempoVidaProyectil = 3f;
    public float dañoProyectil = 10f;
    public float distanciaSalida = 1f;
    public float tiempoEntreDisparos = 0.3f;

    private float tiempoProximoDisparo = 0f;
    private Collider[] collidersJugador;

    // Busca la cámara si no está asignada y guarda los colliders del jugador
    void Awake()
    {
        if (camara == null)
        {
            UnderwaterSwimController controlador = GetComponent<UnderwaterSwimController>();
            if (controlador != null) camara = controlador.camara;
        }

        collidersJugador = GetComponentsInChildren<Collider>();
    }

    // Dispara al pulsar el clic izquierdo cuando está permitido
    void Update()
    {
        if (!disparoDesbloqueado) return;

        if (EstadoJuego.ControlesBloqueados || EstadoJuego.RatonLibre || EstadoJuego.JugadorEnZonaCompra) return;

        if (Input.GetMouseButtonDown(0) && Time.time >= tiempoProximoDisparo) Disparar();
    }

    // Activa el disparo al comprarlo en la tienda
    public void Desbloquear()
    {
        disparoDesbloqueado = true;
    }

    // Crea un proyectil delante de la cámara y lo lanza hacia donde mira el jugador
    private void Disparar()
    {
        if (camara == null || prefabProyectil == null) return;

        tiempoProximoDisparo = Time.time + tiempoEntreDisparos;

        Vector3 posicion = camara.position + camara.forward * distanciaSalida;
        Proyectil proyectil = Instantiate(prefabProyectil, posicion, camara.rotation);

        Collider colliderProyectil = proyectil.GetComponent<Collider>();
        foreach (Collider c in collidersJugador)
        {
            if (c != null) Physics.IgnoreCollision(colliderProyectil, c);
        }

        proyectil.Lanzar(camara.forward * velocidadProyectil, dañoProyectil, tiempoVidaProyectil);
    }
}
