using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(-1)]
public class LoadingScreen : MonoBehaviour
{
    private static LoadingScreen instance;

    public static event Action onDoneLoading;

    [SerializeField] private GameObject screen;
    [SerializeField] private TMP_Text loadDescription;

    private WaitForSeconds nextDelay = new(.4f);

    private List<Checklist> checklists = new();
    private List<string> texts = new();
    
    [SerializeField] private Image background;
    [SerializeField] private Sprite[] loadingBackground;
    public static Checklist lastOnList => instance.checklists[^1];

    private void Awake()
    {
        onDoneLoading = null;
        instance = this;
        gameObject.SetActive(false);
        ChangeBackground();
    }

    public static void AddOrChangeQueue(Checklist checklist, string text)
    {
        var index = instance.checklists.IndexOf(checklist);
        if (index >= 0)
        {
            instance.texts[index] = text;
        }
        else
        {
            index = instance.texts.Count;
            instance.checklists.Add(checklist);
            instance.texts.Add(text);
            if (index == 0) StartLoading();
        }

        if (index == 0)
        {
            instance.loadDescription.text = text;
        }
    }

    private void ChangeBackground()
    {
        background.sprite = loadingBackground[Random.Range(0, loadingBackground.Length)];
    }
    private static void NextChecklist()
    {
        var current = instance.checklists[0];
        current.onCompleted -= NextChecklist;
        instance.checklists.RemoveAt(0);
        instance.texts.RemoveAt(0);
        instance.StartCoroutine(NextDelay());
    }

    private static IEnumerator NextDelay()
    {
        if (instance.checklists.Count <= 0)
        {
            DoneLoading();
            yield break;
        }
        var current = instance.checklists[0];
        instance.loadDescription.text = instance.texts[0];
        yield return instance.nextDelay;
        if (!current.isDone) current.onCompleted += NextChecklist;
        else NextChecklist();
    }

    private static void StartLoading()
    {
        instance.screen.SetActive(true);
        instance.StartCoroutine(NextDelay());
    }

    private static void DoneLoading()
    {
        instance.screen.SetActive(false);
        onDoneLoading?.Invoke();
    }

    public static string GetCurrentText() => instance.texts.Count > 0 ? instance.texts[0] : string.Empty;
    public static void ChangeCurrentText(string text)
    {
        if (instance.texts.Count <= 0) return;
        instance.texts[0] = text;
        instance.loadDescription.text = instance.texts[0];
    }
}
