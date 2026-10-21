// Muestra en el HUD el plazo actual de la deuda y lo que falta, cambiando de color cuando ya se puede pagar.
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

    // Busca su texto y se suscribe a los cambios de dinero y de capítulo
    void OnEnable()
    {
        if (textoPlazo == null) textoPlazo = GetComponent<TextMeshProUGUI>();

        Dinero.AlCambiar += AlCambiarDinero;
        GestorPlazos.AlCambiarCapitulo += AlCambiarCapitulo;
        Actualizar();
    }

    // Deja de escuchar los cambios
    void OnDisable()
    {
        Dinero.AlCambiar -= AlCambiarDinero;
        GestorPlazos.AlCambiarCapitulo -= AlCambiarCapitulo;
    }

    // Vuelve a escribir el texto cuando el gestor ya está listo
    void Start()
    {
        Actualizar();
    }

    // Refresca el texto cuando cambia el dinero
    private void AlCambiarDinero(int total)
    {
        Actualizar();
    }

    // Refresca el texto cuando cambia el capítulo
    private void AlCambiarCapitulo(int capitulo)
    {
        Actualizar();
    }

    // Escribe el plazo actual, su importe y lo que falta, con el color que corresponda
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

        int falta = importe - Dinero.Total;
        if (falta > 0)
        {
            textoPlazo.text = "Plazo " + gestor.capituloActual + ": " + importe + "$ (faltan " + falta + "$)";
            textoPlazo.color = colorNormal;
        }
        else
        {
            textoPlazo.text = "Plazo " + gestor.capituloActual + ": " + importe + "$ (¡ya puedes pagar!)";
            textoPlazo.color = colorPuedePagar;
        }
    }
}
