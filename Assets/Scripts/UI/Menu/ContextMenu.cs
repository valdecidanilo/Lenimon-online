using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ContextMenu<T> : MonoBehaviour
{
    [SerializeField] protected ContextSelection contextSelection;

    public Action onReturn;

    public abstract void OpenMenu(T data);
    
    protected virtual void Update()
    {
        if (InputSystem.actions.FindAction("UI/Cancel").WasPressedThisFrame())
            ReturnCall();
    }

    protected virtual void ReturnCall() => onReturn?.Invoke();

    public abstract void CloseMenu();
}
