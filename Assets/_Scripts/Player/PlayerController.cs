using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks.CompilerServices;

public class PlayerController : MonoBehaviour
{
    public int PlayerIndex { get; private set; }
    public bool isUsedForPreview = false;
    private PlayerView playerView;

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
        { "leftStick", "l-stick" },
        { "rightStick", "r-stick" },
        { "leftTrigger", "l-trigger" },
        { "rightTrigger", "r-trigger" }
    };

    // private Dictionary<string, PunchState> punchStates = new Dictionary<string, PunchState>
    // {
    //     { "Left", PunchState.Idle },
    //     { "Right", PunchState.Idle }
    // };

    private PlayerState myState;

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

    public InputCache inputCache;

    private void Awake()
    {
        if (isUsedForPreview) return;

        playerInput = GetComponent<PlayerInput>(); // ✅ 获取当前 Player 的 PlayerInput 组件
        actionMap = playerInput.actions.FindActionMap("PlayerControl"); // ✅ 获取 PlayerControls ActionMap

        // set player index
        PlayerIndex = playerInput.playerIndex;

        // initialize player state
        myState = new PlayerState(PunchState.Idle, PunchState.Idle);

        // initialize input sequences
        inputSequences["l"] = new List<string>();
        inputSequences["r"] = new List<string>();

        // get player viewer
        playerView = GetComponent<PlayerView>();

        // register player
        LocalModeGameManager.Instance.RegisterPlayer(this);

        // set cull off layer
        playerView.SetCullLayer(PlayerIndex);

        // set player transform
        playerView.SetPlayerTransform(PlayerIndex);

        // get input cache
        inputCache = GetComponent<InputCache>();
    }

    private void OnEnable()
    {
        if (isUsedForPreview) return;
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
        if (isUsedForPreview) return;
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
            if (action == "stick")
            {
                input = context.ReadValue<Vector2>();
                direction = GetDirection(input);
                // store input sequence
                if (inputSequences[hand].Count == 0 || inputSequences[hand][inputSequences[hand].Count - 1] != direction)
                {
                    inputSequences[hand].Add(direction);
                    Debug.Log($"Input Sequence: {string.Join(", ", inputSequences[hand])}");
                    if ((hand == "l" && (direction == "Left" || direction == "LeftDown" || direction == "Down")) ||
                        (hand == "r" && (direction == "Right" || direction == "RightDown" || direction == "Down")))
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
            else if (action == "trigger")
            {
                inputCache.HoldAction(hand, "Block");
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
            if (action == "stick")
            {
                inputSequences[hand].Clear();
                EndPunch(hand);
            }
            else if (action == "trigger")
            {
                inputCache.ReleaseAction(hand, "Block");
                EndBlock(hand);
            }
        }
    }


    private void StartCharge(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Charge");
        inputCache.PushAction(hand, "Charge");
    }

    private void StartPunch(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Punch");
        inputCache.PushAction(hand, "Punch");
    }

    private void EndPunch(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "CancelCharge");
    }

    private void StartBlock(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Block");
        inputCache.PushAction(hand, "Block");
    }

    private void EndBlock(string hand)
    {
        _ = LocalModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "CancelBlock");
    }

    private string GetDirection(Vector2 input)
    {
        Debug.Log($"Input: {input}, Magnitude: {input.magnitude}");
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

    public async UniTaskVoid ReceiveGameEvent(string message, float d = 0){
        Debug.Log($"Player {PlayerIndex} received message: {message}");
        string[] msg = message.Split('-');
        // example string "0-1-recovery"
        string pIndex = msg[0];
        if(pIndex != $"{PlayerIndex}") return;
        string hand = msg[1];
        int handIndex = hand == "l" ? 0 : 1;
        string action = msg[2];

        if(action == "Recovery"){
            myState.punchStates[handIndex] = PunchState.Recovery;
            playerView.AnimateRecovery(hand, d);
        }else if(action == "Straight"){
            myState.punchStates[handIndex] = PunchState.StraightPunch;
            playerView.AnimatePunch(hand, d);
        }else if(action == "Hook"){
            myState.punchStates[handIndex] = PunchState.HookPunch;
            playerView.AnimatePunch(hand, d);
        }else if(action == "Parry"){
            myState.punchStates[handIndex] = PunchState.Parry;
            playerView.AnimateBlock(hand);
        }else if(action == "Charge"){
            myState.punchStates[handIndex] = PunchState.HookCharge;
            playerView.AnimateCharge(hand, d);
        }else if(action == "ChargeComplete"){
            myState.punchStates[handIndex] = PunchState.HookChargeComplete;
            playerView.AnimateChargeComplete(hand);
        }else if(action == "Idle"){
            myState.punchStates[handIndex] = PunchState.Idle;
            playerView.ResetGloves(hand);
        }

        await UniTask.Yield();
    }
}
