using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

public class PlayerControllerNetwork : NetworkBehaviour
{
    public int PlayerIndex;
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

    private Dictionary<string, List<string>> inputSequences = new Dictionary<string, List<string>>
    {
        { "Left", new List<string>() },
        { "Right", new List<string>() }
    };
    PlayerState myState;

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

    // Components
    private ControllerAction playerControlAction;
    private PlayerViewNetwork playerView;

    public override void OnStartClient()
    {
        base.OnStartClient();
        InitializePlayer();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        DisableInput();
    }

    private void InitializePlayer(){
        // set player index
        PlayerIndex = OwnerId;
        Debug.Log($"Client: Player[{PlayerIndex}] started network");

        // set player controller
        playerControlAction = new ControllerAction();

        // initialize input sequences
        inputSequences["l"] = new List<string>();
        inputSequences["r"] = new List<string>();

        // initialize my state
        myState = new PlayerState(PunchState.Idle, PunchState.Idle);

        // get player viewer
        playerView = GetComponent<PlayerViewNetwork>();

        // register player
        OnlineModeGameManager.Instance.RegisterPlayer(this);

        // set player transform
        playerView.SetPlayerTransform(PlayerIndex);

        // check if player is local
        if (base.Owner.IsLocalClient)
        {
            // enable player input
            EnableInput();
        }
        else
        {
            DisableInput();
            playerView.DisableCamera();
        }
    }

    private void EnableInput()
    {
        // enable player input
        playerControlAction.Enable();
        playerControlAction.PlayerControl.LStick.performed += OnInputPerformed;
        playerControlAction.PlayerControl.RStick.performed += OnInputPerformed;
        playerControlAction.PlayerControl.LStick.canceled += OnInputCanceled;
        playerControlAction.PlayerControl.RStick.canceled += OnInputCanceled;
        playerControlAction.PlayerControl.LTrigger.performed += OnInputPerformed;
        playerControlAction.PlayerControl.RTrigger.performed += OnInputPerformed;
        playerControlAction.PlayerControl.LTrigger.canceled += OnInputCanceled;
        playerControlAction.PlayerControl.RTrigger.canceled += OnInputCanceled;
    }

    private void DisableInput()
    {
        // disable player input
        playerControlAction.Disable();
        playerControlAction.PlayerControl.LStick.performed -= OnInputPerformed;
        playerControlAction.PlayerControl.RStick.performed -= OnInputPerformed;
        playerControlAction.PlayerControl.LStick.canceled -= OnInputCanceled;
        playerControlAction.PlayerControl.RStick.canceled -= OnInputCanceled;
        playerControlAction.PlayerControl.LTrigger.performed -= OnInputPerformed;
        playerControlAction.PlayerControl.RTrigger.performed -= OnInputPerformed;
        playerControlAction.PlayerControl.LTrigger.canceled -= OnInputCanceled;
        playerControlAction.PlayerControl.RTrigger.canceled -= OnInputCanceled;
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        // early return
        if (!base.Owner.IsLocalClient) return;

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
                // store input sequence(if empty / current input is different from last input)
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
            else if (action == "trigger")
            {
                StartBlock(hand);
            }
        }
    }

    private void OnInputCanceled(InputAction.CallbackContext context)
    {
        // early return
        if (!base.Owner.IsLocalClient) return;

        string key = context.control.name;
        if (buttonMappings.ContainsKey(key))
        {
            string[] strInputs = buttonMappings[key].Split('-');
            string hand = strInputs[0];
            string action = strInputs[1];
            if (action == "stick")
            {
                EndPunch(hand);
            }
            else if (action == "trigger")
            {
                EndBlock(hand);
            }
        }
    }


    private void StartCharge(string hand)
    {
        if (IsOwner){
            OnlineModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Charge");
            playerView.AnimateCharge(hand, 0.1f);
            _= StartChargeTimer(hand);
        }
    }

    private void StartPunch(string hand)
    {
        if (IsOwner){
            OnlineModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Punch");
            playerView.AnimatePunch(hand, straightPunchWindup);

            // do animation
            int handIndex = hand == "l" ? 0 : 1;
            if(myState.punchStates[handIndex] == PunchState.HookChargeComplete){
                myState.punchStates[handIndex] = PunchState.HookPunch;
                playerView.AnimatePunch(hand, hookPunchWindup);
            }else{
                myState.punchStates[handIndex] = PunchState.StraightPunch;
                playerView.AnimatePunch(hand, straightPunchWindup);
            }
        }
    }

    private async UniTask StartBlock(string hand)
    {
        if (IsOwner){
            OnlineModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "Block");
            // update state
            int handIndex = hand == "l" ? 0 : 1;
            myState.punchStates[handIndex] = PunchState.Block;
            // do animation
            playerView.AnimateBlock(hand);
        }
    }

    private async UniTask EndPunch(string hand)
    {
        if (IsOwner){
            OnlineModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "CancelCharge");
            int handIndex = hand == "l" ? 0 : 1;
            // do animation & update state
            if (myState.punchStates[handIndex] == PunchState.StraightPunch){
                myState.punchStates[handIndex] = PunchState.Recovery;
                playerView.AnimateRecovery(hand, straightPunchRecovery);
            }else if(myState.punchStates[handIndex] == PunchState.HookPunch){
                myState.punchStates[handIndex] = PunchState.Recovery;
                playerView.AnimateRecovery(hand, hookPunchRecovery);
            }

        }

    }

    private void EndBlock(string hand)
    {
        if (IsOwner){
            OnlineModeGameManager.Instance.HandlePlayerAction(PlayerIndex, hand, "CancelBlock");
            int handIndex = hand == "l" ? 0 : 1;
            // do animation & update state
            if (myState.punchStates[handIndex] == PunchState.Block){
                myState.punchStates[handIndex] = PunchState.Recovery;
                playerView.AnimateRecovery(hand, blockRecovery);
            }
        }
    }

    private async UniTask StartChargeTimer(string hand)
    {
        int handIndex = hand == "l" ? 0 : 1;
        if(myState.punchStates[handIndex] == PunchState.Idle){
            myState.punchStates[handIndex] = PunchState.HookCharge;
            while(myState.chargeTimes[handIndex] < hookChargeDuration && 
                myState.punchStates[handIndex] == PunchState.HookCharge)
            {
                myState.chargeTimes[handIndex] += Time.deltaTime;
                await UniTask.Yield();
            }
            if(myState.punchStates[handIndex] == PunchState.HookCharge)
                myState.punchStates[handIndex] = PunchState.HookChargeComplete;
        }
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

    [ObserversRpc]
    public void ReceiveGameEvent(string message, float d = 0)
    {   
        _ = ProcessReceiveGameEvent(message, d);
    }


    public async UniTaskVoid ProcessReceiveGameEvent(string message, float d = 0)
    {
        Debug.Log($"Client: Player[{PlayerIndex}] Received Game Event: {message}");
        string[] msg = message.Split('-');
        // example string "0-1-recovery"
        string pIndex = msg[0];
        if (pIndex != $"{PlayerIndex}") return;
        string hand = msg[1];
        string action = msg[2];

        if (action == "Recovery")
        {
            //punchStates[hand] = PunchState.Recovery;
            //playerView.AnimateRecovery(hand, d);
        }
        else if (action == "Charge")
        {
            //punchStates[hand] = PunchState.HookCharge;
            //playerView.AnimateCharge(hand, d);
        }
        else if (action == "ChargeComplete")
        {
            //punchStates[hand] = PunchState.HookChargeComplete;
        }
        else if (action == "Straight")
        {
            //punchStates[hand] = PunchState.StraightPunch;
            //playerView.AnimatePunch(hand, d);
        }
        else if (action == "Hook")
        {
            //punchStates[hand] = PunchState.HookPunch;
            //playerView.AnimatePunch(hand, d);
        }
        else if (action == "Block")
        {
            //punchStates[hand] = PunchState.Block;
            //playerView.AnimateBlock(hand);
        }
        else if (action == "Idle"){
            //punchStates[hand] = PunchState.Idle;
            //playerView.ResetGloves(hand);
        }

        await UniTask.Yield();
    }
}
