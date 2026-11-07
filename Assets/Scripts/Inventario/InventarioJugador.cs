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
    public event Action AlCambiar;

    private GameObject[] slots = new GameObject[NumeroSlots];

    /*========================================================================================*/

    void Awake()
    {
        if (agarrar == null) 
            agarrar = GetComponent<InteracciónAgarrar>();
        if (uiInventario == null) 
            uiInventario = FindFirstObjectByType<UIInventario>();
    }

    /*========================================================================================*/

    private void Update()
    {
        if (EstadoJuego.ControlesBloqueados) return;
        if (Input.GetKeyDown(teclaInventario)) PulsarTeclaInventario();
    }

    /*========================================================================================*/

    private void PulsarTeclaInventario()
    {
        ConectarUI();

        if (agarrar != null && agarrar.TieneObjetoAgarrado())
        {
            if (HayHueco()) Guardar(agarrar.QuitarObjetoAgarrado());
        }
        else
        {
            if (uiInventario != null) uiInventario.AlternarVisible();
        }       
    }

    /*========================================================================================*/

    public void SacarAMano(int slot)
    {
        if (agarrar == null || agarrar.TieneObjetoAgarrado()) return;

        GameObject objeto = QuitarObjeto(slot);
        if (objeto == null) return;

        ItemRed itemRed = objeto.GetComponent<ItemRed>();
        if (itemRed != null && itemRed.IsSpawned)
        {
            itemRed.PedirSacarRpc();
        }
        else
        {
            objeto.SetActive(true);
            agarrar.Agarrar(objeto);
        }

        if (uiInventario != null) uiInventario.Cerrar();
    }

    /*========================================================================================*/

    private void ConectarUI()
    {
        if (uiInventario == null) uiInventario = FindFirstObjectByType<UIInventario>();
        if (uiInventario != null) uiInventario.AsignarInventario(this);
    }

    /*========================================================================================*/

    public bool HayHueco()
    {
        return BuscarSlotLibre() != -1;
    }

    /*========================================================================================*/

    public bool Guardar(GameObject objeto)
    {
        if (objeto == null) return false;

        int slot = BuscarSlotLibre();
        if (slot == -1) return false;

        slots[slot] = objeto;

        ItemRed itemRed = objeto.GetComponent<ItemRed>();
        if (itemRed != null && itemRed.IsSpawned) itemRed.PedirGuardarRpc();
        else objeto.SetActive(false);

        AlCambiar?.Invoke();
        return true;
    }

    /*========================================================================================*/

    public GameObject ObtenerObjeto(int slot)
    {
        if (slot < 0 || slot >= NumeroSlots) return null;
        return slots[slot];
    }

    /*========================================================================================*/

    public GameObject QuitarObjeto(int slot)
    {
        if (slot < 0 || slot >= NumeroSlots) return null;

        GameObject objeto = slots[slot];
        slots[slot] = null;

        if (objeto != null) AlCambiar?.Invoke();
        return objeto;
    }

    /*========================================================================================*/

    public int VaciarYDestruir()
    {
        int cantidad = 0;
        for (int i = 0; i < NumeroSlots; i++)
        {
            if (slots[i] == null) continue;

            ItemRed itemRed = slots[i].GetComponent<ItemRed>();
            if (itemRed != null && itemRed.IsSpawned) itemRed.PedirDestruirRpc();
            else Destroy(slots[i]);
            slots[i] = null;
            cantidad++;
        }

        if (cantidad > 0) AlCambiar?.Invoke();
        return cantidad;
    }

    /*========================================================================================*/

    private int BuscarSlotLibre()
    {
        for (int i = 0; i < NumeroSlots; i++)
            if (slots[i] == null) return i;
        
        return -1;
    }
}
