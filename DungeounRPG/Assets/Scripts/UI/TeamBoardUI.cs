using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays two views of a squad:
///   - rosterTeam:  every character in the squad (the template / full list)
///   - combatTeam:  the subset currently active in combat
///
/// Cards in the roster container get a visual "in combat" marker when the same
/// Character also appears in the combat team.
/// </summary>
public class TeamBoardUI : MonoBehaviour
{
    [Header("Teams")]
    [SerializeField] private Team rosterTeam;
    [SerializeField] private Team combatTeam;

    [Header("Roster panel  (pre-made panels, player team only)")]
    [SerializeField] private List<CharacterCardUI> rosterPanels = new();

    [Header("Combat panel  (active fighters only)")]
    [SerializeField] private CharacterCardUI combatCardPrefab;
    [SerializeField] private Transform combatContainer;

    // character → card in the roster panel
    private readonly Dictionary<Character, CharacterCardUI> _rosterCards = new();
    // character → card in the combat panel
    private readonly Dictionary<Character, CharacterCardUI> _combatCards = new();

    // pre-made roster panels not currently assigned to a character
    private readonly Queue<CharacterCardUI> _freeRosterPanels = new();

    // which roster members are currently in the combat team
    private readonly HashSet<Character> _inCombat = new();

    private void Awake()
    {
        foreach (var panel in rosterPanels)
        {
            if (panel == null) continue;
            panel.gameObject.SetActive(false);
            _freeRosterPanels.Enqueue(panel);
        }
    }

    private void OnEnable()
    {
        if (rosterTeam != null) BindRoster(rosterTeam);
        if (combatTeam != null) BindCombat(combatTeam);
    }

    private void OnDisable()
    {
        if (rosterTeam != null) UnbindRoster(rosterTeam);
        if (combatTeam != null) UnbindCombat(combatTeam);
    }

    // ── public API ──────────────────────────────────────────────────────────

    public void SetRosterTeam(Team team)
    {
        if (rosterTeam != null) UnbindRoster(rosterTeam);
        rosterTeam = team;
        BindRoster(rosterTeam);
    }

    public void SetCombatTeam(Team team)
    {
        if (combatTeam != null) UnbindCombat(combatTeam);
        combatTeam = team;
        BindCombat(combatTeam);
    }

    // ── roster ──────────────────────────────────────────────────────────────

    private void BindRoster(Team t)
    {
        t.OnMemberAdded   += OnRosterMemberAdded;
        t.OnMemberRemoved += OnRosterMemberRemoved;

        foreach (var member in t.Members)
            SpawnRosterCard(member);
    }

    private void UnbindRoster(Team t)
    {
        t.OnMemberAdded   -= OnRosterMemberAdded;
        t.OnMemberRemoved -= OnRosterMemberRemoved;

        foreach (var character in new List<Character>(_rosterCards.Keys))
            DestroyRosterCard(character);
    }

    private void OnRosterMemberAdded(Character c)   => SpawnRosterCard(c);
    private void OnRosterMemberRemoved(Character c) => DestroyRosterCard(c);

    private void SpawnRosterCard(Character character)
    {
        if (_rosterCards.ContainsKey(character)) return;

        if (_freeRosterPanels.Count == 0)
        {
            Debug.LogWarning($"[TeamBoardUI] No free roster panel available for {character.TargetName}.");
            return;
        }

        var card = _freeRosterPanels.Dequeue();
        card.gameObject.SetActive(true);
        card.Bind(character);
        card.SetInCombat(_inCombat.Contains(character));
        _rosterCards[character] = card;
    }

    private void DestroyRosterCard(Character character)
    {
        if (!_rosterCards.TryGetValue(character, out var card)) return;
        _rosterCards.Remove(character);

        card.gameObject.SetActive(false);
        _freeRosterPanels.Enqueue(card);
    }

    // ── combat ──────────────────────────────────────────────────────────────

    private void BindCombat(Team t)
    {
        t.OnMemberAdded   += OnCombatMemberAdded;
        t.OnMemberRemoved += OnCombatMemberRemoved;

        foreach (var member in t.Members)
            OnCombatMemberAdded(member);
    }

    private void UnbindCombat(Team t)
    {
        t.OnMemberAdded   -= OnCombatMemberAdded;
        t.OnMemberRemoved -= OnCombatMemberRemoved;

        foreach (var card in _combatCards.Values)
            Destroy(card.gameObject);

        _combatCards.Clear();
        _inCombat.Clear();
    }

    private void OnCombatMemberAdded(Character character)
    {
        _inCombat.Add(character);

        // Spawn card in combat panel
        if (!_combatCards.ContainsKey(character))
        {
            // var card = Instantiate(combatCardPrefab, combatContainer);
            // card.Bind(character);
            // _combatCards[character] = card;
        }

        // Mark the matching roster card
        if (_rosterCards.TryGetValue(character, out var rosterCard))
            rosterCard.SetInCombat(true);
    }

    private void OnCombatMemberRemoved(Character character)
    {
        _inCombat.Remove(character);

        if (_combatCards.TryGetValue(character, out var card))
        {
            _combatCards.Remove(character);
            Destroy(card.gameObject);
        }

        if (_rosterCards.TryGetValue(character, out var rosterCard))
            rosterCard.SetInCombat(false);
    }
}
