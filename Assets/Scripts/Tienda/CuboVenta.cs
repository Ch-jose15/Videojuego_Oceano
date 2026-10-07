// Cubo de venta: mientras el jugador está en su zona, vende al momento el objeto que lleva en la mano.
using UnityEngine;

public class CuboVenta : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar jugador;

    [Header("Feedback al vender")]
    public TextoFlotante plantillaTextoVenta;
    public AudioClip sonidoVenta;

    private AudioSource fuenteAudio;
    private bool jugadorEnZona = false;

    // Busca al jugador y prepara el AudioSource del sonido de venta
    void Awake()
    {
        if (jugador == null) jugador = FindFirstObjectByType<InteracciónAgarrar>();

        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null) fuenteAudio = gameObject.AddComponent<AudioSource>();
        fuenteAudio.playOnAwake = false;
    }

    // Vende el objeto de la mano si el jugador está dentro de la zona
    void Update()
    {
        if (jugadorEnZona && jugador != null && jugador.TieneObjetoAgarrado()) Vender();
    }

    // Marca que el jugador ha entrado en la zona de venta
    public void JugadorEntraEnZona()
    {
        jugadorEnZona = true;
    }

    // Marca que el jugador ha salido de la zona de venta
    public void JugadorSaleDeZona()
    {
        jugadorEnZona = false;
    }

    // Suma el valor del objeto vendido al dinero del jugador, lo destruye y muestra el feedback
    private void Vender()
    {
        GameObject objeto = jugador.QuitarObjetoAgarrado();
        if (objeto == null) return;

        int valor = 0;
        ValorItem valorItem = objeto.GetComponent<ValorItem>();
        if (valorItem != null) valor = valorItem.valor;
        else Debug.LogWarning("El objeto " + objeto.name + " no tiene el componente ValorItem, se vende por 0.");

        Dinero.Sumar(valor);
        Debug.Log("Vendido " + objeto.name + " por " + valor + ". Dinero total: " + Dinero.Total);

        Destroy(objeto);

        MostrarTextoVenta(valor);
        if (sonidoVenta != null) fuenteAudio.PlayOneShot(sonidoVenta);
    }

    // Muestra en pantalla el texto flotante "+X$" con el valor vendido
    private void MostrarTextoVenta(int valor)
    {
        if (plantillaTextoVenta == null) return;

        TextoFlotante copia = Instantiate(plantillaTextoVenta, plantillaTextoVenta.transform.parent);
        copia.gameObject.SetActive(true);
        copia.Mostrar("+" + valor + "$");
    }
}
