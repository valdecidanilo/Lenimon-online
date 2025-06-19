using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OptionsMenu : ContextMenu<int>
{
    [Header("BattleLevel")]
    [SerializeField] private HoldButton levelDown;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private HoldButton levelUp;

    private int level = 1;

    private InputAction navigationAction;

    protected override void Awake()
    {
        base.Awake();
        navigationAction = InputSystem.actions.FindAction("UI/Navigate");
        levelDown.performed += () => ChangeBattleLevel(-1);
    }

    public override void OpenMenu(int data)
    {
        base.OpenMenu(data);
        gameObject.SetActive(true);
    }

    private void ChangeBattleLevel(int delta)
    {
        level = Mathf.Clamp((level + delta + 101) % 101, 1, 100);
        levelText.text = level.ToString();
    }

    public override void CloseMenu()
    {
        base.CloseMenu();
        gameObject.SetActive(false);
    }
}
