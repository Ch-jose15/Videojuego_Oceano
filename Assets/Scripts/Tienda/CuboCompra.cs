using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class CuboCompra : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar jugador;
    public GameObject avisoComprar;
    public GameObject panelComprar;

    [Header("Compras")]
    public VidaJugador vidaJugador;
    public StaminaJugador staminaJugador;
    public DisparoJugador disparoJugador;
    public Button botonMejorarVida;
    public Button botonMejorarStamina;
    public Button botonDesbloquearDisparo;
    public Button botonCerrar;
    public int costeMejoraVida = 30;
    public float cantidadMejoraVida = 25f;
    public int costeMejoraStamina = 30;
    public float cantidadMejoraStamina = 30f;
    public int costeDesbloquearDisparo = 50;

    [Header("Linterna")]
    public Button botonLinterna;
    public int costeLinterna = 0;
    public Linterna prefabLinterna;
    public Transform puntoManoIzquierda;

    [Header("Teclas")]
    public KeyCode teclaCerrar = KeyCode.Escape;

    [Header("Resaltado")]
    public Color colorResaltado = new Color(0.3f, 1f, 0.4f);

    [Header("Animación del menú")]
    public float duracionPop = 0.18f;

    private Renderer rendererCubo;
    private Color colorOriginal;
    private bool resaltado = false;
    private bool comprarAbierto = false;
    private bool jugadorEnZona = false;
    private Transform panelAnimado;
    private float tiempoPop = 0f;

    /*========================================================================================*/

    void Awake()
    {
        rendererCubo = GetComponent<Renderer>();

        GetComponent<Collider>().isTrigger = false;

        PrepararBoton(botonMejorarVida, "Mejorar Vida - " + costeMejoraVida + "$", ComprarMejoraVida);
        PrepararBoton(botonMejorarStamina, "Mejorar Stamina - " + costeMejoraStamina + "$", ComprarMejoraStamina);
        PrepararBoton(botonDesbloquearDisparo, "Desbloquear Disparo - " + costeDesbloquearDisparo + "$", ComprarDesbloquearDisparo);
        PrepararBoton(botonLinterna, "Linterna - " + costeLinterna + "$", ComprarLinterna);
        PrepararBoton(botonCerrar, "Cerrar", CerrarComprar);

        OcultarPanel();
        if (avisoComprar != null) avisoComprar.SetActive(false);
    }

    /*========================================================================================*/

    void OnEnable()
    {
        MuerteJugador.AlMorirJugador += CerrarSiAbierto;
    }

    /*========================================================================================*/

    void OnDisable()
    {
        MuerteJugador.AlMorirJugador -= CerrarSiAbierto;
    }

    /*========================================================================================*/

    private void CerrarSiAbierto()
    {
        if (comprarAbierto) CerrarComprar();
    }

    /*========================================================================================*/

    public void VolverAVenderLinterna()
    {
        if (botonLinterna != null) botonLinterna.gameObject.SetActive(true);
    }

    /*========================================================================================*/

    public void VolverAVenderDisparo()
    {
        if (botonDesbloquearDisparo != null) botonDesbloquearDisparo.gameObject.SetActive(true);
    }

    /*========================================================================================*/

    void Update()
    {
        if (jugador == null) return;

        ActualizarPop();

        bool manosVacias = !jugador.TieneObjetoAgarrado();
        if (avisoComprar != null) avisoComprar.SetActive(jugadorEnZona && manosVacias && !comprarAbierto);

        if (comprarAbierto)
        {
            if (Input.GetKeyDown(teclaCerrar)) CerrarComprar();
            return;
        }

        if (EstadoJuego.ControlesBloqueados) return;

        if (jugadorEnZona && manosVacias && Input.GetMouseButtonDown(0) && !EstadoJuego.RatonLibre) AbrirComprar();
    }

    /*========================================================================================*/

    public void JugadorEntraEnZona()
    {
        BuscarJugadorLocal();
        jugadorEnZona = true;
        EstadoJuego.CambiarJugadorEnZonaCompra(true);
        Resaltar();
    }

    /*========================================================================================*/

    public void JugadorSaleDeZona()
    {
        jugadorEnZona = false;
        EstadoJuego.CambiarJugadorEnZonaCompra(false);
        QuitarResaltado();
    }

    /*========================================================================================*/

    private void BuscarJugadorLocal()
    {
        jugador = JugadorLocal.Agarrar();
        if (jugador == null) return;

        vidaJugador = jugador.GetComponent<VidaJugador>();
        staminaJugador = jugador.GetComponent<StaminaJugador>();
        disparoJugador = jugador.GetComponent<DisparoJugador>();
        if (jugador.camara != null) puntoManoIzquierda = jugador.camara.Find("PuntoManoIzquierda");
    }

    /*========================================================================================*/

    private void ActualizarBotones()
    {
        if (botonDesbloquearDisparo != null)
            botonDesbloquearDisparo.gameObject.SetActive(disparoJugador == null || !disparoJugador.disparoDesbloqueado);

        if (botonLinterna != null) botonLinterna.gameObject.SetActive(!TieneLinterna());
    }

    /*========================================================================================*/

    private void AbrirComprar()
    {
        ActualizarBotones();
        comprarAbierto = true;
        panelComprar.SetActive(true);
        AnimarPop(panelComprar);
        EstadoJuego.AbrirMenu();
    }

    /*========================================================================================*/

    private void CerrarComprar()
    {
        comprarAbierto = false;
        OcultarPanel();
        EstadoJuego.CerrarMenu();
    }

    /*========================================================================================*/

    private void OcultarPanel()
    {
        if (panelAnimado != null) panelAnimado.localScale = Vector3.one;
        panelAnimado = null;

        if (panelComprar != null) panelComprar.SetActive(false);
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

    /*========================================================================================*/

    private void PrepararBoton(Button boton, string texto, UnityEngine.Events.UnityAction alPulsar)
    {
        if (boton == null) return;

        boton.onClick.AddListener(alPulsar);
        boton.onClick.AddListener(EstadoJuego.QuitarSeleccionUI);

        TextMeshProUGUI textoBoton = boton.GetComponentInChildren<TextMeshProUGUI>();
        if (textoBoton != null) textoBoton.text = texto;
    }

    /*========================================================================================*/

    public void ComprarMejoraVida()
    {
        if (vidaJugador == null) return;
        if (!Dinero.Gastar(costeMejoraVida)) return;

        vidaJugador.AumentarVidaMaxima(cantidadMejoraVida);
        Debug.Log("Mejora de vida comprada. Vida máxima: " + vidaJugador.vidaMaxima);
    }

    /*========================================================================================*/

    public void ComprarMejoraStamina()
    {
        if (staminaJugador == null) return;
        if (!Dinero.Gastar(costeMejoraStamina)) return;

        staminaJugador.AumentarStaminaMaxima(cantidadMejoraStamina);
        Debug.Log("Mejora de stamina comprada. Stamina máxima: " + staminaJugador.staminaMaxima);
    }

    /*========================================================================================*/

    public void ComprarDesbloquearDisparo()
    {
        if (disparoJugador == null || disparoJugador.disparoDesbloqueado) return;
        if (!Dinero.Gastar(costeDesbloquearDisparo)) return;

        disparoJugador.Desbloquear();
        if (botonDesbloquearDisparo != null) botonDesbloquearDisparo.gameObject.SetActive(false);
        Debug.Log("Disparo desbloqueado");
    }

    /*========================================================================================*/

    public void ComprarLinterna()
    {
        if (prefabLinterna == null || puntoManoIzquierda == null || TieneLinterna()) return;
        if (costeLinterna > 0 && !Dinero.Gastar(costeLinterna)) return;

        Linterna linterna = Instantiate(prefabLinterna, puntoManoIzquierda);
        linterna.transform.localPosition = Vector3.zero;
        linterna.transform.localRotation = Quaternion.identity;
        linterna.name = "Linterna";

        if (botonLinterna != null) botonLinterna.gameObject.SetActive(false);
        Debug.Log("Linterna comprada");
    }

    /*========================================================================================*/

    private bool TieneLinterna()
    {
        return puntoManoIzquierda != null && puntoManoIzquierda.GetComponentInChildren<Linterna>() != null;
    }
}