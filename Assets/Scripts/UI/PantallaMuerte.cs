// Pantalla negra de muerte: hace fundidos a negro y de vuelta, y muestra «Has muerto» con lo que se ha perdido.
using System.Collections;
using TMPro;
using UnityEngine;

public class PantallaMuerte : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup grupo;
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoPerdidas;

    // Empieza transparente y sin textos
    void Awake()
    {
        if (grupo == null) grupo = GetComponentInChildren<CanvasGroup>(true);
        PonerOpacidad(0f);
        MostrarTextos(false);
    }

    // Funde la pantalla hasta negro total en el tiempo indicado, aunque el juego esté en pausa
    public IEnumerator FundirANegro(float duracion)
    {
        yield return Fundir(0f, 1f, duracion);
    }

    // Funde la pantalla de negro a transparente en el tiempo indicado
    public IEnumerator FundirDeVuelta(float duracion)
    {
        yield return Fundir(1f, 0f, duracion);
        MostrarTextos(false);
    }

    // Escribe «Has muerto» y la lista de pérdidas y los muestra
    public void MostrarMensaje(string perdidas)
    {
        if (textoTitulo != null) textoTitulo.text = "Has muerto";
        if (textoPerdidas != null) textoPerdidas.text = perdidas;
        MostrarTextos(true);
    }

    // Cambia la opacidad de la pantalla poco a poco entre dos valores
    private IEnumerator Fundir(float desde, float hasta, float duracion)
    {
        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            PonerOpacidad(Mathf.Lerp(desde, hasta, tiempo / duracion));
            yield return null;
        }
        PonerOpacidad(hasta);
    }

    // Pone la opacidad de toda la pantalla de muerte
    private void PonerOpacidad(float opacidad)
    {
        if (grupo != null) grupo.alpha = opacidad;
    }

    // Enseña u oculta los textos
    private void MostrarTextos(bool mostrar)
    {
        if (textoTitulo != null) textoTitulo.gameObject.SetActive(mostrar);
        if (textoPerdidas != null) textoPerdidas.gameObject.SetActive(mostrar);
    }
}
