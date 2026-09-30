using TMPro;
using UnityEngine;

//Muestra el dinero total en un texto de TextMeshPro. Cuando cambia, el número cuenta rápidamente
//desde el valor anterior hasta el nuevo (por ejemplo de 0 a 15 en medio segundo)
public class UIDinero : MonoBehaviour
{
    [Header("Referencias")]
    public TextMeshProUGUI textoDinero;

    [Header("Animación")]
    public float duracionConteo = 0.5f;

    private float valorMostrado = 0f;   //El número que se ve ahora mismo en pantalla
    private float valorInicioConteo = 0f;
    private int valorDestino = 0;
    private float tiempoConteo = 0f;
    private bool contando = false;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void OnEnable()
    {
        //Nos apuntamos al aviso de "el dinero ha cambiado"
        Dinero.AlCambiar += EmpezarConteo;

        //Al empezar mostramos el valor directamente, sin animación
        valorMostrado = Dinero.Total;
        contando = false;
        EscribirTexto(Dinero.Total);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void OnDisable()
    {
        //Nos borramos del aviso para no recibirlo si este objeto se desactiva o destruye
        Dinero.AlCambiar -= EmpezarConteo;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Empieza a contar desde el número que se ve ahora hasta el nuevo total
    //(si ya estaba contando, por ejemplo con dos ventas muy seguidas, sigue desde donde iba)
    private void EmpezarConteo(int total)
    {
        valorInicioConteo = valorMostrado;
        valorDestino = total;
        tiempoConteo = 0f;
        contando = true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        if (!contando) return;

        //Tiempo "sin escalar": funciona aunque el juego esté en pausa (timeScale = 0)
        tiempoConteo += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(tiempoConteo / duracionConteo);

        valorMostrado = Mathf.Lerp(valorInicioConteo, valorDestino, t);
        EscribirTexto(Mathf.RoundToInt(valorMostrado));

        //Conteo terminado
        if (t >= 1f) contando = false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void EscribirTexto(int cantidad)
    {
        if (textoDinero != null) textoDinero.text = "Dinero: " + cantidad;
    }
}
