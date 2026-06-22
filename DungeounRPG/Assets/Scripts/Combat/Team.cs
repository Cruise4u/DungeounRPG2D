using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Team : MonoBehaviour
{
    [SerializeField] protected List<Character> members = new();

    public event Action<Character> OnMemberAdded;
    public event Action<Character> OnMemberRemoved;

    public IReadOnlyList<Character> Members => members;
    public List<ITarget> AliveMembers => members.Where(c => c != null && c.IsAlive).Cast<ITarget>().ToList();

    public void AddMember(Character character)
    {
        members.Add(character);
        OnMemberAdded?.Invoke(character);
    }

    public void RemoveMember(Character character)
    {
        members.Remove(character);
        OnMemberRemoved?.Invoke(character);
    }
}
