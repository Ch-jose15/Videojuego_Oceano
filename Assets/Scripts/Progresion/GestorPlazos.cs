// Gestor de los plazos de la deuda con la mafia de las orcas: guarda el capítulo actual, cobra los plazos y avisa con eventos cuando cambia el capítulo.
using System;
using System.Collections.Generic;
using UnityEngine;

public class GestorPlazos : MonoBehaviour
{
    public static GestorPlazos Instancia { get; private set; }

    public static event Action<int> AlCambiarCapitulo;
    public static event Action AlLlegarCobroFinal;

    [Header("Progreso")]
    public int capituloActual = 1;

    [Header("Importes")]
    public List<int> importesPlazos = new List<int> { 100, 400, 1000, 2500 };
    public int importeCobroFinal = 100000;

    // Se registra como gestor de la escena
    void Awake()
    {
        if (Instancia != null && Instancia != this)
            Debug.LogWarning("Hay más de un GestorPlazos en la escena; se usará el último.");

        Instancia = this;
        capituloActual = Mathf.Max(1, capituloActual);
    }

    // Deja de ser el gestor activo al destruirse
    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    // Devuelve el capítulo actual, o 1 si no hay gestor en la escena
    public static int CapituloEnCurso()
    {
        return Instancia != null ? Instancia.capituloActual : 1;
    }

    // Número total de plazos que se pueden pagar
    public int CantidadPlazos()
    {
        return importesPlazos.Count;
    }

    // Indica si ya se pagaron todos los plazos
    public bool PlazosPagados()
    {
        return capituloActual > importesPlazos.Count;
    }

    // Devuelve lo que hay que pagar ahora: el plazo del capítulo actual o el cobro final
    public int ObtenerImportePlazoActual()
    {
        if (PlazosPagados()) return importeCobroFinal;
        return importesPlazos[capituloActual - 1];
    }

    // Indica si el jugador tiene dinero suficiente y aún quedan plazos por pagar
    public bool PuedePagar()
    {
        return !PlazosPagados() && Dinero.Total >= ObtenerImportePlazoActual();
    }

    // Cobra el plazo actual, sube de capítulo y avisa a los demás sistemas; devuelve false si no se pudo pagar
    public bool PagarPlazo()
    {
        if (!PuedePagar()) return false;

        int importe = ObtenerImportePlazoActual();
        if (!Dinero.Gastar(importe)) return false;

        capituloActual++;
        Debug.Log("Plazo de " + importe + "$ pagado. Capítulo actual: " + capituloActual);
        AlCambiarCapitulo?.Invoke(capituloActual);

        if (PlazosPagados()) LlegarCobroFinal();
        return true;
    }

    // Se llama al pagar el último plazo; de momento solo avisa por consola y lanza el evento para el final del juego
    private void LlegarCobroFinal()
    {
        Debug.Log("Todos los plazos pagados. Llega el cobro final de " + importeCobroFinal + "$.");
        AlLlegarCobroFinal?.Invoke();
    }

    // Limpia los eventos cada vez que se pulsa Play en el editor
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        Instancia = null;
        AlCambiarCapitulo = null;
        AlLlegarCobroFinal = null;
    }
}
