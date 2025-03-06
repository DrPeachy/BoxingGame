using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private bool isLoadingSceneAsync = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartLoadingSceneAsync(string sceneName)
    {
        if (isLoadingSceneAsync) return;

        isLoadingSceneAsync = true;
        LoadSceneAsync(sceneName).Forget();
    }

    private async UniTaskVoid LoadSceneAsync(string sceneName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);
        loadOperation.allowSceneActivation = false; // Prevent auto-switching to the new scene

        while (loadOperation.progress < 0.9f)
        {
            // Wait for the scene to be loaded
            await UniTask.Yield();
        }

        loadOperation.allowSceneActivation = true; // Allow auto-switching to the new scene
        isLoadingSceneAsync = false;
    }

    public void QuitApplication(){
        Debug.Log("Quitting application...");
        Application.Quit();
    }
}
