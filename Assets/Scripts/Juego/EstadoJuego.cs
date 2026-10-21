// Estado global de la partida: si hay un menú abierto (pausa), si el jugador está muerto, si el ratón debe estar libre y si el jugador está en una zona de compra o cobro.
using UnityEngine;
using UnityEngine.EventSystems;

public static class EstadoJuego
{
    public static bool MenuAbierto { get; private set; } = false;

    private static bool inventarioAbierto = false;

    public static bool JugadorEnZonaCompra { get { return zonasCompraOcupadas > 0; } }

    private static int zonasCompraOcupadas = 0;

    private static int frameCierreMenu = -1;

    private static float tiempoCierreUI = -10f;

    private const float duracionReaplicarCursor = 0.5f;

    public static bool JugadorMuerto { get; private set; } = false;

    public static bool ControlesBloqueados
    {
        get { return MenuAbierto || JugadorMuerto || Time.frameCount == frameCierreMenu; }
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
        tiempoCierreUI = Time.unscaledTime;
        Time.timeScale = 1f;
        AplicarCursor();
    }

    // Apunta si el inventario está abierto y actualiza el cursor
    public static void CambiarInventarioAbierto(bool abierto)
    {
        inventarioAbierto = abierto;
        if (!abierto) tiempoCierreUI = Time.unscaledTime;
        AplicarCursor();
    }

    // Vuelve a bloquear el ratón durante un instante tras cerrar una UI, por si el editor lo soltó al pulsar Esc
    public static void MantenerCursorTrasCerrar()
    {
        if (RatonLibre) return;
        if (Time.unscaledTime - tiempoCierreUI > duracionReaplicarCursor) return;

        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible) AplicarCursor();
    }

    // Apunta si el jugador está muerto (durante el fundido de muerte), lo que bloquea todos los controles
    public static void CambiarJugadorMuerto(bool muerto)
    {
        JugadorMuerto = muerto;
    }

    // Apunta si el jugador entra o sale de una zona con menú (CuboCompra o PuntoCobro); cuenta las zonas por si se solapan
    public static void CambiarJugadorEnZonaCompra(bool enZona)
    {
        zonasCompraOcupadas = Mathf.Max(0, zonasCompraOcupadas + (enZona ? 1 : -1));
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
        JugadorMuerto = false;
        inventarioAbierto = false;
        zonasCompraOcupadas = 0;
        frameCierreMenu = -1;
        tiempoCierreUI = -10f;
        Time.timeScale = 1f;
    }
}
