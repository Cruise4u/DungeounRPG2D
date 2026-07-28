using System.Collections;
using UnityEngine;

public class AICharacter : Character
{
    [SerializeField] private float actionDelay = 0.5f;

    private AIController _controller;

    protected override void Awake()
    {
        base.Awake();
    }
}
