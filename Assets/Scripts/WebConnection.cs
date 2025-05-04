using Newtonsoft.Json;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

public class WebConnection : MonoBehaviour
{
    private static WebConnection instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Setup()
    {
        instance = new GameObject("WebConnection").AddComponent<WebConnection>();
        DontDestroyOnLoad(instance.gameObject);
    }

    public static void GetRequest<T>(string route, Action<T> onSuccess) =>
        instance.StartCoroutine(Get(route, onSuccess));

    public static void GetTexture(string route, Action<Texture2D> onSuccess) =>
        instance.StartCoroutine(DownloadTexture(route, onSuccess));

    private static IEnumerator Get<T>(string route, Action<T> onSuccess)
    {
        Logger.Log($"Request to: \n{route}", LogFlags.Request);
        UnityWebRequest request = UnityWebRequest.Get(route);
        yield return request.SendWebRequest();

        //Debug.Log($"Request[{route}]: {request.result}");
        if (request.result == UnityWebRequest.Result.Success)
        {
            Logger.Log($"{route} response:\n{request.downloadHandler.text}", LogFlags.Response);
            T data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            onSuccess?.Invoke(data);
        }
        else
        {
            HandleError(request.responseCode, request.error, Get(route, onSuccess));
        }
    }

    private static IEnumerator DownloadTexture(string route, Action<Texture2D> onSuccess)
    {
        Logger.Log($"Request to: \n{route}", LogFlags.Request);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(route);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Logger.Log($"{route} response: Texture found", LogFlags.Response);
            Texture2D data = DownloadHandlerTexture.GetContent(request);
            data.filterMode = FilterMode.Point;
            onSuccess?.Invoke(data);
        }
        else
        {
            HandleError(request.responseCode, request.error, DownloadTexture(route, onSuccess));
        }
    }

    private static IEnumerator DelayedCoroutine(float delay, IEnumerator coroutine)
    {
        yield return new WaitForSeconds(delay);
        yield return coroutine;
    }

    private static void HandleError(long code, string message, IEnumerator coroutine)
    {
        switch (code)
        {
            case 429:
                Logger.LogWarning("Too many requests, retrying", LogFlags.Response);
                instance.StartCoroutine(DelayedCoroutine(1, coroutine));
                break;
            default:
                Logger.LogError($"REQUEST ERROR: {message}", LogFlags.Response);
                break;
        }
    }
}
