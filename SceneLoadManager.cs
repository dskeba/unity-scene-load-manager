using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    private Image loadBar;

    public AsyncOperation ChangeScene(string currentSceneName, string nextSceneName)
    {
        SceneManager.UnloadSceneAsync(currentSceneName);
        return SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
    }

    public void ChangeSceneWithLoadingScreen(string currentSceneName, string nextSceneName)
    {
        SceneManager.UnloadSceneAsync(currentSceneName);
        StartCoroutine(AsyncLoadLoadingScreen(nextSceneName));
    }

    public AsyncOperation LoadScene(string sceneName)
    {
        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        StartCoroutine(AsyncLoadLoadingScreen(sceneName));
    }

    public bool SetActiveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return SceneManager.SetActiveScene(scene);
    }

    private IEnumerator AsyncLoadLoadingScreen(string nextSceneName)
    {
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);
        while (!sceneLoadOperation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        GetLoadingSceneObjects();
        yield return AsyncLoadSceneWithLoadingScreen("Loading", nextSceneName);
    }

    private void GetLoadingSceneObjects()
    {
        GameObject loadBarGameObject = GameObject.FindGameObjectsWithTag("LoadBar")[0];
        loadBar = loadBarGameObject.GetComponent<Image>();
    }

    private IEnumerator AsyncLoadSceneWithLoadingScreen(string currentSceneName, string nextSceneName)
    {
        loadBar.fillAmount = 0;
        yield return new WaitForSeconds(1);
        AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        sceneLoadOperation.allowSceneActivation = false;
        while (sceneLoadOperation.progress < 0.9f)
        {
            loadBar.fillAmount = sceneLoadOperation.progress;
        }
        loadBar.fillAmount = 1f;
        yield return new WaitForSeconds(1);
        sceneLoadOperation.allowSceneActivation = true;
        SceneManager.UnloadSceneAsync(currentSceneName);
    }
} 
