using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Team : MonoBehaviour
{
    [SerializeField] protected List<Character> members = new();

    public IReadOnlyList<Character> Members => members;
    public List<ITarget> AliveMembers => members.Where(c => c.IsAlive).Cast<ITarget>().ToList();

    public abstract IEnumerator TakeTurn(CombatManager combat);
}
