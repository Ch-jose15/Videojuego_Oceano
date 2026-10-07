// Controla al jugador bajo el agua: nadar, mirar con el ratón, hundirse despacio y hacer dash gastando stamina.
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnderwaterSwimController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform camara;

    [Header("Movimiento")]
    public float velocidadNado = 11f;
    public float dragAgua = 2.5f;
    public float dragAngular = 3f;

    [Header("Flotabilidad")]
    public float velocidadHundimiento = 1.5f;

    [Header("Mirar (Mouse Look)")]
    public float sensibilidadRaton = 2.5f;
    public float limiteVerticalMin = -85f;
    public float limiteVerticalMax = 85f;

    [Header("Dash")]
    public KeyCode teclaDash = KeyCode.LeftShift;
    public float fuerzaDash = 20f;
    public float duracionDash = 0.25f;
    public float cooldownDash = 1.2f;
    public float dashFovExtra = 12f;
    public float dashFovVelocidad = 8f;

    [Header("Stamina")]
    public StaminaJugador stamina;
    public float costeStaminaDash = 30f;

    private Rigidbody rb;
    private float rotacionVertical = 0f;
    private float fovBase;
    private bool puedeDashear = true;
    private bool dasheando = false;

    // Configura el Rigidbody, bloquea el cursor y guarda las referencias iniciales
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = dragAgua;
        rb.angularDamping = dragAngular;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (camara != null) fovBase = camara.GetComponent<Camera>().fieldOfView;

        if (stamina == null) stamina = GetComponent<StaminaJugador>();
    }

    // Gestiona la cámara, la regeneración de stamina y la tecla de dash cada frame
    void Update()
    {
        if (EstadoJuego.ControlesBloqueados) return;

        if (!EstadoJuego.RatonLibre) ManejarMirarCamara();

        if (stamina != null && !dasheando) stamina.Regenerar(Time.deltaTime);

        if (Input.GetKeyDown(teclaDash) && puedeDashear && !dasheando && TieneStaminaParaDash()) StartCoroutine(HacerDash());
    }

    // Indica si hay stamina suficiente para hacer un dash
    private bool TieneStaminaParaDash()
    {
        if (stamina == null) return true;
        return stamina.TieneSuficiente(costeStaminaDash);
    }

    // Aplica el movimiento de nado y la flotabilidad en cada paso de física
    void FixedUpdate()
    {
        ManejarMovimiento();
        AplicarFlotabilidad();
    }

    // Gira al jugador y la cámara según el movimiento del ratón
    private void ManejarMirarCamara()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadRaton;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadRaton;

        transform.Rotate(Vector3.up * mouseX);

        rotacionVertical -= mouseY;
        rotacionVertical = Mathf.Clamp(rotacionVertical, limiteVerticalMin, limiteVerticalMax);
        if (camara != null) camara.localRotation = Quaternion.Euler(rotacionVertical, 0f, 0f);
    }

    // Empuja al jugador en la dirección de las teclas pulsadas
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

    // Contrarresta casi toda la gravedad para que el jugador se hunda a velocidadHundimiento
    private void AplicarFlotabilidad()
    {
        float aceleracionHundimiento = velocidadHundimiento * dragAgua;
        float empujeAgua = Physics.gravity.magnitude - aceleracionHundimiento;
        rb.AddForce(Vector3.up * empujeAgua, ForceMode.Acceleration);
    }

    // Hace el dash: gasta stamina, impulsa al jugador, anima el FOV y espera el cooldown
    private IEnumerator HacerDash()
    {
        puedeDashear = false;
        dasheando = true;

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
