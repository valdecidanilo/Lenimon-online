using LenixSO.Logger;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public abstract class ContextMenu<T> : MonoBehaviour
{
    [SerializeField] protected ContextSelection contextSelection;

    protected InputAction cancelAction;

    public event Action onReturn;

    protected virtual void Awake()
    {
        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
    }

    public virtual void OpenMenu(T data)
    {
        if (cancelAction == null) return;
        cancelAction.performed += ReturnCall;
    }

    protected virtual void ReturnCall(CallbackContext context)
    {
        onReturn?.Invoke();
    }

    public virtual void CloseMenu()
    {
        if (cancelAction == null) return;
        cancelAction.performed -= ReturnCall;
    }
}
