using UnityEditor;
using UnityEngine;

[CustomEditor((typeof(GenerationController)))]
public class GenerationControllerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        GenerationController generationController = (GenerationController)target;

        if (GUILayout.Button("GENERATE")) {
            generationController.Generate();
        }

        if (GUILayout.Button("CLEAR")) {
            generationController.Clear();
        }
    }
}