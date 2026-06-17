using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using DungeonRPG.Grid;

public class GridBuilderWindow : EditorWindow
{
    // ── Config ────────────────────────────────────────────────────────────────
    private GameObject gridSquarePrefab;
    private int   width      = 4;
    private int   height     = 4;
    private float cellSize    = 1f;
    private float cellSpacing = 0f;
    private Vector2 cellOffset = Vector2.zero;

    // ── Cell painter (row-major [y, x]) ──────────────────────────────────────
    private CellState[,] cellStates = new CellState[4, 4];

    // ── UI ───────────────────────────────────────────────────────────────────
    private Vector2 scroll;
    private const float BtnSize = 40f;

    [MenuItem("DungeonRPG/Grid Builder")]
    public static void Open() => GetWindow<GridBuilderWindow>("Grid Builder");

    // ─────────────────────────────────────────────────────────────────────────

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        Section("Grid Settings");
        DrawSettings();

        EditorGUILayout.Space(8);
        Section("Cell State Painter");
        DrawPainter();

        EditorGUILayout.Space(12);
        DrawActions();

        EditorGUILayout.EndScrollView();
    }

    // ── Sections ─────────────────────────────────────────────────────────────

    void DrawSettings()
    {
        gridSquarePrefab = (GameObject)EditorGUILayout.ObjectField(
            "GridSquare Prefab", gridSquarePrefab, typeof(GameObject), false);

        EditorGUI.BeginChangeCheck();
        int  newW = Mathf.Clamp(EditorGUILayout.IntField("Width",   width),   1, 32);
        int  newH = Mathf.Clamp(EditorGUILayout.IntField("Height",  height),  1, 32);
        if (EditorGUI.EndChangeCheck() && (newW != width || newH != height))
        {
            ResizePainter(newW, newH);
            width  = newW;
            height = newH;
        }

        cellSize    = Mathf.Max(0.01f, EditorGUILayout.FloatField("Cell Size",    cellSize));
        cellSpacing = Mathf.Max(-5f,    EditorGUILayout.FloatField("Cell Spacing", cellSpacing));
        cellOffset  = EditorGUILayout.Vector2Field("Cell Offset", cellOffset);
    }

    void DrawPainter()
    {
        EditorGUILayout.HelpBox("Click a cell to toggle its state.   Gray = Free   |   Red = Occupied", MessageType.None);
        EditorGUILayout.Space(4);

        // rows: y from top to bottom
        for (int y = height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Y:{y}", GUILayout.Width(30));

            for (int x = 0; x < width; x++)
            {
                bool occ = cellStates[y, x] == CellState.Occupied;
                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = occ ? new Color(1f, 0.35f, 0.35f) : new Color(0.72f, 0.72f, 0.72f);

                if (GUILayout.Button(occ ? "OCC" : "---", GUILayout.Width(BtnSize), GUILayout.Height(BtnSize)))
                    cellStates[y, x] = occ ? CellState.Free : CellState.Occupied;

                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
        }

        // X-axis labels
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(34);
        for (int x = 0; x < width; x++)
            EditorGUILayout.LabelField($"X:{x}", GUILayout.Width(BtnSize));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All", GUILayout.Width(90))) cellStates = new CellState[height, width];
        if (GUILayout.Button("Fill All",  GUILayout.Width(90))) FillAll(CellState.Occupied);
        EditorGUILayout.EndHorizontal();
    }

    void DrawActions()
    {
        bool ready = gridSquarePrefab != null;
        if (!ready)
            EditorGUILayout.HelpBox("Assign a GridSquare Prefab to enable building.", MessageType.Warning);

        EditorGUI.BeginDisabledGroup(!ready);

        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.35f, 0.8f, 0.35f);
        if (GUILayout.Button("Build Grid", GUILayout.Height(38)))
            BuildGrid();
        GUI.backgroundColor = prev;

        if (GUILayout.Button("Clear Scene Grid", GUILayout.Height(28)))
            ClearSceneGrid();

        EditorGUI.EndDisabledGroup();
    }

    // ── Grid operations ───────────────────────────────────────────────────────

    void BuildGrid()
    {
        ClearSceneGrid();

        var root = new GameObject("Grid");
        Undo.RegisterCreatedObjectUndo(root, "Build Grid");

        float step = cellSize + cellSpacing;
        Vector3 baseOffset = new Vector3(-(width  - 1) * step * 0.5f + cellOffset.x, -(height - 1) * step * 0.5f + cellOffset.y, 0f);

        int id = 1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = baseOffset + new Vector3(x * step, y * step, 0f);
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(gridSquarePrefab);
                Undo.RegisterCreatedObjectUndo(go, "Build Grid Cell");
                go.transform.SetParent(root.transform, false);
                go.transform.localPosition = pos;
                GridSquare sq = go.GetComponent<GridSquare>();
                if (sq == null) sq = go.AddComponent<GridSquare>();
                sq.Init(id++, x, y, cellStates[y, x]);
            }
        }

        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log($"[GridBuilder] Built {width}×{height} grid ({width * height} cells).");
    }

    void ClearSceneGrid()
    {
        var existing = GameObject.Find("Grid");
        if (existing == null) return;
        Undo.DestroyObjectImmediate(existing);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void ResizePainter(int newW, int newH)
    {
        var next = new CellState[newH, newW];
        for (int y = 0; y < Mathf.Min(height, newH); y++)
            for (int x = 0; x < Mathf.Min(width,  newW); x++)
                next[y, x] = cellStates[y, x];
        cellStates = next;
    }

    void FillAll(CellState state)
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width;  x++)
                cellStates[y, x] = state;
    }

    static void Section(string label)
    {
        EditorGUILayout.LabelField(label, new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 });
        var r = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(r, new Color(0.4f, 0.4f, 0.4f));
        EditorGUILayout.Space(2);
    }
}
