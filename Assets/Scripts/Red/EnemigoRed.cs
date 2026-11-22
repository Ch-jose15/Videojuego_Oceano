// Sincroniza un enemigo en red: el host lo mueve y le resta vida, y su muerte se ve en todas las pantallas.
using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(EnemigoSimple))]
public class EnemigoRed : NetworkBehaviour
{
    private readonly NetworkVariable<bool> muerto = new NetworkVariable<bool>(false);

    private EnemigoSimple enemigo;

    // Guarda el script del enemigo del mismo objeto
    void Awake()
    {
        enemigo = GetComponent<EnemigoSimple>();
    }

    // Al aparecer en red escucha su muerte, y si ya estaba muerto lo muestra así
    public override void OnNetworkSpawn()
    {
        muerto.OnValueChanged += AlCambiarMuerto;
        if (muerto.Value) enemigo.MostrarMuerte();
    }

    // Al retirarse de la red deja de escuchar y lo oculta en todas las pantallas
    public override void OnNetworkDespawn()
    {
        muerto.OnValueChanged -= AlCambiarMuerto;

        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
    }

    // Cuando el host lo marca como muerto, los clientes también lo ven morir
    private void AlCambiarMuerto(bool anterior, bool actual)
    {
        if (actual) enemigo.MostrarMuerte();
    }

    // Un cliente le ha dado con un proyectil: el host le resta la vida
    [Rpc(SendTo.Server)]
    public void PedirDañoRpc(float cantidad)
    {
        enemigo.RecibirDaño(cantidad);
    }

    // El host marca la muerte y lo retira de la red tras un instante
    public void MarcarMuerto(float espera)
    {
        if (!IsServer) return;

        muerto.Value = true;
        StartCoroutine(RetirarTras(espera));
    }

    // Espera y retira el enemigo de la red (los colocados en la escena solo se desactivan)
    private IEnumerator RetirarTras(float espera)
    {
        yield return new WaitForSeconds(espera);
        if (IsSpawned) NetworkObject.Despawn(!NetworkObject.InScenePlaced);
    }
}