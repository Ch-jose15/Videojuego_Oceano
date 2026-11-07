// Guarda la stamina del jugador, la gasta con el dash, la regenera tras una espera y la muestra en su barra.
using UnityEngine;
using UnityEngine.UI;

public class StaminaJugador : MonoBehaviour
{
    [Header("Stamina")]
    public float staminaMaxima = 90f;
    public float regeneracionPorSegundo = 15f;
    public float esperaAntesDeRegenerar = 3f;

    [Header("UI")]
    public Image rellenoBarraStamina;
    public RectTransform barraStamina;

    private float staminaActual;
    private float staminaMaximaInicial;
    private float anchoInicialBarra;
    private float tiempoUltimoGasto = -999f;

    /*========================================================================================*/

    void Awake()
    {
        staminaActual = staminaMaxima;
        ActualizarBarra();

        staminaMaximaInicial = staminaMaxima;
        if (barraStamina != null) 
            anchoInicialBarra = barraStamina.sizeDelta.x;
    }

    /*========================================================================================*/

    public bool TieneSuficiente(float cantidad)
    {
        return staminaActual >= cantidad;
    }

    /*========================================================================================*/

    public bool Consumir(float cantidad)
    {
        if (!TieneSuficiente(cantidad)) return false;

        staminaActual -= cantidad;
        tiempoUltimoGasto = Time.time;
        ActualizarBarra();
        return true;
    }

    /*========================================================================================*/

    public void Regenerar(float tiempo)
    {
        if (staminaActual >= staminaMaxima) return;

        if (Time.time < tiempoUltimoGasto + esperaAntesDeRegenerar) return;

        staminaActual = Mathf.Min(staminaActual + regeneracionPorSegundo * tiempo, staminaMaxima);
        ActualizarBarra();
    }

    /*========================================================================================*/

    public void RellenarStamina()
    {
        staminaActual = staminaMaxima;
        ActualizarBarra();
    }

    /*========================================================================================*/

    public float ObtenerStamina()
    {
        return staminaActual;
    }

    /*========================================================================================*/

    public void AumentarStaminaMaxima(float cantidad)
    {
        if (cantidad <= 0f) return;

        staminaMaxima += cantidad;
        staminaActual = staminaMaxima;
        ActualizarBarra();

        if (barraStamina != null)
        {
            float nuevoAncho = anchoInicialBarra * (staminaMaxima / staminaMaximaInicial);
            barraStamina.sizeDelta = new Vector2(nuevoAncho, barraStamina.sizeDelta.y);
        }
    }

    /*========================================================================================*/

    private void ActualizarBarra()
    {
        if (rellenoBarraStamina != null) 
            rellenoBarraStamina.fillAmount = staminaActual / staminaMaxima;
    }

    /*========================================================================================*/

    public void ConectarBarra(Image relleno, RectTransform barra)
    {
        if (barra != barraStamina)
        {
            barraStamina = barra;
            if (barraStamina != null)
            {
                anchoInicialBarra = barraStamina.sizeDelta.x;
                float nuevoAncho = anchoInicialBarra * (staminaMaxima / staminaMaximaInicial);
                barraStamina.sizeDelta = new Vector2(nuevoAncho, barraStamina.sizeDelta.y);
            }
        }

        rellenoBarraStamina = relleno;
        ActualizarBarra();
    }
}
