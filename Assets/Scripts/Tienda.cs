using UnityEngine;

//Tienda sólida. Si el jugador la mira dentro de su alcance se resalta, y con F se abre un menú:
//  1 = Vender el objeto que llevamos en la mano
//  2 = Comprar (de momento solo abre un panel vacío)
//  F o Escape = cerrar
//Mientras el menú está abierto el juego se pausa (ver EstadoJuego)
[RequireComponent(typeof(Collider))]
public class Tienda : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar jugador;
    public GameObject panelMenu;      //Panel con las opciones Vender / Comprar
    public GameObject panelComprar;   //Panel de compra (placeholder)

    [Header("Teclas")]
    public KeyCode teclaAbrir = KeyCode.F;
    public KeyCode teclaVender = KeyCode.Alpha1;
    public KeyCode teclaComprar = KeyCode.Alpha2;
    public KeyCode teclaCerrar = KeyCode.Escape;

    [Header("Resaltado")]
    public Color colorResaltado = new Color(0.3f, 1f, 0.4f);

    private Collider colliderTienda;
    private Renderer rendererTienda;
    private Color colorOriginal;
    private bool resaltada = false;
    private bool menuAbierto = false;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        colliderTienda = GetComponent<Collider>();
        rendererTienda = GetComponent<Renderer>();

        //La tienda es sólida: no se puede atravesar
        colliderTienda.isTrigger = false;

        //Si no se ha asignado el jugador en el Inspector, lo buscamos en la escena
        if (jugador == null) jugador = FindFirstObjectByType<InteracciónAgarrar>();

        //Los paneles empiezan ocultos
        OcultarPaneles();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        if (jugador == null) return;

        if (menuAbierto)
        {
            ManejarMenu();
            return;
        }

        //Si otro menú está abierto, la tienda no hace nada
        if (EstadoJuego.ControlesBloqueados) return;

        bool mirando = JugadorMirandoTienda();

        if (mirando) Resaltar();
        else QuitarResaltado();

        if (mirando && Input.GetKeyDown(teclaAbrir)) AbrirMenu();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Teclas mientras el menú está abierto
    private void ManejarMenu()
    {
        //F o Escape cierran siempre (tanto el menú como el panel de comprar)
        if (Input.GetKeyDown(teclaAbrir) || Input.GetKeyDown(teclaCerrar))
        {
            CerrarMenu();
            return;
        }

        //Las opciones solo se pueden elegir en el menú principal, no dentro del panel de comprar
        if (!panelMenu.activeSelf) return;

        if (Input.GetKeyDown(teclaVender))
        {
            Vender();
            CerrarMenu();
        }
        else if (Input.GetKeyDown(teclaComprar))
        {
            //Cambiamos al panel de comprar. Se cierra con F o Escape
            panelMenu.SetActive(false);
            panelComprar.SetActive(true);
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void AbrirMenu()
    {
        menuAbierto = true;
        panelMenu.SetActive(true);
        panelComprar.SetActive(false);
        EstadoJuego.AbrirMenu();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void CerrarMenu()
    {
        menuAbierto = false;
        OcultarPaneles();
        EstadoJuego.CerrarMenu();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void OcultarPaneles()
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelComprar != null) panelComprar.SetActive(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Lanza un rayo desde la cámara del jugador (con su mismo alcance) y comprueba si choca con la tienda
    private bool JugadorMirandoTienda()
    {
        Ray rayo = new Ray(jugador.camara.position, jugador.camara.forward);
        RaycastHit hit;
        if (Physics.Raycast(rayo, out hit, jugador.distanciaAlcance))
        {
            return hit.collider == colliderTienda;
        }
        return false;
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
    }
}
