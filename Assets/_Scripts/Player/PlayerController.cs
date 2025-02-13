using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DG.Tweening;
using UnityEngine.SceneManagement;
using TMPro;
using Cysharp.Threading.Tasks.CompilerServices;

public class PlayerController : MonoBehaviour
{
    public int PlayerIndex { get; private set; }
    private PlayerViewer playerViewer;

    private readonly Dictionary<Vector2, string> directionMap = new Dictionary<Vector2, string>
    {
        { Vector2.left, "Left" },
        { Vector2.right, "Right" },
        { Vector2.up, "Up" },
        { Vector2.down, "Down" },
        { new Vector2(-1, 1), "LeftUp" },
        { new Vector2(1, 1), "RightUp" },
        { new Vector2(-1, -1), "LeftDown" },
        { new Vector2(1, -1), "RightDown" }
    };

    // Button mappings
    private Dictionary<string, string> buttonMappings = new Dictionary<string, string>
    {
        { "leftStick", "l-punch" },
        { "rightStick", "r-punch" },
        { "leftTrigger", "l-block" },
        { "rightTrigger", "r-block" }
    };

    private Dictionary<string, PunchState> punchStates = new Dictionary<string, PunchState>
    {
        { "Left", PunchState.Idle },
        { "Right", PunchState.Idle }
    };

    private Dictionary<string, List<string>> inputSequences = new Dictionary<string, List<string>>
    {
        { "Left", new List<string>() },
        { "Right", new List<string>() }
    };

    //public event Action<PunchAction> OnPunchPerformed;

    [Header("Straight Punch Settings")]
    public float straightPunchWindup = 0.5f;
    public float straightPunchRecovery = 0.3f;
    public float straightPunchDamage = 5f;
    public float straightInterruptTime = 0.5f;

    [Header("Hook Punch Settings")]
    public float hookChargeDuration = 0.8f;
    public float hookPunchWindup = 0.7f;
    public float hookPunchRecovery = 0.4f;
    public float hookPunchDamage = 7f;

    [Header("Block Settings")]
    public float blockRecovery = 0.25f;
    public float parryDuration = 0.25f;
    public float parryRecovery = 0.9f;
    public float blockDamageReduction = 5f;

    [Header("Animation Settings")]
    public float punchMovementFactor = 15f;
    [Header("Stage Settings")]
    public float stageMovingFactor = 0.1f;

    [Header("Combos")]
    private readonly string[] leftHookCombo = { "Left", "LeftUp", "Up" };
    private readonly string[] rightHookCombo = { "Right", "RightUp", "Up" };

    private PlayerInput playerInput;
    private InputActionMap actionMap;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>(); // ✅ 获取当前 Player 的 PlayerInput 组件
        actionMap = playerInput.actions.FindActionMap("PlayerControl"); // ✅ 获取 PlayerControls ActionMap

        // set player index
        PlayerIndex = playerInput.playerIndex;

        // initialize input sequences
        inputSequences["l"] = new List<string>();
        inputSequences["r"] = new List<string>();

        // get player viewer
        playerViewer = GetComponent<PlayerViewer>();

        // register player
        LocalModeGameManager.Instance.RegisterPlayer(this);

        // set player transform
        playerViewer.SetPlayerTransform(PlayerIndex);
    }

    private void OnEnable()
    {

        actionMap.Enable();
        actionMap.FindAction("LStick").performed += OnInputPerformed;
        actionMap.FindAction("RStick").performed += OnInputPerformed;
        actionMap.FindAction("LStick").canceled += OnInputCanceled;
        actionMap.FindAction("RStick").canceled += OnInputCanceled;
        actionMap.FindAction("LTrigger").performed += OnInputPerformed;
        actionMap.FindAction("RTrigger").performed += OnInputPerformed;
        actionMap.FindAction("LTrigger").canceled += OnInputCanceled;
        actionMap.FindAction("RTrigger").canceled += OnInputCanceled;
    }

    private void OnDisable()
    {
        actionMap.Disable();
        actionMap.FindAction("LStick").performed -= OnInputPerformed;
        actionMap.FindAction("RStick").performed -= OnInputPerformed;
        actionMap.FindAction("LStick").canceled -= OnInputCanceled;
        actionMap.FindAction("RStick").canceled -= OnInputCanceled;
        actionMap.FindAction("LTrigger").performed -= OnInputPerformed;
        actionMap.FindAction("RTrigger").performed -= OnInputPerformed;
        actionMap.FindAction("LTrigger").canceled -= OnInputCanceled;
        actionMap.FindAction("RTrigger").canceled -= OnInputCanceled;
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        string key = context.control.name;
        if (buttonMappings.ContainsKey(key))
        {
            string[] strInputs = buttonMappings[key].Split('-');
            string hand = strInputs[0];
            string action = strInputs[1];
            Vector2 input = Vector2.zero;
            string direction = "";
            if (action == "punch")
            {
                input = context.ReadValue<Vector2>();
                direction = GetDirection(input);
                // store input sequence
                if (inputSequences[hand].Count == 0 || inputSequences[hand][inputSequences[hand].Count - 1] != direction)
                {
                    inputSequences[hand].Add(direction);
                    if ((hand == "l" && (direction == "Left" || direction == "LeftDown")) ||
                        (hand == "r" && (direction == "Right" || direction == "RightDown")))
                    {
                        StartCharge(hand);
                    }
                    else if ((hand == "l" && (direction == "Up" || direction == "RightUp" || direction == "LeftUp")) ||
                        (hand == "r" && (direction == "Up" || direction == "RightUp" || direction == "LeftUp")))
                    {
                        StartPunch(hand);
                    }
                }
            }
            else if (action == "block")
            {
                StartBlock(hand);
            }
        }
    }

    private void OnInputCanceled(InputAction.CallbackContext context)
    {
        string key = context.control.name;
        if (buttonMappings.ContainsKey(key))
        {
            string[] strInputs = buttonMappings[key].Split('-');
            string hand = strInputs[0];
            string action = strInputs[1];
            Vector2 input = context.ReadValue<Vector2>();
            string direction = GetDirection(input);

            // store input sequence
            if (inputSequences[hand].Count == 0 || inputSequences[hand][inputSequences[hand].Count - 1] != direction)
            {
                inputSequences[hand].Add(direction);
            }

            // process input
            if (action == "punch")
            {
                if ((hand == "left" && (direction == "Left" || direction == "LeftDown")) ||
                    (hand == "right" && (direction == "Right" || direction == "RightDown")))
                {
                    EndPunch(hand);
                }
            }
            else if (action == "block")
            {
                if ((hand == "left" && (direction == "Left" || direction == "LeftDown")) ||
                    (hand == "right" && (direction == "Right" || direction == "RightDown")))
                {
                    EndBlock(hand);
                }
            }
        }
    }


    private void StartCharge(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Charge");
    }

    private void StartPunch(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Punch");

    }

    private void EndPunch(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "EndPunch");
    }

    private void StartBlock(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Block");
    }

    private void EndBlock(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "EndBlock");
    }











    private void OnLeftPunchPerformed(InputAction.CallbackContext context)
    {
        HandlePunchInput(context, "Left").Forget();
    }

    private void OnRightPunchPerformed(InputAction.CallbackContext context)
    {
        HandlePunchInput(context, "Right").Forget();
    }

    private void OnLeftPunchCanceled(InputAction.CallbackContext context)
    {
        HandlePunchCancel("Left").Forget();
    }

    private void OnRightPunchCanceled(InputAction.CallbackContext context)
    {
        HandlePunchCancel("Right").Forget();
    }

    private void OnLeftBlockPerformed(InputAction.CallbackContext context)
    {
        HandleBlockInput("Left").Forget();
    }

    private void OnRightBlockPerformed(InputAction.CallbackContext context)
    {
        HandleBlockInput("Right").Forget();
    }

    private async UniTaskVoid HandlePunchInput(InputAction.CallbackContext context, string hand)
    {
        if (punchStates[hand] != PunchState.Idle && punchStates[hand] != PunchState.HookCharge) return;

        Vector2 input = context.ReadValue<Vector2>();
        string direction = GetDirection(input);

        if (direction == "Neutral" ||
            (hand == "Left" && direction == "Right") ||
            (hand == "Right" && direction == "Left") ||
            (direction == "Down") ||
            (hand == "Left" && direction == "RightDown") ||
            (hand == "Right" && direction == "LeftDown"))
        {
            return;
        }

        // Add direction to the sequence
        if (inputSequences[hand].Count == 0 || inputSequences[hand][inputSequences[hand].Count - 1] != direction)
        {
            inputSequences[hand].Add(direction);
            //UIManager.Instance.AddArrow(hand, direction);
        }

        Debug.Log($"{hand} Input Sequence: {string.Join(", ", inputSequences[hand])}");

        // 判断是否触发蓄力
        if ((direction == "Left" || direction == "LeftDown") && hand == "Left" && punchStates[hand] == PunchState.Idle)
        {
            StartHookCharge(hand, input);
        }
        else if ((direction == "Right" || direction == "RightDown") && hand == "Right" && punchStates[hand] == PunchState.Idle)
        {
            StartHookCharge(hand, input);
        }

        // 勾拳蓄力显示
        if (((direction == "Left" || direction == "LeftDown") && hand == "Left") || ((direction == "Right" || direction == "RightDown") && hand == "Right"))
        {
            //OnPunchPerformed?.Invoke(new PunchAction(hand, "HookCharge", input, 0f));
        }

        // Check for punch end
        if (direction == "Up" || direction == "RightUp" || direction == "LeftUp")
        {
            if (inputSequences[hand].Count == 1 && punchStates[hand] == PunchState.Idle)
            { // straight punch
                await PerformStraightPunch(hand);
            }
            else if (punchStates[hand] == PunchState.HookCharge &&
                (hand == "Left" && (direction == "Up" || direction == "RightUp")) ||
                (hand == "Right" && (direction == "Up" || direction == "LeftUp"))
            )
            { // hook punch
                if (MatchesCombo(inputSequences[hand], hand == "Left" ? leftHookCombo : rightHookCombo))
                {
                    await PerformHookPunch(hand, input);
                }
                else
                {
                    Debug.LogWarning($"{hand} Input did not match any combo");
                    //OnPunchPerformed?.Invoke(new PunchAction(hand, "Idle", Vector2.zero, 0, 0.001f));
                    //UIManager.Instance.AddArrow(hand, "Neutral");
                }

                inputSequences[hand].Clear();
            }
        }
    }

    private async UniTaskVoid HandleBlockInput(string hand)
    {
        if (punchStates[hand] != PunchState.Idle && punchStates[hand] != PunchState.HookCharge) return;
        Debug.Log($"{hand} Block!");
        //OnPunchPerformed?.Invoke(new PunchAction(hand, "Block", Vector2.zero));

        await PerformBlock(hand);

    }

    private void StartHookCharge(string hand, Vector2 input)
    {
        punchStates[hand] = PunchState.HookCharge;
        //OnPunchPerformed?.Invoke(new PunchAction(hand, "HookCharge", input, Time.time));
        Debug.Log($"{hand} Hook Charge Started");
        // punchStates[hand] = PunchState.HookChargeComplete;
    }

    private async UniTaskVoid HandlePunchCancel(string hand)
    {
        Debug.Log($"{hand} Stick Canceled");
        if (punchStates[hand] != PunchState.HookCharge)
        {
            return;
        }
        // 设置状态为 Idle
        punchStates[hand] = PunchState.Idle;

        // UI 箭头显示为中立
        //UIManager.Instance.AddArrow(hand, "Neutral");

        // 通知外部切换到 Idle 动画
        //OnPunchPerformed?.Invoke(new PunchAction(hand, "Idle", Vector2.zero, 0, 0.001f));

        // 清空输入序列
        inputSequences[hand].Clear();

        await UniTask.Yield(); // 确保异步流程完成
    }

    private async UniTask PerformStraightPunch(string hand)
    {
        punchStates[hand] = PunchState.StraightPunch;

        Debug.Log($"{hand} Straight Punch!");

    }

    private async UniTask PerformHookPunch(string hand, Vector2 input)
    {
        punchStates[hand] = PunchState.HookPunch;

        Debug.Log($"{hand} Hook Punch!");

    }

    private async UniTask PerformBlock(string hand)
    {
        punchStates[hand] = PunchState.Block;

        Debug.Log($"{hand} Block!");

    }

    private string GetDirection(Vector2 input)
    {
        if (input.magnitude < 0.4f) return "Neutral";

        input.Normalize();
        float angle = Vector2.SignedAngle(Vector2.up, input);
        foreach (var pair in directionMap)
        {
            float dirAngle = Vector2.SignedAngle(Vector2.up, pair.Key);
            if (Mathf.Abs(Mathf.DeltaAngle(angle, dirAngle)) < 22.5f)
            {
                return pair.Value;
            }
        }

        return "Neutral";
    }

    private bool MatchesCombo(List<string> sequence, string[] combo)
    {
        Debug.Log($"Checking Combo: {string.Join(", ", sequence)}");
        Debug.Log($"Combo: {string.Join(", ", combo)}");
        if (sequence.Count < combo.Length) return false;
        for (int i = 0; i < combo.Length; i++)
        {
            if (sequence[sequence.Count - combo.Length + i] != combo[i])
            {
                return false;
            }
        }

        return true;
    }
}
