using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ZonaJugador : MonoBehaviour
{
    public UnityEvent alEntrarJugador;
    public UnityEvent alSalirJugador;

    /*========================================================================================*/

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    /*========================================================================================*/

    private void OnTriggerEnter(Collider other)
    {
        if (EsJugadorLocal(other)) alEntrarJugador.Invoke();
    }

    /*========================================================================================*/

    private void OnTriggerExit(Collider other)
    {
        if (EsJugadorLocal(other)) alSalirJugador.Invoke();
    }

    /*========================================================================================*/

    private bool EsJugadorLocal(Collider other)
    {
        InteracciónAgarrar agarrar = other.GetComponentInParent<InteracciónAgarrar>();
        return agarrar != null && JugadorLocal.EsLocal(agarrar);
    }
}