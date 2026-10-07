// Añade al Inspector del GeneradorRelieve los botones para generar el relieve con la semilla actual o con una nueva.
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GeneradorRelieve))]
public class GeneradorRelieveEditor : Editor
{
    // Dibuja los parámetros del generador y los botones de generar
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GeneradorRelieve generador = (GeneradorRelieve)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generar con esta semilla", GUILayout.Height(30)))
        {
            Generar(generador);
        }

        if (GUILayout.Button("Nueva semilla aleatoria y generar", GUILayout.Height(30)))
        {
            Undo.RecordObject(generador, "Nueva semilla");
            generador.semilla = Random.Range(0, 1000000);
            Generar(generador);
        }
    }

    // Genera el relieve permitiendo deshacerlo con Ctrl+Z y marca los datos para guardarlos
    private void Generar(GeneradorRelieve generador)
    {
        TerrainData datos = generador.GetComponent<Terrain>().terrainData;

        Undo.RegisterCompleteObjectUndo(datos, "Generar relieve");

        generador.Generar();

        EditorUtility.SetDirty(datos);
    }
}
