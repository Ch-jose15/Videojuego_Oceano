// Punto de cobro de la deuda: con las manos vacías en su zona, un clic abre el panel para pagar el plazo actual a la mafia de las orcas.
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

    // Busca las referencias, conecta los botones y oculta la UI de cobro
    void Awake()
    {
        if (jugador == null) jugador = FindFirstObjectByType<InteracciónAgarrar>();
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

    // Empieza a escuchar la muerte del jugador para cerrar el panel
    void OnEnable()
    {
        MuerteJugador.AlMorirJugador += CerrarCobro;
    }

    // Deja de escuchar la muerte del jugador
    void OnDisable()
    {
        MuerteJugador.AlMorirJugador -= CerrarCobro;
    }

    // Busca el gestor de plazos de la escena si no se asignó
    void Start()
    {
        if (gestorPlazos == null) gestorPlazos = GestorPlazos.Instancia;
        if (gestorPlazos == null) Debug.LogWarning("PuntoCobro: no hay ningún GestorPlazos en la escena.");
    }

    // Muestra el aviso de cobro, abre o cierra el panel y refresca el estado del botón Pagar
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

    // Marca que el jugador ha entrado en la zona de cobro y resalta el cubo
    public void JugadorEntraEnZona()
    {
        if (jugadorEnZona) return;

        jugadorEnZona = true;
        EstadoJuego.CambiarJugadorEnZonaCompra(true);
        Resaltar();
    }

    // Marca que el jugador ha salido de la zona de cobro y quita el resaltado
    public void JugadorSaleDeZona()
    {
        if (!jugadorEnZona) return;

        jugadorEnZona = false;
        EstadoJuego.CambiarJugadorEnZonaCompra(false);
        QuitarResaltado();
    }

    // Abre el panel de cobro con animación y pausa el juego
    private void AbrirCobro()
    {
        if (panelCobro == null) return;

        cobroAbierto = true;
        panelCobro.SetActive(true);
        ActualizarPanel();
        AnimarPop(panelCobro);
        EstadoJuego.AbrirMenu();
    }

    // Cierra el panel de cobro y reanuda el juego
    private void CerrarCobro()
    {
        if (!cobroAbierto) return;

        cobroAbierto = false;
        OcultarPanel();
        EstadoJuego.CerrarMenu();
    }

    // Paga el plazo actual mediante el gestor, muestra "-X$" y actualiza el panel con el siguiente plazo
    public void PagarPlazo()
    {
        if (!SePuedePagarAhora()) return;

        int importe = gestorPlazos.ObtenerImportePlazoActual();
        if (!gestorPlazos.PagarPlazo()) return;

        tiempoUltimoPago = Time.unscaledTime;
        MostrarTextoCobro(importe);
        ActualizarPanel();
    }

    // Indica si ahora mismo se puede pagar: hay dinero, quedan plazos y no se acaba de pagar (evita el doble clic)
    private bool SePuedePagarAhora()
    {
        if (gestorPlazos == null) return false;
        if (Time.unscaledTime - tiempoUltimoPago < esperaTrasPagar) return false;
        return gestorPlazos.PuedePagar();
    }

    // Escribe en el panel el plazo, el importe, el dinero del jugador y lo que falta
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
            if (textoTitulo != null) textoTitulo.text = "<b><size=48>COBRO FINAL</size></b>";
            info = "Importe: <b>" + importe + "$</b>\nTu dinero: " + Dinero.Total + "$\n<color=#" + colorHex + ">Las orcas no aceptan negociar</color>";
        }
        else
        {
            if (textoTitulo != null) textoTitulo.text = "<b><size=48>PLAZO " + gestorPlazos.capituloActual + " DE " + gestorPlazos.CantidadPlazos() + "</size></b>";
            info = "Importe: <b>" + importe + "$</b>\nTu dinero: " + Dinero.Total + "$";

            int falta = importe - Dinero.Total;
            if (falta > 0) info += "\n<color=#" + colorHex + ">Te faltan " + falta + "$</color>";
            else info += "\nTienes suficiente para pagar";
        }

        if (textoInfo != null) textoInfo.text = info;
    }

    // Muestra en pantalla el texto flotante "-X$" con lo pagado
    private void MostrarTextoCobro(int importe)
    {
        if (plantillaTextoCobro == null) return;

        TextoFlotante copia = Instantiate(plantillaTextoCobro, plantillaTextoCobro.transform.parent);
        copia.gameObject.SetActive(true);
        copia.transform.SetAsLastSibling();
        copia.Mostrar("-" + importe + "$");
    }

    // Oculta el panel y termina la animación si estaba a medias
    private void OcultarPanel()
    {
        if (panelAnimado != null) panelAnimado.localScale = Vector3.one;
        panelAnimado = null;

        if (panelCobro != null) panelCobro.SetActive(false);
    }

    // Empieza la animación de aparición del panel desde escala cero
    private void AnimarPop(GameObject panel)
    {
        panelAnimado = panel.transform;
        panelAnimado.localScale = Vector3.zero;
        tiempoPop = 0f;
    }

    // Hace crecer el panel con suavizado aunque el juego esté en pausa
    private void ActualizarPop()
    {
        if (panelAnimado == null) return;

        tiempoPop += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(tiempoPop / duracionPop);

        float suavizado = 1f - Mathf.Pow(1f - t, 3f);
        panelAnimado.localScale = Vector3.one * suavizado;

        if (t >= 1f) panelAnimado = null;
    }

    // Pinta el cubo con el color de resaltado
    private void Resaltar()
    {
        if (resaltado || rendererCubo == null) return;

        colorOriginal = rendererCubo.material.color;
        rendererCubo.material.color = colorResaltado;
        resaltado = true;
    }

    // Devuelve al cubo su color original
    private void QuitarResaltado()
    {
        if (!resaltado || rendererCubo == null) return;

        rendererCubo.material.color = colorOriginal;
        resaltado = false;
    }
}
