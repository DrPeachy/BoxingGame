using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class CursorController : MonoBehaviour
{
    public RectTransform cursorLeft;
    public RectTransform cursorRight;

    public float cursorSpeed = 1f;

    public Camera mainCamera;

    private Vector2 cursorLeftPosition;
    private Vector2 cursorRightPosition;

    private Button buttonLeftClicked;
    private Button buttonRightClicked;

    private void Start()
    {
        cursorLeftPosition = cursorLeft.anchoredPosition;
        cursorRightPosition = cursorRight.anchoredPosition;
    }

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;

        Vector2 leftStick = gamepad.leftStick.ReadValue();
        Vector2 rightStick = gamepad.rightStick.ReadValue();

        cursorLeftPosition += leftStick * cursorSpeed * Time.deltaTime;
        cursorRightPosition += rightStick * cursorSpeed * Time.deltaTime;

        cursorLeft.anchoredPosition = ClampToScreen(cursorLeftPosition);
        cursorRight.anchoredPosition = ClampToScreen(cursorRightPosition);

        if(gamepad.leftShoulder.wasPressedThisFrame){
            buttonLeftClicked = GetButtonUnderCursor(cursorLeft);
        }

        if(gamepad.rightShoulder.wasPressedThisFrame){
            buttonRightClicked = GetButtonUnderCursor(cursorRight);
        }

        if(buttonLeftClicked != null && buttonRightClicked != null && buttonLeftClicked == buttonRightClicked){
            Debug.Log("A button was clicked by both cursors!");
            // invoke an answer selected event

        }
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


}
