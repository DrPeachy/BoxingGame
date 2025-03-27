using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

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
            SceneManager.sceneLoaded += OnSceneLoaded;
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

    public void StartLoadingSceneAdditiveAsync(string sceneName)
    {
        if (isLoadingSceneAsync) return;

        isLoadingSceneAsync = true;
        LoadSceneAdditiveAsync(sceneName).Forget();
    }

    private async UniTaskVoid LoadSceneAdditiveAsync(string sceneName)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false; // Prevent auto-switching to the new scene

        while (loadOperation.progress < 0.9f)
        {
            // Wait for the scene to be loaded
            await UniTask.Yield();
        }
        loadOperation.allowSceneActivation = true; // Now activate the scene
        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        isLoadingSceneAsync = false;
    }

    public void UnloadScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName);
        Debug.Log("Unloading scene: " + sceneName);
    }

    // private async UniTaskVoid UnloadSceneAsync(string sceneName)
    // {
    //     AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(sceneName);
    //     while (!unloadOperation.isDone)
    //     {
    //         await UniTask.Yield();
    //     }
    // }

    public void QuitApplication(){
        Debug.Log("Quitting application...");
        Application.Quit();
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        // reassign buttons
        ReassignButtons();
    }

    public void ReassignButtons()
    {
        // Reassign buttons
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            // if button name started with LOAD, add a listener to load the scene
            // format: LOAD_<SCENE_NAME>
            if (button.name.StartsWith("LOAD_"))
            {
                string sceneName = button.name.Replace("LOAD_", "");
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => StartLoadingSceneAsync(sceneName));
            }
            if (button.name.StartsWith("ALOAD_"))
            {
                string sceneName = button.name.Replace("ALOAD_", "");
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => StartLoadingSceneAdditiveAsync(sceneName));
            }
            if(button.name.StartsWith("ULoad_"))
            {
                string sceneName = button.name.Replace("ULoad_", "");
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => UnloadScene(sceneName));
            }
            if(button.name == "Quit"){
                button.onClick.AddListener(QuitApplication);
            }
            // add button click sound
            button.onClick.AddListener(() => AudioManager.Instance.PlayGeneralButtonClicked());
        }
    }
}
