using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class LoadingScreen : MonoBehaviour
{
    private static LoadingScreen instance;

    [SerializeField] private GameObject screen;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        instance = this;
    }

    public static void QueueLoading()
    {

    }

    private static void DoneLoading()
    {
        instance.screen.SetActive(false);
    }
}
