// Permite al jugador coger y soltar objetos con la tecla E, resaltando el objeto que está mirando.
using UnityEngine;

public class InteracciónAgarrar : MonoBehaviour
{
    [Header("Referencias")]
    public Transform camara;
    public Transform puntoMano;

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

    // Resalta el objeto mirado y gestiona la tecla de coger y soltar
    void Update()
    {
        if (EstadoJuego.ControlesBloqueados) return;

        if (objetoAgarrado == null) ActualizarObjetivoResaltado();
        else QuitarResaltadoActual();

        if (Input.GetKeyDown(teclaInteractuar))
        {
            if (objetoAgarrado == null)
            {
                if (objetivoActual != null) Agarrar(objetivoActual);
            }
            else Soltar();
        }
    }

    // Busca con un rayo el objeto recogible al que mira el jugador y lo resalta
    private void ActualizarObjetivoResaltado()
    {
        GameObject nuevoObjetivo = null;

        Ray rayo = new Ray(camara.position, camara.forward);
        RaycastHit hit;
        if (Physics.Raycast(rayo, out hit, distanciaAlcance))
        {
            if (hit.collider.CompareTag(tagItem)) nuevoObjetivo = hit.collider.gameObject;
        }

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

        objeto.transform.SetParent(puntoMano);
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
