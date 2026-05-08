using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnomalyController))]
public class AnomalyControllerEditor : Editor
{
    private SerializedProperty anomalyTypeProp;
    private SerializedProperty transitionSpeedProp;
    private SerializedProperty targetColorProp;
    private SerializedProperty scaleMultiplierProp;
    private SerializedProperty targetShadowModeProp;
    private SerializedProperty illusionJitterIntensityProp;
    private SerializedProperty illusionRotationSpeedProp;
    private SerializedProperty peripheralThresholdProp;
    private SerializedProperty onReportSoundProp;
    private SerializedProperty intervalSoundProp;
    private SerializedProperty delayBetweenSoundsProp;

    private void OnEnable()
    {
        anomalyTypeProp = serializedObject.FindProperty("anomalyType");
        transitionSpeedProp = serializedObject.FindProperty("transitionSpeed");
        targetColorProp = serializedObject.FindProperty("targetColor");
        scaleMultiplierProp = serializedObject.FindProperty("scaleMultiplier");
        targetShadowModeProp = serializedObject.FindProperty("targetShadowMode");
        illusionJitterIntensityProp = serializedObject.FindProperty("illusionJitterIntensity");
        illusionRotationSpeedProp = serializedObject.FindProperty("illusionRotationSpeed");
        peripheralThresholdProp = serializedObject.FindProperty("peripheralThreshold");
        onReportSoundProp = serializedObject.FindProperty("onReportSound");
        intervalSoundProp = serializedObject.FindProperty("intervalSound");
        delayBetweenSoundsProp = serializedObject.FindProperty("delayBetweenSounds");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Type
        EditorGUILayout.PropertyField(anomalyTypeProp);
        AnomalyController.AnomalyType type = (AnomalyController.AnomalyType)anomalyTypeProp.enumValueIndex;

        EditorGUILayout.Space();

        // Audio je pro všechny stejné
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Audio Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(onReportSoundProp);
        EditorGUILayout.PropertyField(intervalSoundProp);
        EditorGUILayout.PropertyField(delayBetweenSoundsProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        // Vždy chceme Transition Speed u těch co se mění plynule
        if (type == AnomalyController.AnomalyType.ColorChange ||
            type == AnomalyController.AnomalyType.ScaleChange ||
            type == AnomalyController.AnomalyType.LightColorChange)
        {
            EditorGUILayout.PropertyField(transitionSpeedProp);
        }

        switch (type)
        {
            case AnomalyController.AnomalyType.ColorChange:
            case AnomalyController.AnomalyType.LightColorChange:
                EditorGUILayout.PropertyField(targetColorProp);
                break;

            case AnomalyController.AnomalyType.ScaleChange:
                EditorGUILayout.PropertyField(scaleMultiplierProp);
                break;

            case AnomalyController.AnomalyType.VisualIllusion:
                EditorGUILayout.PropertyField(illusionJitterIntensityProp);
                EditorGUILayout.PropertyField(illusionRotationSpeedProp);
                EditorGUILayout.PropertyField(peripheralThresholdProp);
                break;

            case AnomalyController.AnomalyType.ShadowChange:
            case AnomalyController.AnomalyType.MissingObject:
            case AnomalyController.AnomalyType.AddedObject:
                EditorGUILayout.HelpBox("Tento typ anomálie nevyžaduje žádné dodatečné nastavení.", MessageType.Info);
                break;
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug & Testing", EditorStyles.boldLabel);
        
        AnomalyController controller = (AnomalyController)target;
        
        GUI.enabled = Application.isPlaying; // Tlačítka budou fungovat jen v Play módu
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Test Activate"))
        {
            controller.Activate();
        }
        if (GUILayout.Button("Test Reset"))
        {
            controller.ResetAnomaly();
        }
        EditorGUILayout.EndHorizontal();
        GUI.enabled = true;
    }
}
