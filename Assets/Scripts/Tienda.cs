using UnityEngine;

//Tienda sólida. Si el jugador la mira dentro de su alcance llevando un objeto, se resalta y con F vende el objeto
[RequireComponent(typeof(Collider))]
public class Tienda : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar jugador;

    [Header("Ajustes")]
    public KeyCode teclaVender = KeyCode.F;

    [Header("Resaltado")]
    public Color colorResaltado = new Color(0.3f, 1f, 0.4f);

    private Collider colliderTienda;
    private Renderer rendererTienda;
    private Color colorOriginal;
    private bool resaltada = false;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        colliderTienda = GetComponent<Collider>();
        rendererTienda = GetComponent<Renderer>();

        //La tienda es sólida: no se puede atravesar
        colliderTienda.isTrigger = false;

        //Si no se ha asignado el jugador en el Inspector, lo buscamos en la escena
        if (jugador == null) jugador = FindFirstObjectByType<InteracciónAgarrar>();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        if (jugador == null) return;

        //Solo se puede vender si llevamos algo en la mano y estamos mirando la tienda
        bool puedeVender = jugador.TieneObjetoAgarrado() && JugadorMirandoTienda();

        if (puedeVender) Resaltar();
        else QuitarResaltado();

        if (puedeVender && Input.GetKeyDown(teclaVender)) Vender();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Lanza un rayo desde la cámara del jugador (con su mismo alcance) y comprueba si choca con la tienda
    private bool JugadorMirandoTienda()
    {
        Ray rayo = new Ray(jugador.camara.position, jugador.camara.forward);
        RaycastHit hit;
        if (Physics.Raycast(rayo, out hit, jugador.distanciaAlcance))
        {
            return hit.collider == colliderTienda;
        }
        return false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Resaltar()
    {
        if (resaltada || rendererTienda == null) return;

        colorOriginal = rendererTienda.material.color;
        rendererTienda.material.color = colorResaltado;
        resaltada = true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void QuitarResaltado()
    {
        if (!resaltada || rendererTienda == null) return;

        rendererTienda.material.color = colorOriginal;
        resaltada = false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Vender()
    {
        //Le quitamos el objeto de la mano al jugador (se queda con las manos libres)
        GameObject objeto = jugador.QuitarObjetoAgarrado();
        if (objeto == null) return;

        //Miramos cuánto vale. Si no tiene ValorItem, vale 0 y avisamos en consola
        int valor = 0;
        ValorItem valorItem = objeto.GetComponent<ValorItem>();
        if (valorItem != null) valor = valorItem.valor;
        else Debug.LogWarning("El objeto " + objeto.name + " no tiene el componente ValorItem, se vende por 0.");

        Dinero.Sumar(valor);
        Debug.Log("Vendido " + objeto.name + " por " + valor + ". Dinero total: " + Dinero.Total);

        //El objeto vendido desaparece
        Destroy(objeto);

        //Ya no llevamos nada, así que la tienda deja de estar resaltada
        QuitarResaltado();
    }
}
