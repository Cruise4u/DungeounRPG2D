using UnityEngine;

namespace DungeonRPG.Grid
{
    public enum CellState { Free, Occupied }

    public enum TileItemType { None, Character, Item }

    public class GridSquare : MonoBehaviour
    {
        [Header("Tile Info")]
        [SerializeField] private int tileID;
        [SerializeField] private int tileX;
        [SerializeField] private int tileY;

        [Header("State")]
        [SerializeField] private CellState tileState;

        [Header("Item")]
        [SerializeField] private TileItemType itemType;
        [SerializeField] private GameObject itemObject;

        public int TileID    => tileID;
        public int X         => tileX;
        public int Y         => tileY;
        public CellState State    => tileState;
        public TileItemType ItemType  => itemType;
        public GameObject ItemObject  => itemObject;

        public void Init(int id, int x, int y, CellState state = CellState.Free)
        {
            tileID    = id;
            tileX     = x;
            tileY     = y;
            tileState = state;
            itemType  = TileItemType.None;
            itemObject = null;
            name = $"Tile_{id} [{x},{y}]";
        }

        public void PlaceItem(TileItemType type, GameObject obj)
        {
            itemType   = type;
            itemObject = obj;
            tileState  = type == TileItemType.None ? CellState.Free : CellState.Occupied;
        }

        public void ClearItem()
        {
            itemType   = TileItemType.None;
            itemObject = null;
            tileState  = CellState.Free;
        }
    }
}
