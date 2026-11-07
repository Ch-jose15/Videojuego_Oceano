using UnityEngine;
using UnityEngine.UI;

public class HUDJugador : MonoBehaviour
{
    [Header("Vida")]
    public Image rellenoBarraVida;
    public RectTransform barraVida;

    [Header("Stamina")]
    public Image rellenoBarraStamina;
    public RectTransform barraStamina;

    /*========================================================================================*/

    public void Conectar(GameObject jugador)
    {
        if (jugador == null) return;

        VidaJugador vida = jugador.GetComponentInChildren<VidaJugador>();
        if (vida != null) vida.ConectarBarra(rellenoBarraVida, barraVida);

        StaminaJugador stamina = jugador.GetComponentInChildren<StaminaJugador>();
        if (stamina != null) stamina.ConectarBarra(rellenoBarraStamina, barraStamina);
    }
}
