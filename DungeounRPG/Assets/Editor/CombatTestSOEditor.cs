using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CombatTestSO))]
public class CombatTestSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var so = (CombatTestSO)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("action"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("amount"));
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Combat Test", EditorStyles.boldLabel);

        // ── Target dropdown ───────────────────────────────────────────────────
        var targets = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.InstanceID)
                            .OfType<ITarget>()
                            .ToArray();

        if (targets.Length == 0)
        {
            EditorGUILayout.HelpBox("No ITarget objects found in the scene.", MessageType.Warning);
        }
        else
        {
            var names    = targets.Select(t => t.TargetName).ToArray();
            int curIndex = Mathf.Max(System.Array.IndexOf(names, so.targetName), 0);
            int newIndex = EditorGUILayout.Popup("Target", curIndex, names);

            if (newIndex != curIndex || string.IsNullOrEmpty(so.targetName))
            {
                Undo.RecordObject(so, "Select CombatTest Target");
                so.targetName = names[newIndex];
                EditorUtility.SetDirty(so);
            }
        }

        EditorGUILayout.Space(6);

        // ── Action buttons ────────────────────────────────────────────────────
        var label = so.action == CombatTestAction.Damage
            ? $"Deal {so.amount} Damage"
            : $"Heal {so.amount} HP";

        GUI.backgroundColor = so.action == CombatTestAction.Damage
            ? new Color(1f, 0.4f, 0.4f)
            : new Color(0.4f, 1f, 0.6f);

        if (GUILayout.Button(label, GUILayout.Height(32)))
            so.ExecuteAction();

        GUI.backgroundColor = Color.white;
        EditorGUILayout.Space(4);

        GUI.backgroundColor = new Color(0.6f, 0.8f, 1f);
        if (GUILayout.Button("End Player Turn", GUILayout.Height(28)))
            so.EndPlayerTurn();
        GUI.backgroundColor = Color.white;
    }
}
