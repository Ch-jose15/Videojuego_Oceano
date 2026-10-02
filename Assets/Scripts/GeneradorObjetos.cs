using UnityEngine;

//Genera copias de uno o varios prefabs en posiciones aleatorias al empezar la partida.
//Las posiciones siempre quedan:
// - dentro del área del Terrain,
// - un poco por encima del suelo (entre alturaMinSobreSuelo y alturaMaxSobreSuelo),
// - por debajo de la superficie del agua,
// - sin chocar con otros objetos (por ejemplo los cubos de venta y compra) y no demasiado cerca del jugador.
//Se usa tanto para los enemigos como para los cubos vendibles (un GeneradorObjetos para cada cosa)
//
//Opcionalmente se puede repartir de forma no uniforme (en "Distribución"; con todo a 0 el reparto es uniforme):
// - preferenciaProfundidad: más objetos en las zonas hondas del Terrain.
// - preferenciaLejosDeBase: más objetos lejos de la base del jugador (zonaBase).
//Funciona como un sorteo: cada posición candidata tiene una probabilidad de ser aceptada.
//Las posiciones "buenas" (hondas, lejanas...) se aceptan casi siempre y las demás se descartan más a menudo.
//La probabilidad es valor^(preferencia * 4): con preferencia 0 siempre da 1 (reparto uniforme),
//y cuanto mayor es la preferencia, más se "castigan" las posiciones con valor bajo.
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

    [Header("Distribución (opcional)")]
    public Transform zonaBase;                                      //Base del jugador (por ejemplo la tienda). Vacío = no se tiene en cuenta
    public float distanciaMinimaBase = 0f;                          //No se genera nada más cerca que esto de la base
    [Range(0f, 1f)] public float preferenciaProfundidad = 0f;       //0 = da igual la profundidad; 1 = casi todo en las zonas más hondas
    [Range(0f, 1f)] public float preferenciaLejosDeBase = 0f;       //0 = da igual; 1 = casi todo lejos de la base

    //Veces que se intenta encontrar un sitio válido para cada objeto antes de rendirse
    //(bastantes, porque con las preferencias de distribución se descartan más posiciones)
    private const int IntentosMaximos = 100;

    //Altura del suelo más hondo y más alto que existen de verdad en el Terrain (para medir la profundidad)
    private float alturaSueloMinima;
    private float alturaSueloMaxima;

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

        CalcularRangoDelSuelo();

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
            if (zonaBase != null && DistanciaHorizontal(posicion, zonaBase.position) < distanciaMinimaBase) continue;
            if (Physics.CheckSphere(posicion, radioLibre, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;

            //4. Sorteo según las preferencias de distribución (con todo a 0 la probabilidad es 1 y siempre se acepta)
            if (Random.value > ProbabilidadAceptar(posicion, alturaSuelo)) continue;

            return true;
        }

        posicion = Vector3.zero;
        return false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Probabilidad (de 0 a 1) de aceptar una posición según las preferencias de distribución
    private float ProbabilidadAceptar(Vector3 posicion, float alturaSuelo)
    {
        float probabilidad = 1f;

        //Profundidad del suelo en ese punto: 0 = el suelo más alto del mapa, 1 = el más hondo
        float profundidad = Mathf.InverseLerp(alturaSueloMaxima, alturaSueloMinima, alturaSuelo);
        //Potencia: con preferencia 0 da 1 (no cambia nada). Con preferencia alta, las zonas poco hondas
        //(profundidad baja) dan un número muy pequeño, y casi nunca se aceptan
        probabilidad *= Mathf.Pow(profundidad, preferenciaProfundidad * 4f);

        if (zonaBase != null)
        {
            //Lejanía a la base: 0 = encima de la base, 1 = a la distancia del ancho del mapa o más
            float lejania = Mathf.Clamp01(DistanciaHorizontal(posicion, zonaBase.position) / terreno.terrainData.size.x);
            probabilidad *= Mathf.Pow(lejania, preferenciaLejosDeBase * 4f);
        }

        return probabilidad;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Busca la altura del suelo más hondo y más alto del Terrain (mirando un punto de cada 8 para ir rápido).
    //Así la profundidad se mide respecto al relieve real, y no respecto al alto total del Terrain
    //(que casi nunca se usa entero, y entonces todas las zonas parecerían "de profundidad media")
    private void CalcularRangoDelSuelo()
    {
        TerrainData datos = terreno.terrainData;
        int resolucion = datos.heightmapResolution;
        float[,] alturas = datos.GetHeights(0, 0, resolucion, resolucion);

        float minimo = 1f, maximo = 0f;
        for (int z = 0; z < resolucion; z += 8)
        {
            for (int x = 0; x < resolucion; x += 8)
            {
                minimo = Mathf.Min(minimo, alturas[z, x]);
                maximo = Mathf.Max(maximo, alturas[z, x]);
            }
        }

        //GetHeights da valores de 0 a 1: los pasamos a metros del mundo
        alturaSueloMinima = terreno.transform.position.y + minimo * datos.size.y;
        alturaSueloMaxima = terreno.transform.position.y + maximo * datos.size.y;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Distancia sin tener en cuenta la altura (solo X y Z)
    private float DistanciaHorizontal(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
