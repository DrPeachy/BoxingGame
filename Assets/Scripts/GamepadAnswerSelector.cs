// GamepadAnswerSelector.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class GamepadAnswerSelector : MonoBehaviour
{
    public static GamepadAnswerSelector Instance { get; private set; }
    
    // Answer buttons in order (index 0: left, 1: up, 2: right, 3: down)
    public List<Button> answerButtons;

    // Colors for highlighting the selection for each player
    public Color leftPlayerColor = new Color(1f, 0f, 0f, 0.6f);   // Transparent red
    public Color rightPlayerColor = new Color(0f, 0f, 1f, 0.5f);    // Transparent blue

    // Selected answer index for each player (-1 means no selection yet)
    private int leftPlayerSelection = -1;
    private int rightPlayerSelection = -1;
    
    // Gamepad devices for left and right players
    public Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    private Gamepad gamepadLeft;
    private Gamepad gamepadRight;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    // This method is used to assign a gamepad to a player. For playerIndex 0 (left) and 1 (right)
    public void AddPlayerInput(int playerIndex, PlayerInput playerInput){
        playerInputs[playerIndex] = playerInput;
        if(playerIndex == 0) gamepadLeft = playerInput.GetDevice<Gamepad>();
        if(playerIndex == 1) gamepadRight = playerInput.GetDevice<Gamepad>();
    }

    private void Update()
    {
        // Process input for left player's gamepad
        if (gamepadLeft != null)
        {
            if (gamepadLeft.dpad.left.wasPressedThisFrame)
                UpdateSelection(0, true);
            else if (gamepadLeft.dpad.up.wasPressedThisFrame)
                UpdateSelection(1, true);
            else if (gamepadLeft.dpad.right.wasPressedThisFrame)
                UpdateSelection(2, true);
            else if (gamepadLeft.dpad.down.wasPressedThisFrame)
                UpdateSelection(3, true);
        }
        
        // Process input for right player's gamepad
        if (gamepadRight != null)
        {
            if (gamepadRight.dpad.left.wasPressedThisFrame)
                UpdateSelection(0, false);
            else if (gamepadRight.dpad.up.wasPressedThisFrame)
                UpdateSelection(1, false);
            else if (gamepadRight.dpad.right.wasPressedThisFrame)
                UpdateSelection(2, false);
            else if (gamepadRight.dpad.down.wasPressedThisFrame)
                UpdateSelection(3, false);
        }
    }

    // Updates the selection for the player.
    // isLeftPlayer: true for left player, false for right player.
    private void UpdateSelection(int answerIndex, bool isLeftPlayer)
    {
        if (answerIndex < 0 || answerIndex >= answerButtons.Count)
            return;
        
        if (isLeftPlayer)
        {
            if (leftPlayerSelection != answerIndex)
            {
                // Reset previous button highlight if any
                if (leftPlayerSelection != -1)
                    ResetButtonHighlight(answerButtons[leftPlayerSelection], leftPlayerColor, true);
                
                leftPlayerSelection = answerIndex;
                SetButtonHighlight(answerButtons[answerIndex], leftPlayerColor, true);
                Debug.Log($"Left player selected answer index: {answerIndex}");
                
            }
        }
        else
        {
            if (rightPlayerSelection != answerIndex)
            {
                if (rightPlayerSelection != -1)
                    ResetButtonHighlight(answerButtons[rightPlayerSelection], rightPlayerColor, false);
                
                rightPlayerSelection = answerIndex;
                SetButtonHighlight(answerButtons[answerIndex], rightPlayerColor, false);
                Debug.Log($"Right player selected answer index: {answerIndex}");
                
            }
        }
    }

    // Sets the highlight for the answer button for a given player.
    // isLeftPlayer determines whether it is the left or right player's highlight.
    private void SetButtonHighlight(Button button, Color highlightColor, bool isLeftPlayer)
    {
        // Attempt to get child images named "Left" and "Right"
        Image leftImage = button.transform.Find("Left")?.GetComponent<Image>();
        Image rightImage = button.transform.Find("Right")?.GetComponent<Image>();
        if (leftImage != null && rightImage != null)
        {
            if (isLeftPlayer)
            {
                leftImage.color = highlightColor;
            }
            else
            {
                rightImage.color = highlightColor;
            }
        }
        else
        {
            // If not found, fallback to setting the whole button's image color
            button.image.color = highlightColor;
        }
    }

    // Resets the highlight for the answer button for the given player's color.
    private void ResetButtonHighlight(Button button, Color highlightColor, bool isLeftPlayer)
    {
        Image leftImage = button.transform.Find("Left")?.GetComponent<Image>();
        Image rightImage = button.transform.Find("Right")?.GetComponent<Image>();
        Color transparent = new Color(1f, 1f, 1f, 0f);
        if (leftImage != null && rightImage != null)
        {
            if (isLeftPlayer)
            {
                leftImage.color = transparent;
            }
            else
            {
                rightImage.color = transparent;
            }
        }
        else
        {
            button.image.color = Color.white;
        }
    }
    
    // Resets the selections and highlights on all answer buttons. Call this when a new question is generated.
    public void ResetSelections()
    {
        leftPlayerSelection = -1;
        rightPlayerSelection = -1;
        
        foreach (Button button in answerButtons)
        {
            Image leftImage = button.transform.Find("Left")?.GetComponent<Image>();
            Image rightImage = button.transform.Find("Right")?.GetComponent<Image>();
            Color transparent = new Color(1f, 1f, 1f, 0f);
            if (leftImage != null)
                leftImage.color = transparent;
            if (rightImage != null)
                rightImage.color = transparent;
            else
                button.image.color = Color.white;
        }
    }

    public Tuple<bool, bool> CheckAnswerCorrectness(Button correctAnswer){
        bool leftPlayerCorrect = false;
        bool rightPlayerCorrect = false;
        
        if (leftPlayerSelection != -1 && answerButtons[leftPlayerSelection] == correctAnswer)
        {
            leftPlayerCorrect = true;
        }
        
        if (rightPlayerSelection != -1 && answerButtons[rightPlayerSelection] == correctAnswer)
        {
            rightPlayerCorrect = true;
        }
        return Tuple.Create(leftPlayerCorrect, rightPlayerCorrect);
    }
}
