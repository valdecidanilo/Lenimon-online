using System;
using System.Collections;

public class CoroutineAction<T>
{
    private Func<T, IEnumerator> action;

    public void Add(Func<T, IEnumerator> coroutine) => action += coroutine;
    public void Remove(Func<T, IEnumerator> coroutine) => action -= coroutine;

    public IEnumerator Invoke(T value)
    {
        if (action == null) yield break;
        Delegate[] delegates = action.GetInvocationList();
        for (int i = 0; i < delegates.Length; i++)
            yield return delegates[i].DynamicInvoke(value);
    }
}

public class CoroutineAction<T, T2>
{
    private Func<T, T2, IEnumerator> action;

    public void RegisterCallback(Func<T, T2, IEnumerator> coroutine) => action += coroutine;
    public void RemoveCallback(Func<T, T2, IEnumerator> coroutine) => action -= coroutine;

    public IEnumerator Invoke(T value, T2 value2)
    {
        if (action == null) yield break;
        Delegate[] delegates = action.GetInvocationList();
        for (int i = 0; i < delegates.Length; i++)
            yield return delegates[i].DynamicInvoke(value, value2);
    }
}
public class CoroutineAction<T, T2, T3>
{
    private Func<T, T2, T3, IEnumerator> action;

    public void Add(Func<T, T2, T3, IEnumerator> coroutine) => action += coroutine;
    public void Remove(Func<T, T2, T3, IEnumerator> coroutine) => action -= coroutine;

    public IEnumerator Invoke(T value, T2 value2, T3 value3)
    {
        if (action == null) yield break;
        Delegate[] delegates = action.GetInvocationList();
        for (int i = 0; i < delegates.Length; i++)
            yield return delegates[i].DynamicInvoke(value, value2, value3);
    }
}
public class CoroutineAction<T, T2, T3, T4>
{
    private Func<T, T2, T3, T4, IEnumerator> action;

    public void Add(Func<T, T2, T3, T4, IEnumerator> coroutine) => action += coroutine;
    public void Remove(Func<T, T2, T3, T4, IEnumerator> coroutine) => action -= coroutine;

    public IEnumerator Invoke(T value, T2 value2, T3 value3, T4 value4)
    {
        if (action == null) yield break;
        Delegate[] delegates = action.GetInvocationList();
        for (int i = 0; i < delegates.Length; i++)
            yield return delegates[i].DynamicInvoke(value, value2, value3, value4);
    }
}