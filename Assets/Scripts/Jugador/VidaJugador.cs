// Guarda la vida del jugador, la actualiza en su barra y permite recibir daño, curarse y subir el máximo.
using UnityEngine;
using UnityEngine.UI;

public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;

    [Header("UI")]
    public Image rellenoBarraVida;
    public RectTransform barraVida;

    private float vidaActual;
    private float vidaMaximaInicial;
    private float anchoInicialBarra;

    // Empieza con la vida llena y guarda los valores iniciales de la barra
    void Awake()
    {
        vidaActual = vidaMaxima;
        ActualizarBarra();

        vidaMaximaInicial = vidaMaxima;
        if (barraVida != null) anchoInicialBarra = barraVida.sizeDelta.x;
    }

    // Resta vida al jugador
    public void RecibirDaño(float cantidad)
    {
        if (cantidad <= 0f) return;

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        ActualizarBarra();
    }

    // Suma vida al jugador sin pasar del máximo
    public void Curar(float cantidad)
    {
        if (cantidad <= 0f) return;

        vidaActual = Mathf.Clamp(vidaActual + cantidad, 0f, vidaMaxima);
        ActualizarBarra();
    }

    // Devuelve la vida actual del jugador
    public float ObtenerVida()
    {
        return vidaActual;
    }

    // Sube la vida máxima, cura hasta el nuevo máximo y ensancha la barra en proporción
    public void AumentarVidaMaxima(float cantidad)
    {
        if (cantidad <= 0f) return;

        vidaMaxima += cantidad;
        vidaActual = vidaMaxima;
        ActualizarBarra();

        if (barraVida != null)
        {
            float nuevoAncho = anchoInicialBarra * (vidaMaxima / vidaMaximaInicial);
            barraVida.sizeDelta = new Vector2(nuevoAncho, barraVida.sizeDelta.y);
        }
    }

    // Ajusta el relleno de la barra a la vida actual
    private void ActualizarBarra()
    {
        if (rellenoBarraVida != null) rellenoBarraVida.fillAmount = vidaActual / vidaMaxima;
    }
}
