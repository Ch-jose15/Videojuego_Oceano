// Herramienta de editor que monta en la escena abierta el sistema de plazos (gestor, punto de cobro, texto del HUD y recinto de prueba) y guarda sus prefabs.
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class ConstructorPlazosEditor
{
    private const string carpetaPrefabs = "Assets/Prefabs";
    private const string carpetaMateriales = "Assets/Materiales";

    // Monta todo el sistema de plazos en la escena abierta tras pedir confirmación
    [MenuItem("Herramientas/Plazos/Montar sistema de plazos en la escena abierta")]
    public static void MontarSistemaPlazos()
    {
        Montar(true);
    }

    // Monta el sistema de plazos; con preguntar a false no abre ventanas y avisa por consola (para automatizarlo)
    public static string Montar(bool preguntar)
    {
        var escena = EditorSceneManager.GetActiveScene();

        CuboCompra cuboCompra = Object.FindFirstObjectByType<CuboCompra>(FindObjectsInactive.Include);
        CuboVenta cuboVenta = Object.FindFirstObjectByType<CuboVenta>(FindObjectsInactive.Include);
        UIDinero uiDinero = Object.FindFirstObjectByType<UIDinero>(FindObjectsInactive.Include);

        if (cuboCompra == null || cuboVenta == null || uiDinero == null || uiDinero.textoDinero == null || cuboCompra.panelComprar == null || cuboCompra.avisoComprar == null)
            return Avisar(preguntar, "En la escena '" + escena.name + "' falta CuboCompra (con su panel y aviso), CuboVenta o el HUD con UIDinero. No se ha creado nada.");

        if (Object.FindFirstObjectByType<PuntoCobro>(FindObjectsInactive.Include) != null)
            return Avisar(preguntar, "La escena ya tiene un PuntoCobro. No se ha creado nada para no duplicarlo.");

        if (preguntar && !EditorUtility.DisplayDialog("Sistema de plazos", "Se va a montar el sistema de plazos en la escena '" + escena.name + "' y se guardará la escena. ¿Seguir?", "Sí", "Cancelar"))
            return "Cancelado";

        Material materialCobro = CrearMaterial(cuboCompra, "PuntoCobro", new Color(0.45f, 0.04f, 0.04f));
        Material materialRocas = CrearMaterial(cuboCompra, "RocasBarrera", new Color(0.36f, 0.36f, 0.38f));

        CrearGestorPlazos();

        Vector3 posVenta = cuboVenta.transform.position;
        Vector3 posCompra = cuboCompra.transform.position;
        Vector3 direccionFila = posCompra - posVenta;
        direccionFila.y = 0f;
        float separacion = direccionFila.magnitude;
        direccionFila.Normalize();

        float alturaSobreSuelo = posCompra.y - AlturaSuelo(posCompra);
        Vector3 posCobro = posCompra + direccionFila * separacion;
        posCobro.y = AlturaSuelo(posCobro) + alturaSobreSuelo;

        CrearPuntoCobro(cuboCompra, cuboVenta, materialCobro, posCobro);
        CrearTextoPlazo(uiDinero);

        GameObject prefabBarrera = CrearPrefabBarrera(materialRocas);
        CrearRecintoPrueba(prefabBarrera, posVenta, posCompra, direccionFila);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        AssetDatabase.SaveAssets();

        Debug.Log("Sistema de plazos montado en la escena " + escena.name);
        return "Montado en " + escena.name;
    }

    // Muestra un aviso en una ventana o en la consola y lo devuelve
    private static string Avisar(bool preguntar, string mensaje)
    {
        if (preguntar) EditorUtility.DisplayDialog("Sistema de plazos", mensaje, "Vale");
        else Debug.LogWarning(mensaje);
        return mensaje;
    }

    // Crea (o reutiliza) un material copiando el del CuboCompra con otro color
    private static Material CrearMaterial(CuboCompra cuboCompra, string nombre, Color color)
    {
        string ruta = carpetaMateriales + "/" + nombre + ".mat";
        Material existente = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (existente != null) return existente;

        Renderer rendererCompra = cuboCompra.GetComponent<Renderer>();
        Material material = new Material(rendererCompra.sharedMaterial);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);

        AssetDatabase.CreateAsset(material, ruta);
        return material;
    }

    // Crea el objeto GestorPlazos y lo guarda como prefab
    private static void CrearGestorPlazos()
    {
        if (Object.FindFirstObjectByType<GestorPlazos>(FindObjectsInactive.Include) != null) return;

        GameObject gestor = new GameObject("GestorPlazos");
        gestor.AddComponent<GestorPlazos>();
        GuardarPrefab(gestor, "GestorPlazos");
    }

    // Crea el PuntoCobro copiando el aspecto del CuboCompra y con su propio canvas para el aviso, el panel y el texto "-X$"
    private static void CrearPuntoCobro(CuboCompra cuboCompra, CuboVenta cuboVenta, Material material, Vector3 posicion)
    {
        GameObject raiz = new GameObject("PuntoCobro");
        raiz.transform.position = posicion;
        raiz.transform.rotation = cuboCompra.transform.rotation;
        PuntoCobro puntoCobro = raiz.AddComponent<PuntoCobro>();

        GameObject cubo = Object.Instantiate(cuboCompra.gameObject, raiz.transform);
        cubo.name = "Cubo";
        cubo.transform.localPosition = Vector3.zero;
        cubo.transform.localRotation = Quaternion.identity;
        Object.DestroyImmediate(cubo.GetComponent<CuboCompra>());
        cubo.GetComponent<Renderer>().sharedMaterial = material;

        foreach (TMP_Text etiqueta in cubo.GetComponentsInChildren<TMP_Text>(true)) etiqueta.text = "COBRO";

        foreach (ZonaJugador zona in cubo.GetComponentsInChildren<ZonaJugador>(true))
        {
            VaciarEvento(zona.alEntrarJugador);
            VaciarEvento(zona.alSalirJugador);
            UnityEventTools.AddVoidPersistentListener(zona.alEntrarJugador, new UnityAction(puntoCobro.JugadorEntraEnZona));
            UnityEventTools.AddVoidPersistentListener(zona.alSalirJugador, new UnityAction(puntoCobro.JugadorSaleDeZona));
        }

        Canvas canvasHud = cuboCompra.panelComprar.GetComponentInParent<Canvas>(true).rootCanvas;
        GameObject canvasCobro = new GameObject("CanvasCobro", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasCobro.transform.SetParent(raiz.transform, false);
        Canvas canvas = canvasCobro.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = canvasHud.sortingOrder + 1;
        EditorUtility.CopySerialized(canvasHud.GetComponent<CanvasScaler>(), canvasCobro.GetComponent<CanvasScaler>());

        GameObject aviso = Object.Instantiate(cuboCompra.avisoComprar, canvasCobro.transform, false);
        aviso.name = "AvisoCobro";
        aviso.SetActive(false);
        foreach (TMP_Text texto in aviso.GetComponentsInChildren<TMP_Text>(true)) texto.text = "<b>Clic</b> para pagar plazo";

        GameObject panel = Object.Instantiate(cuboCompra.panelComprar, canvasCobro.transform, false);
        panel.name = "PanelCobro";
        panel.SetActive(false);

        Button botonPagar = null;
        Button botonCerrar = null;
        foreach (Button boton in panel.GetComponentsInChildren<Button>(true))
        {
            VaciarEvento(boton.onClick);

            if (cuboCompra.botonCerrar != null && boton.name == cuboCompra.botonCerrar.name) botonCerrar = boton;
            else if (botonPagar == null) botonPagar = boton;
            else Object.DestroyImmediate(boton.gameObject);
        }

        TextMeshProUGUI titulo = null;
        foreach (TextMeshProUGUI texto in panel.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (texto.GetComponentInParent<Button>(true) == null) { titulo = texto; break; }
        }

        if (botonPagar != null)
        {
            botonPagar.name = "BotonPagar";
            ((RectTransform)botonPagar.transform).anchoredPosition = new Vector2(0f, -125f);
            PonerTextoBoton(botonPagar, "Pagar");
        }

        if (botonCerrar != null) PonerTextoBoton(botonCerrar, "Cerrar");

        TextMeshProUGUI textoInfo = null;
        if (titulo != null)
        {
            titulo.text = "<b><size=48>PLAZO</size></b>";

            GameObject info = Object.Instantiate(titulo.gameObject, panel.transform, false);
            info.name = "TextoInfo";
            RectTransform rectInfo = (RectTransform)info.transform;
            rectInfo.anchorMin = new Vector2(0.5f, 0.5f);
            rectInfo.anchorMax = new Vector2(0.5f, 0.5f);
            rectInfo.pivot = new Vector2(0.5f, 0.5f);
            rectInfo.anchoredPosition = new Vector2(0f, 45f);
            rectInfo.sizeDelta = new Vector2(480f, 230f);

            textoInfo = info.GetComponent<TextMeshProUGUI>();
            textoInfo.enableAutoSizing = false;
            textoInfo.fontSize = 34f;
            textoInfo.alignment = TextAlignmentOptions.Center;
            textoInfo.text = "Importe: 100$\nTu dinero: 0$\nTe faltan 100$";
        }

        TextoFlotante plantilla = null;
        if (cuboVenta.plantillaTextoVenta != null)
        {
            GameObject copia = Object.Instantiate(cuboVenta.plantillaTextoVenta.gameObject, canvasCobro.transform, false);
            copia.name = "TextoCobroPlantilla";
            ((RectTransform)copia.transform).anchoredPosition = new Vector2(0f, 330f);
            copia.SetActive(false);
            TextMeshProUGUI textoCopia = copia.GetComponent<TextMeshProUGUI>();
            if (textoCopia != null)
            {
                textoCopia.text = "-0$";
                textoCopia.color = new Color(1f, 0.4f, 0.35f);
            }
            plantilla = copia.GetComponent<TextoFlotante>();
        }

        puntoCobro.rendererCubo = cubo.GetComponent<Renderer>();
        puntoCobro.avisoCobro = aviso;
        puntoCobro.panelCobro = panel;
        puntoCobro.textoTitulo = titulo;
        puntoCobro.textoInfo = textoInfo;
        puntoCobro.botonPagar = botonPagar;
        puntoCobro.botonCerrar = botonCerrar;
        puntoCobro.plantillaTextoCobro = plantilla;

        GuardarPrefab(raiz, "PuntoCobro");
    }

    // Crea el texto del plazo en el HUD, debajo de las barras, y lo guarda como prefab
    private static void CrearTextoPlazo(UIDinero uiDinero)
    {
        GameObject texto = Object.Instantiate(uiDinero.textoDinero.gameObject, uiDinero.textoDinero.transform.parent, false);
        texto.name = "TextoPlazo";
        foreach (UIDinero sobrante in texto.GetComponents<UIDinero>()) Object.DestroyImmediate(sobrante);

        RectTransform rect = (RectTransform)texto.transform;
        rect.anchoredPosition = new Vector2(30f, -180f);
        rect.sizeDelta = new Vector2(800f, 40f);

        TextMeshProUGUI tmp = texto.GetComponent<TextMeshProUGUI>();
        tmp.fontSize = 28f;
        tmp.text = "Plazo 1: 100$ (faltan 100$)";

        UIPlazo uiPlazo = texto.AddComponent<UIPlazo>();
        uiPlazo.textoPlazo = tmp;

        GuardarPrefab(texto, "TextoPlazo");
    }

    // Crea el prefab de barrera: un muro de rocas grises de 8 x 6 metros con un collider que lo cierra entero
    private static GameObject CrearPrefabBarrera(Material material)
    {
        string ruta = carpetaPrefabs + "/BarreraRocas.prefab";
        GameObject existente = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
        if (existente != null) return existente;

        GameObject raiz = new GameObject("BarreraRocas");
        raiz.AddComponent<BarreraZona>().capituloNecesario = 2;

        BoxCollider bloqueo = raiz.AddComponent<BoxCollider>();
        bloqueo.center = new Vector3(0f, 3f, 0f);
        bloqueo.size = new Vector3(8f, 6f, 1.6f);

        System.Random azar = new System.Random(7);
        int numeroRoca = 0;
        for (int fila = 0; fila < 3; fila++)
        {
            for (int columna = 0; columna < 4; columna++)
            {
                PrimitiveType tipo = (fila + columna) % 3 == 0 ? PrimitiveType.Cube : PrimitiveType.Sphere;
                GameObject roca = GameObject.CreatePrimitive(tipo);
                roca.name = "Roca" + (++numeroRoca);
                roca.transform.SetParent(raiz.transform, false);

                float x = -3f + columna * 2f + Rango(azar, -0.3f, 0.3f);
                float y = 1f + fila * 2f + Rango(azar, -0.2f, 0.2f);
                float z = Rango(azar, -0.25f, 0.25f);
                roca.transform.localPosition = new Vector3(x, y, z);
                roca.transform.localRotation = Quaternion.Euler(Rango(azar, 0f, 360f), Rango(azar, 0f, 360f), Rango(azar, 0f, 360f));

                float tamano = tipo == PrimitiveType.Cube ? 2f : 2.6f;
                roca.transform.localScale = new Vector3(tamano * Rango(azar, 0.9f, 1.2f), tamano * Rango(azar, 0.8f, 1.1f), tamano * Rango(azar, 0.7f, 0.9f));
                roca.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, ruta);
        Object.DestroyImmediate(raiz);
        return prefab;
    }

    // Coloca un recinto cerrado de barreras (capítulo 2) con un objeto vendible dentro, al lado de la base y alejado del jugador
    private static void CrearRecintoPrueba(GameObject prefabBarrera, Vector3 posVenta, Vector3 posCompra, Vector3 direccionFila)
    {
        Vector3 lateral = Vector3.Cross(Vector3.up, direccionFila).normalized;
        Vector3 medio = (posVenta + posCompra) * 0.5f;

        InteracciónAgarrar jugador = Object.FindFirstObjectByType<InteracciónAgarrar>(FindObjectsInactive.Include);
        if (jugador != null && Vector3.Dot(jugador.transform.position - medio, lateral) > 0f) lateral = -lateral;

        Vector3 centro = medio + lateral * 14f;
        float suelo = AlturaSuelo(centro);
        float baseMuros = suelo - 1.5f;
        Quaternion giro = Quaternion.LookRotation(direccionFila, Vector3.up);

        GameObject recinto = new GameObject("RecintoPrueba_Capitulo2");
        recinto.transform.SetPositionAndRotation(new Vector3(centro.x, baseMuros, centro.z), giro);

        ColocarBarrera(prefabBarrera, recinto.transform, "Muro_Delante", new Vector3(0f, 0f, 4f), Quaternion.identity);
        ColocarBarrera(prefabBarrera, recinto.transform, "Muro_Detras", new Vector3(0f, 0f, -4f), Quaternion.identity);
        ColocarBarrera(prefabBarrera, recinto.transform, "Muro_Derecha", new Vector3(4f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
        ColocarBarrera(prefabBarrera, recinto.transform, "Muro_Izquierda", new Vector3(-4f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
        ColocarBarrera(prefabBarrera, recinto.transform, "Techo_1", new Vector3(0f, 6.8f, -6f), Quaternion.Euler(90f, 0f, 0f));
        ColocarBarrera(prefabBarrera, recinto.transform, "Techo_2", new Vector3(0f, 6.8f, 0f), Quaternion.Euler(90f, 0f, 0f));

        GameObject prefabItem = AssetDatabase.LoadAssetAtPath<GameObject>(carpetaPrefabs + "/CuboItem.prefab");
        if (prefabItem != null)
        {
            GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(prefabItem);
            item.name = "ItemDentroRecinto";
            item.transform.position = new Vector3(centro.x, suelo + 1f, centro.z);
        }
    }

    // Instancia una barrera del prefab dentro del recinto con capítulo necesario 2
    private static void ColocarBarrera(GameObject prefab, Transform padre, string nombre, Vector3 posicionLocal, Quaternion rotacionLocal)
    {
        GameObject barrera = (GameObject)PrefabUtility.InstantiatePrefab(prefab, padre);
        barrera.name = nombre;
        barrera.transform.localPosition = posicionLocal;
        barrera.transform.localRotation = rotacionLocal;
        barrera.GetComponent<BarreraZona>().capituloNecesario = 2;
    }

    // Devuelve la altura del terreno bajo un punto, o la del propio punto si no hay terreno
    private static float AlturaSuelo(Vector3 punto)
    {
        Terrain terreno = Terrain.activeTerrain;
        if (terreno == null) return punto.y;
        return terreno.SampleHeight(punto) + terreno.transform.position.y;
    }

    // Guarda un objeto de la escena como prefab y lo deja conectado a él
    private static void GuardarPrefab(GameObject objeto, string nombre)
    {
        PrefabUtility.SaveAsPrefabAssetAndConnect(objeto, carpetaPrefabs + "/" + nombre + ".prefab", InteractionMode.AutomatedAction);
    }

    // Quita todas las llamadas guardadas de un evento
    private static void VaciarEvento(UnityEventBase evento)
    {
        for (int i = evento.GetPersistentEventCount() - 1; i >= 0; i--) UnityEventTools.RemovePersistentListener(evento, i);
    }

    // Cambia el texto visible de un botón
    private static void PonerTextoBoton(Button boton, string texto)
    {
        TMP_Text tmp = boton.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) tmp.text = texto;
    }

    // Devuelve un número aleatorio entre dos valores
    private static float Rango(System.Random azar, float minimo, float maximo)
    {
        return minimo + (float)azar.NextDouble() * (maximo - minimo);
    }
}
