using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CharacterHealthBarSetup
{
    [MenuItem("Tools/Setup Health Bars for All Characters")]
    public static void SetupAll()
    {
        var characters = Object.FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);

        if (characters.Length == 0)
        {
            Debug.LogWarning("[CharacterHealthBarSetup] No GameObjects with CharacterStats found in the scene.");
            return;
        }

        foreach (var stats in characters)
            SetupHealthBar(stats.gameObject);

        Debug.Log($"[CharacterHealthBarSetup] Health bars set up for {characters.Length} characterRequisitor(s).");
    }

    private static void SetupHealthBar(GameObject character)
    {
        // Remove any existing HealthBarCanvas so we don't duplicate
        var existing = character.transform.Find("HealthBarCanvas");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        // --- Canvas ---
        var canvasGO = new GameObject("HealthBarCanvas");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create HealthBarCanvas");
        canvasGO.transform.SetParent(character.transform, false);
        canvasGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        var rectCanvas = canvasGO.GetComponent<RectTransform>();
        rectCanvas.sizeDelta = new Vector2(200f, 40f);

        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // --- Background panel ---
        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRect = bgGO.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var bgImage = bgGO.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        // --- Health text ---
        var textGO = new GameObject("HealthText");
        textGO.transform.SetParent(canvasGO.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "100 / 100";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 18f;
        tmp.color = Color.white;

        // --- HealthBar component on the Canvas root ---
        var healthBar = canvasGO.AddComponent<HealthBar>();
        // Wire the text reference via SerializedObject so Undo/prefab diffs work
        var so = new SerializedObject(healthBar);
        so.FindProperty("healthText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(character);
        Debug.Log($"[CharacterHealthBarSetup] Created HealthBarCanvas on '{character.name}'.");
    }
}
