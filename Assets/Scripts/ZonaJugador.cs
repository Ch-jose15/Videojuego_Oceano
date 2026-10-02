using UnityEngine;
using UnityEngine.Events;

//Zona que detecta cuándo entra y sale el jugador. Va en un objeto HIJO del cubo, con un Collider "Is Trigger"
//más grande que el cubo. Lo usan el CuboVenta y el CuboCompra.
//
//Al entrar/salir el jugador lanza los eventos alEntrarJugador / alSalirJugador.
//Son UnityEvents, como el OnClick de los botones: en el Inspector se elige a qué función llaman
//(por ejemplo CuboVenta.JugadorEntraEnZona).
//
//(Está en un objeto aparte porque los avisos de trigger solo le llegan al objeto que tiene ese collider,
//y así además puede estar en la capa "Ignore Raycast" y no tapar el rayo con el que agarramos objetos)
[RequireComponent(typeof(Collider))]
public class ZonaJugador : MonoBehaviour
{
    public UnityEvent alEntrarJugador;
    public UnityEvent alSalirJugador;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void OnTriggerEnter(Collider other)
    {
        //Solo nos interesa el jugador (el que tiene el script de agarrar)
        if (other.GetComponentInParent<InteracciónAgarrar>() != null) alEntrarJugador.Invoke();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<InteracciónAgarrar>() != null) alSalirJugador.Invoke();
    }
}
