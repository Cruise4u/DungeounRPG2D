using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Character : MonoBehaviour, ITarget
{
    private SpriteRenderer spriteRenderer;

    public Team team { get; private set; }
    public CharacterStats Stats { get; private set; }

    public EEvolutionTypeID EvolutionType { get; private set; } = EEvolutionTypeID.Basic;

    /// <summary>Raised after SetEvolution, so bound UI can show the new tier without polling.</summary>
    public event Action<Character> OnEvolutionChanged;

    /// <summary>
    /// The team this character currently belongs to — the single source of truth for
    /// allegiance. Assigned by Team.AddMember/RemoveMember; read via TeamRegistry.
    /// </summary>
    public Team Team { get; private set; }

    /// <summary>Called by Team when this character joins or leaves a roster. Pass null to clear.</summary>
    public void SetTeam(Team team) => Team = team;
    
    // ITarget
    public string TargetName => gameObject.name;
    public bool IsAlive => !Stats.IsDead;
    
    public void TakeDamage(int damage) => Stats.TakeDamage(damage);
    public void Heal(int amount)       => Stats.Heal(amount);
    
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Stats = GetComponent<CharacterStats>();
    }
    
    public void TakeStatusEffect(StatusEffect effect)
    {
        //Get's the statuseffectshandler of this character
        //Applies status effect into it
        throw new System.NotImplementedException();
    }

    /// <summary>Mirrors CharacterFigurine.SetEvolution — tints the sprite to reflect the current evolution tier.</summary>
    public void SetEvolution(EEvolutionTypeID evolution, Color color)
    {
        EvolutionType = evolution;
        if (spriteRenderer != null)
            spriteRenderer.color = color;

        OnEvolutionChanged?.Invoke(this);
    }

    public virtual void SetHighlighted(bool highlighted)
    {
        if (spriteRenderer != null)
            spriteRenderer.color = highlighted ? Color.yellow : Color.white;
    }
}