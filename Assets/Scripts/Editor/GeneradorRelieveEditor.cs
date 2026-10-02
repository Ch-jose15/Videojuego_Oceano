using UnityEditor;
using UnityEngine;

//Añade los botones de "Generar" al Inspector del GeneradorRelieve.
//Tiene que estar en una carpeta llamada "Editor": así Unity sabe que solo se usa en el editor y no va en el juego final
[CustomEditor(typeof(GeneradorRelieve))]
public class GeneradorRelieveEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //Dibuja todos los parámetros normales
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

    /*--------------------------------------------------------------------------------------------------------------------------*/

    private void Generar(GeneradorRelieve generador)
    {
        TerrainData datos = generador.GetComponent<Terrain>().terrainData;

        //Para poder deshacer con Ctrl+Z
        Undo.RegisterCompleteObjectUndo(datos, "Generar relieve");

        generador.Generar();

        //Marcamos los datos como modificados para que se guarden
        EditorUtility.SetDirty(datos);
    }
}
