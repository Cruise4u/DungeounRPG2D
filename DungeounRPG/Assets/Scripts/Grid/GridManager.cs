using System.Collections.Generic;
using UnityEngine;

namespace DungeonRPG.Grid
{
    /// <summary>
    /// Runtime manager. Auto-discovers GridSquare children on Awake and exposes
    /// query / mutation operations. Does NOT build the grid — use the Grid Builder
    /// editor window for that.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        public static GridManager Instance { get; private set; }

        // ── Map ───────────────────────────────────────────────────────────────
        private Dictionary<Vector2Int, GridSquare> coordMap = new();
        private Dictionary<int, GridSquare>        idMap    = new();

        public IReadOnlyDictionary<Vector2Int, GridSquare> CoordMap => coordMap;
        public IReadOnlyDictionary<int, GridSquare>        IdMap    => idMap;

        public int Width  { get; private set; }
        public int Height { get; private set; }

        // ─────────────────────────────────────────────────────────────────────

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            BuildMap();
        }

        // ── Map building ──────────────────────────────────────────────────────

        /// <summary>
        /// Scans all GridSquare children and populates the lookup maps.
        /// Call this again if the grid is rebuilt at runtime.
        /// </summary>
        public void BuildMap()
        {
            coordMap.Clear();
            idMap.Clear();

            int maxX = 0, maxY = 0;

            foreach (var sq in GetComponentsInChildren<GridSquare>())
            {
                var key = new Vector2Int(sq.X, sq.Y);
                coordMap[key] = sq;
                idMap[sq.TileID] = sq;

                if (sq.X > maxX) maxX = sq.X;
                if (sq.Y > maxY) maxY = sq.Y;
            }

            Width  = maxX + 1;
            Height = maxY + 1;

            Debug.Log($"[GridManager] Map built — {coordMap.Count} tiles ({Width}×{Height}).");
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public GridSquare GetTile(int x, int y)          => coordMap.GetValueOrDefault(new Vector2Int(x, y));
        public GridSquare GetTile(Vector2Int coord)       => coordMap.GetValueOrDefault(coord);
        public GridSquare GetTileByID(int id)             => idMap.GetValueOrDefault(id);

        public bool IsInBounds(int x, int y)             => x >= 0 && x < Width && y >= 0 && y < Height;
        public bool IsFree(int x, int y)                  => GetTile(x, y)?.State == CellState.Free;
        public bool IsOccupied(int x, int y)              => GetTile(x, y)?.State == CellState.Occupied;

        /// <summary>Returns a random free tile from the entire grid, or null if all are occupied.</summary>
        public GridSquare GetRandomFreeTile()
        {
            var candidates = GetTilesByState(CellState.Free);
            if (candidates.Count == 0) return null;
            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        /// <summary>Returns all tiles matching the given state.</summary>
        public List<GridSquare> GetTilesByState(CellState state)
        {
            var result = new List<GridSquare>();
            foreach (var sq in coordMap.Values)
                if (sq.State == state) result.Add(sq);
            return result;
        }

        /// <summary>Returns orthogonal neighbours (N/E/S/W) that exist in the grid.</summary>
        public List<GridSquare> GetOrthogonalNeighbours(int x, int y)
        {
            var result = new List<GridSquare>();
            TryAdd(result, x + 1, y);
            TryAdd(result, x - 1, y);
            TryAdd(result, x, y + 1);
            TryAdd(result, x, y - 1);
            return result;
        }

        /// <summary>Returns all 8-directional neighbours that exist in the grid.</summary>
        public List<GridSquare> GetAllNeighbours(int x, int y)
        {
            var result = new List<GridSquare>();
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    if (dx != 0 || dy != 0)
                        TryAdd(result, x + dx, y + dy);
            return result;
        }

        // ── Mutations ─────────────────────────────────────────────────────────

        /// <summary>Places an item on a tile. Returns false if the tile is already occupied.</summary>
        public bool PlaceItem(int x, int y, TileItemType type, GameObject obj, bool force = false)
        {
            var tile = GetTile(x, y);
            if (tile == null) return false;
            if (!force && tile.State == CellState.Occupied) return false;

            tile.PlaceItem(type, obj);
            return true;
        }
        
        
        

        /// <summary>Removes the item from a tile and marks it Free.</summary>
        public bool ClearTile(int x, int y)
        {
            var tile = GetTile(x, y);
            if (tile == null) return false;

            tile.ClearItem();
            return true;
        }
        
        // ── Helpers ───────────────────────────────────────────────────────────

        private void TryAdd(List<GridSquare> list, int x, int y)
        {
            var sq = GetTile(x, y);
            if (sq != null) list.Add(sq);
        }
    }
}
