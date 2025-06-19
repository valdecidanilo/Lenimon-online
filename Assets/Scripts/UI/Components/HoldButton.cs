using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float timeBeforeLoop = .5f;
    [SerializeField] private float loopInterval = .05f;

    private bool onButton;
    private WaitForSeconds initialDelay;
    private WaitForSeconds repeatDelay;
    private Coroutine coroutine;

    public event Action performed;

    private void Start()
    {
        initialDelay = new(timeBeforeLoop);
        repeatDelay = new(loopInterval);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        onButton = true;
        coroutine = StartCoroutine(LoopInput());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onButton = false;
        if (coroutine == null) return;
        StopCoroutine(coroutine);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!onButton) return;
        if (!ReferenceEquals(eventData.pointerCurrentRaycast.gameObject, gameObject)) return;
        onButton = false;
        if (coroutine == null) return;
        StopCoroutine(coroutine);
    }

    private IEnumerator LoopInput()
    {
        performed?.Invoke();
        yield return initialDelay;
        while (onButton)
        {
            performed?.Invoke();
            yield return repeatDelay;
        }
    }
}
