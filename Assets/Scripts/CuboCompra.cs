using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Cubo de compra, con una zona alrededor (objeto hijo con ZonaJugador).
//Mientras el jugador está dentro de la zona con las manos vacías, el cubo se resalta y aparece "Clic para comprar".
//Con el clic izquierdo se abre el panel de mejoras (Mejorar Vida, Mejorar Stamina, Desbloquear Disparo, Cerrar).
//Así el ratón sigue bloqueado y la cámara gira normal hasta abrir el panel.
//Mientras el panel está abierto el juego se pausa (ver EstadoJuego)
[RequireComponent(typeof(Collider))]
public class CuboCompra : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar jugador;  //Si está vacío, se busca en la escena
    public GameObject avisoComprar;     //Texto "Clic para comprar"
    public GameObject panelComprar;     //Panel con los botones de mejoras y Cerrar

    [Header("Compras")]
    public VidaJugador vidaJugador;         //Si está vacío, se busca en el jugador
    public StaminaJugador staminaJugador;   //Si está vacío, se busca en el jugador
    public DisparoJugador disparoJugador;   //Si está vacío, se busca en el jugador
    public Button botonMejorarVida;
    public Button botonMejorarStamina;
    public Button botonDesbloquearDisparo;  //Desaparece al comprarlo (solo se compra una vez)
    public Button botonCerrar;
    public int costeMejoraVida = 30;
    public float cantidadMejoraVida = 25f;
    public int costeMejoraStamina = 30;
    public float cantidadMejoraStamina = 30f; //30 = el coste de un dash, así cada mejora da exactamente 1 dash más
    public int costeDesbloquearDisparo = 50;

    [Header("Teclas")]
    public KeyCode teclaCerrar = KeyCode.Escape;

    [Header("Resaltado")]
    public Color colorResaltado = new Color(0.3f, 1f, 0.4f);

    [Header("Animación del menú")]
    public float duracionPop = 0.18f; //Segundos que tarda el panel en crecer de 0 a su tamaño normal

    private Renderer rendererCubo;
    private Color colorOriginal;
    private bool resaltado = false;
    private bool comprarAbierto = false;
    private bool jugadorEnZona = false;
    private Transform panelAnimado; //Panel que está haciendo la animación "pop" (null si ninguno)
    private float tiempoPop = 0f;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        rendererCubo = GetComponent<Renderer>();

        //El cubo es sólido: no se puede atravesar (la zona de interacción es el trigger del objeto hijo)
        GetComponent<Collider>().isTrigger = false;

        //Si no se ha asignado el jugador en el Inspector, lo buscamos en la escena
        if (jugador == null) jugador = FindFirstObjectByType<InteracciónAgarrar>();
        if (jugador != null && vidaJugador == null) vidaJugador = jugador.GetComponent<VidaJugador>();
        if (jugador != null && staminaJugador == null) staminaJugador = jugador.GetComponent<StaminaJugador>();
        if (jugador != null && disparoJugador == null) disparoJugador = jugador.GetComponent<DisparoJugador>();

        //Conectamos cada botón con su función
        PrepararBoton(botonMejorarVida, "Mejorar Vida - " + costeMejoraVida + "$", ComprarMejoraVida);
        PrepararBoton(botonMejorarStamina, "Mejorar Stamina - " + costeMejoraStamina + "$", ComprarMejoraStamina);
        PrepararBoton(botonDesbloquearDisparo, "Desbloquear Disparo - " + costeDesbloquearDisparo + "$", ComprarDesbloquearDisparo);
        PrepararBoton(botonCerrar, "Cerrar", CerrarComprar);

        //Si el disparo ya estuviera desbloqueado (por ejemplo, marcado a mano en el Inspector), ocultamos su botón
        if (botonDesbloquearDisparo != null && disparoJugador != null && disparoJugador.disparoDesbloqueado)
            botonDesbloquearDisparo.gameObject.SetActive(false);

        //Todo empieza oculto
        OcultarPanel();
        if (avisoComprar != null) avisoComprar.SetActive(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        if (jugador == null) return;

        ActualizarPop();

        //El aviso solo se ve en la zona, con las manos vacías y con el panel cerrado
        bool manosVacias = !jugador.TieneObjetoAgarrado();
        if (avisoComprar != null) avisoComprar.SetActive(jugadorEnZona && manosVacias && !comprarAbierto);

        if (comprarAbierto)
        {
            if (Input.GetKeyDown(teclaCerrar)) CerrarComprar();
            return;
        }

        if (EstadoJuego.ControlesBloqueados) return;

        //Clic izquierdo en la zona con las manos vacías: abre el panel
        //(salvo que el ratón esté libre para otra UI, por ejemplo el inventario abierto: ahí el clic es para sus slots)
        if (jugadorEnZona && manosVacias && Input.GetMouseButtonDown(0) && !EstadoJuego.RatonLibre) AbrirComprar();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Los llama la ZonaJugador hija (eventos alEntrarJugador / alSalirJugador)
    public void JugadorEntraEnZona()
    {
        jugadorEnZona = true;
        EstadoJuego.CambiarJugadorEnZonaCompra(true); //Para que el clic aquí sea para comprar y no para disparar
        Resaltar();
    }

    public void JugadorSaleDeZona()
    {
        jugadorEnZona = false;
        EstadoJuego.CambiarJugadorEnZonaCompra(false);
        QuitarResaltado();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Clic izquierdo en la zona: abre el panel de mejoras, pausa el juego y libera el ratón para los botones
    private void AbrirComprar()
    {
        comprarAbierto = true;
        panelComprar.SetActive(true);
        AnimarPop(panelComprar);
        EstadoJuego.AbrirMenu();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Botón "Cerrar" (o Escape): cierra el panel y volvemos a jugar
    private void CerrarComprar()
    {
        comprarAbierto = false;
        OcultarPanel();
        EstadoJuego.CerrarMenu();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void OcultarPanel()
    {
        //Si se cierra a mitad de la animación, la terminamos (dejamos el panel a su tamaño normal)
        if (panelAnimado != null) panelAnimado.localScale = Vector3.one;
        panelAnimado = null;

        if (panelComprar != null) panelComprar.SetActive(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Empieza la animación "pop" de un panel: lo pone a escala 0 y ActualizarPop() lo hace crecer cada frame
    private void AnimarPop(GameObject panel)
    {
        panelAnimado = panel.transform;
        panelAnimado.localScale = Vector3.zero;
        tiempoPop = 0f;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Escala el panel de 0 a 1 con "ease-out": empieza rápido y frena al final, lo que da el efecto de "pop"
    private void ActualizarPop()
    {
        if (panelAnimado == null) return;

        //Tiempo "sin escalar": el menú pausa el juego (timeScale = 0) y con el tiempo normal no avanzaría
        tiempoPop += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(tiempoPop / duracionPop);

        //Ease-out cúbico: 1 - (1 - t)^3
        float suavizado = 1f - Mathf.Pow(1f - t, 3f);
        panelAnimado.localScale = Vector3.one * suavizado;

        //Animación terminada
        if (t >= 1f) panelAnimado = null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Resaltar()
    {
        if (resaltado || rendererCubo == null) return;

        colorOriginal = rendererCubo.material.color;
        rendererCubo.material.color = colorResaltado;
        resaltado = true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void QuitarResaltado()
    {
        if (!resaltado || rendererCubo == null) return;

        rendererCubo.material.color = colorOriginal;
        resaltado = false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Conecta el botón con su función y le pone el texto (por ejemplo "Mejorar Vida - 30$")
    private void PrepararBoton(Button boton, string texto, UnityEngine.Events.UnityAction alPulsar)
    {
        if (boton == null) return;

        boton.onClick.AddListener(alPulsar);
        boton.onClick.AddListener(EstadoJuego.QuitarSeleccionUI); //Para que Espacio no repita el clic

        TextMeshProUGUI textoBoton = boton.GetComponentInChildren<TextMeshProUGUI>();
        if (textoBoton != null) textoBoton.text = texto;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Si hay dinero suficiente, lo gasta y sube la vida máxima (y cura hasta el nuevo máximo). Si no, no hace nada
    public void ComprarMejoraVida()
    {
        if (vidaJugador == null) return;
        if (!Dinero.Gastar(costeMejoraVida)) return; //Gastar devuelve false (y no resta nada) si no llega el dinero

        vidaJugador.AumentarVidaMaxima(cantidadMejoraVida);
        Debug.Log("Mejora de vida comprada. Vida máxima: " + vidaJugador.vidaMaxima);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Igual que la de vida, pero con la stamina
    public void ComprarMejoraStamina()
    {
        if (staminaJugador == null) return;
        if (!Dinero.Gastar(costeMejoraStamina)) return;

        staminaJugador.AumentarStaminaMaxima(cantidadMejoraStamina);
        Debug.Log("Mejora de stamina comprada. Stamina máxima: " + staminaJugador.staminaMaxima);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Solo se puede comprar una vez: si hay dinero, lo gasta, desbloquea el disparo y oculta el botón
    public void ComprarDesbloquearDisparo()
    {
        if (disparoJugador == null || disparoJugador.disparoDesbloqueado) return;
        if (!Dinero.Gastar(costeDesbloquearDisparo)) return;

        disparoJugador.Desbloquear();
        if (botonDesbloquearDisparo != null) botonDesbloquearDisparo.gameObject.SetActive(false);
        Debug.Log("Disparo desbloqueado");
    }
}
