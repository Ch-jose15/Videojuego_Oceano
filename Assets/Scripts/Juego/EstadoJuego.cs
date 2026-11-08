using UnityEngine;
using UnityEngine.EventSystems;

public static class EstadoJuego
{
    private static bool inventarioAbierto = false;
    private static int zonasCompraOcupadas = 0;
    private static int frameCierreMenu = -1;
    private static float tiempoCierreUI = -10f;
    private const float duracionReaplicarCursor = 0.5f;

    public static bool MenuAbierto { get; private set; } = false;
    public static bool JugadorEnZonaCompra { get { return zonasCompraOcupadas > 0; } }
    public static bool JugadorMuerto { get; private set; } = false;

    /*========================================================================================*/

    public static bool ControlesBloqueados
    {
        get { return MenuAbierto || JugadorMuerto || Time.frameCount == frameCierreMenu; }
    }

    /*========================================================================================*/

    public static bool RatonLibre
    {
        get { return MenuAbierto || inventarioAbierto; }
    }

    /*========================================================================================*/

    public static void AbrirMenu()
    {
        MenuAbierto = true;
        if (!JugadorLocal.EnRed()) Time.timeScale = 0f;
        AplicarCursor();
    }

    /*========================================================================================*/

    public static void CerrarMenu()
    {
        MenuAbierto = false;
        frameCierreMenu = Time.frameCount;
        tiempoCierreUI = Time.unscaledTime;
        Time.timeScale = 1f;
        AplicarCursor();
    }

    /*========================================================================================*/

    public static void CambiarInventarioAbierto(bool abierto)
    {
        inventarioAbierto = abierto;
        if (!abierto) tiempoCierreUI = Time.unscaledTime;
        AplicarCursor();
    }

    /*========================================================================================*/

    public static void MantenerCursorTrasCerrar()
    {
        if (RatonLibre) return;
        if (Time.unscaledTime - tiempoCierreUI > duracionReaplicarCursor) return;
        if (Cursor.lockState != CursorLockMode.Locked || Cursor.visible) AplicarCursor();
    }

    /*========================================================================================*/

    public static void CambiarJugadorMuerto(bool muerto)
    {
        JugadorMuerto = muerto;
    }

    /*========================================================================================*/

    public static void CambiarJugadorEnZonaCompra(bool enZona)
    {
        zonasCompraOcupadas = Mathf.Max(0, zonasCompraOcupadas + (enZona ? 1 : -1));
    }

    /*========================================================================================*/

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

    /*========================================================================================*/

    public static void QuitarSeleccionUI()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    /*========================================================================================*/

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