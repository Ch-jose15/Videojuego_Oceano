using UnityEngine;

//Genera copias de uno o varios prefabs en posiciones aleatorias al empezar la partida.
//Las posiciones siempre quedan:
// - dentro del área del Terrain,
// - un poco por encima del suelo (entre alturaMinSobreSuelo y alturaMaxSobreSuelo),
// - por debajo de la superficie del agua,
// - sin chocar con otros objetos (por ejemplo la Tienda) y no demasiado cerca del jugador.
//Se usa tanto para los enemigos como para los cubos vendibles (un GeneradorObjetos para cada cosa)
public class GeneradorObjetos : MonoBehaviour
{
    [Header("Qué generar")]
    public GameObject[] prefabs; //Para cada objeto se elige uno de la lista al azar
    public int cantidad = 6;

    [Header("Zona")]
    public Terrain terreno;             //Si está vacío, se usa Terrain.activeTerrain
    public Transform superficieAgua;    //El Ocean: su altura Y es el límite superior
    public float margenBordeTerreno = 5f;
    public float alturaMinSobreSuelo = 1.5f;
    public float alturaMaxSobreSuelo = 4f;
    public float margenBajoAgua = 2f;

    [Header("Separación")]
    public Transform jugador;
    public float distanciaMinimaJugador = 10f;
    public float radioLibre = 1.2f; //Espacio libre que necesita cada objeto para no aparecer dentro de otro

    //Veces que se intenta encontrar un sitio válido para cada objeto antes de rendirse
    private const int IntentosMaximos = 30;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    void Start()
    {
        if (terreno == null) terreno = Terrain.activeTerrain;
        Generar();
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    public void Generar()
    {
        if (terreno == null || prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning(name + ": falta el Terrain o los prefabs, no se genera nada.");
            return;
        }

        int generados = 0;
        for (int i = 0; i < cantidad; i++)
        {
            Vector3 posicion;
            if (!BuscarPosicion(out posicion)) continue;

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            Quaternion rotacion = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            //Los hacemos hijos del generador para tener la jerarquía ordenada
            GameObject nuevo = Instantiate(prefab, posicion, rotacion, transform);
            nuevo.name = prefab.name + "_" + (i + 1);
            generados++;
        }

        if (generados < cantidad) Debug.LogWarning(name + ": solo se encontró sitio para " + generados + " de " + cantidad + " objetos.");
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Busca una posición aleatoria válida. Devuelve false si no la encuentra tras varios intentos
    private bool BuscarPosicion(out Vector3 posicion)
    {
        Vector3 origen = terreno.transform.position;
        Vector3 tamaño = terreno.terrainData.size;

        //Si no hay superficie asignada, no ponemos límite por arriba
        float limiteAgua = superficieAgua != null ? superficieAgua.position.y - margenBajoAgua : Mathf.Infinity;

        for (int intento = 0; intento < IntentosMaximos; intento++)
        {
            //1. X y Z al azar dentro del terreno (dejando un margen en los bordes)
            float x = Random.Range(origen.x + margenBordeTerreno, origen.x + tamaño.x - margenBordeTerreno);
            float z = Random.Range(origen.z + margenBordeTerreno, origen.z + tamaño.z - margenBordeTerreno);

            //2. Altura del suelo en ese punto. SampleHeight devuelve la altura relativa al Terrain, por eso sumamos su Y
            float alturaSuelo = terreno.SampleHeight(new Vector3(x, 0f, z)) + origen.y;
            float y = alturaSuelo + Random.Range(alturaMinSobreSuelo, alturaMaxSobreSuelo);

            posicion = new Vector3(x, y, z);

            //3. Comprobaciones: bajo el agua, lejos del jugador y sin chocar con nada
            if (y > limiteAgua) continue;
            if (jugador != null && Vector3.Distance(posicion, jugador.position) < distanciaMinimaJugador) continue;
            if (Physics.CheckSphere(posicion, radioLibre, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;

            return true;
        }

        posicion = Vector3.zero;
        return false;
    }
}
