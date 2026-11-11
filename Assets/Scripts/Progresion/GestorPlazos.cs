using System;
using System.Collections.Generic;
using UnityEngine;

public class GestorPlazos : MonoBehaviour
{
    public static GestorPlazos Instancia { get; private set; }

    public static event Action<int> AlCambiarCapitulo;
    public static event Action AlLlegarCobroFinal;
    public static event Action AlCambiarAportado;

    [Header("Progreso")]
    public int capituloActual = 1;
    public int aportadoActual = 0;

    [Header("Importes")]
    public List<int> importesPlazos = new List<int> { 100, 400, 1000, 2500 };
    public int importeCobroFinal = 100000;

    private GestorPlazosRed red;

    /*========================================================================================*/

    void Awake()
    {
        if (Instancia != null && Instancia != this)
            Debug.LogWarning("Hay más de un GestorPlazos en la escena; se usará el último.");

        Instancia = this;
        capituloActual = Mathf.Max(1, capituloActual);
        red = GetComponent<GestorPlazosRed>();
    }

    /*========================================================================================*/

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    /*========================================================================================*/

    public static int CapituloEnCurso()
    {
        return Instancia != null ? Instancia.capituloActual : 1;
    }

    /*========================================================================================*/

    public int CantidadPlazos()
    {
        return importesPlazos.Count;
    }

    /*========================================================================================*/


    public bool PlazosPagados()
    {
        return capituloActual > importesPlazos.Count;
    }

    /*========================================================================================*/


    public int ObtenerImportePlazoActual()
    {
        if (PlazosPagados()) return importeCobroFinal;
        return importesPlazos[capituloActual - 1];
    }

    /*========================================================================================*/

    public int FaltaPlazoActual()
    {
        if (PlazosPagados()) return importeCobroFinal;
        return Mathf.Max(0, ObtenerImportePlazoActual() - aportadoActual);
    }

    /*========================================================================================*/

    public bool PuedeAportar()
    {
        return !PlazosPagados() && Dinero.Total > 0;
    }

    /*========================================================================================*/

    public int Aportar()
    {
        if (!PuedeAportar()) return 0;

        int cantidad = Mathf.Min(Dinero.Total, FaltaPlazoActual());
        if (cantidad <= 0 || !Dinero.Gastar(cantidad)) return 0;

        if (red != null && red.IsSpawned) red.EnviarAportacion(cantidad);
        else RegistrarAportacion(cantidad);

        return cantidad;
    }

    /*========================================================================================*/

    public int RegistrarAportacion(int cantidad)
    {
        if (cantidad <= 0 || PlazosPagados()) return Mathf.Max(0, cantidad);

        int aceptada = Mathf.Min(cantidad, FaltaPlazoActual());
        aportadoActual += aceptada;

        if (aportadoActual >= ObtenerImportePlazoActual())
        {
            Debug.Log("Plazo de " + ObtenerImportePlazoActual() + "$ completado. Capítulo actual: " + (capituloActual + 1));
            aportadoActual = 0;
            CambiarCapitulo(capituloActual + 1);
        }

        AlCambiarAportado?.Invoke();
        return cantidad - aceptada;
    }

    /*========================================================================================*/

    public void AplicarEstadoRed(int capitulo, int aportado)
    {
        aportadoActual = aportado;
        if (capitulo != capituloActual) CambiarCapitulo(capitulo);
        AlCambiarAportado?.Invoke();
    }

    /*========================================================================================*/

    private void CambiarCapitulo(int nuevoCapitulo)
    {
        capituloActual = nuevoCapitulo;
        AlCambiarCapitulo?.Invoke(capituloActual);

        if (PlazosPagados()) LlegarCobroFinal();
    }

    /*========================================================================================*/

    private void LlegarCobroFinal()
    {
        Debug.Log("Todos los plazos pagados. Llega el cobro final de " + importeCobroFinal + "$.");
        AlLlegarCobroFinal?.Invoke();
    }

    /*========================================================================================*/

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        Instancia = null;
        AlCambiarCapitulo = null;
        AlLlegarCobroFinal = null;
        AlCambiarAportado = null;
    }
}
