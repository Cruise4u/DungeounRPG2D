using System;
using UnityEngine;
using UnityEngine.UI;

// Attach to any action button. Wire inputController and action in the Inspector.
[RequireComponent(typeof(Button))]
public class AttackButton : MonoBehaviour
{
    public Character characterRequisitor;
    [SerializeField] private InputController inputController;
    [SerializeField] private CharacterActionSO action;

    public void OnEnable()
    {
        
    }

    public void OnDisable()
    {
        
    }

    private void Awake()
    {
        
    }


}
