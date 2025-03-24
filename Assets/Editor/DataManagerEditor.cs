using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataManager))]
public class DataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw the normal inspector layout

        DataManager dataManager = (DataManager)target;

        // Add a space before the buttons
        GUILayout.Space(10);

        // Save Button
        if (GUILayout.Button("Test Save Data"))
        {
            Debug.Log("Test Save Data");
            dataManager.Save();
        }

        // Load Button
        if (GUILayout.Button("Test Load Data"))
        {
            Debug.Log("Test Load Data");
            dataManager.Load();
        }
    }
}
