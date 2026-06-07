using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AnomalyManager))]
public class AnomalyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox("Use the 'Add Anomaly...' buttons below. They will only show anomalies that haven't been assigned yet.", MessageType.Info);

        DrawListWithCustomAdd("day1Anomalies", "Day 1 Anomalies");
        DrawListWithCustomAdd("day2Anomalies", "Day 2 Anomalies");
        DrawListWithCustomAdd("day3To5Anomalies", "Days 3-5 Anomalies (Shared)");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Delay Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("minDelay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDelay"));

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawListWithCustomAdd(string propertyName, string label)
    {
        SerializedProperty listProp = serializedObject.FindProperty(propertyName);
        
        EditorGUILayout.LabelField($"{label} ({listProp.arraySize})", EditorStyles.boldLabel);
        
        // Background for the list
        EditorGUILayout.BeginVertical("box");
        
        if (listProp.arraySize == 0)
        {
            EditorGUILayout.LabelField("Empty", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            for (int i = 0; i < listProp.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                SerializedProperty element = listProp.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(element, GUIContent.none);
                
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    if (element.objectReferenceValue != null)
                    {
                        element.objectReferenceValue = null;
                    }
                    listProp.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndHorizontal();
                    break; // Break to prevent GUI layout errors when array changes
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Add Anomaly...", GUILayout.Height(24)))
        {
            ShowAddMenu(listProp);
        }
        EditorGUILayout.Space();
    }

    private void ShowAddMenu(SerializedProperty listProp)
    {
        GenericMenu menu = new GenericMenu();
        AnomalyManager manager = (AnomalyManager)target;

        // Get all anomalies in the scene
        AnomalyController[] allAnomalies = Object.FindObjectsByType<AnomalyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        // Collect all already assigned anomalies across all days
        HashSet<AnomalyController> assigned = new HashSet<AnomalyController>();
        if (manager.day1Anomalies != null) assigned.UnionWith(manager.day1Anomalies);
        if (manager.day2Anomalies != null) assigned.UnionWith(manager.day2Anomalies);
        if (manager.day3To5Anomalies != null) assigned.UnionWith(manager.day3To5Anomalies);

        int addedCount = 0;
        foreach (var anomaly in allAnomalies)
        {
            if (anomaly != null && !assigned.Contains(anomaly))
            {
                // We use the GameObject name for the menu
                string menuName = anomaly.gameObject.name;
                
                // If there are duplicate names, Unity's GenericMenu handles it, but adding a suffix might be safer
                // However, let's keep it simple.
                menu.AddItem(new GUIContent(menuName), false, () => 
                {
                    serializedObject.Update();
                    int index = listProp.arraySize;
                    listProp.InsertArrayElementAtIndex(index);
                    listProp.GetArrayElementAtIndex(index).objectReferenceValue = anomaly;
                    serializedObject.ApplyModifiedProperties();
                });
                addedCount++;
            }
        }

        if (addedCount == 0)
        {
            menu.AddDisabledItem(new GUIContent("No available anomalies (All assigned)"));
        }

        menu.ShowAsContext();
    }
}
