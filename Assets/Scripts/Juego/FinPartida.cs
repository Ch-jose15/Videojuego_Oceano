// Game Over: funde a negro, avisa de que se empieza desde cero y reinicia la partida (en red cierra la conexión y vuelve al Lobby; en solitario recarga la escena).
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinPartida : MonoBehaviour
{
    private static bool enCurso = false;

    /*========================================================================================*/

    public static void Lanzar(float espera, string escenaLobby)
    {
        if (enCurso) return;
        enCurso = true;

        GameObject objeto = new GameObject("FinPartida");
        DontDestroyOnLoad(objeto);
        FinPartida fin = objeto.AddComponent<FinPartida>();
        fin.StartCoroutine(fin.Secuencia(espera, escenaLobby));
    }

    /*========================================================================================*/

    private IEnumerator Secuencia(float espera, string escenaLobby)
    {
        EstadoJuego.CambiarJugadorMuerto(true);

        PantallaMuerte pantalla = FindFirstObjectByType<PantallaMuerte>();
        if (pantalla != null)
        {
            pantalla.MostrarAvisoEspectador(false, "");
            if (!pantalla.EstaEnNegro) yield return pantalla.FundirANegro(1f);
            pantalla.MostrarTitulo("GAME OVER", "La mafia de las orcas se cobra la deuda...\nSe empieza desde cero");
        }

        yield return new WaitForSecondsRealtime(espera);

        bool enRed = JugadorLocal.EnRed();
        if (enRed)
        {
            NetworkManager red = NetworkManager.Singleton;
            red.Shutdown();
            while (red != null && red.ShutdownInProgress) yield return null;
            if (red != null) Destroy(red.gameObject);
            yield return null;
        }

        Dinero.Reiniciar();
        EstadoJuego.Reiniciar();
        Time.timeScale = 1f;

        if (enRed)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(escenaLobby);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        enCurso = false;
        Destroy(gameObject);
    }

    /*========================================================================================*/

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        enCurso = false;
    }
}