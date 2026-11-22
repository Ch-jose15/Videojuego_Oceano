// Combate del jugador en red: el daño de los enemigos llega al dueño del jugador y sus disparos se ven en la pantalla del compañero.
using Unity.Netcode;
using UnityEngine;

public class CombateRed : NetworkBehaviour
{
    private VidaJugador vida;
    private DisparoJugador disparo;

    // Guarda la vida y el disparo del jugador
    void Awake()
    {
        vida = GetComponentInChildren<VidaJugador>();
        disparo = GetComponentInChildren<DisparoJugador>();
    }

    // Resta vida a este jugador: si no es tuyo, se lo manda a su dueño
    public void AplicarDaño(float cantidad)
    {
        if (IsOwner) vida.RecibirDaño(cantidad);
        else RecibirDañoRpc(cantidad);
    }

    // El dueño del jugador recibe el daño en su ordenador
    [Rpc(SendTo.Owner)]
    private void RecibirDañoRpc(float cantidad)
    {
        if (vida != null) vida.RecibirDaño(cantidad);
    }

    // Avisa a los demás de que has disparado para que vean el proyectil
    public void AvisarDisparo(Vector3 posicion, Vector3 direccion)
    {
        MostrarDisparoRpc(posicion, direccion);
    }

    // En las otras pantallas aparece un proyectil sin daño (el daño ya lo cuenta quien dispara)
    [Rpc(SendTo.NotMe)]
    private void MostrarDisparoRpc(Vector3 posicion, Vector3 direccion)
    {
        if (disparo != null) disparo.DispararVisual(posicion, direccion);
    }
}