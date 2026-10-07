// Genera copias de prefabs en posiciones aleatorias del Terrain, sobre el suelo y bajo el agua, con preferencias opcionales de profundidad y distancia a la base.
using UnityEngine;

public class GeneradorObjetos : MonoBehaviour
{
    [Header("Qué generar")]
    public GameObject[] prefabs;
    public int cantidad = 6;

    [Header("Zona")]
    public Terrain terreno;
    public Transform superficieAgua;
    public float margenBordeTerreno = 5f;
    public float alturaMinSobreSuelo = 1.5f;
    public float alturaMaxSobreSuelo = 4f;
    public float margenBajoAgua = 2f;

    [Header("Separación")]
    public Transform jugador;
    public float distanciaMinimaJugador = 10f;
    public float radioLibre = 1.2f;

    [Header("Distribución (opcional)")]
    public Transform zonaBase;
    public float distanciaMinimaBase = 0f;
    [Range(0f, 1f)] public float preferenciaProfundidad = 0f;
    [Range(0f, 1f)] public float preferenciaLejosDeBase = 0f;

    private const int IntentosMaximos = 100;

    private float alturaSueloMinima;
    private float alturaSueloMaxima;

    // Usa el Terrain activo si no hay uno asignado y genera los objetos
    void Start()
    {
        if (terreno == null) terreno = Terrain.activeTerrain;
        Generar();
    }

    // Crea los objetos en posiciones válidas como hijos de este generador
    public void Generar()
    {
        if (terreno == null || prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning(name + ": falta el Terrain o los prefabs, no se genera nada.");
            return;
        }

        CalcularRangoDelSuelo();

        int generados = 0;
        for (int i = 0; i < cantidad; i++)
        {
            Vector3 posicion;
            if (!BuscarPosicion(out posicion)) continue;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Quaternion rotacion = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject nuevo = Instantiate(prefab, posicion, rotacion, transform);
            nuevo.name = prefab.name + "_" + (i + 1);
            generados++;
        }

        if (generados < cantidad) Debug.LogWarning(name + ": solo se encontró sitio para " + generados + " de " + cantidad + " objetos.");
    }

    // Busca una posición aleatoria que cumpla todas las condiciones; devuelve false si no la encuentra
    private bool BuscarPosicion(out Vector3 posicion)
    {
        Vector3 origen = terreno.transform.position;
        Vector3 tamaño = terreno.terrainData.size;

        float limiteAgua = superficieAgua != null ? superficieAgua.position.y - margenBajoAgua : Mathf.Infinity;

        for (int intento = 0; intento < IntentosMaximos; intento++)
        {
            float x = Random.Range(origen.x + margenBordeTerreno, origen.x + tamaño.x - margenBordeTerreno);
            float z = Random.Range(origen.z + margenBordeTerreno, origen.z + tamaño.z - margenBordeTerreno);

            float alturaSuelo = terreno.SampleHeight(new Vector3(x, 0f, z)) + origen.y;
            float y = alturaSuelo + Random.Range(alturaMinSobreSuelo, alturaMaxSobreSuelo);

            posicion = new Vector3(x, y, z);

            if (y > limiteAgua) continue;
            if (jugador != null && Vector3.Distance(posicion, jugador.position) < distanciaMinimaJugador) continue;
            if (zonaBase != null && DistanciaHorizontal(posicion, zonaBase.position) < distanciaMinimaBase) continue;
            if (Physics.CheckSphere(posicion, radioLibre, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;

            if (Random.value > ProbabilidadAceptar(posicion, alturaSuelo)) continue;

            return true;
        }

        posicion = Vector3.zero;
        return false;
    }

    // Calcula la probabilidad de aceptar una posición según su profundidad y su distancia a la base
    private float ProbabilidadAceptar(Vector3 posicion, float alturaSuelo)
    {
        float probabilidad = 1f;

        float profundidad = Mathf.InverseLerp(alturaSueloMaxima, alturaSueloMinima, alturaSuelo);
        probabilidad *= Mathf.Pow(profundidad, preferenciaProfundidad * 4f);

        if (zonaBase != null)
        {
            float lejania = Mathf.Clamp01(DistanciaHorizontal(posicion, zonaBase.position) / terreno.terrainData.size.x);
            probabilidad *= Mathf.Pow(lejania, preferenciaLejosDeBase * 4f);
        }

        return probabilidad;
    }

    // Busca la altura del suelo más hondo y más alto del Terrain mirando uno de cada 8 puntos
    private void CalcularRangoDelSuelo()
    {
        TerrainData datos = terreno.terrainData;
        int resolucion = datos.heightmapResolution;

        float minimo = float.MaxValue, maximo = float.MinValue;
        for (int z = 0; z < resolucion; z += 8)
        {
            for (int x = 0; x < resolucion; x += 8)
            {
                float altura = datos.GetHeight(x, z);
                minimo = Mathf.Min(minimo, altura);
                maximo = Mathf.Max(maximo, altura);
            }
        }

        alturaSueloMinima = terreno.transform.position.y + minimo;
        alturaSueloMaxima = terreno.transform.position.y + maximo;
    }

    // Devuelve la distancia entre dos puntos sin tener en cuenta la altura
    private float DistanciaHorizontal(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
