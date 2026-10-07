// Genera de forma procedural el relieve del Terrain sumando ruido Perlin, montes, fosas y detalle fino, con escalas en metros.
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class GeneradorRelieve : MonoBehaviour
{
    [Header("Semilla (cambia todo el mapa)")]
    public int semilla = 12345;

    [Header("1. Altura base (0 = fondo del Terrain, 1 = techo)")]
    [Range(0f, 1f)] public float alturaBase = 0.45f;

    [Header("2. Formas grandes (relieve general)")]
    public float tamañoFormasGrandes = 330f;
    [Range(0f, 0.5f)] public float intensidadFormasGrandes = 0.24f;
    [Range(1, 6)] public int octavasFormasGrandes = 4;
    public float tamañoDeformacion = 500f;
    public float intensidadDeformacion = 150f;

    [Header("3. Zonas de profundidad")]
    public float elevacionesPorKm2 = 7f;
    public float fosasPorKm2 = 7f;
    public float radioMinimoZona = 40f;
    public float radioMaximoZona = 110f;
    [Range(0f, 0.6f)] public float alturaElevaciones = 0.45f;
    [Range(0f, 0.6f)] public float profundidadFosas = 0.42f;
    [Range(1f, 5f)] public float estiramientoMaximo = 3f;

    [Header("4. Detalle fino")]
    public float tamañoDetalle = 22f;
    [Range(0f, 0.1f)] public float intensidadDetalle = 0.012f;
    [Range(1, 6)] public int octavasDetalle = 3;

    [Header("Límites de altura")]
    [Range(0f, 1f)] public float alturaMinima = 0.02f;
    [Range(0f, 1f)] public float alturaMaxima = 0.98f;
    [Range(0.01f, 0.2f)] public float margenSuavizado = 0.06f;

    private struct Zona
    {
        public Vector2 centro;
        public float radio;
        public float estiramiento;
        public float coseno, seno;
        public float intensidad;
    }

    private Vector2 desplazamientoGrande, desplazamientoDeformacionX, desplazamientoDeformacionY, desplazamientoDetalle;
    private Zona[] elevaciones;
    private Zona[] fosas;

    // Calcula todas las alturas del mapa y las aplica al Terrain
    public void Generar()
    {
        Terrain terreno = GetComponent<Terrain>();
        TerrainData datos = terreno.terrainData;
        int resolucion = datos.heightmapResolution;
        Vector3 tamañoMapa = datos.size;

        PrepararSemilla(tamañoMapa);

        float[,] alturas = new float[resolucion, resolucion];
        for (int z = 0; z < resolucion; z++)
        {
            for (int x = 0; x < resolucion; x++)
            {
                float xMetros = (float)x / (resolucion - 1) * tamañoMapa.x;
                float zMetros = (float)z / (resolucion - 1) * tamañoMapa.z;
                alturas[z, x] = CalcularAltura(xMetros, zMetros);
            }
        }

        datos.SetHeights(0, 0, alturas);
    }

    // Devuelve la altura (de 0 a 1) en un punto del mapa sumando todas las capas del relieve
    private float CalcularAltura(float x, float z)
    {
        float altura = alturaBase;

        float deformacionX = (Mathf.PerlinNoise(x / tamañoDeformacion + desplazamientoDeformacionX.x, z / tamañoDeformacion + desplazamientoDeformacionX.y) - 0.5f) * 2f;
        float deformacionZ = (Mathf.PerlinNoise(x / tamañoDeformacion + desplazamientoDeformacionY.x, z / tamañoDeformacion + desplazamientoDeformacionY.y) - 0.5f) * 2f;
        float xDeformado = x + deformacionX * intensidadDeformacion;
        float zDeformado = z + deformacionZ * intensidadDeformacion;
        altura += RuidoFractal(xDeformado / tamañoFormasGrandes, zDeformado / tamañoFormasGrandes, octavasFormasGrandes, desplazamientoGrande) * intensidadFormasGrandes;

        for (int i = 0; i < elevaciones.Length; i++) altura += Campana(xDeformado, zDeformado, elevaciones[i]) * elevaciones[i].intensidad;
        for (int i = 0; i < fosas.Length; i++) altura -= Campana(xDeformado, zDeformado, fosas[i]) * fosas[i].intensidad;

        altura += RuidoFractal(x / tamañoDetalle, z / tamañoDetalle, octavasDetalle, desplazamientoDetalle) * intensidadDetalle;

        return LimiteSuave(altura);
    }

    // Mantiene la altura entre el mínimo y el máximo frenándola suavemente cerca de los límites
    private float LimiteSuave(float altura)
    {
        float umbralAbajo = alturaMinima + margenSuavizado;
        float umbralArriba = alturaMaxima - margenSuavizado;

        if (altura < umbralAbajo)
        {
            altura = alturaMinima + margenSuavizado * Mathf.Exp((altura - umbralAbajo) / margenSuavizado);
        }
        else if (altura > umbralArriba)
        {
            altura = alturaMaxima - margenSuavizado * Mathf.Exp((umbralArriba - altura) / margenSuavizado);
        }

        return altura;
    }

    // Suma varias octavas de ruido Perlin y devuelve un valor aproximadamente entre -1 y 1
    private float RuidoFractal(float x, float y, int octavas, Vector2 desplazamiento)
    {
        float resultado = 0f;
        float frecuencia = 1f;
        float amplitud = 1f;
        float sumaAmplitudes = 0f;

        for (int i = 0; i < octavas; i++)
        {
            float ruido = Mathf.PerlinNoise(x * frecuencia + desplazamiento.x, y * frecuencia + desplazamiento.y);
            resultado += (ruido - 0.5f) * 2f * amplitud;
            sumaAmplitudes += amplitud;

            frecuencia *= 2f;
            amplitud *= 0.5f;
        }

        return resultado / sumaAmplitudes;
    }

    // Devuelve la influencia de una zona elíptica en un punto, de 1 en su centro a casi 0 fuera de ella
    private float Campana(float x, float z, Zona zona)
    {
        float dx = x - zona.centro.x;
        float dz = z - zona.centro.y;

        float alcance = zona.radio * zona.estiramiento * 2f;
        if (dx > alcance || dx < -alcance || dz > alcance || dz < -alcance) return 0f;

        float aLoLargo = (zona.coseno * dx + zona.seno * dz) / zona.estiramiento;
        float aLoAncho = -zona.seno * dx + zona.coseno * dz;

        float distanciaCuadrado = aLoLargo * aLoLargo + aLoAncho * aLoAncho;
        float radio = zona.radio;
        return Mathf.Exp(-distanciaCuadrado / (2f * radio * radio * 0.25f));
    }

    // Prepara los desplazamientos de ruido y las zonas a partir de la semilla
    private void PrepararSemilla(Vector3 tamañoMapa)
    {
        System.Random aleatorio = new System.Random(semilla);

        desplazamientoGrande = new Vector2(Aleatorio(aleatorio, 1000f, 9000f), Aleatorio(aleatorio, 1000f, 9000f));
        desplazamientoDeformacionX = new Vector2(Aleatorio(aleatorio, 1000f, 9000f), Aleatorio(aleatorio, 1000f, 9000f));
        desplazamientoDeformacionY = new Vector2(Aleatorio(aleatorio, 1000f, 9000f), Aleatorio(aleatorio, 1000f, 9000f));
        desplazamientoDetalle = new Vector2(Aleatorio(aleatorio, 1000f, 9000f), Aleatorio(aleatorio, 1000f, 9000f));

        float km2 = (tamañoMapa.x / 1000f) * (tamañoMapa.z / 1000f);
        elevaciones = CrearZonas(aleatorio, Mathf.RoundToInt(elevacionesPorKm2 * km2), alturaElevaciones, tamañoMapa);
        fosas = CrearZonas(aleatorio, Mathf.RoundToInt(fosasPorKm2 * km2), profundidadFosas, tamañoMapa);
    }

    // Crea zonas con posición, tamaño, forma, dirección e intensidad aleatorias
    private Zona[] CrearZonas(System.Random aleatorio, int cantidad, float intensidadMaxima, Vector3 tamañoMapa)
    {
        Zona[] zonas = new Zona[cantidad];
        for (int i = 0; i < cantidad; i++)
        {
            zonas[i].centro = new Vector2(Aleatorio(aleatorio, 0.08f, 0.92f) * tamañoMapa.x, Aleatorio(aleatorio, 0.08f, 0.92f) * tamañoMapa.z);
            zonas[i].radio = Aleatorio(aleatorio, radioMinimoZona, radioMaximoZona);
            zonas[i].estiramiento = Aleatorio(aleatorio, 1f, estiramientoMaximo);
            float angulo = Aleatorio(aleatorio, 0f, Mathf.PI);
            zonas[i].coseno = Mathf.Cos(angulo);
            zonas[i].seno = Mathf.Sin(angulo);
            zonas[i].intensidad = intensidadMaxima * Aleatorio(aleatorio, 0.5f, 1f);
        }
        return zonas;
    }

    // Devuelve un número aleatorio entre mínimo y máximo usando el generador de la semilla
    private float Aleatorio(System.Random aleatorio, float minimo, float maximo)
    {
        return minimo + (float)aleatorio.NextDouble() * (maximo - minimo);
    }
}
