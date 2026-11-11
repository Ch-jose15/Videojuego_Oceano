using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuntoCobro : MonoBehaviour
{
    [Header("Referencias (se buscan solas si están vacías)")]
    public InteracciónAgarrar jugador;
    public GestorPlazos gestorPlazos;
    public Renderer rendererCubo;

    [Header("UI de cobro")]
    public GameObject avisoCobro;
    public GameObject panelCobro;
    public TextMeshProUGUI textoTitulo;
    public TextMeshProUGUI textoInfo;
    public Button botonPagar;
    public Button botonCerrar;
    public TextoFlotante plantillaTextoCobro;

    [Header("Ajustes")]
    public KeyCode teclaCerrar = KeyCode.Escape;
    public float esperaTrasPagar = 0.6f;
    public Color colorFalta = new Color(1f, 0.45f, 0.4f);
    public Color colorResaltado = new Color(1f, 0.35f, 0.3f);
    public float duracionPop = 0.18f;

    private Color colorOriginal;
    private bool resaltado = false;
    private bool cobroAbierto = false;
    private bool jugadorEnZona = false;
    private float tiempoUltimoPago = -10f;
    private Transform panelAnimado;
    private float tiempoPop = 0f;

    /*========================================================================================*/

    void Awake()
    {
        if (rendererCubo == null) rendererCubo = GetComponentInChildren<Renderer>();

        if (botonPagar != null)
        {
            botonPagar.onClick.AddListener(PagarPlazo);
            botonPagar.onClick.AddListener(EstadoJuego.QuitarSeleccionUI);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.AddListener(CerrarCobro);
            botonCerrar.onClick.AddListener(EstadoJuego.QuitarSeleccionUI);
        }

        if (plantillaTextoCobro != null) plantillaTextoCobro.gameObject.SetActive(false);
        OcultarPanel();
        if (avisoCobro != null) avisoCobro.SetActive(false);
    }

    /*========================================================================================*/

    void OnEnable()
    {
        MuerteJugador.AlMorirJugador += CerrarCobro;
        GestorPlazos.AlCambiarAportado += RefrescarSiAbierto;
    }

    /*========================================================================================*/

    void OnDisable()
    {
        MuerteJugador.AlMorirJugador -= CerrarCobro;
        GestorPlazos.AlCambiarAportado -= RefrescarSiAbierto;
    }

    /*========================================================================================*/

    private void RefrescarSiAbierto()
    {
        if (cobroAbierto) ActualizarPanel();
    }

    /*========================================================================================*/

    void Start()
    {
        if (gestorPlazos == null) gestorPlazos = GestorPlazos.Instancia;
        if (gestorPlazos == null) Debug.LogWarning("PuntoCobro: no hay ningún GestorPlazos en la escena.");
    }

    /*========================================================================================*/

    void Update()
    {
        if (jugador == null) return;

        ActualizarPop();

        bool manosVacias = !jugador.TieneObjetoAgarrado();
        if (avisoCobro != null) avisoCobro.SetActive(jugadorEnZona && manosVacias && !cobroAbierto);

        if (cobroAbierto)
        {
            if (botonPagar != null) botonPagar.interactable = SePuedePagarAhora();
            if (Input.GetKeyDown(teclaCerrar)) CerrarCobro();
            return;
        }

        if (EstadoJuego.ControlesBloqueados) return;

        if (jugadorEnZona && manosVacias && Input.GetMouseButtonDown(0) && !EstadoJuego.RatonLibre) AbrirCobro();
    }

    /*========================================================================================*/

    public void JugadorEntraEnZona()
    {
        if (jugadorEnZona) return;

        jugador = JugadorLocal.Agarrar();
        jugadorEnZona = true;
        EstadoJuego.CambiarJugadorEnZonaCompra(true);
        Resaltar();
    }

    /*========================================================================================*/

    public void JugadorSaleDeZona()
    {
        if (!jugadorEnZona) return;

        jugadorEnZona = false;
        EstadoJuego.CambiarJugadorEnZonaCompra(false);
        QuitarResaltado();
    }

    /*========================================================================================*/

    private void AbrirCobro()
    {
        if (panelCobro == null) return;

        cobroAbierto = true;
        panelCobro.SetActive(true);
        ActualizarPanel();
        AnimarPop(panelCobro);
        EstadoJuego.AbrirMenu();
    }

    /*========================================================================================*/

    private void CerrarCobro()
    {
        if (!cobroAbierto) return;

        cobroAbierto = false;
        OcultarPanel();
        EstadoJuego.CerrarMenu();
    }

    /*========================================================================================*/

    public void PagarPlazo()
    {
        if (!SePuedePagarAhora()) return;

        int aportado = gestorPlazos.Aportar();
        if (aportado <= 0) return;

        tiempoUltimoPago = Time.unscaledTime;
        MostrarTextoCobro(aportado);
        ActualizarPanel();
    }

    /*========================================================================================*/

    private bool SePuedePagarAhora()
    {
        if (gestorPlazos == null) return false;
        if (Time.unscaledTime - tiempoUltimoPago < esperaTrasPagar) return false;
        return gestorPlazos.PuedeAportar();
    }

    /*========================================================================================*/

    private void ActualizarPanel()
    {
        if (botonPagar != null) botonPagar.interactable = SePuedePagarAhora();

        if (gestorPlazos == null)
        {
            if (textoInfo != null) textoInfo.text = "No hay gestor de plazos en la escena";
            return;
        }

        int importe = gestorPlazos.ObtenerImportePlazoActual();
        string colorHex = ColorUtility.ToHtmlStringRGB(colorFalta);
        string info;

        if (gestorPlazos.PlazosPagados())
        {
            if (textoTitulo != null) 
                textoTitulo.text = "<b><size=48>COBRO FINAL</size></b>";
            info = "Importe: <b>" + importe + "$</b>\nTu dinero: " + Dinero.Total + "$\n<color=#" + colorHex + ">Las orcas no aceptan negociar</color>";
        }
        else
        {
            if (textoTitulo != null) 
                textoTitulo.text = "<b><size=48>PLAZO " + gestorPlazos.capituloActual + " DE " + gestorPlazos.CantidadPlazos() + "</size></b>";
            int falta = gestorPlazos.FaltaPlazoActual();
            info = "Importe: <b>" + importe + "$</b>\nAportado: " + gestorPlazos.aportadoActual + "$\nTu dinero: " + Dinero.Total + "$";
            info += "\n<color=#" + colorHex + ">Faltan " + falta + "$</color>";
        }

        if (textoInfo != null) textoInfo.text = info;
    }

    /*========================================================================================*/

    private void MostrarTextoCobro(int importe)
    {
        if (plantillaTextoCobro == null) return;

        TextoFlotante copia = Instantiate(plantillaTextoCobro, plantillaTextoCobro.transform.parent);
        copia.gameObject.SetActive(true);
        copia.transform.SetAsLastSibling();
        copia.Mostrar("-" + importe + "$");
    }

    /*========================================================================================*/

    private void OcultarPanel()
    {
        if (panelAnimado != null) panelAnimado.localScale = Vector3.one;
        panelAnimado = null;

        if (panelCobro != null) panelCobro.SetActive(false);
    }

    /*========================================================================================*/

    private void AnimarPop(GameObject panel)
    {
        panelAnimado = panel.transform;
        panelAnimado.localScale = Vector3.zero;
        tiempoPop = 0f;
    }

    /*========================================================================================*/

    private void ActualizarPop()
    {
        if (panelAnimado == null) return;

        tiempoPop += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(tiempoPop / duracionPop);

        float suavizado = 1f - Mathf.Pow(1f - t, 3f);
        panelAnimado.localScale = Vector3.one * suavizado;

        if (t >= 1f) panelAnimado = null;
    }

    /*========================================================================================*/

    private void Resaltar()
    {
        if (resaltado || rendererCubo == null) return;

        colorOriginal = rendererCubo.material.color;
        rendererCubo.material.color = colorResaltado;
        resaltado = true;
    }

    /*========================================================================================*/

    private void QuitarResaltado()
    {
        if (!resaltado || rendererCubo == null) return;

        rendererCubo.material.color = colorOriginal;
        resaltado = false;
    }
}