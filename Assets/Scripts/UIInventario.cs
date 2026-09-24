using TMPro;
using UnityEngine;

//Muestra los slots del inventario. Va en un objeto que esté siempre activo (el Canvas),
//y lo que se muestra/oculta es el panel hijo con los slots
public class UIInventario : MonoBehaviour
{
    [Header("Referencias")]
    public InventarioJugador inventario;
    public GameObject panelInventario;
    public TextMeshProUGUI[] textosSlots = new TextMeshProUGUI[InventarioJugador.NumeroSlots];

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        if (inventario == null) inventario = FindFirstObjectByType<InventarioJugador>();

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

    //Muestra el panel si estaba oculto, o lo oculta si estaba visible
    public void AlternarVisible()
    {
        if (panelInventario == null) return;

        panelInventario.SetActive(!panelInventario.activeSelf);
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
