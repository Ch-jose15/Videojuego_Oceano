using UnityEngine;

//Cubo de venta, con una zona alrededor (objeto hijo con ZonaJugador).
//Mientras el jugador está dentro de la zona, lo que lleve en la mano se vende automáticamente, sin pulsar nada.
//Si entra sin nada en la mano, no pasa nada.
public class CuboVenta : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar jugador; //Si está vacío, se busca en la escena

    [Header("Feedback al vender")]
    public TextoFlotante plantillaTextoVenta; //Texto desactivado en el Canvas que se copia al vender ("+15$")
    public AudioClip sonidoVenta;             //Se asigna a mano en el Inspector

    private AudioSource fuenteAudio;
    private bool jugadorEnZona = false;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        if (jugador == null) jugador = FindFirstObjectByType<InteracciónAgarrar>();

        //AudioSource para el sonido de venta. Si no hay uno en el objeto, lo creamos
        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null) fuenteAudio = gameObject.AddComponent<AudioSource>();
        fuenteAudio.playOnAwake = false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        //Mientras estemos en la zona, si llevamos algo en la mano se vende al momento.
        //(Lo miramos cada frame y no solo al entrar: así también se vende si entramos con las manos vacías
        //y luego cogemos algo o lo sacamos del inventario aquí dentro)
        if (jugadorEnZona && jugador != null && jugador.TieneObjetoAgarrado()) Vender();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Los llama la ZonaJugador hija (eventos alEntrarJugador / alSalirJugador)
    public void JugadorEntraEnZona()
    {
        jugadorEnZona = true;
    }

    public void JugadorSaleDeZona()
    {
        jugadorEnZona = false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Vende el objeto que llevamos en la mano. Si no llevamos nada, no hace nada
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

        Dinero.Sumar(valor); //UIDinero se entera solo y hace la animación de contar hacia arriba
        Debug.Log("Vendido " + objeto.name + " por " + valor + ". Dinero total: " + Dinero.Total);

        //El objeto vendido desaparece
        Destroy(objeto);

        //Feedback: texto "+X$" y sonido
        MostrarTextoVenta(valor);
        if (sonidoVenta != null) fuenteAudio.PlayOneShot(sonidoVenta);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Hace una copia de la plantilla (en el mismo sitio del Canvas), la activa y le pone el texto
    private void MostrarTextoVenta(int valor)
    {
        if (plantillaTextoVenta == null) return;

        TextoFlotante copia = Instantiate(plantillaTextoVenta, plantillaTextoVenta.transform.parent);
        copia.gameObject.SetActive(true);
        copia.Mostrar("+" + valor + "$");
    }
}
