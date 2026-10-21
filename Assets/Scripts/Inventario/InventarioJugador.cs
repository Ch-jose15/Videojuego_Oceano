// Inventario de 5 slots: con G guarda el objeto de la mano (ocultándolo) o abre y cierra la UI si las manos están vacías.
using System;
using UnityEngine;

public class InventarioJugador : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar agarrar;
    public UIInventario uiInventario;

    [Header("Ajustes")]
    public KeyCode teclaInventario = KeyCode.G;

    public const int NumeroSlots = 5;

    private GameObject[] slots = new GameObject[NumeroSlots];

    public event Action AlCambiar;

    // Busca las referencias que no se hayan asignado en el Inspector
    void Awake()
    {
        if (agarrar == null) agarrar = GetComponent<InteracciónAgarrar>();
        if (uiInventario == null) uiInventario = FindFirstObjectByType<UIInventario>();
    }

    // Comprueba la tecla del inventario cada frame
    void Update()
    {
        if (EstadoJuego.ControlesBloqueados) return;

        if (Input.GetKeyDown(teclaInventario)) PulsarTeclaInventario();
    }

    // Guarda el objeto de la mano o, con las manos vacías, abre o cierra la UI del inventario
    private void PulsarTeclaInventario()
    {
        if (agarrar != null && agarrar.TieneObjetoAgarrado())
        {
            if (HayHueco()) Guardar(agarrar.QuitarObjetoAgarrado());
        }
        else
        {
            if (uiInventario != null) uiInventario.AlternarVisible();
        }
    }

    // Saca el objeto de un slot, lo reactiva, lo pone en la mano del jugador y cierra el inventario
    public void SacarAMano(int slot)
    {
        if (agarrar == null || agarrar.TieneObjetoAgarrado()) return;

        GameObject objeto = QuitarObjeto(slot);
        if (objeto == null) return;

        objeto.SetActive(true);
        agarrar.Agarrar(objeto);

        if (uiInventario != null) uiInventario.Cerrar();
    }

    // Indica si queda algún slot libre
    public bool HayHueco()
    {
        return BuscarSlotLibre() != -1;
    }

    // Guarda un objeto en el primer slot libre y lo oculta
    public bool Guardar(GameObject objeto)
    {
        if (objeto == null) return false;

        int slot = BuscarSlotLibre();
        if (slot == -1) return false;

        slots[slot] = objeto;
        objeto.SetActive(false);

        AlCambiar?.Invoke();
        return true;
    }

    // Devuelve el objeto de un slot sin sacarlo del inventario
    public GameObject ObtenerObjeto(int slot)
    {
        if (slot < 0 || slot >= NumeroSlots) return null;
        return slots[slot];
    }

    // Saca el objeto de un slot y lo devuelve todavía oculto
    public GameObject QuitarObjeto(int slot)
    {
        if (slot < 0 || slot >= NumeroSlots) return null;

        GameObject objeto = slots[slot];
        slots[slot] = null;

        if (objeto != null) AlCambiar?.Invoke();
        return objeto;
    }

    // Destruye para siempre todos los objetos guardados y devuelve cuántos había
    public int VaciarYDestruir()
    {
        int cantidad = 0;
        for (int i = 0; i < NumeroSlots; i++)
        {
            if (slots[i] == null) continue;

            Destroy(slots[i]);
            slots[i] = null;
            cantidad++;
        }

        if (cantidad > 0) AlCambiar?.Invoke();
        return cantidad;
    }

    // Devuelve el índice del primer slot libre o -1 si están todos ocupados
    private int BuscarSlotLibre()
    {
        for (int i = 0; i < NumeroSlots; i++)
        {
            if (slots[i] == null) return i;
        }
        return -1;
    }
}
