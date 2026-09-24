using System;
using UnityEngine;

//Inventario simple de 5 slots. Va en el jugador, junto a InteracciónAgarrar.
//Los objetos guardados no se destruyen: se ocultan (SetActive(false)) para no perder sus datos (por ejemplo su ValorItem)
public class InventarioJugador : MonoBehaviour
{
    [Header("Referencias")]
    public InteracciónAgarrar agarrar;
    public UIInventario uiInventario;

    [Header("Ajustes")]
    public KeyCode teclaInventario = KeyCode.G;

    //Número de slots del inventario
    public const int NumeroSlots = 5;

    //Cada posición es un slot. Si vale null, ese slot está libre
    private GameObject[] slots = new GameObject[NumeroSlots];

    //Evento que avisa cuando el inventario cambia (la UI se suscribe para redibujar los slots)
    public event Action AlCambiar;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Awake()
    {
        //Si no se han asignado en el Inspector, los buscamos
        if (agarrar == null) agarrar = GetComponent<InteracciónAgarrar>();
        if (uiInventario == null) uiInventario = FindFirstObjectByType<UIInventario>();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        if (!Input.GetKeyDown(teclaInventario)) return;

        if (agarrar != null && agarrar.TieneObjetoAgarrado())
        {
            //Llevamos algo en la mano: lo guardamos (si está lleno, no hace nada y seguimos con el objeto en la mano)
            if (HayHueco()) Guardar(agarrar.QuitarObjetoAgarrado());
        }
        else
        {
            //Manos libres: abrimos/cerramos la UI del inventario
            if (uiInventario != null) uiInventario.AlternarVisible();
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Devuelve true si queda al menos un slot libre
    public bool HayHueco()
    {
        return BuscarSlotLibre() != -1;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Guarda el objeto en el primer slot libre y lo oculta. Devuelve false si no se ha podido guardar
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

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Devuelve el objeto de un slot (o null si está vacío), sin sacarlo del inventario
    public GameObject ObtenerObjeto(int slot)
    {
        if (slot < 0 || slot >= NumeroSlots) return null;
        return slots[slot];
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Saca el objeto de un slot y lo devuelve (sigue oculto: quien lo reciba decide qué hacer con él, por ejemplo venderlo)
    public GameObject QuitarObjeto(int slot)
    {
        if (slot < 0 || slot >= NumeroSlots) return null;

        GameObject objeto = slots[slot];
        slots[slot] = null;

        if (objeto != null) AlCambiar?.Invoke();
        return objeto;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Devuelve el índice del primer slot libre, o -1 si están todos ocupados
    private int BuscarSlotLibre()
    {
        for (int i = 0; i < NumeroSlots; i++)
        {
            if (slots[i] == null) return i;
        }
        return -1;
    }
}
