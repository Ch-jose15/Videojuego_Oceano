using UnityEngine;

//Proyectil que dispara el jugador (lo crea DisparoJugador).
//Avanza en línea recta a velocidad fija, se destruye solo pasado un tiempo,
//y al chocar con algo desaparece. Si lo que golpea es un enemigo, le quita vida.
[RequireComponent(typeof(Rigidbody))]
public class Proyectil : MonoBehaviour
{
    private float daño;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Lo llama DisparoJugador justo después de crearlo
    public void Lanzar(Vector3 velocidad, float dañoAlImpactar, float tiempoVida)
    {
        daño = dañoAlImpactar;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false;      //Va recto, no cae
        rb.linearDamping = 0f;      //Sin frenado: velocidad fija
        rb.linearVelocity = velocidad;

        //Si no golpea nada, desaparece solo pasados esos segundos
        Destroy(gameObject, tiempoVida);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void OnCollisionEnter(Collision choque)
    {
        //¿Hemos golpeado a un enemigo? (buscamos el script también en el padre, por si el collider está en un hijo)
        EnemigoSimple enemigo = choque.collider.GetComponentInParent<EnemigoSimple>();
        if (enemigo != null) enemigo.RecibirDaño(daño);

        //Choque con lo que sea: el proyectil desaparece
        Destroy(gameObject);
    }
}
