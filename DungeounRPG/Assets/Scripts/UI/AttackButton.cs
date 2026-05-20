using System;
using UnityEngine;
using UnityEngine.UI;

// Attach to any action button. Wire playerController and action in the Inspector.
[RequireComponent(typeof(Button))]
public class AttackButton : MonoBehaviour
{
    public Character characterRequisitor;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CharacterActionSO action;

    public void OnEnable()
    {
        GameEventSingleton.OnCharacterActionSelected.Subscribe(SelectCharacterAction);
    }

    public void OnDisable()
    {
        GameEventSingleton.OnCharacterActionSelected.Unsubscribe(SelectCharacterAction);
    }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        GameEventSingleton.OnCharacterActionSelected.Raise(playerController);
    }

    private void SelectCharacterAction(CharacterController controller)
    {
        playerController.BeginTargeting(action);
    }

}
