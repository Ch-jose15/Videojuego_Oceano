// Gestiona la tecla E del jugador: si mira una planta curativa la usa y, si no, coge, suelta y resalta objetos en la mano derecha.
using UnityEngine;
using UnityEngine.Serialization;

public class InteracciónAgarrar : MonoBehaviour
{
    [Header("Referencias")]
    public Transform camara;
    [FormerlySerializedAs("puntoMano")] public Transform puntoManoDerecha;

    [Header("Ajustes")]
    public float distanciaAlcance = 5f;
    public KeyCode teclaInteractuar = KeyCode.E;
    public string tagItem = "Item";

    [Header("Resaltado")]
    public Color colorResaltado = new Color(1f, 0.9f, 0.3f);

    private GameObject objetoAgarrado;
    private Rigidbody rbObjetoAgarrado;
    private Collider colliderObjetoAgarrado;
    private GameObject objetivoActual;
    private Renderer rendererObjetivo;
    private Color colorOriginalObjetivo;
    private VidaJugador vidaJugador;

    // Busca la vida del jugador para poder curarle con las plantas
    void Awake()
    {
        vidaJugador = GetComponent<VidaJugador>();
    }

    // Lanza un rayo, resalta el objeto mirado y decide qué hace la tecla E
    void Update()
    {
        if (EstadoJuego.ControlesBloqueados) return;

        Collider mirado = ColliderMirado();
        PlantaCurativa planta = mirado != null ? mirado.GetComponentInParent<PlantaCurativa>() : null;

        if (objetoAgarrado == null) ActualizarObjetivoResaltado(mirado);
        else QuitarResaltadoActual();

        if (Input.GetKeyDown(teclaInteractuar))
        {
            if (planta != null)
            {
                planta.Usar(vidaJugador);
            }
            else if (objetoAgarrado == null)
            {
                if (objetivoActual != null) Agarrar(objetivoActual);
            }
            else Soltar();
        }
    }

    // Devuelve el collider al que mira el jugador dentro de su alcance, o null si no mira nada
    private Collider ColliderMirado()
    {
        RaycastHit hit;
        if (Physics.Raycast(camara.position, camara.forward, out hit, distanciaAlcance)) return hit.collider;
        return null;
    }

    // Resalta el objeto recogible al que mira el jugador
    private void ActualizarObjetivoResaltado(Collider mirado)
    {
        GameObject nuevoObjetivo = null;
        if (mirado != null && mirado.CompareTag(tagItem)) nuevoObjetivo = mirado.gameObject;

        if (nuevoObjetivo != objetivoActual)
        {
            QuitarResaltadoActual();
            objetivoActual = nuevoObjetivo;

            if (objetivoActual != null)
            {
                rendererObjetivo = objetivoActual.GetComponent<Renderer>();
                if (rendererObjetivo != null)
                {
                    colorOriginalObjetivo = rendererObjetivo.material.color;
                    rendererObjetivo.material.color = colorResaltado;
                }
            }
        }
    }

    // Devuelve su color original al objeto que estaba resaltado
    private void QuitarResaltadoActual()
    {
        if (rendererObjetivo != null) rendererObjetivo.material.color = colorOriginalObjetivo;
        objetivoActual = null;
        rendererObjetivo = null;
    }

    // Pone un objeto en la mano del jugador y desactiva su física
    public void Agarrar(GameObject objeto)
    {
        QuitarResaltadoActual();

        objetoAgarrado = objeto;
        rbObjetoAgarrado = objeto.GetComponent<Rigidbody>();
        colliderObjetoAgarrado = objeto.GetComponent<Collider>();

        if (rbObjetoAgarrado != null) rbObjetoAgarrado.isKinematic = true;

        if (colliderObjetoAgarrado != null) colliderObjetoAgarrado.enabled = false;

        objeto.transform.SetParent(puntoManoDerecha);
        objeto.transform.localPosition = Vector3.zero;
        objeto.transform.localRotation = Quaternion.identity;
    }

    // Suelta el objeto de la mano y le devuelve la física
    private void Soltar()
    {
        if (objetoAgarrado == null) return;

        objetoAgarrado.transform.SetParent(null);

        if (rbObjetoAgarrado != null) rbObjetoAgarrado.isKinematic = false;

        if (colliderObjetoAgarrado != null) colliderObjetoAgarrado.enabled = true;

        OlvidarObjetoAgarrado();
    }

    // Indica si el jugador lleva algo en la mano
    public bool TieneObjetoAgarrado()
    {
        return objetoAgarrado != null;
    }

    // Devuelve el objeto que lleva el jugador en la mano sin quitárselo
    public GameObject ObtenerObjetoAgarrado()
    {
        return objetoAgarrado;
    }

    // Quita el objeto de la mano sin devolverle la física y lo entrega a quien lo pide
    public GameObject QuitarObjetoAgarrado()
    {
        GameObject objeto = objetoAgarrado;
        if (objeto == null) return null;

        objeto.transform.SetParent(null);

        OlvidarObjetoAgarrado();

        return objeto;
    }

    // Borra las referencias al objeto que se llevaba en la mano
    private void OlvidarObjetoAgarrado()
    {
        objetoAgarrado = null;
        rbObjetoAgarrado = null;
        colliderObjetoAgarrado = null;
    }
}
