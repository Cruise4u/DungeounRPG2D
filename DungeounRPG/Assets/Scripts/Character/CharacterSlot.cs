using UnityEngine;

public class CharacterSlot : MonoBehaviour
{
    public int slotNumber;
    public CharacterToken OccupantToken { get; private set; }
    public Character OccupantCharacter { get; private set; }

    public bool IsOccupied => OccupantToken != null || OccupantCharacter != null;

    public void Occupy(CharacterToken token)
    {
        OccupantToken = token;
        token.SetSlot(this);
    }

    public void Vacate()
    {
        OccupantToken = null;
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
