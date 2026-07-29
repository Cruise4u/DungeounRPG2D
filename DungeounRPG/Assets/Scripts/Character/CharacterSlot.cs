using UnityEngine;

public class CharacterSlot : MonoBehaviour
{
    
    // 0 -> player
    // 1 -> CPU
    public int slotID; 
    
    public int slotNumber;
    public CharacterFigurine OccupantFigurine { get; private set; }
    public Character OccupantCharacter { get; private set; }

    public bool IsOccupied => OccupantFigurine != null || OccupantCharacter != null;

    public void Occupy(CharacterFigurine figurine)
    {
        OccupantFigurine = figurine;
        figurine.SetSlot(this);
    }

    public void Vacate()
    {
        OccupantFigurine = null;
    }

    public void OccupyCharacter(Character character)
    {
        OccupantCharacter = character;
    }

    public void VacateCharacter()
    {
        OccupantCharacter = null;
    }
}
