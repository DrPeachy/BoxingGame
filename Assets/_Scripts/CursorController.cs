using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;


public class CursorController : MonoBehaviour
{
    public static CursorController Instance { get; private set; }
    public RectTransform cursorLeft;
    public RectTransform cursorRight;

    public float cursorSpeed = 1f;

    public Camera mainCamera;

    private Vector2 cursorLeftPosition;
    private Vector2 cursorRightPosition;
    private Vector2 cursorLeftOrigin;
    private Vector2 cursorRightOrigin;

    public Button buttonLeftClicked;
    public Button buttonRightClicked;

    public Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    private Gamepad gamepadLeft;
    private Gamepad gamepadRight;

    private Color transparentRed = new Color(1, 0, 0, 0.5f);
    private Color transparentBlue = new Color(0, 0, 1, 0.5f);

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

    private void Start()
    {
        cursorLeftPosition = cursorLeft.anchoredPosition;
        cursorRightPosition = cursorRight.anchoredPosition;
        cursorLeftOrigin = cursorLeft.anchoredPosition;
        cursorRightOrigin = cursorRight.anchoredPosition;
    }

    private void Update()
    {
        // Gamepad gamepad = Gamepad.current;
        // if (gamepad == null) return;
        if(playerInputs.Count == 0 || playerInputs.Count == 1) return;


        Vector2 leftStick = gamepadLeft.leftStick.ReadValue();
        leftStick += leftStick == Vector2.zero ? gamepadLeft.rightStick.ReadValue() : Vector2.zero;

        Vector2 rightStick = gamepadRight.leftStick.ReadValue();
        rightStick += rightStick == Vector2.zero ? gamepadRight.rightStick.ReadValue() : Vector2.zero;

        UpdateCursorPosition(leftStick, rightStick);

        // check if a button was clicked
        // left player red cursor
        if(gamepadLeft.leftShoulder.wasPressedThisFrame || gamepadLeft.rightShoulder.wasPressedThisFrame){
            Button currentClickedButtonLeft = GetButtonUnderCursor(cursorLeft);
            if(currentClickedButtonLeft == buttonLeftClicked) return;

            // show button pressed effect
            if(currentClickedButtonLeft != null){
                Debug.Log($"Button clicked: {currentClickedButtonLeft.name}");
                SetButtonColor(currentClickedButtonLeft, transparentRed, transparentBlue, true);
                if(buttonLeftClicked != null){
                    ResetButtonColor(buttonLeftClicked, transparentRed);
                }
                buttonLeftClicked = currentClickedButtonLeft;
            }
        }

        // right player blue cursor
        if(gamepadRight.leftShoulder.wasPressedThisFrame || gamepadRight.rightShoulder.wasPressedThisFrame){
            Button currentClickedButtonRight = GetButtonUnderCursor(cursorRight);
            if(currentClickedButtonRight == buttonRightClicked) return;

            // show button pressed effect
            if(currentClickedButtonRight != null){
                Debug.Log($"Button clicked: {currentClickedButtonRight.name}");
                SetButtonColor(currentClickedButtonRight, transparentBlue, transparentRed, false);
                if(buttonRightClicked != null){
                    ResetButtonColor(buttonRightClicked, transparentBlue);
                }
                buttonRightClicked = currentClickedButtonRight;
            }
        }
        
    }

    private void SetButtonColor(Button button, Color myColor, Color otherColor, bool isLeft){
        Image leftHalf = button.transform.Find("Left").GetComponent<Image>();
        Image rightHalf = button.transform.Find("Right").GetComponent<Image>();
        if(isLeft){
            leftHalf.color = myColor;
            if(rightHalf.color != otherColor){
                rightHalf.color = myColor;
            }
        }else{
            rightHalf.color = myColor;
            if(leftHalf.color != otherColor){
                leftHalf.color = myColor;
            }
        }
    }

    private void ResetButtonColor(Button button, Color removedColor){
        Image leftHalf = button.transform.Find("Left").GetComponent<Image>();
        Image rightHalf = button.transform.Find("Right").GetComponent<Image>();
        Color transparentWhite = new Color(1, 1, 1, 0);
        if(leftHalf.color == removedColor){
            leftHalf.color = transparentWhite;
        }
        if(rightHalf.color == removedColor){
            rightHalf.color = transparentWhite;
        }
        // set the color to other half if the other half is not transparent
        if(leftHalf.color != transparentWhite){
            rightHalf.color = leftHalf.color;
        }
        if(rightHalf.color != transparentWhite){
            leftHalf.color = rightHalf.color;
        }
    }

    public void Reset()
    {
        ResetCursors();
        buttonLeftClicked = null;
        buttonRightClicked = null;
        // reset button colors
        foreach(Button button in FindObjectsOfType<Button>()){
            ResetButtonColor(button, transparentRed);
            ResetButtonColor(button, transparentBlue);
        }   
    }

    public void ResetCursors(){
        Debug.Log($"cursorLeftOrigin: {cursorLeftOrigin}");
        Debug.Log($"cursorRightOrigin: {cursorRightOrigin}");
        Debug.Log($"cursorLeft.anchoredPosition: {cursorLeft.anchoredPosition}");
        Debug.Log($"cursorRight.anchoredPosition: {cursorRight.anchoredPosition}");
        cursorLeft.anchoredPosition = cursorLeftOrigin;
        cursorRight.anchoredPosition = cursorRightOrigin;
        cursorLeftPosition = cursorLeftOrigin;
        cursorRightPosition = cursorRightOrigin;
    }

    public void UpdateCursorPosition(Vector2 leftStick, Vector2 rightStick){
        cursorLeftPosition += leftStick * cursorSpeed * Time.deltaTime;
        cursorRightPosition += rightStick * cursorSpeed * Time.deltaTime;

        cursorLeft.anchoredPosition = ClampToScreen(cursorLeftPosition);
        cursorRight.anchoredPosition = ClampToScreen(cursorRightPosition);

        // if(gamepad.leftShoulder.wasPressedThisFrame){
        //     buttonLeftClicked = GetButtonUnderCursor(cursorLeft);
        // }

        // if(gamepad.rightShoulder.wasPressedThisFrame){
        //     buttonRightClicked = GetButtonUnderCursor(cursorRight);
        // }

        // if(buttonLeftClicked != null && buttonRightClicked != null && buttonLeftClicked == buttonRightClicked){
        //     Debug.Log("A button was clicked by both cursors!");
        //     // invoke an answer selected event
        // }
    }

    private Vector2 ClampToScreen(Vector2 position) {
        float xMin = -Screen.width, xMax = Screen.width;
        float yMin = -Screen.height, yMax = Screen.height;

        position.x = Mathf.Clamp(position.x, xMin, xMax);
        position.y = Mathf.Clamp(position.y, yMin, yMax);
        return position;
    }

    private Button GetButtonUnderCursor(RectTransform cursor) {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = cursor.position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            Button button = result.gameObject.GetComponent<Button>();
            if (button != null) return button;
        }
        return null;
    }

    public void AddPlayerInput(int playerIndex, PlayerInput playerInput){
        playerInputs[playerIndex] = playerInput;
        if(playerIndex == 0) gamepadLeft = playerInput.GetDevice<Gamepad>();
        if(playerIndex == 1) gamepadRight = playerInput.GetDevice<Gamepad>();
    }

    public Tuple<bool, bool> CheckAnswerCorrectness(Button correctAnswer){
        return new Tuple<bool, bool>(buttonLeftClicked==null? false : correctAnswer == buttonLeftClicked, buttonRightClicked == null? false: correctAnswer == buttonRightClicked);
    }

}
