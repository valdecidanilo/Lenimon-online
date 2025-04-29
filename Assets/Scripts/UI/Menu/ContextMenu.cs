using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class ContextMenu<T> : MonoBehaviour
{
    [SerializeField] protected ContextSelection contextSelection;

    protected InputAction cancelAction;

    public Action onReturn;

    protected virtual void Awake()
    {
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
    }

    public abstract void OpenMenu(T data);
    
    protected virtual void Update()
    {
        if (cancelAction.WasPressedThisFrame())
            ReturnCall();
    }

    protected virtual void ReturnCall() => onReturn?.Invoke();

    public abstract void CloseMenu();
}
