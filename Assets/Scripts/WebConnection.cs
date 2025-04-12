using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WebConnection : MonoBehaviour
{
    private static WebConnection instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Setup()
    {
        instance = new GameObject("WebConnection").AddComponent<WebConnection>();
    }

    public static void GetRequest<T>(string route, Action<T> onSuccess) =>
        instance.StartCoroutine(Get(route, onSuccess));

    private static IEnumerator Get<T>(string route, Action<T> onSuccess)
    {
        UnityWebRequest request = UnityWebRequest.Get(route);
        yield return request.SendWebRequest();

        //Debug.Log($"Request[{route}]: {request.result}");
        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log($"Raw:\n{request.downloadHandler.text}");
            T data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            onSuccess?.Invoke(data);
        }

    }

    public static void GetTexture(string route, Action<Texture2D> onSuccess) =>
        instance.StartCoroutine(DownloadTexture(route, onSuccess));

    private static IEnumerator DownloadTexture(string route, Action<Texture2D> onSuccess)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(route);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D data = DownloadHandlerTexture.GetContent(request);
            data.filterMode = FilterMode.Point;
            onSuccess?.Invoke(data);
        }
    }
}
