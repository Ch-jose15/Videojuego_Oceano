using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
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
    [Range(0, 100)] public int porcentajeDineroPerdido = 50;
    public bool pierdeNivelMejoras = true;
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

    /*----------------------------------------------------------------------------*/

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (vida == null) vida = GetComponent<VidaJugador>();
        if (stamina == null) stamina = GetComponent<StaminaJugador>();
        if (agarrar == null) agarrar = GetComponent<InteracciónAgarrar>();
        if (inventario == null) inventario = GetComponent<InventarioJugador>();
        if (disparo == null) disparo = GetComponent<DisparoJugador>();

        if (vida != null) vida.AlQuedarseSinVida += Morir;
    }

    /*----------------------------------------------------------------------------*/

    void OnDestroy()
    {
        if (vida != null) vida.AlQuedarseSinVida -= Morir;
    }

    /*----------------------------------------------------------------------------*/

    void Update()
    {
        if (!Application.isEditor || teclaMorir == KeyCode.None || vida == null) return;

        if (Input.GetKeyDown(teclaMorir) && !EstadoJuego.ControlesBloqueados) 
            vida.RecibirDaño(vida.vidaMaxima * 10f);
    }

    /*----------------------------------------------------------------------------*/

    private void Morir()
    {
        if (muriendo) return;

        StartCoroutine(SecuenciaMuerte());
    }

    /*----------------------------------------------------------------------------*/

    private void BuscarReferenciasEscena()
    {
        if (cuboCompra == null) cuboCompra = FindFirstObjectByType<CuboCompra>();
        if (puntoReaparicion == null) puntoReaparicion = FindFirstObjectByType<PuntoReaparicion>();
        if (pantallaMuerte == null) pantallaMuerte = FindFirstObjectByType<PantallaMuerte>();
    }

    /*----------------------------------------------------------------------------*/

    private IEnumerator SecuenciaMuerte()
    {
        muriendo = true;
        BuscarReferenciasEscena();
        if (vida != null) vida.invulnerable = true;

        if (inventario != null && inventario.uiInventario != null) 
            inventario.uiInventario.Cerrar();
        AlMorirJugador?.Invoke();
        EstadoJuego.CambiarJugadorMuerto(true);

        if (pantallaMuerte != null) 
            yield return pantallaMuerte.FundirANegro(duracionFundido);

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

    /*----------------------------------------------------------------------------*/

    private string AplicarPenalizacion()
    {
        List<string> perdido = new List<string>();

        int dineroPerdido = Mathf.FloorToInt(Dinero.Total * porcentajeDineroPerdido / 100f);
        if (dineroPerdido > 0 && Dinero.Gastar(dineroPerdido)) 
            perdido.Add(dineroPerdido + "$");

        if (pierdeNivelMejoras)
        {
            float bajadaVida = cuboCompra != null ? cuboCompra.cantidadMejoraVida : 25f;
            float bajadaStamina = cuboCompra != null ? cuboCompra.cantidadMejoraStamina : 30f;
            if (vida != null && vida.ReducirVidaMaxima(bajadaVida))
                perdido.Add("una mejora de vida");
            if (stamina != null && stamina.ReducirStaminaMaxima(bajadaStamina))
                perdido.Add("una mejora de stamina");
        }

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

    /*----------------------------------------------------------------------------*/

    private bool QuitarLinterna()
    {
        Linterna linterna = GetComponentInChildren<Linterna>(true);
        if (linterna == null) return false;

        linterna.transform.SetParent(null);
        Destroy(linterna.gameObject);

        if (cuboCompra != null) cuboCompra.VolverAVenderLinterna();
        return true;
    }

    /*----------------------------------------------------------------------------*/

    private int QuitarObjetos()
    {
        int cantidad = 0;

        if (agarrar != null)
        {
            GameObject enMano = agarrar.QuitarObjetoAgarrado();
            if (enMano != null)
            {
                ItemRed itemRed = enMano.GetComponent<ItemRed>();
                if (itemRed != null && itemRed.IsSpawned)
                    itemRed.PedirDestruirRpc();
                else Destroy(enMano);
                cantidad++;
            }
        }

        if (inventario != null) cantidad += inventario.VaciarYDestruir();
        return cantidad;
    }

    /*----------------------------------------------------------------------------*/

    private void Reaparecer()
    {
        if (puntoReaparicion == null)
            puntoReaparicion = FindFirstObjectByType<PuntoReaparicion>();

        if (puntoReaparicion != null)
        {
            Vector3 posicion = puntoReaparicion.transform.position;
            Quaternion giro = Quaternion.Euler(0f, puntoReaparicion.transform.eulerAngles.y, 0f);

            PropietarioRed propietario = GetComponent<PropietarioRed>();
            if (propietario != null && propietario.IsSpawned)
                posicion += Vector3.right * propietario.separacionEntreJugadores * propietario.OwnerClientId;

            NetworkTransform transformRed = GetComponent<NetworkTransform>();
            if (transformRed != null && transformRed.IsSpawned)
                transformRed.Teleport(posicion, giro, transform.localScale);
            else transform.SetPositionAndRotation(posicion, giro);
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

    /*----------------------------------------------------------------------------*/

    private string TextoPerdidas(List<string> perdido)
    {
        if (perdido.Count == 0) return "No has perdido nada";
        if (perdido.Count == 1) return "Has perdido " + perdido[0];

        string lista = string.Join(", ", perdido.GetRange(0, perdido.Count - 1));
        return "Has perdido " + lista + " y " + perdido[perdido.Count - 1];
    }

    /*----------------------------------------------------------------------------*/

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        AlMorirJugador = null;
    }
}