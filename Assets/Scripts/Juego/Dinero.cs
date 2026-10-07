// Dinero total del jugador, accesible desde cualquier script, con un evento que avisa cada vez que cambia.
using System;
using UnityEngine;

public static class Dinero
{
    public static int Total { get; private set; } = 0;

    public static event Action<int> AlCambiar;

    // Suma dinero al total y avisa del cambio
    public static void Sumar(int cantidad)
    {
        if (cantidad <= 0) return;

        Total += cantidad;
        AlCambiar?.Invoke(Total);
    }

    // Resta dinero si hay suficiente y avisa del cambio; devuelve false si no llega
    public static bool Gastar(int cantidad)
    {
        if (cantidad <= 0 || cantidad > Total) return false;

        Total -= cantidad;
        AlCambiar?.Invoke(Total);
        return true;
    }

    // Pone el dinero a cero cada vez que se pulsa Play en el editor
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        Total = 0;
        AlCambiar = null;
    }
}
