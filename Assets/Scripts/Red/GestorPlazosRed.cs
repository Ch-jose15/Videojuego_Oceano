using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(GestorPlazos))]
public class GestorPlazosRed : NetworkBehaviour
{
    private readonly NetworkVariable<int> capitulo = new NetworkVariable<int>(1);
    private readonly NetworkVariable<int> aportado = new NetworkVariable<int>(0);
    private GestorPlazos gestor;

    /*========================================================================================*/

    private void Awake()
    {
        gestor = GetComponent<GestorPlazos>();
    }

    /*========================================================================================*/

    public override void OnNetworkSpawn()
    {
        capitulo.OnValueChanged += AlCambiarValor;
        aportado.OnValueChanged += AlCambiarValor;

        if (IsServer) PublicarEstado();
        else gestor.AplicarEstadoRed(capitulo.Value, aportado.Value);
    }

    /*========================================================================================*/

    public override void OnNetworkDespawn()
    {
        capitulo.OnValueChanged -= AlCambiarValor;
        aportado.OnValueChanged -= AlCambiarValor;
    }

    /*========================================================================================*/

    private void AlCambiarValor(int anterior, int actual)
    {
        if (!IsServer) gestor.AplicarEstadoRed(capitulo.Value, aportado.Value);
    }

    /*========================================================================================*/

    public void EnviarAportacion(int cantidad)
    {
        AportarRpc(cantidad);
    }

    /*========================================================================================*/

    [Rpc(SendTo.Server)]
    private void AportarRpc(int cantidad, RpcParams parametros = default)
    {
        if (cantidad <= 0) return;

        int sobrante = gestor.RegistrarAportacion(cantidad);
        PublicarEstado();

        if (sobrante > 0)
            DevolverRpc(sobrante, RpcTarget.Single(parametros.Receive.SenderClientId, RpcTargetUse.Temp));
    }

    /*========================================================================================*/

    [Rpc(SendTo.SpecifiedInParams)]
    private void DevolverRpc(int cantidad, RpcParams parametros)
    {
        Dinero.Sumar(cantidad);
        Debug.Log("Plazo ya cubierto: se te devuelven " + cantidad + "$");
    }

    /*========================================================================================*/

    private void PublicarEstado()
    {
        capitulo.Value = gestor.capituloActual;
        aportado.Value = gestor.aportadoActual;
    }
}