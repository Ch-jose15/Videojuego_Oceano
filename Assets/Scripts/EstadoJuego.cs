using UnityEngine;

//Estado general de la partida que varios scripts necesitan consultar.
//Por ahora solo guarda si hay un menú abierto: mientras lo esté, el jugador no se mueve ni usa sus teclas
//(así las teclas del menú no activan otras cosas a la vez) y el juego se pausa
public static class EstadoJuego
{
    public static bool MenuAbierto { get; private set; } = false;

    //Frame en el que se cerró el último menú
    private static int frameCierreMenu = -1;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public static bool ControlesBloqueados
    {
        get { return MenuAbierto || Time.frameCount == frameCierreMenu; }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public static void AbrirMenu()
    {
        MenuAbierto = true;
        Time.timeScale = 0f; //Pausa la física, los enemigos y todo lo que dependa del tiempo
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public static void CerrarMenu()
    {
        MenuAbierto = false;
        frameCierreMenu = Time.frameCount;
        Time.timeScale = 1f;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Las variables estáticas pueden conservar su valor entre partidas en el editor. Esto las reinicia al pulsar Play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        MenuAbierto = false;
        frameCierreMenu = -1;
        Time.timeScale = 1f;
    }
}
