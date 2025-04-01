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
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown("joystick button 3")) // "joystick button 3" corresponds to the Y button on most gamepads
        {
            // toggle the pause menu
            UIManager.Instance.TogglePauseMenu(!UIManager.Instance.pauseMenu.activeSelf);
            SceneLoader.Instance.ReassignButtons(); // force reassign the buttons to prevent missing references
        }
    }
}
