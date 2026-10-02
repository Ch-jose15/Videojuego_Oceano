using UnityEngine;

//Disparo del jugador. Va en el jugador, igual que VidaJugador o StaminaJugador.
//Empieza bloqueado: se desbloquea comprándolo en el CuboCompra (que llama a Desbloquear()).
//Con el disparo desbloqueado, el clic izquierdo lanza un proyectil hacia donde mira la cámara.
//Si está bloqueado, el clic izquierdo no hace nada.
public class DisparoJugador : MonoBehaviour
{
    [Header("Referencias")]
    public Transform camara;            //Si está vacío, se usa la cámara del UnderwaterSwimController
    public Proyectil prefabProyectil;   //Esfera pequeña con Rigidbody, Collider y el script Proyectil

    [Header("Estado")]
    public bool disparoDesbloqueado = false;

    [Header("Ajustes del disparo")]
    public float velocidadProyectil = 25f;      //Metros por segundo
    public float tiempoVidaProyectil = 3f;      //Segundos hasta que se destruye solo si no choca con nada
    public float dañoProyectil = 10f;
    public float distanciaSalida = 1f;          //A qué distancia delante de la cámara aparece el proyectil
    public float tiempoEntreDisparos = 0.3f;    //Para no poder lanzar decenas de proyectiles por segundo

    private float tiempoProximoDisparo = 0f;
    private Collider[] collidersJugador;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        if (camara == null)
        {
            UnderwaterSwimController controlador = GetComponent<UnderwaterSwimController>();
            if (controlador != null) camara = controlador.camara;
        }

        //Guardamos los colliders del jugador para que sus propios proyectiles no choquen con él
        collidersJugador = GetComponentsInChildren<Collider>();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        if (!disparoDesbloqueado) return;

        //No disparamos con un menú o el inventario abiertos (el clic es para la UI),
        //ni en la zona del CuboCompra (allí el clic abre el panel de comprar)
        if (EstadoJuego.ControlesBloqueados || EstadoJuego.RatonLibre || EstadoJuego.JugadorEnZonaCompra) return;

        if (Input.GetMouseButtonDown(0) && Time.time >= tiempoProximoDisparo) Disparar();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Lo llama el CuboCompra al comprar "Desbloquear Disparo"
    public void Desbloquear()
    {
        disparoDesbloqueado = true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Disparar()
    {
        if (camara == null || prefabProyectil == null) return;

        tiempoProximoDisparo = Time.time + tiempoEntreDisparos;

        //Creamos el proyectil un poco delante de la cámara, mirando hacia donde mira el jugador
        Vector3 posicion = camara.position + camara.forward * distanciaSalida;
        Proyectil proyectil = Instantiate(prefabProyectil, posicion, camara.rotation);

        //Que el proyectil no choque con el propio jugador
        Collider colliderProyectil = proyectil.GetComponent<Collider>();
        foreach (Collider c in collidersJugador)
        {
            if (c != null) Physics.IgnoreCollision(colliderProyectil, c);
        }

        proyectil.Lanzar(camara.forward * velocidadProyectil, dañoProyectil, tiempoVidaProyectil);
    }
}
