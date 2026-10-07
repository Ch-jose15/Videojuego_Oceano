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

    // Empieza con la stamina llena y guarda los valores iniciales de la barra
    void Awake()
    {
        staminaActual = staminaMaxima;
        ActualizarBarra();

        staminaMaximaInicial = staminaMaxima;
        if (barraStamina != null) anchoInicialBarra = barraStamina.sizeDelta.x;
    }

    // Indica si queda al menos esa cantidad de stamina
    public bool TieneSuficiente(float cantidad)
    {
        return staminaActual >= cantidad;
    }

    // Resta stamina si hay suficiente y apunta el momento del gasto
    public bool Consumir(float cantidad)
    {
        if (!TieneSuficiente(cantidad)) return false;

        staminaActual -= cantidad;
        tiempoUltimoGasto = Time.time;
        ActualizarBarra();
        return true;
    }

    // Recupera stamina con el tiempo una vez pasada la espera tras el último gasto
    public void Regenerar(float tiempo)
    {
        if (staminaActual >= staminaMaxima) return;

        if (Time.time < tiempoUltimoGasto + esperaAntesDeRegenerar) return;

        staminaActual = Mathf.Min(staminaActual + regeneracionPorSegundo * tiempo, staminaMaxima);
        ActualizarBarra();
    }

    // Devuelve la stamina actual del jugador
    public float ObtenerStamina()
    {
        return staminaActual;
    }

    // Sube la stamina máxima, la rellena y ensancha la barra en proporción
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

    // Ajusta el relleno de la barra a la stamina actual
    private void ActualizarBarra()
    {
        if (rellenoBarraStamina != null) rellenoBarraStamina.fillAmount = staminaActual / staminaMaxima;
    }
}
