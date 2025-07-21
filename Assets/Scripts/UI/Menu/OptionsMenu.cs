using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class OptionsMenu : ContextMenu<int>
{
    //Config data
    public static int? battleLevel;
    public static bool invertChart;

    [Header("BattleLevel")]
    [SerializeField] private HoldButton levelDown;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private HoldButton levelUp;

    [Header("InvertTypeChat")]
    [SerializeField] private HoldButton leftButton;
    [SerializeField] private TMP_Text optionText;
    [SerializeField] private HoldButton rightButton;

    private int level;
    private bool invertTypeChart = true;

    private InputAction navigationAction;

    protected override void Awake()
    {
        base.Awake();
        navigationAction = InputSystem.actions.FindAction("UI/Navigate");
        navigationAction.performed += ShiftOption;
        levelDown.performed += () => ChangeBattleLevel(-1);
        levelUp.performed += () => ChangeBattleLevel(1);

        leftButton.performed += ToggleInvertChart;
        rightButton.performed += ToggleInvertChart;

        contextSelection.onItemPick += OnPickItem;
        gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        navigationAction.performed -= ShiftOption;
    }

    public override void OpenMenu(int data)
    {
        base.OpenMenu(data);
        gameObject.SetActive(true);
        ChangeBattleLevel(0);
        level = battleLevel ?? 0;
        invertTypeChart = invertChart;
        levelText.text = level == 0 ? "Random" : level.ToString();
        optionText.text = invertTypeChart ? "Yes" : "No";
        contextSelection.Select(data);
    }

    private void OnPickItem(int id)
    {
        switch (id)
        {
            case 0:
                ResetBattle();
                break;
            case 3:
                ReturnCall(new());
                break;
        }
    }

    private void ShiftOption(CallbackContext context)
    {
        if (!isOpen) return;
        int direction = Mathf.RoundToInt(context.ReadValue<Vector2>().x);
        if (direction == 0) return;

        if(contextSelection.selectedId != 1 && contextSelection.selectedId != 2) return;

        if (contextSelection.selectedId == 1)
            ChangeBattleLevel(direction);
        else
            ToggleInvertChart();
    }

    private void ChangeBattleLevel(int delta)
    {
        level = (level + delta + 101) % 101;
        levelText.text = level == 0 ? "Random" : level.ToString();
        
        AudioManager.Instance.PlaySelectAudio();
    }
    private void ToggleInvertChart()
    {
        invertTypeChart = !invertTypeChart;
        optionText.text = invertTypeChart ? "Yes" : "No";
        
        AudioManager.Instance.PlaySelectAudio();
    }

    private void ResetBattle()
    {
        battleLevel = level == 0 ? null : level;
        invertChart = invertTypeChart;
        SceneManager.LoadScene(0);
    }

    public override void CloseMenu()
    {
        base.CloseMenu();
        gameObject.SetActive(false);
    }
}
