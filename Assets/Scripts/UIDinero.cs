using TMPro;
using UnityEngine;

//Muestra el dinero total en un texto de TextMeshPro y lo actualiza cuando cambia
public class UIDinero : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI textoDinero;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void OnEnable()
    {
        //Nos apuntamos al aviso de "el dinero ha cambiado"
        Dinero.AlCambiar += ActualizarTexto;
        ActualizarTexto(Dinero.Total);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void OnDisable()
    {
        //Nos borramos del aviso para no recibirlo si este objeto se desactiva o destruye
        Dinero.AlCambiar -= ActualizarTexto;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void ActualizarTexto(int total)
    {
        if (textoDinero != null) textoDinero.text = "Dinero: " + total;
    }
}
