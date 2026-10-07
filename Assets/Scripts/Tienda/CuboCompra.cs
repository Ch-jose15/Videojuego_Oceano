// Cubo de compra: con las manos vacías en su zona, un clic abre el panel de mejoras (vida, stamina y disparo) y pausa el juego.
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

    // Busca las referencias, conecta los botones y oculta la UI de compra
    void Awake()
    {
        rendererCubo = GetComponent<Renderer>();

        GetComponent<Collider>().isTrigger = false;

        if (jugador == null) jugador = FindFirstObjectByType<InteracciónAgarrar>();
        if (jugador != null && vidaJugador == null) vidaJugador = jugador.GetComponent<VidaJugador>();
        if (jugador != null && staminaJugador == null) staminaJugador = jugador.GetComponent<StaminaJugador>();
        if (jugador != null && disparoJugador == null) disparoJugador = jugador.GetComponent<DisparoJugador>();

        PrepararBoton(botonMejorarVida, "Mejorar Vida - " + costeMejoraVida + "$", ComprarMejoraVida);
        PrepararBoton(botonMejorarStamina, "Mejorar Stamina - " + costeMejoraStamina + "$", ComprarMejoraStamina);
        PrepararBoton(botonDesbloquearDisparo, "Desbloquear Disparo - " + costeDesbloquearDisparo + "$", ComprarDesbloquearDisparo);
        PrepararBoton(botonCerrar, "Cerrar", CerrarComprar);

        if (botonDesbloquearDisparo != null && disparoJugador != null && disparoJugador.disparoDesbloqueado)
            botonDesbloquearDisparo.gameObject.SetActive(false);

        OcultarPanel();
        if (avisoComprar != null) avisoComprar.SetActive(false);
    }

    // Muestra el aviso de compra y abre o cierra el panel según el clic y la tecla de cerrar
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

    // Marca que el jugador ha entrado en la zona de compra y resalta el cubo
    public void JugadorEntraEnZona()
    {
        jugadorEnZona = true;
        EstadoJuego.CambiarJugadorEnZonaCompra(true);
        Resaltar();
    }

    // Marca que el jugador ha salido de la zona de compra y quita el resaltado
    public void JugadorSaleDeZona()
    {
        jugadorEnZona = false;
        EstadoJuego.CambiarJugadorEnZonaCompra(false);
        QuitarResaltado();
    }

    // Abre el panel de mejoras con animación y pausa el juego
    private void AbrirComprar()
    {
        comprarAbierto = true;
        panelComprar.SetActive(true);
        AnimarPop(panelComprar);
        EstadoJuego.AbrirMenu();
    }

    // Cierra el panel de mejoras y reanuda el juego
    private void CerrarComprar()
    {
        comprarAbierto = false;
        OcultarPanel();
        EstadoJuego.CerrarMenu();
    }

    // Oculta el panel y termina la animación si estaba a medias
    private void OcultarPanel()
    {
        if (panelAnimado != null) panelAnimado.localScale = Vector3.one;
        panelAnimado = null;

        if (panelComprar != null) panelComprar.SetActive(false);
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

    // Conecta un botón con su función y le pone el texto
    private void PrepararBoton(Button boton, string texto, UnityEngine.Events.UnityAction alPulsar)
    {
        if (boton == null) return;

        boton.onClick.AddListener(alPulsar);
        boton.onClick.AddListener(EstadoJuego.QuitarSeleccionUI);

        TextMeshProUGUI textoBoton = boton.GetComponentInChildren<TextMeshProUGUI>();
        if (textoBoton != null) textoBoton.text = texto;
    }

    // Cobra la mejora de vida y sube la vida máxima del jugador si hay dinero suficiente
    public void ComprarMejoraVida()
    {
        if (vidaJugador == null) return;
        if (!Dinero.Gastar(costeMejoraVida)) return;

        vidaJugador.AumentarVidaMaxima(cantidadMejoraVida);
        Debug.Log("Mejora de vida comprada. Vida máxima: " + vidaJugador.vidaMaxima);
    }

    // Cobra la mejora de stamina y sube la stamina máxima del jugador si hay dinero suficiente
    public void ComprarMejoraStamina()
    {
        if (staminaJugador == null) return;
        if (!Dinero.Gastar(costeMejoraStamina)) return;

        staminaJugador.AumentarStaminaMaxima(cantidadMejoraStamina);
        Debug.Log("Mejora de stamina comprada. Stamina máxima: " + staminaJugador.staminaMaxima);
    }

    // Cobra y desbloquea el disparo una sola vez, ocultando después su botón
    public void ComprarDesbloquearDisparo()
    {
        if (disparoJugador == null || disparoJugador.disparoDesbloqueado) return;
        if (!Dinero.Gastar(costeDesbloquearDisparo)) return;

        disparoJugador.Desbloquear();
        if (botonDesbloquearDisparo != null) botonDesbloquearDisparo.gameObject.SetActive(false);
        Debug.Log("Disparo desbloqueado");
    }
}
