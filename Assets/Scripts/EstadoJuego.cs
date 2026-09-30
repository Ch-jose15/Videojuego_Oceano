using UnityEngine;
using UnityEngine.EventSystems;

//Estado general de la partida que varios scripts necesitan consultar:
// - Si hay un menú abierto (la Tienda): el juego se pausa y el jugador no usa sus teclas.
// - Si el ratón tiene que estar libre (menú de la tienda o inventario): se ve el cursor y la cámara no gira.
public static class EstadoJuego
{
    public static bool MenuAbierto { get; private set; } = false;

    //El inventario se usa con el ratón pero NO pausa el juego
    private static bool inventarioAbierto = false;

    //Frame en el que se cerró el último menú
    private static int frameCierreMenu = -1;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public static bool ControlesBloqueados
    {
        get { return MenuAbierto || Time.frameCount == frameCierreMenu; }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //True si hay alguna UI que se usa con el ratón: el cursor se ve y la cámara no gira
    public static bool RatonLibre
    {
        get { return MenuAbierto || inventarioAbierto; }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public static void AbrirMenu()
    {
        MenuAbierto = true;
        Time.timeScale = 0f; //Pausa la física, los enemigos y todo lo que dependa del tiempo
        AplicarCursor();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public static void CerrarMenu()
    {
        MenuAbierto = false;
        frameCierreMenu = Time.frameCount;
        Time.timeScale = 1f;
        AplicarCursor();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Lo llama la UI del inventario al abrirse/cerrarse
    public static void CambiarInventarioAbierto(bool abierto)
    {
        inventarioAbierto = abierto;
        AplicarCursor();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Si alguna UI necesita el ratón, lo liberamos; si no, lo bloqueamos en el centro para mirar con él
    private static void AplicarCursor()
    {
        if (RatonLibre)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Después de hacer clic en un botón, Unity lo deja "seleccionado" y la tecla Espacio lo vuelve a pulsar.
    //Como Espacio es nadar hacia arriba, quitamos la selección tras cada clic para que no se repita sin querer
    public static void QuitarSeleccionUI()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Las variables estáticas pueden conservar su valor entre partidas en el editor. Esto las reinicia al pulsar Play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        MenuAbierto = false;
        inventarioAbierto = false;
        frameCierreMenu = -1;
        Time.timeScale = 1f;
    }
}
