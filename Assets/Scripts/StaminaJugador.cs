using UnityEngine;
using UnityEngine.UI;

//Stamina del jugador. UnderwaterSwimController la gasta al hacer dash y la regenera cuando no está dasheando
//(y solo después de esperaAntesDeRegenerar segundos sin gastarla)
public class StaminaJugador : MonoBehaviour
{
    [Header("Stamina")]
    public float staminaMaxima = 90f; //90 / 30 (coste del dash) = exactamente 3 dashes seguidos
    public float regeneracionPorSegundo = 15f;

    //Segundos que hay que esperar desde el último gasto para que empiece a regenerarse.
    //Tiene que ser mayor que el tiempo entre dos dashes seguidos (~2 s: dash + vuelta del FOV + cooldown);
    //si no, se regeneraría entre dash y dash y nunca llegaría a 0
    public float esperaAntesDeRegenerar = 3f;

    [Header("UI")]
    public Image rellenoBarraStamina; //Image con Image Type = Filled

    private float staminaActual;
    private float tiempoUltimoGasto = -999f;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        //Empezamos con la stamina llena
        staminaActual = staminaMaxima;
        ActualizarBarra();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Devuelve true si hay al menos esa cantidad de stamina
    public bool TieneSuficiente(float cantidad)
    {
        return staminaActual >= cantidad;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Resta stamina. Devuelve false (y no resta nada) si no hay suficiente
    public bool Consumir(float cantidad)
    {
        if (!TieneSuficiente(cantidad)) return false;

        staminaActual -= cantidad;
        tiempoUltimoGasto = Time.time;
        ActualizarBarra();
        return true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Se llama cada frame desde fuera cuando se puede regenerar (tiempo = Time.deltaTime)
    public void Regenerar(float tiempo)
    {
        if (staminaActual >= staminaMaxima) return;

        //Todavía no ha pasado la espera desde el último gasto
        if (Time.time < tiempoUltimoGasto + esperaAntesDeRegenerar) return;

        staminaActual = Mathf.Min(staminaActual + regeneracionPorSegundo * tiempo, staminaMaxima);
        ActualizarBarra();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public float ObtenerStamina()
    {
        return staminaActual;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //fillAmount va de 0 (barra vacía) a 1 (barra llena)
    private void ActualizarBarra()
    {
        if (rellenoBarraStamina != null) rellenoBarraStamina.fillAmount = staminaActual / staminaMaxima;
    }
}
