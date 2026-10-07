// Texto de UI que sube, se desvanece y se destruye solo, usado para mostrar "+X$" al vender.
using TMPro;
using UnityEngine;

public class TextoFlotante : MonoBehaviour
{
    [Header("Animación")]
    public float duracion = 1f;
    public float distanciaSubida = 80f;

    private TextMeshProUGUI texto;
    private RectTransform rect;
    private Vector2 posicionInicial;
    private float tiempo = 0f;

    // Pone el mensaje y prepara la animación desde la posición actual
    public void Mostrar(string mensaje)
    {
        texto = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();

        texto.text = mensaje;
        posicionInicial = rect.anchoredPosition;
        tiempo = 0f;
    }

    // Sube y desvanece el texto y lo destruye al terminar
    void Update()
    {
        if (texto == null) return;

        tiempo += Time.unscaledDeltaTime;
        float progreso = tiempo / duracion;

        rect.anchoredPosition = posicionInicial + Vector2.up * distanciaSubida * progreso;

        Color color = texto.color;
        color.a = 1f - progreso;
        texto.color = color;

        if (progreso >= 1f) Destroy(gameObject);
    }
}
