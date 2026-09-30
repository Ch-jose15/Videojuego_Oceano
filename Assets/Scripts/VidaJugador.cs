using UnityEngine;
using UnityEngine.UI;

//Vida del jugador. Los enemigos la reducen con RecibirDaño y la Tienda puede subir el máximo con AumentarVidaMaxima
public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;

    [Header("UI")]
    public Image rellenoBarraVida;      //Image con Image Type = Filled
    public RectTransform barraVida;     //Contenedor de la barra (el fondo). Se hace más ancho al mejorar la vida máxima

    private float vidaActual;
    private float vidaMaximaInicial;    //Para calcular cuánto tiene que crecer la barra
    private float anchoInicialBarra;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        //Empezamos con la vida llena
        vidaActual = vidaMaxima;
        ActualizarBarra();

        //Guardamos los valores iniciales para que la barra crezca en proporción al máximo
        vidaMaximaInicial = vidaMaxima;
        if (barraVida != null) anchoInicialBarra = barraVida.sizeDelta.x;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public void RecibirDaño(float cantidad)
    {
        if (cantidad <= 0f) return;

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        ActualizarBarra();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public void Curar(float cantidad)
    {
        if (cantidad <= 0f) return;

        vidaActual = Mathf.Clamp(vidaActual + cantidad, 0f, vidaMaxima);
        ActualizarBarra();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public float ObtenerVida()
    {
        return vidaActual;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Sube la vida máxima y cura al jugador hasta el nuevo máximo (lo usa la Tienda al comprar la mejora)
    public void AumentarVidaMaxima(float cantidad)
    {
        if (cantidad <= 0f) return;

        vidaMaxima += cantidad;
        vidaActual = vidaMaxima;
        ActualizarBarra();

        //La barra se hace más ancha en proporción: si la vida máxima pasa de 100 a 125, la barra crece un 25%
        if (barraVida != null)
        {
            float nuevoAncho = anchoInicialBarra * (vidaMaxima / vidaMaximaInicial);
            barraVida.sizeDelta = new Vector2(nuevoAncho, barraVida.sizeDelta.y);
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //fillAmount va de 0 (barra vacía) a 1 (barra llena)
    private void ActualizarBarra()
    {
        if (rellenoBarraVida != null) rellenoBarraVida.fillAmount = vidaActual / vidaMaxima;
    }
}
