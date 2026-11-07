using Unity.Netcode;
using UnityEngine;

public class ItemRed : NetworkBehaviour
{
    public const ulong Nadie = ulong.MaxValue;

    private readonly NetworkVariable<ulong> portador = new NetworkVariable<ulong>(Nadie);

    private Rigidbody rb;
    private Collider colisionador;
    private Transform manoPortador;
    private bool enMiMano;

    /*========================================================================================*/

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colisionador = GetComponent<Collider>();
    }

    /*========================================================================================*/

    public override void OnNetworkSpawn()
    {
        portador.OnValueChanged += AlCambiarPortador;
        AplicarCollider(portador.Value);
        if (IsServer) NetworkManager.OnClientDisconnectCallback += AlDesconectarseCliente;
    }

    /*========================================================================================*/

    public override void OnNetworkDespawn()
    {
        portador.OnValueChanged -= AlCambiarPortador;
        if (IsServer && NetworkManager != null) NetworkManager.OnClientDisconnectCallback -= AlDesconectarseCliente;
    }

    /*========================================================================================*/

    public bool EstaCogido()
    {
        return portador.Value != Nadie;
    }

    /*========================================================================================*/

    [Rpc(SendTo.Server)]
    public void PedirAgarrarRpc(RpcParams parametros = default)
    {
        if (portador.Value != Nadie) return;
        ulong solicitante = parametros.Receive.SenderClientId;
        NetworkObject.ChangeOwnership(solicitante);
        portador.Value = solicitante;
    }

    /*========================================================================================*/

    [Rpc(SendTo.Server)]
    public void PedirSoltarRpc(RpcParams parametros = default)
    {
        if (portador.Value != parametros.Receive.SenderClientId) return;
        portador.Value = Nadie;
    }

    /*========================================================================================*/

    private void AlCambiarPortador(ulong anterior, ulong actual)
    {
        AplicarCollider(actual);
    }

    /*========================================================================================*/

    private void AplicarCollider(ulong actual)
    {
        if (colisionador != null) colisionador.enabled = actual == Nadie;
    }

    /*========================================================================================*/

    private void Update()
    {
        if (!IsSpawned) return;
        bool esMio = IsOwner && portador.Value == NetworkManager.LocalClientId;
        if (esMio && !enMiMano) EntrarEnMiMano();
        else if (!esMio && enMiMano) SalirDeMiMano();
    }

    /*========================================================================================*/

    private void EntrarEnMiMano()
    {
        NetworkObject jugador = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (jugador == null)
        {
            Debug.LogWarning("ItemRed: todavia no hay jugador local.");
            return;
        }

        InteracciónAgarrar agarrar = jugador.GetComponentInChildren<InteracciónAgarrar>();
        if (agarrar == null || agarrar.puntoManoDerecha == null)
        {
            Debug.LogWarning("ItemRed: el jugador no tiene InteraccionAgarrar o le falta PuntoManoDerecha.");
            return;
        }

        enMiMano = true;
        manoPortador = agarrar.puntoManoDerecha;
        if (rb != null) rb.isKinematic = true;
        agarrar.ConfirmarAgarreRed(gameObject);
    }

    /*========================================================================================*/

    private void SalirDeMiMano()
    {
        enMiMano = false;
        manoPortador = null;
        if (rb != null && IsOwner)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }
    }

    /*========================================================================================*/

    private void LateUpdate()
    {
        if (!enMiMano || manoPortador == null) return;
        transform.SetPositionAndRotation(manoPortador.position, manoPortador.rotation);
    }

    /*========================================================================================*/

    private void AlDesconectarseCliente(ulong idcliente)
    {
        if (portador.Value == idcliente) portador.Value = Nadie;
    }

}
