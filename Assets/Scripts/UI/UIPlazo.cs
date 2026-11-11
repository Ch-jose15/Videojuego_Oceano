using TMPro;
using UnityEngine;

public class UIPlazo : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI textoPlazo;

    [Header("Colores")]
    public Color colorNormal = new Color(0.85f, 0.85f, 0.85f);
    public Color colorPuedePagar = new Color(0.3f, 1f, 0.4f);
    public Color colorCobroFinal = new Color(1f, 0.35f, 0.3f);

    /*========================================================================================*/

    void OnEnable()
    {
        if (textoPlazo == null) textoPlazo = GetComponent<TextMeshProUGUI>();

        Dinero.AlCambiar += AlCambiarDinero;
        GestorPlazos.AlCambiarCapitulo += AlCambiarCapitulo;
        GestorPlazos.AlCambiarAportado += Actualizar;
        Actualizar();
    }

    /*========================================================================================*/

    void OnDisable()
    {
        Dinero.AlCambiar -= AlCambiarDinero;
        GestorPlazos.AlCambiarCapitulo -= AlCambiarCapitulo;
        GestorPlazos.AlCambiarAportado -= Actualizar;
    }

    /*========================================================================================*/

    void Start()
    {
        Actualizar();
    }

    /*========================================================================================*/

    private void AlCambiarDinero(int total)
    {
        Actualizar();
    }

    /*========================================================================================*/

    private void AlCambiarCapitulo(int capitulo)
    {
        Actualizar();
    }

    /*========================================================================================*/

    private void Actualizar()
    {
        if (textoPlazo == null) return;

        GestorPlazos gestor = GestorPlazos.Instancia;
        if (gestor == null)
        {
            textoPlazo.text = "";
            return;
        }

        int importe = gestor.ObtenerImportePlazoActual();

        if (gestor.PlazosPagados())
        {
            textoPlazo.text = "Cobro final: " + importe + "$";
            textoPlazo.color = colorCobroFinal;
            return;
        }

        int falta = gestor.FaltaPlazoActual();
        textoPlazo.text = "Plazo " + gestor.capituloActual + ": " + gestor.aportadoActual + "/" + importe + "$ (faltan " + falta + "$)";
        textoPlazo.color = Dinero.Total >= falta ? colorPuedePagar : colorNormal;
    }
}