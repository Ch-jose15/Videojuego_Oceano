// Proyectil del jugador: avanza recto, quita vida al enemigo que golpea y se destruye al chocar o pasado un tiempo.
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Proyectil : MonoBehaviour
{
    private float daño;

    // Le da velocidad, guarda su daño y programa su destrucción
    public void Lanzar(Vector3 velocidad, float dañoAlImpactar, float tiempoVida)
    {
        daño = dañoAlImpactar;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.linearVelocity = velocidad;

        Destroy(gameObject, tiempoVida);
    }

    // Daña al enemigo golpeado y destruye el proyectil al chocar con cualquier cosa
    private void OnCollisionEnter(Collision choque)
    {
        EnemigoSimple enemigo = choque.collider.GetComponentInParent<EnemigoSimple>();
        if (enemigo != null) enemigo.RecibirDaño(daño);

        Destroy(gameObject);
    }
}
