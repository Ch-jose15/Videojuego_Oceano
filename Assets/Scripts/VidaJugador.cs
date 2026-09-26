using UnityEngine;
using UnityEngine.UI;

//Vida del jugador. Por ahora nada hace daño: solo deja preparado RecibirDaño y Curar para usarlos más adelante
public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;

    [Header("UI")]
    public Image rellenoBarraVida; //Image con Image Type = Filled

    private float vidaActual;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        //Empezamos con la vida llena
        vidaActual = vidaMaxima;
        ActualizarBarra();
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

    //fillAmount va de 0 (barra vacía) a 1 (barra llena)
    private void ActualizarBarra()
    {
        if (rellenoBarraVida != null) rellenoBarraVida.fillAmount = vidaActual / vidaMaxima;
    }
}
