using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "RPG/Character Stats")]
public class CharacterStatsSO : ScriptableObject
{
    [Min(1)] public int maxHp = 100;
    [Min(0)] public int attackPower = 10;
    [Min(0)] public int armor = 5;
    [Min(0f)] public float speed = 5f;
}
