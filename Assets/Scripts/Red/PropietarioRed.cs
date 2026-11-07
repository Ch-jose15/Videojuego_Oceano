using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PropietarioRed : NetworkBehaviour
{
    [Header("Solo funciona en tu propio jugador")]
    public Camera camara;
    public AudioListener oido;
    public Behaviour[] scriptSoloDueño;

    [Header("Cuerpo")]
    public GameObject cuerpo;

    [Header("Aparación")]
    public float separacionEntreJugadores = 2f;

    /*========================================================================================*/

    public override void OnNetworkSpawn()
    {
        if (camara == null) camara = GetComponentInChildren<Camera>(true);
        if (oido == null) oido = GetComponentInChildren<AudioListener>(true);

        if (!IsOwner)
        {
            if (camara != null) camara.enabled = false;
            if (oido != null) oido.enabled = false;
            foreach (Behaviour script in scriptSoloDueño) 
                if (script != null) script.enabled = false;
            return;
        }

        OcultarCuerpoPropio();
        ColocarEnReaparicion();

        SceneManager.sceneLoaded += AlCargarEscena;
    }

    /*========================================================================================*/

    private void OcultarCuerpoPropio()
    {
        if (cuerpo == null) return;
        foreach (Renderer r in cuerpo.GetComponentsInChildren<Renderer>())
            r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
    }

    /*========================================================================================*/

    private void ColocarEnReaparicion()
    {
        PuntoReaparicion punto = FindAnyObjectByType<PuntoReaparicion>();
        if (punto == null) return;

        Vector3 posicion = punto.transform.position + Vector3.right * separacionEntreJugadores * OwnerClientId;
        transform.position = posicion;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = posicion;
            rb.linearVelocity = Vector3.zero;
        }
    }

    /*========================================================================================*/

    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        ColocarEnReaparicion();
    }

    /*========================================================================================*/

    public override void OnNetworkDespawn()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
    }
}
