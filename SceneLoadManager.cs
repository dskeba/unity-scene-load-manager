using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public enum LoadScreenBehvavior
{
    None,
    AnyKey,
    SpecificKey,
    WaitSeconds
}

public struct LoadScreenOptions
{
    public LoadScreenBehvavior LoadScreenBehvavior;
    public KeyCode KeyCode;
    public float Seconds;
    public string LoadBarTag;

    public LoadScreenOptions(LoadScreenBehvavior loadScreenBehavior)
    {
        LoadScreenBehvavior = loadScreenBehavior;
        KeyCode = KeyCode.Space;
        Seconds = 1f;
        LoadBarTag = "LoadBar";
    }
}

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    public AsyncOperation ChangeScene(string currentSceneName, string nextSceneName)
    {
        SceneManager.UnloadSceneAsync(currentSceneName);
        return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
    }

    public void ChangeScene(string currentSceneName, string nextSceneName, Action preloadAction)
    {
        SceneManager.UnloadSceneAsync(currentSceneName);
        LoadScene(nextSceneName, preloadAction);
    }

    public void ChangeSceneWithLoadScreen(string currentSceneName, string nextSceneName)
    {
        LoadScreenOptions options = new LoadScreenOptions();
        options.LoadScreenBehvavior = LoadScreenBehvavior.None;
        ChangeSceneWithLoadScreen(currentSceneName, nextSceneName, () => { }, options);
    }

    public void ChangeSceneWithLoadScreen(string currentSceneName, string nextSceneName, Action preloadAction, LoadScreenOptions options)
    {
        SceneManager.UnloadSceneAsync(currentSceneName);
        StartCoroutine(AsyncLoadLoadScreen(nextSceneName, preloadAction, options));
    }

    public AsyncOperation LoadScene(string sceneName)
    {
        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void LoadScene(string sceneName, Action preloadAction)
    {
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        sceneLoadOperation.allowSceneActivation = false;
        while (sceneLoadOperation.progress < 0.9f) { }
        preloadAction();
        sceneLoadOperation.allowSceneActivation = true;
    }

    public void LoadSceneWithLoadScreen(string sceneName, LoadScreenOptions options)
    {
        StartCoroutine(AsyncLoadLoadScreen(sceneName, () => { }, options));
    }

    public void LoadSceneWithLoadScreen(string sceneName, Action preloadAction, LoadScreenOptions options)
    {
        StartCoroutine(AsyncLoadLoadScreen(sceneName, preloadAction, options));
    }

    public AsyncOperation UnloadScene(string sceneName)
    {
        return SceneManager.UnloadSceneAsync(sceneName);
    }

    public bool SetActiveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return SceneManager.SetActiveScene(scene);
    }

    private IEnumerator AsyncLoadLoadScreen(string nextSceneName, Action preloadAction, LoadScreenOptions options)
    {
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
        while (!sceneLoadOperation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        yield return AsyncLoadSceneWithLoadScreen("Loading", nextSceneName, preloadAction, options);
    }

    private IEnumerator AsyncLoadSceneWithLoadScreen(string currentSceneName, string nextSceneName, Action preloadAction, LoadScreenOptions options)
    {
        GameObject loadBarGameObject = GameObject.FindGameObjectsWithTag("LoadBar")[0];
        Image loadBar = loadBarGameObject.GetComponent<Image>();
        loadBar.fillAmount = 0;
        yield return new WaitForSeconds(1);
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        sceneLoadOperation.allowSceneActivation = false;
        while (sceneLoadOperation.progress < 0.9f)
        {
            loadBar.fillAmount = (sceneLoadOperation.progress + 0.1f) * 0.5f;
        }
        yield return new WaitForSeconds(1);
        preloadAction();
        loadBar.fillAmount = 1f;
        if (options.LoadScreenBehvavior.Equals(LoadScreenBehvavior.WaitSeconds))
        {
            yield return new WaitForSeconds(options.Seconds);
        }
        else if (options.LoadScreenBehvavior.Equals(LoadScreenBehvavior.SpecificKey))
        {
            while (!sceneLoadOperation.isDone)
            {
                if (Input.GetKeyDown(options.KeyCode))
                {
                    break;
                }
                yield return null;
            }
        } 
        else if (options.LoadScreenBehvavior.Equals(LoadScreenBehvavior.AnyKey))
        {
            while (!sceneLoadOperation.isDone)
            {
                if (Input.anyKeyDown)
                {
                    break;
                }
                yield return null;
            }
        }
        sceneLoadOperation.allowSceneActivation = true;
        SceneManager.UnloadSceneAsync(currentSceneName);
    }
} 
