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

    /*========================================================================================*/

    void Awake()
    {
        vidaJugador = GetComponent<VidaJugador>();
    }

    /*========================================================================================*/

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
                if (objetivoActual != null) IntentarAgarrar(objetivoActual);
            }
            else Soltar();
        }
    }

    /*========================================================================================*/

    private Collider ColliderMirado()
    {
        RaycastHit hit;
        if (Physics.Raycast(camara.position, camara.forward, out hit, distanciaAlcance)) return hit.collider;
        return null;
    }

    /*========================================================================================*/

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

    /*========================================================================================*/

    private void QuitarResaltadoActual()
    {
        if (rendererObjetivo != null) rendererObjetivo.material.color = colorOriginalObjetivo;
        objetivoActual = null;
        rendererObjetivo = null;
    }

    /*========================================================================================*/

    private void IntentarAgarrar(GameObject objeto)
    {
        ItemRed itemRed = objeto.GetComponent<ItemRed>();
        if (itemRed != null && itemRed.IsSpawned)
        {
            if (itemRed.EstaCogido()) return;
            QuitarResaltadoActual();
            itemRed.PedirAgarrarRpc();
        }
        else Agarrar(objeto);
    }

    /*========================================================================================*/

    public void ConfirmarAgarreRed(GameObject objeto)
    {
        QuitarResaltadoActual();
        objetoAgarrado = objeto;
        rbObjetoAgarrado = objeto.GetComponent<Rigidbody>();
        colliderObjetoAgarrado = objeto.GetComponent<Collider>();
    }

    /*========================================================================================*/

    public void Agarrar(GameObject objeto)
    {
        QuitarResaltadoActual();
        objetoAgarrado = objeto;
        rbObjetoAgarrado = objeto.GetComponent<Rigidbody>();
        colliderObjetoAgarrado = objeto.GetComponent<Collider>();

        if (rbObjetoAgarrado != null) 
            rbObjetoAgarrado.isKinematic = true;

        if (colliderObjetoAgarrado != null)
            colliderObjetoAgarrado.enabled = false;

        objeto.transform.SetParent(puntoManoDerecha);
        objeto.transform.localPosition = Vector3.zero;
        objeto.transform.localRotation = Quaternion.identity;
    }

    /*========================================================================================*/

    private void Soltar()
    {
        if (objetoAgarrado == null) return;
        ItemRed itemRed = objetoAgarrado.GetComponent<ItemRed>();
        if (itemRed != null && itemRed.IsSpawned)
        {
            itemRed.PedirSoltarRpc();
            OlvidarObjetoAgarrado();
            return;
        }

        objetoAgarrado.transform.SetParent(null);

        if (rbObjetoAgarrado != null)
            rbObjetoAgarrado.isKinematic = false;

        if (colliderObjetoAgarrado != null)
            colliderObjetoAgarrado.enabled = true;

        OlvidarObjetoAgarrado();
    }

    /*========================================================================================*/

    public bool TieneObjetoAgarrado()
    {
        return objetoAgarrado != null;
    }

    /*========================================================================================*/

    public GameObject ObtenerObjetoAgarrado()
    {
        return objetoAgarrado;
    }

    /*========================================================================================*/

    public GameObject QuitarObjetoAgarrado()
    {
        GameObject objeto = objetoAgarrado;
        if (objeto == null) return null;
        objeto.transform.SetParent(null);
        OlvidarObjetoAgarrado();
        return objeto;
    }

    /*========================================================================================*/

    private void OlvidarObjetoAgarrado()
    {
        objetoAgarrado = null;
        rbObjetoAgarrado = null;
        colliderObjetoAgarrado = null;
    }
}
