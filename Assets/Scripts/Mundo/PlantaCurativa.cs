// Planta que cura al jugador al usarla con E; después queda como un brote y, pasado un tiempo, vuelve a crecer y se puede usar otra vez.
using UnityEngine;

public class PlantaCurativa : MonoBehaviour
{
    [Header("Curación")]
    public float cantidadCuracion = 25f;

    [Header("Rebrote")]
    public float tiempoRebrote = 60f;
    [Range(0.05f, 1f)] public float escalaBrote = 0.25f;
    public float duracionCrecimiento = 2f;

    private Vector3 escalaOriginal;
    private bool gastada = false;
    private float tiempoParaRebrotar = 0f;
    private float tiempoCreciendo = 0f;

    // Guarda el tamaño original de la planta
    void Awake()
    {
        escalaOriginal = transform.localScale;
    }

    // Cura al jugador si la planta está disponible y le falta vida; devuelve true si se ha usado
    public bool Usar(VidaJugador vidaJugador)
    {
        if (gastada || vidaJugador == null) return false;
        if (vidaJugador.ObtenerVida() >= vidaJugador.vidaMaxima) return false;

        vidaJugador.Curar(cantidadCuracion);

        gastada = true;
        tiempoParaRebrotar = tiempoRebrote;
        tiempoCreciendo = 0f;
        transform.localScale = escalaOriginal * escalaBrote;
        return true;
    }

    // Indica si la planta se puede usar ahora mismo
    public bool EstaDisponible()
    {
        return !gastada;
    }

    // Espera el tiempo de rebrote y después hace crecer la planta suavemente hasta su tamaño original
    void Update()
    {
        if (!gastada) return;

        if (tiempoParaRebrotar > 0f)
        {
            tiempoParaRebrotar -= Time.deltaTime;
            return;
        }

        tiempoCreciendo += Time.deltaTime;
        float t = Mathf.Clamp01(tiempoCreciendo / duracionCrecimiento);
        float suavizado = Mathf.SmoothStep(0f, 1f, t);
        transform.localScale = Vector3.Lerp(escalaOriginal * escalaBrote, escalaOriginal, suavizado);

        if (t >= 1f) gastada = false;
    }
}
