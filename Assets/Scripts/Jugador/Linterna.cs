// Linterna de la mano izquierda: su Spot Light se enciende y apaga con una tecla, con intensidad, alcance y ángulo del haz ajustables.
using UnityEngine;

public class Linterna : MonoBehaviour
{
    [Header("Luz")]
    public Light luz;
    public float intensidad = 1000000f;
    public float alcance = 30f;
    [Range(1f, 179f)] public float anguloHaz = 50f;

    [Header("Control")]
    public KeyCode teclaEncender = KeyCode.L;
    public bool encendidaAlEmpezar = true;

    // Busca la luz si no está asignada, aplica los ajustes y la deja encendida o apagada
    void Awake()
    {
        if (luz == null) luz = GetComponentInChildren<Light>();
        AplicarAjustes();
        if (luz != null) luz.enabled = encendidaAlEmpezar;
    }

    // Enciende o apaga la luz al pulsar la tecla, salvo con un menú abierto
    void Update()
    {
        if (EstadoJuego.ControlesBloqueados) return;

        if (Input.GetKeyDown(teclaEncender)) Alternar();
    }

    // Cambia la luz entre encendida y apagada
    public void Alternar()
    {
        if (luz != null) luz.enabled = !luz.enabled;
    }

    // Indica si la luz está encendida
    public bool EstaEncendida()
    {
        return luz != null && luz.enabled;
    }

    // Copia la intensidad, el alcance y el ángulo del haz a la Spot Light
    public void AplicarAjustes()
    {
        if (luz == null) return;

        luz.intensity = intensidad;
        luz.range = alcance;
        luz.spotAngle = anguloHaz;
    }

    // Actualiza la luz en cuanto se cambia un valor en el Inspector
    void OnValidate()
    {
        AplicarAjustes();
    }
}
