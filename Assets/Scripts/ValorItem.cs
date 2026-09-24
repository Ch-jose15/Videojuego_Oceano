using UnityEngine;

//Se añade a cada objeto recolectable (tag "Item") para indicar cuánto dinero vale al venderlo
public class ValorItem : MonoBehaviour
{
    [Header("Venta")]
    public int valor = 10;
}
