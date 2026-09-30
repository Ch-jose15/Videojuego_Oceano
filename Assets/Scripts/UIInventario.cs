using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Muestra los slots del inventario. Va en un objeto que esté siempre activo (el Canvas),
//y lo que se muestra/oculta es el panel hijo con los slots.
//Cada slot es un botón: al hacer clic se saca ese objeto a la mano (si las manos están vacías)
public class UIInventario : MonoBehaviour
{
    [Header("Referencias")]
    public InventarioJugador inventario;
    public GameObject panelInventario;
    public TextMeshProUGUI[] textosSlots = new TextMeshProUGUI[InventarioJugador.NumeroSlots];
    public Button[] botonesSlots = new Button[InventarioJugador.NumeroSlots];

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        if (inventario == null) inventario = FindFirstObjectByType<InventarioJugador>();

        //Conectamos cada botón con su slot
        for (int i = 0; i < botonesSlots.Length; i++)
        {
            if (botonesSlots[i] == null) continue;

            //Copiamos i en otra variable: si usáramos i directamente, todos los botones usarían
            //el último valor de i (5) cuando se pulsen, en vez del número de su propio slot
            int numeroSlot = i;
            botonesSlots[i].onClick.AddListener(() => PulsarSlot(numeroSlot));
        }

        //La UI del inventario empieza oculta
        if (panelInventario != null) panelInventario.SetActive(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void OnEnable()
    {
        //Nos apuntamos al aviso de "el inventario ha cambiado"
        if (inventario != null) inventario.AlCambiar += ActualizarSlots;
        ActualizarSlots();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void OnDisable()
    {
        if (inventario != null) inventario.AlCambiar -= ActualizarSlots;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Muestra el panel si estaba oculto, o lo oculta si estaba visible.
    //Mientras está abierto el ratón queda libre para poder hacer clic en los slots
    public void AlternarVisible()
    {
        if (panelInventario == null) return;

        panelInventario.SetActive(!panelInventario.activeSelf);
        EstadoJuego.CambiarInventarioAbierto(panelInventario.activeSelf);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Devuelve true si el panel del inventario se está mostrando
    public bool EstaVisible()
    {
        return panelInventario != null && panelInventario.activeSelf;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Clic en un slot: sacamos su objeto a la mano (SacarAMano ya comprueba que las manos estén vacías y el slot tenga algo)
    private void PulsarSlot(int numeroSlot)
    {
        if (inventario != null) inventario.SacarAMano(numeroSlot);
        EstadoJuego.QuitarSeleccionUI();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Escribe en cada slot el nombre del objeto guardado, o lo deja vacío
    private void ActualizarSlots()
    {
        if (inventario == null) return;

        for (int i = 0; i < textosSlots.Length; i++)
        {
            if (textosSlots[i] == null) continue;

            GameObject objeto = inventario.ObtenerObjeto(i);
            if (objeto != null) textosSlots[i].text = objeto.name;
            else textosSlots[i].text = "";
        }
    }
}
