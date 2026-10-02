using UnityEngine;

//Genera el relieve (mapa de alturas) de un Terrain de forma procedural.
//Va en el mismo GameObject que el Terrain. Se usa desde el Inspector con los botones "Generar"
//(los dibuja GeneradorRelieveEditor, en la carpeta Editor).
//
//Todos los tamaños están en METROS (no en proporción al mapa): así, si el Terrain es más grande,
//el relieve mantiene su escala real y aparecen MÁS montes y fosas, en vez de montes más grandes y estirados.
//
//El relieve se construye sumando capas. Todas las alturas van de 0 (lo más hondo del Terrain) a 1 (lo más alto):
//  1. Altura base: el nivel "normal" del fondo.
//  2. Formas grandes: ruido Perlin de baja frecuencia (colinas y valles amplios y suaves).
//     Se "deforma" con otro ruido (domain warping) para que las formas no salgan redondas y repetitivas.
//  3. Zonas de profundidad: varias elevaciones (montes submarinos) y varias fosas repartidas al azar.
//     Cada una es una "campana" suave (gaussiana), así que las transiciones son graduales, sin escalones.
//     Son elipses estiradas y giradas al azar (dorsales, cañones...) y usan las mismas coordenadas deformadas
//     que las formas grandes, así que sus bordes son irregulares y no parecen conos perfectos.
//  4. Detalle fino: ruido Perlin de alta frecuencia y poca intensidad, para que de cerca no se vea liso.
[RequireComponent(typeof(Terrain))]
public class GeneradorRelieve : MonoBehaviour
{
    [Header("Semilla (cambia todo el mapa)")]
    public int semilla = 12345;

    [Header("1. Altura base (0 = fondo del Terrain, 1 = techo)")]
    [Range(0f, 1f)] public float alturaBase = 0.45f;

    [Header("2. Formas grandes (relieve general)")]
    public float tamañoFormasGrandes = 330f;        //Metros: tamaño aproximado de una colina o valle grande
    [Range(0f, 0.5f)] public float intensidadFormasGrandes = 0.24f;
    [Range(1, 6)] public int octavasFormasGrandes = 4;
    public float tamañoDeformacion = 500f;          //Metros: tamaño de la "torsión" de las formas
    public float intensidadDeformacion = 150f;      //Metros: cuánto se desplaza el relieve como máximo al torcerlo

    [Header("3. Zonas de profundidad")]
    public float elevacionesPorKm2 = 7f;            //Cuántos montes hay por cada km² de mapa
    public float fosasPorKm2 = 7f;
    public float radioMinimoZona = 40f;             //Metros
    public float radioMaximoZona = 110f;            //Metros
    [Range(0f, 0.6f)] public float alturaElevaciones = 0.45f;
    [Range(0f, 0.6f)] public float profundidadFosas = 0.42f;
    [Range(1f, 5f)] public float estiramientoMaximo = 3f;       //1 = todas redondas; 3 = hasta 3 veces más largas que anchas

    [Header("4. Detalle fino")]
    public float tamañoDetalle = 22f;               //Metros: tamaño de las ondulaciones pequeñas
    [Range(0f, 0.1f)] public float intensidadDetalle = 0.012f;
    [Range(1, 6)] public int octavasDetalle = 3;

    [Header("Límites de altura")]
    [Range(0f, 1f)] public float alturaMinima = 0.02f;
    [Range(0f, 1f)] public float alturaMaxima = 0.98f;
    [Range(0.01f, 0.2f)] public float margenSuavizado = 0.06f;  //Cerca de los límites, la altura se "frena" suavemente en vez de cortarse

    //Datos de una elevación o una fosa
    private struct Zona
    {
        public Vector2 centro;      //Posición en el mapa, en metros
        public float radio;         //Metros
        public float estiramiento;  //1 = redonda; 3 = tres veces más larga que ancha
        public float coseno, seno;  //Dirección en la que está estirada (guardamos el coseno y el seno del ángulo para no recalcularlos)
        public float intensidad;    //Cuánto sube (elevación) o baja (fosa)
    }

    //Datos internos que dependen de la semilla
    private Vector2 desplazamientoGrande, desplazamientoDeformacionX, desplazamientoDeformacionY, desplazamientoDetalle;
    private Zona[] elevaciones;
    private Zona[] fosas;

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Calcula todas las alturas y las aplica al Terrain
    public void Generar()
    {
        Terrain terreno = GetComponent<Terrain>();
        TerrainData datos = terreno.terrainData;
        int resolucion = datos.heightmapResolution;
        Vector3 tamañoMapa = datos.size;

        PrepararSemilla(tamañoMapa);

        //SetHeights espera las alturas como [fila (z), columna (x)]
        float[,] alturas = new float[resolucion, resolucion];
        for (int z = 0; z < resolucion; z++)
        {
            for (int x = 0; x < resolucion; x++)
            {
                //Posición de este punto dentro del mapa, en metros
                float xMetros = (float)x / (resolucion - 1) * tamañoMapa.x;
                float zMetros = (float)z / (resolucion - 1) * tamañoMapa.z;
                alturas[z, x] = CalcularAltura(xMetros, zMetros);
            }
        }

        datos.SetHeights(0, 0, alturas);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Altura (de 0 a 1) en un punto del mapa (en metros desde la esquina del Terrain)
    private float CalcularAltura(float x, float z)
    {
        float altura = alturaBase;

        //2. Formas grandes, con deformación: movemos un poco el punto donde leemos el ruido según otro ruido
        float deformacionX = (Mathf.PerlinNoise(x / tamañoDeformacion + desplazamientoDeformacionX.x, z / tamañoDeformacion + desplazamientoDeformacionX.y) - 0.5f) * 2f;
        float deformacionZ = (Mathf.PerlinNoise(x / tamañoDeformacion + desplazamientoDeformacionY.x, z / tamañoDeformacion + desplazamientoDeformacionY.y) - 0.5f) * 2f;
        float xDeformado = x + deformacionX * intensidadDeformacion;
        float zDeformado = z + deformacionZ * intensidadDeformacion;
        altura += RuidoFractal(xDeformado / tamañoFormasGrandes, zDeformado / tamañoFormasGrandes, octavasFormasGrandes, desplazamientoGrande) * intensidadFormasGrandes;

        //3. Zonas de profundidad: sumamos las elevaciones y restamos las fosas
        //(con las coordenadas deformadas, para que sus bordes sean irregulares)
        for (int i = 0; i < elevaciones.Length; i++) altura += Campana(xDeformado, zDeformado, elevaciones[i]) * elevaciones[i].intensidad;
        for (int i = 0; i < fosas.Length; i++) altura -= Campana(xDeformado, zDeformado, fosas[i]) * fosas[i].intensidad;

        //4. Detalle fino
        altura += RuidoFractal(x / tamañoDetalle, z / tamañoDetalle, octavasDetalle, desplazamientoDetalle) * intensidadDetalle;

        return LimiteSuave(altura);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Mantiene la altura entre alturaMinima y alturaMaxima sin cortarla de golpe.
    //Un corte brusco (Mathf.Clamp) dejaría el fondo de las fosas plano y con un borde marcado.
    //Así, al acercarse al límite (a menos de margenSuavizado), la altura se va acercando cada vez más despacio,
    //como una curva que se aplana, y nunca llega a pasarse
    private float LimiteSuave(float altura)
    {
        float umbralAbajo = alturaMinima + margenSuavizado;
        float umbralArriba = alturaMaxima - margenSuavizado;

        if (altura < umbralAbajo)
        {
            //Cuanto más por debajo del umbral, más cerca de alturaMinima (pero sin llegar a pasarse)
            altura = alturaMinima + margenSuavizado * Mathf.Exp((altura - umbralAbajo) / margenSuavizado);
        }
        else if (altura > umbralArriba)
        {
            altura = alturaMaxima - margenSuavizado * Mathf.Exp((umbralArriba - altura) / margenSuavizado);
        }

        return altura;
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Ruido Perlin "fractal": varias capas (octavas) del mismo ruido, cada una el doble de detallada y la mitad de fuerte.
    //Devuelve un valor aproximadamente entre -1 y 1
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

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Campana de Gauss: vale 1 en el centro de la zona y baja suavemente hasta casi 0 a partir de su radio.
    //Por eso las elevaciones y fosas no tienen bordes bruscos.
    //Para que sea una elipse, giramos el punto según la dirección de la zona y "encogemos" un eje según su estiramiento
    private float Campana(float x, float z, Zona zona)
    {
        float dx = x - zona.centro.x;
        float dz = z - zona.centro.y;

        //Si el punto está muy lejos de la zona, la campana ya vale prácticamente 0: nos ahorramos el cálculo
        float alcance = zona.radio * zona.estiramiento * 2f;
        if (dx > alcance || dx < -alcance || dz > alcance || dz < -alcance) return 0f;

        //Giramos (dx, dz) para alinearlo con la dirección de la zona
        float aLoLargo = (zona.coseno * dx + zona.seno * dz) / zona.estiramiento; //Dividir hace que la zona sea más larga en este eje
        float aLoAncho = -zona.seno * dx + zona.coseno * dz;

        float distanciaCuadrado = aLoLargo * aLoLargo + aLoAncho * aLoAncho;
        float radio = zona.radio;
        return Mathf.Exp(-distanciaCuadrado / (2f * radio * radio * 0.25f));
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Con la misma semilla siempre sale el mismo mapa
    private void PrepararSemilla(Vector3 tamañoMapa)
    {
        System.Random aleatorio = new System.Random(semilla);

        //Desplazamientos grandes y distintos para cada ruido, así no se parecen entre ellos
        desplazamientoGrande = new Vector2(Aleatorio(aleatorio, 1000f, 9000f), Aleatorio(aleatorio, 1000f, 9000f));
        desplazamientoDeformacionX = new Vector2(Aleatorio(aleatorio, 1000f, 9000f), Aleatorio(aleatorio, 1000f, 9000f));
        desplazamientoDeformacionY = new Vector2(Aleatorio(aleatorio, 1000f, 9000f), Aleatorio(aleatorio, 1000f, 9000f));
        desplazamientoDetalle = new Vector2(Aleatorio(aleatorio, 1000f, 9000f), Aleatorio(aleatorio, 1000f, 9000f));

        //El número de zonas depende de la superficie del mapa: un mapa 4 veces más grande tiene 4 veces más montes
        float km2 = (tamañoMapa.x / 1000f) * (tamañoMapa.z / 1000f);
        elevaciones = CrearZonas(aleatorio, Mathf.RoundToInt(elevacionesPorKm2 * km2), alturaElevaciones, tamañoMapa);
        fosas = CrearZonas(aleatorio, Mathf.RoundToInt(fosasPorKm2 * km2), profundidadFosas, tamañoMapa);
    }

    /*--------------------------------------------------------------------------------------------------------------------------*/

    //Crea zonas al azar: centro (sin pegarse demasiado a los bordes), tamaño, forma, dirección e intensidad.
    //Cada zona tiene una intensidad un poco distinta (entre el 50% y el 100% de intensidadMaxima)
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

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private float Aleatorio(System.Random aleatorio, float minimo, float maximo)
    {
        return minimo + (float)aleatorio.NextDouble() * (maximo - minimo);
    }
}
