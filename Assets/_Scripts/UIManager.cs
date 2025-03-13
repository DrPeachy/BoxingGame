using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject questionBoard;
    public GameObject pauseMenu;

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

    public void ToggleQuestionBoard(bool show)
    {
        questionBoard.SetActive(show);
    }

    public void TogglePauseMenu(bool show)
    {
        pauseMenu.SetActive(show);
    }


}
