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

    public Button buttonLeftClicked;
    public Button buttonRightClicked;

    public Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    private Gamepad gamepadLeft;
    private Gamepad gamepadRight;

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
        if(gamepadLeft.leftShoulder.wasPressedThisFrame || gamepadLeft.rightShoulder.wasPressedThisFrame){
            buttonLeftClicked = GetButtonUnderCursor(cursorLeft);
            // show button pressed effect
            if(buttonLeftClicked != null){
                buttonLeftClicked.onClick.Invoke();
            }
        }

        if(gamepadRight.leftShoulder.wasPressedThisFrame || gamepadRight.rightShoulder.wasPressedThisFrame){
            buttonRightClicked = GetButtonUnderCursor(cursorRight);
            // show button pressed effect
            if(buttonRightClicked != null){
                buttonRightClicked.onClick.Invoke();
            }
        }
        
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
        return new Tuple<bool, bool>(buttonLeftClicked == correctAnswer, buttonRightClicked == correctAnswer);
    }

}
