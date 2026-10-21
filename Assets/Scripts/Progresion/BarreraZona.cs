// Barrera que cierra una zona del mapa hasta que el jugador llega al capítulo necesario pagando los plazos de la deuda.
using UnityEngine;

public class BarreraZona : MonoBehaviour
{
    [Header("Apertura")]
    public int capituloNecesario = 2;

    // Empieza a escuchar los cambios de capítulo
    void OnEnable()
    {
        GestorPlazos.AlCambiarCapitulo += ComprobarCapitulo;
    }

    // Deja de escuchar los cambios de capítulo
    void OnDisable()
    {
        GestorPlazos.AlCambiarCapitulo -= ComprobarCapitulo;
    }

    // Comprueba al empezar la partida si la barrera ya debe estar abierta
    void Start()
    {
        ComprobarCapitulo(GestorPlazos.CapituloEnCurso());
    }

    // Desactiva la barrera si el capítulo actual llega al necesario
    private void ComprobarCapitulo(int capitulo)
    {
        if (capitulo >= capituloNecesario) gameObject.SetActive(false);
    }
}
