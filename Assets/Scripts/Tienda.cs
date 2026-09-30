using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Tienda sólida con una zona alrededor (objeto hijo con ZonaTienda).
//Mientras el jugador está dentro de la zona, la tienda se resalta y:
//  - Si lleva un objeto en la mano: aparece "Pulsa F para vender" y con F se vende directamente.
//  - Si no lleva nada: aparece "Clic para comprar" y con el clic izquierdo se abre el panel de mejoras
//    (Mejorar Vida, Mejorar Stamina, Cerrar). Así el ratón sigue bloqueado y la cámara gira normal hasta abrir el panel.
//Mientras el panel de comprar está abierto el juego se pausa (ver EstadoJuego)
[RequireComponent(typeof(Collider))]
public class Tienda : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar jugador;
    public GameObject avisoVender;      //Texto "Pulsa F para vender"
    public GameObject avisoComprar;     //Texto "Clic para comprar"
    public GameObject panelComprar;     //Panel con los botones de mejoras y Cerrar

    [Header("Feedback al vender")]
    public TextoFlotante plantillaTextoVenta; //Texto desactivado en el Canvas que se copia al vender ("+15$")
    public AudioClip sonidoVenta;             //Se asigna a mano en el Inspector

    [Header("Compras")]
    public VidaJugador vidaJugador;         //Si está vacío, se busca en el jugador
    public StaminaJugador staminaJugador;   //Si está vacío, se busca en el jugador
    public Button botonMejorarVida;
    public Button botonMejorarStamina;
    public Button botonCerrar;
    public int costeMejoraVida = 30;
    public float cantidadMejoraVida = 25f;
    public int costeMejoraStamina = 30;
    public float cantidadMejoraStamina = 30f; //30 = el coste de un dash, así cada mejora da exactamente 1 dash más

    [Header("Teclas")]
    public KeyCode teclaVender = KeyCode.F;
    public KeyCode teclaCerrar = KeyCode.Escape;

    [Header("Resaltado")]
    public Color colorResaltado = new Color(0.3f, 1f, 0.4f);

    [Header("Animación del menú")]
    public float duracionPop = 0.18f; //Segundos que tarda el panel en crecer de 0 a su tamaño normal

    private Collider colliderTienda;
    private Renderer rendererTienda;
    private AudioSource fuenteAudio;
    private Color colorOriginal;
    private bool resaltada = false;
    private bool comprarAbierto = false;
    private bool jugadorEnZona = false;
    private Transform panelAnimado; //Panel que está haciendo la animación "pop" (null si ninguno)
    private float tiempoPop = 0f;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        colliderTienda = GetComponent<Collider>();
        rendererTienda = GetComponent<Renderer>();

        //AudioSource para el sonido de venta. Si no hay uno en el objeto, lo creamos
        fuenteAudio = GetComponent<AudioSource>();
        if (fuenteAudio == null) fuenteAudio = gameObject.AddComponent<AudioSource>();
        fuenteAudio.playOnAwake = false;

        //La tienda es sólida: no se puede atravesar (la zona de interacción es el trigger del objeto hijo)
        colliderTienda.isTrigger = false;

        //Si no se ha asignado el jugador en el Inspector, lo buscamos en la escena
        if (jugador == null) jugador = FindFirstObjectByType<InteracciónAgarrar>();
        if (jugador != null && vidaJugador == null) vidaJugador = jugador.GetComponent<VidaJugador>();
        if (jugador != null && staminaJugador == null) staminaJugador = jugador.GetComponent<StaminaJugador>();

        //Conectamos cada botón con su función
        PrepararBoton(botonMejorarVida, "Mejorar Vida - " + costeMejoraVida + "$", ComprarMejoraVida);
        PrepararBoton(botonMejorarStamina, "Mejorar Stamina - " + costeMejoraStamina + "$", ComprarMejoraStamina);
        PrepararBoton(botonCerrar, "Cerrar", CerrarComprar);

        //Todo empieza oculto
        OcultarPanel();
        if (avisoVender != null) avisoVender.SetActive(false);
        if (avisoComprar != null) avisoComprar.SetActive(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        if (jugador == null) return;

        ActualizarPop();
        ActualizarUIZona();

        if (comprarAbierto)
        {
            if (Input.GetKeyDown(teclaCerrar)) CerrarComprar();
            return;
        }

        if (EstadoJuego.ControlesBloqueados) return;

        if (!jugadorEnZona) return;

        //Con algo en la mano: F vende directamente
        if (jugador.TieneObjetoAgarrado())
        {
            if (Input.GetKeyDown(teclaVender)) Vender();
        }
        //Con las manos vacías: clic izquierdo abre el panel de comprar
        //(salvo que el ratón esté libre para otra UI, por ejemplo el inventario abierto: ahí el clic es para sus slots)
        else if (Input.GetMouseButtonDown(0) && !EstadoJuego.RatonLibre)
        {
            AbrirComprar();
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Los llama ZonaTienda cuando el jugador entra o sale del trigger
    public void JugadorEntraEnZona()
    {
        jugadorEnZona = true;
        Resaltar();
    }

    public void JugadorSaleDeZona()
    {
        jugadorEnZona = false;
        QuitarResaltado();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Cada frame decide qué aviso se ve en la zona: el de vender (con objeto) o el de comprar (sin objeto).
    //Se hace cada frame porque el jugador puede coger o guardar objetos estando dentro de la zona
    private void ActualizarUIZona()
    {
        bool enZonaSinPanel = jugadorEnZona && !comprarAbierto;
        bool llevaObjeto = jugador.TieneObjetoAgarrado();

        if (avisoVender != null) avisoVender.SetActive(enZonaSinPanel && llevaObjeto);
        if (avisoComprar != null) avisoComprar.SetActive(enZonaSinPanel && !llevaObjeto);
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
        if (resaltada || rendererTienda == null) return;

        colorOriginal = rendererTienda.material.color;
        rendererTienda.material.color = colorResaltado;
        resaltada = true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void QuitarResaltado()
    {
        if (!resaltada || rendererTienda == null) return;

        rendererTienda.material.color = colorOriginal;
        resaltada = false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Vende el objeto que llevamos en la mano. Si no llevamos nada, no hace nada
    private void Vender()
    {
        //Le quitamos el objeto de la mano al jugador (se queda con las manos libres)
        GameObject objeto = jugador.QuitarObjetoAgarrado();
        if (objeto == null) return;

        //Miramos cuánto vale. Si no tiene ValorItem, vale 0 y avisamos en consola
        int valor = 0;
        ValorItem valorItem = objeto.GetComponent<ValorItem>();
        if (valorItem != null) valor = valorItem.valor;
        else Debug.LogWarning("El objeto " + objeto.name + " no tiene el componente ValorItem, se vende por 0.");

        Dinero.Sumar(valor);
        Debug.Log("Vendido " + objeto.name + " por " + valor + ". Dinero total: " + Dinero.Total);

        //El objeto vendido desaparece
        Destroy(objeto);

        //Feedback: texto "+X$" y sonido
        MostrarTextoVenta(valor);
        if (sonidoVenta != null) fuenteAudio.PlayOneShot(sonidoVenta);
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

    //Hace una copia de la plantilla (en el mismo sitio del Canvas), la activa y le pone el texto
    private void MostrarTextoVenta(int valor)
    {
        if (plantillaTextoVenta == null) return;

        TextoFlotante copia = Instantiate(plantillaTextoVenta, plantillaTextoVenta.transform.parent);
        copia.gameObject.SetActive(true);
        copia.Mostrar("+" + valor + "$");
    }
}
