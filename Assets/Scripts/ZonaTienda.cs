using UnityEngine;

//Va en un objeto HIJO de la Tienda que tiene un Collider con "Is Trigger" más grande que el cubo.
//Solo avisa a la Tienda de cuándo entra y sale el jugador de la zona.
//(Está en un objeto aparte porque los avisos de trigger solo le llegan al objeto que tiene ese collider,
//y así además puede estar en la capa "Ignore Raycast" y no tapar el rayo con el que agarramos objetos)
[RequireComponent(typeof(Collider))]
public class ZonaTienda : MonoBehaviour
{
    public Tienda tienda; //Si está vacío, se busca en el padre

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        if (tienda == null) tienda = GetComponentInParent<Tienda>();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void OnTriggerEnter(Collider other)
    {
        //Solo nos interesa el jugador (el que tiene el script de agarrar)
        if (other.GetComponentInParent<InteracciónAgarrar>() != null) tienda.JugadorEntraEnZona();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<InteracciónAgarrar>() != null) tienda.JugadorSaleDeZona();
    }
}
