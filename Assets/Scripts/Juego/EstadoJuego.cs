// Estado global de la partida: si hay un menú abierto (pausa), si el ratón debe estar libre y si el jugador está en la zona de compra.
using UnityEngine;
using UnityEngine.EventSystems;

public static class EstadoJuego
{
    public static bool MenuAbierto { get; private set; } = false;

    private static bool inventarioAbierto = false;

    public static bool JugadorEnZonaCompra { get; private set; } = false;

    private static int frameCierreMenu = -1;

    public static bool ControlesBloqueados
    {
        get { return MenuAbierto || Time.frameCount == frameCierreMenu; }
    }

    public static bool RatonLibre
    {
        get { return MenuAbierto || inventarioAbierto; }
    }

    // Abre un menú: pausa el juego y libera el ratón
    public static void AbrirMenu()
    {
        MenuAbierto = true;
        Time.timeScale = 0f;
        AplicarCursor();
    }

    // Cierra el menú: reanuda el juego y vuelve a bloquear el ratón
    public static void CerrarMenu()
    {
        MenuAbierto = false;
        frameCierreMenu = Time.frameCount;
        Time.timeScale = 1f;
        AplicarCursor();
    }

    // Apunta si el inventario está abierto y actualiza el cursor
    public static void CambiarInventarioAbierto(bool abierto)
    {
        inventarioAbierto = abierto;
        AplicarCursor();
    }

    // Apunta si el jugador está dentro de la zona del CuboCompra
    public static void CambiarJugadorEnZonaCompra(bool enZona)
    {
        JugadorEnZonaCompra = enZona;
    }

    // Libera el cursor si alguna UI lo necesita o lo bloquea en el centro si no
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

    // Quita la selección del botón pulsado para que la tecla Espacio no lo vuelva a activar
    public static void QuitarSeleccionUI()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    // Reinicia el estado cada vez que se pulsa Play en el editor
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        MenuAbierto = false;
        inventarioAbierto = false;
        JugadorEnZonaCompra = false;
        frameCierreMenu = -1;
        Time.timeScale = 1f;
    }
}
