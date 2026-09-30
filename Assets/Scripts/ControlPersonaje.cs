using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnderwaterSwimController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform camara;

    [Header("Movimiento")]
    public float velocidadNado = 10f; //Aceleración. Velocidad máxima real ≈ velocidadNado / dragAgua (10 / 2.5 = 4 m/s)
    public float dragAgua = 2.5f;
    public float dragAngular = 3f;

    [Header("Boyantez")]
    public float fuerzaBoyantez = 9.8f;
    public bool boyantezNeutral = true;

    [Header("Mirar (Mouse Look)")]
    public float sensibilidadRaton = 2.5f;
    public float limiteVerticalMin = -85f;
    public float limiteVerticalMax = 85f;

    [Header("Dash")]
    public KeyCode teclaDash = KeyCode.LeftShift;
    public float fuerzaDash = 20f; //Impulso instantáneo en m/s (unas 5 veces la velocidad normal)
    public float duracionDash = 0.25f;
    public float cooldownDash = 1.2f;
    public float dashFovExtra = 12f;
    public float dashFovVelocidad = 8f;

    [Header("Stamina")]
    public StaminaJugador stamina;
    public float costeStaminaDash = 30f; //staminaMaxima / 3 (con 90 de máximo = 3 dashes seguidos)

    private Rigidbody rb;
    private float rotacionVertical = 0f;
    private float fovBase;
    private bool puedeDashear = true;
    private bool dasheando = false;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true; // la boyantez cancela esta gravedad para flotar neutro
        rb.constraints = RigidbodyConstraints.FreezeRotation; // evita que la cápsula ruede/vuelque y arrastre la cámara
        rb.linearDamping = dragAgua;
        rb.angularDamping = dragAngular;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (camara != null) fovBase = camara.GetComponent<Camera>().fieldOfView;

        //Si no se ha asignado en el Inspector, la buscamos en el propio jugador
        if (stamina == null) stamina = GetComponent<StaminaJugador>();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        //Con un menú abierto no se mira ni se dashea
        if (EstadoJuego.ControlesBloqueados) return;

        ManejarMirarCamara();

        //La stamina se regenera siempre que no estemos en mitad de un dash
        if (stamina != null && !dasheando) stamina.Regenerar(Time.deltaTime);

        if (Input.GetKeyDown(teclaDash) && puedeDashear && !dasheando && TieneStaminaParaDash()) StartCoroutine(HacerDash());
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private bool TieneStaminaParaDash()
    {
        if (stamina == null) return true;
        return stamina.TieneSuficiente(costeStaminaDash);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void FixedUpdate()
    {
        ManejarMovimiento();
        AplicarBoyantez();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void ManejarMirarCamara()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton;

        transform.Rotate(Vector3.up * mouseX);

        rotacionVertical -= mouseY;
        rotacionVertical = Mathf.Clamp(rotacionVertical, limiteVerticalMin, limiteVerticalMax);
        if (camara != null) camara.localRotation = Quaternion.Euler(rotacionVertical, 0f, 0f);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void ManejarMovimiento()
    {
        if (dasheando || camara == null) return;

        float ejeH = Input.GetAxis("Horizontal");
        float ejeV = Input.GetAxis("Vertical");
        float ejeVertical = 0f;
        if (Input.GetKey(KeyCode.Space)) ejeVertical += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) ejeVertical -= 1f;

        Vector3 direccion = camara.forward * ejeV + camara.right * ejeH + Vector3.up * ejeVertical;
        if (direccion.sqrMagnitude > 1f) direccion.Normalize();

        rb.AddForce(direccion * velocidadNado, ForceMode.Acceleration);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void AplicarBoyantez()
    {
        if (boyantezNeutral) rb.AddForce(Vector3.up * (Physics.gravity.magnitude * rb.mass), ForceMode.Force);
        else rb.AddForce(Vector3.up * fuerzaBoyantez, ForceMode.Acceleration);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private IEnumerator HacerDash()
    {
        puedeDashear = false;
        dasheando = true;

        //Gastamos la stamina del dash (ya se comprobó en Update que había suficiente)
        if (stamina != null) stamina.Consumir(costeStaminaDash);

        Vector3 direccionDash = camara.forward;
        rb.AddForce(direccionDash * fuerzaDash, ForceMode.VelocityChange);

        if (camara != null)
        {
            Camera cam = camara.GetComponent<Camera>();
            float tiempo = 0f;
            float fovObjetivo = fovBase + dashFovExtra;

            while (tiempo < duracionDash)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovObjetivo, Time.deltaTime * dashFovVelocidad);
                tiempo += Time.deltaTime;
                yield return null;
            }

            dasheando = false;

            while (Mathf.Abs(cam.fieldOfView - fovBase) > 0.1f)
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fovBase, Time.deltaTime * dashFovVelocidad);
                yield return null;
            }
            cam.fieldOfView = fovBase;
        }
        else dasheando = false;
        
        yield return new WaitForSeconds(cooldownDash);
        puedeDashear = true;
    }
}