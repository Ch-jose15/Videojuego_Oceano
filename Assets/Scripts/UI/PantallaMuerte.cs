// Pantalla negra de muerte: hace fundidos a negro y de vuelta, muestra «Has muerto» con lo perdido, el Game Over y el aviso mientras eres espectador.
using System.Collections;
using TMPro;
using UnityEngine;

public class PantallaMuerte : MonoBehaviour
{
    [Header("Referencias")]
    public CanvasGroup grupo;
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoPerdidas;

    private TextMeshProUGUI textoEspectador;

    // Indica si la pantalla está ya totalmente en negro
    public bool EstaEnNegro { get { return grupo != null && grupo.alpha > 0.99f; } }

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

    // Escribe un título y un texto cualquiera (por ejemplo, el Game Over) y los muestra
    public void MostrarTitulo(string titulo, string texto)
    {
        if (textoTitulo != null) textoTitulo.text = titulo;
        if (textoPerdidas != null) textoPerdidas.text = texto;
        MostrarTextos(true);
    }

    // Muestra u oculta, abajo en la pantalla, el aviso de que estás observando a tu compañero
    public void MostrarAvisoEspectador(bool mostrar, string texto)
    {
        if (textoEspectador == null)
        {
            if (!mostrar || textoPerdidas == null) return;
            CrearTextoEspectador();
        }

        textoEspectador.text = texto;
        textoEspectador.gameObject.SetActive(mostrar);
    }

    // Crea el aviso del espectador copiando el texto de pérdidas, visible aunque la pantalla negra esté transparente
    private void CrearTextoEspectador()
    {
        GameObject copia = Instantiate(textoPerdidas.gameObject, transform);
        copia.name = "TextoEspectador";

        CanvasGroup grupoPropio = copia.AddComponent<CanvasGroup>();
        grupoPropio.ignoreParentGroups = true;
        grupoPropio.alpha = 1f;
        grupoPropio.interactable = false;
        grupoPropio.blocksRaycasts = false;

        RectTransform rect = (RectTransform)copia.transform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 60f);
        rect.sizeDelta = new Vector2(1000f, 120f);

        textoEspectador = copia.GetComponent<TextMeshProUGUI>();
        textoEspectador.alignment = TextAlignmentOptions.Center;
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