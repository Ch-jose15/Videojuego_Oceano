// Marca el lugar donde reaparece el jugador al morir; se dibuja en la vista Scene para poder moverlo fácilmente.
using UnityEngine;

public class PuntoReaparicion : MonoBehaviour
{
    [Header("Gizmo")]
    public Color colorGizmo = new Color(0.2f, 1f, 0.6f);
    public float radioGizmo = 0.6f;

    // Dibuja una esfera, una línea hasta el suelo aproximado y una flecha hacia donde mirará el jugador
    void OnDrawGizmos()
    {
        Gizmos.color = colorGizmo;
        Gizmos.DrawWireSphere(transform.position, radioGizmo);
        Gizmos.DrawSphere(transform.position, radioGizmo * 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 2f);

        Vector3 punta = transform.position + transform.forward * radioGizmo * 3f;
        Gizmos.DrawLine(transform.position, punta);
        Gizmos.DrawLine(punta, punta - transform.forward * 0.4f + transform.right * 0.3f);
        Gizmos.DrawLine(punta, punta - transform.forward * 0.4f - transform.right * 0.3f);
    }
}
