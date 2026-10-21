// Muestra los slots del inventario y permite sacar un objeto a la mano haciendo clic en su slot.
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

    // Busca el inventario, conecta cada botón con su slot y oculta el panel
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

    // Se suscribe a los cambios del inventario y dibuja los slots
    void OnEnable()
    {
        if (inventario != null) inventario.AlCambiar += ActualizarSlots;
        ActualizarSlots();
    }

    // Deja de escuchar los cambios del inventario
    void OnDisable()
    {
        if (inventario != null) inventario.AlCambiar -= ActualizarSlots;
    }

    // Muestra u oculta el panel del inventario y libera o bloquea el ratón
    public void AlternarVisible()
    {
        if (panelInventario == null) return;

        panelInventario.SetActive(!panelInventario.activeSelf);
        EstadoJuego.CambiarInventarioAbierto(panelInventario.activeSelf);
    }

    // Oculta el panel del inventario si está abierto y vuelve a bloquear el ratón
    public void Cerrar()
    {
        if (!EstaVisible()) return;

        panelInventario.SetActive(false);
        EstadoJuego.CambiarInventarioAbierto(false);
    }

    // Indica si el panel del inventario se está mostrando
    public bool EstaVisible()
    {
        return panelInventario != null && panelInventario.activeSelf;
    }

    // Saca a la mano el objeto del slot pulsado
    private void PulsarSlot(int numeroSlot)
    {
        if (inventario != null) inventario.SacarAMano(numeroSlot);
        EstadoJuego.QuitarSeleccionUI();
    }

    // Escribe en cada slot el nombre del objeto guardado o lo deja vacío
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
