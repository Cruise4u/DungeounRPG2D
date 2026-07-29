using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Team : MonoBehaviour
{
    [SerializeField] protected List<Character> members = new();

    public event Action<Character> OnMemberAdded;
    public event Action<Character> OnMemberRemoved;

    public IReadOnlyList<Character> Members => members;
    public List<ITarget> AliveMembers => members.Where(c => c != null && c.IsAlive).Cast<ITarget>().ToList();

    protected virtual void Awake()
    {
        // Members authored in the Inspector never went through AddMember, so claim them here.
        foreach (var character in members)
            if (character != null) character.SetTeam(this);
    }

    protected virtual void OnEnable()  => TeamRegistry.Register(this);
    protected virtual void OnDisable() => TeamRegistry.Unregister(this);

    public bool Contains(Character character) => character != null && members.Contains(character);

    public void AddMember(Character character)
    {
        members.Add(character);
        if (character != null) character.SetTeam(this);
        OnMemberAdded?.Invoke(character);
    }

    public void RemoveMember(Character character)
    {
        members.Remove(character);
        if (character != null && character.Team == this) character.SetTeam(null);
        OnMemberRemoved?.Invoke(character);
    }
}
