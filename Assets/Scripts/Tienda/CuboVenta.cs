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

    /*========================================================================================*/

    void Awake()
    {
        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null) fuenteAudio = gameObject.AddComponent<AudioSource>();
        fuenteAudio.playOnAwake = false;
    }

    /*========================================================================================*/

    void Update()
    {
        if (jugadorEnZona && jugador != null && jugador.TieneObjetoAgarrado()) Vender();
    }

    /*========================================================================================*/

    public void JugadorEntraEnZona()
    {
        jugador = JugadorLocal.Agarrar();
        jugadorEnZona = true;
    }

    /*========================================================================================*/

    public void JugadorSaleDeZona()
    {
        jugadorEnZona = false;
    }

    /*========================================================================================*/

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

        ItemRed itemRed = objeto.GetComponent<ItemRed>();
        if (itemRed != null && itemRed.IsSpawned) itemRed.PedirDestruirRpc();
        else Destroy(objeto);

        MostrarTextoVenta(valor);
        if (sonidoVenta != null) fuenteAudio.PlayOneShot(sonidoVenta);
    }

    /*========================================================================================*/

    private void MostrarTextoVenta(int valor)
    {
        if (plantillaTextoVenta == null) return;

        TextoFlotante copia = Instantiate(plantillaTextoVenta, plantillaTextoVenta.transform.parent);
        copia.gameObject.SetActive(true);
        copia.Mostrar("+" + valor + "$");
    }
}