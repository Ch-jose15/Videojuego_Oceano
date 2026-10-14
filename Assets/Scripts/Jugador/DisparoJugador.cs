// Dispara proyectiles con el clic izquierdo desde la mano derecha hacia donde mira la cámara, si el disparo está desbloqueado y la mano derecha está libre.
using UnityEngine;

public class DisparoJugador : MonoBehaviour
{
    [Header("Referencias")]
    public Transform camara;
    public Transform puntoDisparo;
    public Proyectil prefabProyectil;

    [Header("Estado")]
    public bool disparoDesbloqueado = false;

    [Header("Ajustes del disparo")]
    public float velocidadProyectil = 25f;
    public float tiempoVidaProyectil = 3f;
    public float dañoProyectil = 10f;
    public float distanciaSalida = 1f;
    public float tiempoEntreDisparos = 0.3f;

    private const float AlcanceApuntado = 200f;

    private float tiempoProximoDisparo = 0f;
    private Collider[] collidersJugador;
    private InteracciónAgarrar agarrar;

    // Busca la cámara, la mano derecha y los colliders del jugador si no están asignados
    void Awake()
    {
        if (camara == null)
        {
            UnderwaterSwimController controlador = GetComponent<UnderwaterSwimController>();
            if (controlador != null) camara = controlador.camara;
        }

        agarrar = GetComponent<InteracciónAgarrar>();
        if (puntoDisparo == null && agarrar != null) puntoDisparo = agarrar.puntoManoDerecha;

        collidersJugador = GetComponentsInChildren<Collider>();
    }

    // Dispara al pulsar el clic izquierdo cuando está permitido y la mano derecha está libre
    void Update()
    {
        if (!disparoDesbloqueado) return;

        if (EstadoJuego.ControlesBloqueados || EstadoJuego.RatonLibre || EstadoJuego.JugadorEnZonaCompra) return;

        if (agarrar != null && agarrar.TieneObjetoAgarrado()) return;

        if (Input.GetMouseButtonDown(0) && Time.time >= tiempoProximoDisparo) Disparar();
    }

    // Activa el disparo al comprarlo en la tienda
    public void Desbloquear()
    {
        disparoDesbloqueado = true;
    }

    // Crea un proyectil en la mano derecha y lo lanza hacia el punto al que apunta el centro de la cámara
    private void Disparar()
    {
        if (camara == null || prefabProyectil == null) return;

        tiempoProximoDisparo = Time.time + tiempoEntreDisparos;

        Vector3 origen = puntoDisparo != null ? puntoDisparo.position : camara.position;
        Vector3 direccion = (PuntoApuntado() - origen).normalized;
        Vector3 posicion = origen + direccion * distanciaSalida;

        Proyectil proyectil = Instantiate(prefabProyectil, posicion, Quaternion.LookRotation(direccion));

        Collider colliderProyectil = proyectil.GetComponent<Collider>();
        foreach (Collider c in collidersJugador)
        {
            if (c != null) Physics.IgnoreCollision(colliderProyectil, c);
        }

        proyectil.Lanzar(direccion * velocidadProyectil, dañoProyectil, tiempoVidaProyectil);
    }

    // Devuelve el punto del mundo que hay en el centro de la vista, o uno lejano si no se mira nada
    private Vector3 PuntoApuntado()
    {
        RaycastHit hit;
        if (Physics.Raycast(camara.position, camara.forward, out hit, AlcanceApuntado, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) return hit.point;
        return camara.position + camara.forward * AlcanceApuntado;
    }
}
