using System.Collections;
using UnityEngine;

public class AICharacter : Character
{
    [SerializeField] private float actionDelay = 0.5f;

    private AIController _controller;

    protected override void Awake()
    {
        base.Awake();
        _controller = GetComponent<AIController>();
        if (_controller == null)
            Debug.LogError($"[AICharacter] Missing AIController on {gameObject.name}.", this);
    }

    public override IEnumerator TakeTurn(CombatManager combat)
    {
        ResetActionState();

        if (_controller == null || !_controller.PerformAction(combat, this))
            yield break;

        ExecutePendingAction();

        yield return new WaitForSeconds(actionDelay);
    }
}
