using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The single owner of one side's character cards. Put one on the player panel and one on the
/// enemy panel, point each at that side's Team, and it does the rest.
///
/// Everything is driven by Team.OnMemberAdded / OnMemberRemoved, which every spawn and merge
/// path already goes through:
///   - CombatEncounter spawns an enemy       -> AddMember    -> card claimed
///   - a figurine merge reaches Super        -> AddMember    -> card claimed
///   - two combat characters merge           -> RemoveMember -> the absorbed unit's card is freed,
///                                              and the survivor's card refreshes off
///                                              CharacterStats.OnStatsChanged / OnHpChanged
/// so nothing has to remember to tell the UI anything.
///
/// Two views are supported:
///   - rosterTeam:  every character in the squad (the template / full list)
///   - combatTeam:  the subset currently active in combat
/// Today each side has exactly one Team, so leave combatTeam empty and only the roster is used.
/// Cards in the roster container get a visual "in combat" marker when the same Character also
/// appears in the combat team.
/// </summary>
public class TeamBoardUI : MonoBehaviour
{
    [Header("Teams")]
    [SerializeField] private Team rosterTeam;
    [Tooltip("Leave empty while each side has a single Team — see the class summary.")]
    [SerializeField] private Team combatTeam;

    [Header("Roster panel  (pre-made panels, player team only)")]
    [Tooltip("Leave empty to auto-collect every CharacterCardUI under this object, in hierarchy order.")]
    [SerializeField] private List<CharacterCardUI> rosterPanels = new();
    [Tooltip("When a character leaves, slide the ones after it up so a merge in the middle of the " +
             "row doesn't leave a hole. Turn off to keep every character on the panel it landed on.")]
    [SerializeField] private bool keepRosterCompact = true;

    [Header("Combat panel  (active fighters only)")]
    [SerializeField] private CharacterCardUI combatCardPrefab;
    [SerializeField] private Transform combatContainer;

    // character → card in the roster panel
    private readonly Dictionary<Character, CharacterCardUI> _rosterCards = new();
    // character → card in the combat panel
    private readonly Dictionary<Character, CharacterCardUI> _combatCards = new();

    // pre-made roster panels not currently assigned to a character
    private readonly Queue<CharacterCardUI> _freeRosterPanels = new();

    // scratch list for CompactRoster, reused so a removal doesn't allocate
    private readonly List<Character> _compactBuffer = new();

    // which roster members are currently in the combat team
    private readonly HashSet<Character> _inCombat = new();

    private void Awake()
    {
        // Hand-assigning six panels in the Inspector is easy to get half-done, and a missed one
        // silently costs a character its card — so an empty list means "use my children".
        if (rosterPanels.Count == 0)
            rosterPanels.AddRange(GetComponentsInChildren<CharacterCardUI>(true));

        foreach (var panel in rosterPanels)
        {
            if (panel == null) continue;
            panel.Unbind();
            panel.gameObject.SetActive(false);
            _freeRosterPanels.Enqueue(panel);
        }
    }

    private void OnEnable()
    {
        if (rosterTeam != null) BindRoster(rosterTeam);
        if (UsesSeparateCombatTeam) BindCombatMembers(combatTeam);
    }

    private void OnDisable()
    {
        if (rosterTeam != null) UnbindRoster(rosterTeam);
        if (UsesSeparateCombatTeam) UnbindCombatMembers(combatTeam);
    }

    /// <summary>
    /// False when combatTeam is unset or is the very same Team as the roster — binding both
    /// would subscribe twice to one event and spawn a duplicate card per member.
    /// </summary>
    private bool UsesSeparateCombatTeam => combatTeam != null && combatTeam != rosterTeam;

    // ── public API ──────────────────────────────────────────────────────────

    public void SetRosterTeam(Team team)
    {
        if (rosterTeam != null) UnbindRoster(rosterTeam);
        rosterTeam = team;
        if (rosterTeam != null) BindRoster(rosterTeam);
    }

    public void SetCombatTeam(Team team)
    {
        if (UsesSeparateCombatTeam) UnbindCombatMembers(combatTeam);
        combatTeam = team;
        if (UsesSeparateCombatTeam) BindCombatMembers(combatTeam);
    }

    /// <summary>The card currently showing this character, if any.</summary>
    public bool TryGetRosterCard(Character character, out CharacterCardUI card)
        => _rosterCards.TryGetValue(character, out card) && card != null;

    /// <summary>
    /// Forces every live card to re-read its character. Cards keep themselves current off
    /// stat/evolution events, so this is only needed after a change that raises none.
    /// </summary>
    public void RefreshAllCards()
    {
        foreach (var card in _rosterCards.Values)
            if (card != null) card.RefreshAll();

        foreach (var card in _combatCards.Values)
            if (card != null) card.RefreshAll();
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

    private void OnRosterMemberAdded(Character c) => SpawnRosterCard(c);

    private void OnRosterMemberRemoved(Character c)
    {
        DestroyRosterCard(c);

        // Only on the event path — UnbindRoster tears every card down in a loop and calls
        // DestroyRosterCard directly, so it never pays for a re-pack it is about to undo.
        if (keepRosterCompact) CompactRoster();
    }

    private void SpawnRosterCard(Character character)
    {
        // Teams can hold empty slots (a Team authored in the Inspector with blank entries),
        // and AddMember does not reject null — a null key would throw on the dictionary.
        if (character == null) return;
        if (_rosterCards.ContainsKey(character)) return;

        if (_freeRosterPanels.Count == 0)
        {
            Debug.LogWarning($"[TeamBoardUI] No free roster panel available for {character.TargetName}.", this);
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
        if (character == null) return;
        if (!_rosterCards.TryGetValue(character, out var card)) return;
        _rosterCards.Remove(character);
        _inCombat.Remove(character);

        if (card == null) return;

        card.Unbind();
        card.SetInCombat(false);
        card.gameObject.SetActive(false);
        _freeRosterPanels.Enqueue(card);
    }

    /// <summary>
    /// Slides the surviving characters onto the top panels so a character leaving the middle of
    /// the row doesn't strand an empty slot between two full ones.
    ///
    /// The panels themselves never move — a character is simply re-bound to an earlier panel, so
    /// this works whether the row is a LayoutGroup or hand-placed RectTransforms. Relative order
    /// is taken from the panels rather than from _rosterCards, because a Dictionary does not
    /// promise insertion order and the player is already looking at the panel order.
    /// </summary>
    public void CompactRoster()
    {
        _compactBuffer.Clear();
        foreach (var panel in rosterPanels)
        {
            // A character destroyed outright reads as null through Unity's == and drops out here,
            // which is what we want: no card should be reserved for it.
            if (panel != null && panel.BoundCharacter != null)
                _compactBuffer.Add(panel.BoundCharacter);
        }

        // Both are rebuilt from scratch below. Re-filling the queue in panel order is what makes
        // the *next* character claim the first empty slot instead of whatever the queue had left.
        _rosterCards.Clear();
        _freeRosterPanels.Clear();

        int next = 0;
        foreach (var panel in rosterPanels)
        {
            if (panel == null) continue;

            if (next < _compactBuffer.Count)
            {
                var character = _compactBuffer[next++];

                panel.gameObject.SetActive(true);
                // Panels at or above the removed one already hold the right character; re-binding
                // them would only cost a pointless resubscribe + full refresh.
                if (panel.BoundCharacter != character) panel.Bind(character);
                panel.SetInCombat(_inCombat.Contains(character));
                _rosterCards[character] = panel;
            }
            else
            {
                panel.Unbind();
                panel.SetInCombat(false);
                panel.gameObject.SetActive(false);
                _freeRosterPanels.Enqueue(panel);
            }
        }
    }

    // ── combat ──────────────────────────────────────────────────────────────

    private void BindCombatMembers(Team t)
    {
        t.OnMemberAdded   += OnCombatMemberAdded;
        t.OnMemberRemoved += OnCombatMemberRemoved;

        foreach (var member in t.Members)
            OnCombatMemberAdded(member);
    }

    private void UnbindCombatMembers(Team t)
    {
        t.OnMemberAdded   -= OnCombatMemberAdded;
        t.OnMemberRemoved -= OnCombatMemberRemoved;

        foreach (var card in _combatCards.Values)
        {
            if (card == null) continue;
            card.Unbind();
            Destroy(card.gameObject);
        }

        _combatCards.Clear();
        _inCombat.Clear();
    }

    private void OnCombatMemberAdded(Character character)
    {
        if (character == null) return;

        _inCombat.Add(character);

        // Spawn card in combat panel
        if (!_combatCards.ContainsKey(character) && combatCardPrefab != null && combatContainer != null)
        {
            var card = Instantiate(combatCardPrefab, combatContainer);
            card.Bind(character);
            _combatCards[character] = card;
        }

        // Mark the matching roster card
        if (_rosterCards.TryGetValue(character, out var rosterCard) && rosterCard != null)
            rosterCard.SetInCombat(true);
    }

    private void OnCombatMemberRemoved(Character character)
    {
        if (character == null) return;

        _inCombat.Remove(character);

        if (_combatCards.TryGetValue(character, out var card))
        {
            _combatCards.Remove(character);
            if (card != null)
            {
                card.Unbind();
                Destroy(card.gameObject);
            }
        }

        if (_rosterCards.TryGetValue(character, out var rosterCard) && rosterCard != null)
            rosterCard.SetInCombat(false);
    }
}
