// Muestra el dinero del jugador en pantalla y anima el número contando hasta el nuevo valor cuando cambia.
using TMPro;
using UnityEngine;

public class UIDinero : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI textoDinero;

    [Header("Animación")]
    public float duracionConteo = 0.5f;

    private float valorMostrado = 0f;
    private float valorInicioConteo = 0f;
    private int valorDestino = 0;
    private float tiempoConteo = 0f;
    private bool contando = false;

    // Se suscribe a los cambios de dinero y muestra el valor actual
    void OnEnable()
    {
        Dinero.AlCambiar += EmpezarConteo;

        valorMostrado = Dinero.Total;
        contando = false;
        EscribirTexto(Dinero.Total);
    }

    // Deja de escuchar los cambios de dinero
    void OnDisable()
    {
        Dinero.AlCambiar -= EmpezarConteo;
    }

    // Empieza a contar desde el número mostrado hasta el nuevo total
    private void EmpezarConteo(int total)
    {
        valorInicioConteo = valorMostrado;
        valorDestino = total;
        tiempoConteo = 0f;
        contando = true;
    }

    // Avanza el conteo del número aunque el juego esté en pausa
    void Update()
    {
        if (!contando) return;

        tiempoConteo += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(tiempoConteo / duracionConteo);

        valorMostrado = Mathf.Lerp(valorInicioConteo, valorDestino, t);
        EscribirTexto(Mathf.RoundToInt(valorMostrado));

        if (t >= 1f) contando = false;
    }

    // Escribe la cantidad de dinero en el texto de la UI
    private void EscribirTexto(int cantidad)
    {
        if (textoDinero != null) textoDinero.text = "Dinero: " + cantidad;
    }
}
