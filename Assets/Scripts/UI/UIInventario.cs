using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIInventario : MonoBehaviour
{
    [Header("Referencias")]
    public InventarioJugador inventario;
    public GameObject panelInventario;
    public TextMeshProUGUI[] textosSlots = new TextMeshProUGUI[InventarioJugador.NumeroSlots];
    public Button[] botonesSlots = new Button[InventarioJugador.NumeroSlots];

    /*========================================================================================*/

    void Awake()
    {
        if (inventario == null) inventario = FindFirstObjectByType<InventarioJugador>();

        for (int i = 0; i < botonesSlots.Length; i++)
        {
            if (botonesSlots[i] == null) continue;

            int numeroSlot = i;
            botonesSlots[i].onClick.AddListener(() => PulsarSlot(numeroSlot));
        }

        if (panelInventario != null) panelInventario.SetActive(false);
    }

    /*========================================================================================*/

    void OnEnable()
    {
        if (inventario != null) inventario.AlCambiar += ActualizarSlots;
        ActualizarSlots();
    }

    /*========================================================================================*/

    void OnDisable()
    {
        if (inventario != null) inventario.AlCambiar -= ActualizarSlots;
    }

    /*========================================================================================*/

    public void AsignarInventario(InventarioJugador nuevo)
    {
        if (inventario == nuevo) return;

        if (inventario != null) inventario.AlCambiar -= ActualizarSlots;
        inventario = nuevo;
        if (inventario != null && isActiveAndEnabled) inventario.AlCambiar += ActualizarSlots;

        ActualizarSlots();
    }

    /*========================================================================================*/

    public void AlternarVisible()
    {
        if (panelInventario == null) return;

        panelInventario.SetActive(!panelInventario.activeSelf);
        EstadoJuego.CambiarInventarioAbierto(panelInventario.activeSelf);
    }

    /*========================================================================================*/

    public void Cerrar()
    {
        if (!EstaVisible()) return;

        panelInventario.SetActive(false);
        EstadoJuego.CambiarInventarioAbierto(false);
    }

    /*========================================================================================*/

    public bool EstaVisible()
    {
        return panelInventario != null && panelInventario.activeSelf;
    }

    /*========================================================================================*/

    private void PulsarSlot(int numeroSlot)
    {
        if (inventario != null) inventario.SacarAMano(numeroSlot);
        EstadoJuego.QuitarSeleccionUI();
    }

    /*========================================================================================*/

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