using Unity.Netcode;
using UnityEngine;

public class ItemRed : NetworkBehaviour
{
    public const ulong Nadie = ulong.MaxValue;

    private readonly NetworkVariable<ulong> portador = new NetworkVariable<ulong>(Nadie);
    private readonly NetworkVariable<bool> guardado = new NetworkVariable<bool>(false);

    private Rigidbody rb;
    private Collider colisionador;
    private Renderer[] renderes;
    private Transform manoPortador;
    private bool enMiMano;

    /*========================================================================================*/

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colisionador = GetComponent<Collider>();
        renderes = GetComponentsInChildren<Renderer>(true);
    }

    /*========================================================================================*/

    public override void OnNetworkSpawn()
    {
        portador.OnValueChanged += AlCambiarPortador;
        guardado.OnValueChanged += AlCambiarGuardado;
        Refrescar();
        if (IsServer) NetworkManager.OnClientDisconnectCallback += AlDesconectarseCliente;
    }

    /*========================================================================================*/

    public override void OnNetworkDespawn()
    {
        portador.OnValueChanged -= AlCambiarPortador;
        guardado.OnValueChanged -= AlCambiarGuardado;
        if (IsServer && NetworkManager != null) NetworkManager.OnClientDisconnectCallback -= AlDesconectarseCliente;

        enMiMano = false;
        manoPortador = null;
        if (colisionador != null) colisionador.enabled = false;
        foreach (Renderer r in renderes)
            if (r != null) r.enabled = false;
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

    [Rpc(SendTo.Server)]
    public void PedirGuardarRpc(RpcParams parametros = default)
    {
        if (portador.Value != parametros.Receive.SenderClientId) return;
        guardado.Value = true;
    }

    /*========================================================================================*/

    [Rpc(SendTo.Server)]
    public void PedirSacarRpc(RpcParams parametros = default)
    {
        if (portador.Value != parametros.Receive.SenderClientId || !guardado.Value) return;
        guardado.Value = false;
    }

    /*========================================================================================*/

    [Rpc(SendTo.Server)]
    public void PedirDestruirRpc(RpcParams parametros = default)
    {
        if (portador.Value != parametros.Receive.SenderClientId) return;
        bool colocadoEnEscena = NetworkObject.InScenePlaced == true;
        NetworkObject.Despawn(!colocadoEnEscena);

    }

    /*========================================================================================*/

    private void AlCambiarPortador(ulong anterior, ulong actual)
    {
        Refrescar();
    }

    /*========================================================================================*/

    private void AlCambiarGuardado(bool anterior, bool actual)
    {
        Refrescar();
    }

    /*========================================================================================*/

    private void Refrescar()
    {
        if (colisionador != null)
            colisionador.enabled = portador.Value == Nadie && !guardado.Value;

        foreach (Renderer r in renderes)
            if (r != null) r.enabled = !guardado.Value;
    }

    /*========================================================================================*/

    private void Update()
    {
        if (!IsSpawned) return;
        bool esMio = IsOwner && portador.Value == NetworkManager.LocalClientId && !guardado.Value;
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
        if (rb != null && IsOwner && !guardado.Value)
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
        if (portador.Value != idcliente) return;
        guardado.Value = false;
        portador.Value = Nadie;
    }
}
