using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // listen for the pause input
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // toggle the pause menu
            UIManager.Instance.TogglePauseMenu(!UIManager.Instance.pauseMenu.activeSelf);
        }
    }
}
