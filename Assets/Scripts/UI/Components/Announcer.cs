using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Announcer : MonoBehaviour
{
    private static Announcer instance;

    [SerializeField] private TMP_Text field;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject continueVisual;

    [Header("Text Typing")]
    // Characters per second.
    [SerializeField, Tooltip("Characters per second"), Min(1f)] private float cps = 0.01f;
    [SerializeField] private bool ignoreSpaces;
    [SerializeField] private bool ignoreLineBreaks;

    private bool pressedInput;
    private Action onInputPress;

    private void Awake()
    {
        continueButton.onClick.AddListener(OnPressedInput);
    }

    private void OnPressedInput()
    {
        pressedInput = true;
        onInputPress?.Invoke();
    }

    public static void ChangeAnnouncer(Announcer newAnnouncer)
    {
        if (instance)
        {
            CloseAnnouncement();
        }
        instance = newAnnouncer;
    }

    public static void Announce(string text, bool awaitInput = false, float holdTime = 0, Action onDone = null)
    {
        if (!instance) return;
        instance.gameObject.SetActive(true);
        instance.StartCoroutine(AnnounceCoroutine(text, awaitInput, holdTime, onDone));
    }

    public static IEnumerator AnnounceCoroutine(string text, bool awaitInput = false, float holdTime = 0, Action onDone = null)
    {
        if (!instance) yield break;

        //setup
        WaitForSeconds step = new(1f / instance.cps);
        StringBuilder typing = new();
        instance.gameObject.SetActive(true);
        instance.continueVisual.SetActive(false);
        if (awaitInput)
        {
            instance.continueButton.Select();
            instance.onInputPress += () =>
            {
                //skip typing
                if (typing.Length >= text.Length) return;
                typing = new(text);
                instance.field.text = typing.ToString();
                instance.pressedInput = false;
            };
        }

        //type
        while (typing.Length < text.Length)
        {
            bool skipDelay = text[typing.Length] == ' ' && instance.ignoreSpaces;
            skipDelay |= text[typing.Length] == '\n' && instance.ignoreLineBreaks;
            typing.Append(text[typing.Length]);
            instance.field.text = typing.ToString();

            if (skipDelay || typing.Length == text.Length) continue;
            yield return step;
        }

        //hold
        instance.continueVisual.SetActive(awaitInput);
        if (awaitInput) yield return new WaitUntil(() => instance.pressedInput);
        instance.pressedInput = false;
        yield return new WaitForSeconds(holdTime);
        onDone?.Invoke();
    }

    public static void CloseAnnouncement()
    {
        instance.field.text = string.Empty;
        instance.gameObject.SetActive(false);
    }
}
