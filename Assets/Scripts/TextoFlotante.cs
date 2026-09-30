using TMPro;
using UnityEngine;

//Texto de UI que sube un poco, se desvanece y se destruye solo (por ejemplo "+15$" al vender).
//Se usa haciendo una copia de un texto "plantilla" que está desactivado en el Canvas y llamando a Mostrar()
public class TextoFlotante : MonoBehaviour
{
    [Header("Animación")]
    public float duracion = 1f;
    public float distanciaSubida = 80f; //En píxeles de la UI

    private TextMeshProUGUI texto;
    private RectTransform rect;
    private Vector2 posicionInicial;
    private float tiempo = 0f;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public void Mostrar(string mensaje)
    {
        texto = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();

        texto.text = mensaje;
        posicionInicial = rect.anchoredPosition;
        tiempo = 0f;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        if (texto == null) return;

        //Usamos el tiempo "sin escalar" para que la animación funcione aunque el juego esté en pausa (timeScale = 0)
        tiempo += Time.unscaledDeltaTime;
        float progreso = tiempo / duracion; //Va de 0 a 1

        //Sube poco a poco
        rect.anchoredPosition = posicionInicial + Vector2.up * distanciaSubida * progreso;

        //Se va haciendo transparente (alpha de 1 a 0)
        Color color = texto.color;
        color.a = 1f - progreso;
        texto.color = color;

        //Al terminar, se destruye
        if (progreso >= 1f) Destroy(gameObject);
    }
}
