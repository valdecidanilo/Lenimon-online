using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class OptionsMenu : ContextMenu<int>
{
    [Header("BattleLevel")]
    [SerializeField] private HoldButton levelDown;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private HoldButton levelUp;

    private int level;
    private int finalLevel => level + 1;

    private InputAction navigationAction;

    protected override void Awake()
    {
        base.Awake();
        navigationAction = InputSystem.actions.FindAction("UI/Navigate");
        navigationAction.performed += ShiftOption;
        levelDown.performed += () => ChangeBattleLevel(-1);
        levelUp.performed += () => ChangeBattleLevel(1);
    }

    public override void OpenMenu(int data)
    {
        base.OpenMenu(data);
        gameObject.SetActive(true);
        ChangeBattleLevel(0);
    }

    private void ShiftOption(CallbackContext context)
    {
        int direction = Mathf.RoundToInt(context.ReadValue<Vector2>().x);
        if (direction == 0) return;

        Debug.Log(context.duration);
        HoldButton holdButton = direction > 0 ? levelUp : levelDown;
        if (context.started)
            holdButton.OnPointerDown(null);
        else if (context.canceled)
            holdButton.OnPointerUp(null);
    }

    private void ChangeBattleLevel(int delta)
    {
        level = (level + delta + 100) % 100;
        levelText.text = finalLevel.ToString();
    }

    public override void CloseMenu()
    {
        base.CloseMenu();
        gameObject.SetActive(false);
    }
}
