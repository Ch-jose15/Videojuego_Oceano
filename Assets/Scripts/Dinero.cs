using System;
using UnityEngine;

public static class Dinero
{
    //Dinero total del jugador. Se puede leer desde fuera, pero solo se cambia con Sumar/Gastar
    public static int Total { get; private set; } = 0;

    //Evento que avisa cuando el dinero cambia (la UI se suscribe para actualizar el texto)
    public static event Action<int> AlCambiar;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public static void Sumar(int cantidad)
    {
        if (cantidad <= 0) return;

        Total += cantidad;
        AlCambiar?.Invoke(Total);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public static bool Gastar(int cantidad)
    {
        if (cantidad <= 0 || cantidad > Total) return false;

        Total -= cantidad;
        AlCambiar?.Invoke(Total);
        return true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        Total = 0;
        AlCambiar = null;
    }
}
