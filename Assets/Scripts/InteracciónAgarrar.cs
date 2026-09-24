using UnityEngine;
using UnityEngine.Experimental.AI;

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
    private Color colorOriginaobjeto;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
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

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void ActualizarObjetivoResaltado()
    {
        GameObject nuevoObjetivo = null;

        Ray rayo = new Ray(camara.position, camara.forward);
        RaycastHit hit;
        if (Physics.Raycast(rayo, out hit, distanciaAlcance))
        {
            if (hit.collider.CompareTag(tagItem)) nuevoObjetivo = hit.collider.gameObject;
        }

        //Si cambiamos de objetivo, quitamos el resaltado anterior
        if (nuevoObjetivo != objetivoActual)
        {
            QuitarResaltadoActual();
            objetivoActual = nuevoObjetivo;

            if (objetivoActual != null)
            {
                rendererObjetivo = objetivoActual.GetComponent<Renderer>();
                if (rendererObjetivo != null)
                {
                    colorOriginaobjeto = rendererObjetivo.material.color;
                    rendererObjetivo.material.color = colorResaltado;
                }
            }
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void QuitarResaltadoActual()
    {
        if (rendererObjetivo != null) rendererObjetivo.material.color = colorOriginaobjeto;
        objetivoActual = null;
        rendererObjetivo = null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Agarrar(GameObject objeto)
    {
        QuitarResaltadoActual();
 
        objetoAgarrado = objeto;
        rbObjetoAgarrado = objeto.GetComponent<Rigidbody>();
        colliderObjetoAgarrado = objeto.GetComponent<Collider>();
 
        if (rbObjetoAgarrado != null)rbObjetoAgarrado.isKinematic = true;
 
        if (colliderObjetoAgarrado != null) colliderObjetoAgarrado.enabled = false;
    
        objeto.transform.SetParent(puntoMano);
        objeto.transform.localPosition = Vector3.zero;
        objeto.transform.localRotation = Quaternion.identity;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Soltar()
    {
        if (objetoAgarrado == null) return;
 
        objetoAgarrado.transform.SetParent(null);
 
        if (rbObjetoAgarrado != null)rbObjetoAgarrado.isKinematic = false;
        
        if (colliderObjetoAgarrado != null) colliderObjetoAgarrado.enabled = true;
        
        objetoAgarrado = null;
        rbObjetoAgarrado = null;
        colliderObjetoAgarrado = null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Devuelve true si el jugador lleva algo en la mano
    public bool TieneObjetoAgarrado()
    {
        return objetoAgarrado != null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Devuelve el objeto que llevamos en la mano (o null si no llevamos nada), sin quitárnoslo
    public GameObject ObtenerObjetoAgarrado()
    {
        return objetoAgarrado;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Quita el objeto de la mano y lo devuelve para que otro script haga algo con él (por ejemplo, la Tienda lo vende).
    //A diferencia de Soltar(), NO le devuelve la física: quien lo reciba decide qué hacer con él.
    public GameObject QuitarObjetoAgarrado()
    {
        GameObject objeto = objetoAgarrado;
        if (objeto == null) return null;

        objeto.transform.SetParent(null);

        objetoAgarrado = null;
        rbObjetoAgarrado = null;
        colliderObjetoAgarrado = null;

        return objeto;
    }
}
