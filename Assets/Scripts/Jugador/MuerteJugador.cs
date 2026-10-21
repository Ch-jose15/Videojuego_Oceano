// Muerte del jugador: al quedarse sin vida bloquea los controles, funde a negro, aplica la penalización, reaparece en el PuntoReaparicion y devuelve el control.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuerteJugador : MonoBehaviour
{
    public static event Action AlMorirJugador;

    [Header("Referencias (se buscan solas si están vacías)")]
    public VidaJugador vida;
    public StaminaJugador stamina;
    public InteracciónAgarrar agarrar;
    public InventarioJugador inventario;
    public DisparoJugador disparo;
    public CuboCompra cuboCompra;
    public PuntoReaparicion puntoReaparicion;
    public PantallaMuerte pantallaMuerte;

    [Header("Penalización")]
    [Range(0, 100)] public int porcentajeDineroPerdido = 100;
    public bool pierdeLinterna = true;
    public bool pierdeObjetos = true;
    public bool pierdeDisparo = false;

    [Header("Fundido (en segundos)")]
    public float duracionFundido = 1.5f;
    public float tiempoEnNegro = 2f;

    [Header("Depuración (solo en el editor)")]
    public KeyCode teclaMorir = KeyCode.K;

    private Rigidbody rb;
    private bool muriendo = false;

    // Busca las referencias que falten y escucha cuándo se queda sin vida
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (vida == null) vida = GetComponent<VidaJugador>();
        if (stamina == null) stamina = GetComponent<StaminaJugador>();
        if (agarrar == null) agarrar = GetComponent<InteracciónAgarrar>();
        if (inventario == null) inventario = GetComponent<InventarioJugador>();
        if (disparo == null) disparo = GetComponent<DisparoJugador>();
        if (cuboCompra == null) cuboCompra = FindFirstObjectByType<CuboCompra>();
        if (puntoReaparicion == null) puntoReaparicion = FindFirstObjectByType<PuntoReaparicion>();
        if (pantallaMuerte == null) pantallaMuerte = FindFirstObjectByType<PantallaMuerte>();

        if (vida != null) vida.AlQuedarseSinVida += Morir;
        if (puntoReaparicion == null) Debug.LogWarning("MuerteJugador: no hay PuntoReaparicion en la escena; el jugador reaparecerá donde muera.");
    }

    // Deja de escuchar a la vida al destruirse
    void OnDestroy()
    {
        if (vida != null) vida.AlQuedarseSinVida -= Morir;
    }

    // En el editor, la tecla de depuración quita toda la vida para probar la muerte
    void Update()
    {
        if (!Application.isEditor || teclaMorir == KeyCode.None || vida == null) return;

        if (Input.GetKeyDown(teclaMorir) && !EstadoJuego.ControlesBloqueados) vida.RecibirDaño(vida.vidaMaxima * 10f);
    }

    // Empieza la muerte una sola vez aunque lleguen más golpes
    private void Morir()
    {
        if (muriendo) return;

        StartCoroutine(SecuenciaMuerte());
    }

    // Bloquea, funde a negro, aplica la penalización, reaparece, rellena vida y stamina y devuelve el control
    private IEnumerator SecuenciaMuerte()
    {
        muriendo = true;
        if (vida != null) vida.invulnerable = true;

        if (inventario != null && inventario.uiInventario != null) inventario.uiInventario.Cerrar();
        AlMorirJugador?.Invoke();
        EstadoJuego.CambiarJugadorMuerto(true);

        if (pantallaMuerte != null) yield return pantallaMuerte.FundirANegro(duracionFundido);

        string perdidas = AplicarPenalizacion();
        Reaparecer();

        if (pantallaMuerte != null)
        {
            pantallaMuerte.MostrarMensaje(perdidas);
            yield return new WaitForSecondsRealtime(tiempoEnNegro);
            yield return pantallaMuerte.FundirDeVuelta(duracionFundido);
        }

        EstadoJuego.CambiarJugadorMuerto(false);
        if (vida != null) vida.invulnerable = false;
        muriendo = false;
    }

    // Quita dinero, linterna, objetos y (si se elige) el disparo, y devuelve un texto con lo perdido
    private string AplicarPenalizacion()
    {
        List<string> perdido = new List<string>();

        int dineroPerdido = Mathf.FloorToInt(Dinero.Total * porcentajeDineroPerdido / 100f);
        if (dineroPerdido > 0 && Dinero.Gastar(dineroPerdido)) perdido.Add(dineroPerdido + "$");

        if (pierdeLinterna && QuitarLinterna()) perdido.Add("la linterna");

        if (pierdeObjetos)
        {
            int objetos = QuitarObjetos();
            if (objetos == 1) perdido.Add("1 objeto");
            else if (objetos > 1) perdido.Add(objetos + " objetos");
        }

        if (pierdeDisparo && disparo != null && disparo.disparoDesbloqueado)
        {
            disparo.Bloquear();
            if (cuboCompra != null) cuboCompra.VolverAVenderDisparo();
            perdido.Add("el disparo");
        }

        return TextoPerdidas(perdido);
    }

    // Destruye la linterna de la mano izquierda y la vuelve a poner a la venta; devuelve true si había linterna
    private bool QuitarLinterna()
    {
        Linterna linterna = GetComponentInChildren<Linterna>(true);
        if (linterna == null) return false;

        linterna.transform.SetParent(null);
        Destroy(linterna.gameObject);

        if (cuboCompra != null) cuboCompra.VolverAVenderLinterna();
        return true;
    }

    // Destruye el objeto de la mano derecha y todo el inventario; devuelve cuántos objetos se perdieron
    private int QuitarObjetos()
    {
        int cantidad = 0;

        if (agarrar != null)
        {
            GameObject enMano = agarrar.QuitarObjetoAgarrado();
            if (enMano != null)
            {
                Destroy(enMano);
                cantidad++;
            }
        }

        if (inventario != null) cantidad += inventario.VaciarYDestruir();
        return cantidad;
    }

    // Lleva al jugador al punto de reaparición parado y le rellena la vida y la stamina
    private void Reaparecer()
    {
        if (puntoReaparicion != null)
        {
            Vector3 posicion = puntoReaparicion.transform.position;
            Quaternion giro = Quaternion.Euler(0f, puntoReaparicion.transform.eulerAngles.y, 0f);

            transform.SetPositionAndRotation(posicion, giro);
            if (rb != null)
            {
                rb.position = posicion;
                rb.rotation = giro;
            }
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (vida != null) vida.RellenarVida();
        if (stamina != null) stamina.RellenarStamina();
    }

    // Construye el texto «Has perdido 120$, la linterna y 2 objetos» a partir de la lista de pérdidas
    private string TextoPerdidas(List<string> perdido)
    {
        if (perdido.Count == 0) return "No has perdido nada";
        if (perdido.Count == 1) return "Has perdido " + perdido[0];

        string lista = string.Join(", ", perdido.GetRange(0, perdido.Count - 1));
        return "Has perdido " + lista + " y " + perdido[perdido.Count - 1];
    }

    // Limpia el evento cada vez que se pulsa Play en el editor
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        AlMorirJugador = null;
    }
}
