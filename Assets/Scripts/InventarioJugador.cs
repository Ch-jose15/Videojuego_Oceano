using System;
using UnityEngine;

//Inventario simple de 5 slots. Va en el jugador, junto a InteracciónAgarrar.
//Los objetos guardados no se destruyen: se ocultan (SetActive(false)) para no perder sus datos
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
        //Con un menú abierto las teclas G y 1-5 no hacen nada (el menú de la tienda también usa 1 y 2)
        if (EstadoJuego.ControlesBloqueados) return;

        if (Input.GetKeyDown(teclaInventario)) PulsarTeclaInventario();

        ComprobarTeclasSlots();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void PulsarTeclaInventario()
    {
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

    //Teclas 1 a 5: sacan el objeto de ese slot a la mano.
    //Solo funcionan con el inventario abierto y las manos vacías (para no cambiar sin querer lo que llevamos)
    private void ComprobarTeclasSlots()
    {
        if (agarrar == null || uiInventario == null) return;
        if (!uiInventario.EstaVisible() || agarrar.TieneObjetoAgarrado()) return;

        for (int i = 0; i < NumeroSlots; i++)
        {
            //KeyCode.Alpha1 + 0 = tecla 1, KeyCode.Alpha1 + 1 = tecla 2, etc.
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SacarAMano(i);
        }
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Saca el objeto del slot, lo vuelve a activar y se lo pone en la mano al jugador. Si el slot está vacío no hace nada
    public void SacarAMano(int slot)
    {
        if (agarrar == null || agarrar.TieneObjetoAgarrado()) return;

        GameObject objeto = QuitarObjeto(slot);
        if (objeto == null) return;

        objeto.SetActive(true);
        agarrar.Agarrar(objeto);
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
