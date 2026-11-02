using Unity.Netcode;
using UnityEngine;

// Movimiento mínimo para probar la red: solo el dueño controla su cápsula
public class JugadorPrueba : NetworkBehaviour
{
    public float velocidad = 5f;
    public float sensibilidad = 2f;

    // Al aparecer, desactiva la cámara y el audio de los jugadores que no son tuyos
    public override void OnNetworkSpawn()
    {
        Camera cam = GetComponentInChildren<Camera>();
        AudioListener oido = GetComponentInChildren<AudioListener>();

        if (!IsOwner)
        {
            if (cam != null) cam.enabled = false;
            if (oido != null) oido.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        transform.position = new Vector3(Random.Range(-3f, 3f), 1f, 0f);
    }

    // Mueve y gira la cápsula solo si es tuya
    void Update()
    {
        if (!IsOwner) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        transform.Translate(new Vector3(h, 0f, v) * velocidad * Time.deltaTime);
        transform.Rotate(0f, Input.GetAxis("Mouse X") * sensibilidad, 0f);
    }
}