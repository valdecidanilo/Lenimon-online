using LenixSO.Logger;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Logger = LenixSO.Logger.Logger;

public class SummaryMenu : ContextMenu<Pokemon>
{
    private InputAction navigateAction;

    protected override void Awake()
    {
        base.Awake();
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
    }

    public override void OpenMenu(Pokemon data)
    {
        gameObject.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();
        if(navigateAction.WasPressedThisFrame()) ChangeWindow(navigateAction.ReadValue<Vector2>());
    }

    private void ChangeWindow(Vector2 direction)
    {
        Logger.Log($"{direction}", LogFlags.Game);
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
