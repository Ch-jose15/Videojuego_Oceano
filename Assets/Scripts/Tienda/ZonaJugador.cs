// Trigger que detecta cuándo entra y sale el jugador y lo avisa mediante eventos configurables en el Inspector.
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ZonaJugador : MonoBehaviour
{
    public UnityEvent alEntrarJugador;
    public UnityEvent alSalirJugador;

    // Convierte el collider en trigger
    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    // Lanza el evento de entrada cuando entra el jugador
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<InteracciónAgarrar>() != null) alEntrarJugador.Invoke();
    }

    // Lanza el evento de salida cuando sale el jugador
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<InteracciónAgarrar>() != null) alSalirJugador.Invoke();
    }
}
