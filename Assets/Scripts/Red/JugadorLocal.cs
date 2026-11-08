using Unity.Netcode;
using UnityEngine;

public static class JugadorLocal
{
    public static bool EnRed()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
    }

    /*========================================================================================*/

    public static InteracciónAgarrar Agarrar()
    {
        if (EnRed())
        {
            NetworkObject jugador = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            return jugador != null ? jugador.GetComponentInChildren<InteracciónAgarrar>() : null;
        }

        return Object.FindFirstObjectByType<InteracciónAgarrar>();
    }

    /*========================================================================================*/

    public static bool EsLocal(Component componente)
    {
        if (componente == null) return false;
        NetworkObject objetoRed = componente.GetComponentInParent<NetworkObject>();
        return objetoRed == null || !objetoRed.IsSpawned || objetoRed.IsOwner;
    }
}
