using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet.Demo.AdditiveScenes;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LocalModeGameManager : MonoBehaviour
{
    public static LocalModeGameManager Instance { get; private set; }
    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    private Dictionary<int, PlayerState> playerStates = new Dictionary<int, PlayerState>();
    public Dictionary<int, PlayerInput> playerInputs = new Dictionary<int, PlayerInput>();
    public int PlayerCount => players.Count;
    public bool isGameModeLocal = true;

    [Header("UI")]
    public Transform playerStateTexts;
    private Dictionary<int, TMP_Text> playerStateTextsDict = new Dictionary<int, TMP_Text>();

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
    public float blockDamageReduction = 4f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 玩家加入时注册
    public void RegisterPlayer(PlayerController player)
    {
        if (!players.ContainsKey(player.PlayerIndex))
        {
            players[player.PlayerIndex] = player;
            playerInputs[player.PlayerIndex] = player.GetComponent<PlayerInput>();
            CursorController.Instance.AddPlayerInput(player.PlayerIndex, playerInputs[player.PlayerIndex]);
            Debug.Log($"玩家 {player.PlayerIndex} 加入游戏");

            // 初始化玩家状态
            playerStates[player.PlayerIndex] = new PlayerState(PunchState.Idle, PunchState.Idle);

            // update game state
            GameStateManager.Instance.PlayerJoined();

            // if player size is 2, initialize tmp texts
            if (players.Count == 2)
            {
                foreach (Transform child in playerStateTexts)
                {
                    TMP_Text text = child.GetComponent<TMP_Text>();
                    playerStateTextsDict[int.Parse(child.name)] = text;
                }
                
            }
        }
    }

    // enable player's input
    public void EnablePlayersInput()
    {
        // loop over player controllers and enable input
        foreach (PlayerController player in players.Values)
        {
            player.enabled = true;
        }
    }

    // disable player's input
    public void DisablePlayersInput()
    {
        // loop over player controllers and disable input
        foreach (PlayerController player in players.Values)
        {
            player.enabled = false;
        }
    }


    // **处理玩家输入**
    public async UniTaskVoid HandlePlayerAction(int playerIndex, string hand, string action)
    {
        Debug.Log($"接收到 玩家 {playerIndex} 的输入：{hand} 手 {action}");
        // **处理游戏逻辑（例如：攻击、移动等）**+
        // get punch state
        int handIndex = hand == "l" ? 0 : 1;
        PlayerState playerState = playerStates[playerIndex];
        PunchState punchState = playerState.punchStates[handIndex];
        // get opponent's punch state
        int opponentIndex = playerIndex == 0 ? 1 : 0;
        int opponentHandIndex = handIndex == 0 ? 1 : 0;
        if (!playerStates.ContainsKey(opponentIndex)) return;
        PunchState opponentPunchState = playerStates[opponentIndex].punchStates[opponentHandIndex];


        /// handle action
        if(punchState != PunchState.Idle && (action == "Charge")) return;

        // handle punch charge
        if(punchState == PunchState.Idle && action == "Charge"){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.HookCharge;
            playerStates[playerIndex].chargeTimes[handIndex] = 0;
            NotifyAllPlayers($"{playerIndex}-{hand}-Charge");
            AudioManager.Instance.PlayCharge(); // play charge sound

            // start charge
            while(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge && 
                playerStates[playerIndex].chargeTimes[handIndex] < hookChargeDuration){
                playerStates[playerIndex].chargeTimes[handIndex] += Time.deltaTime;
                await UniTask.Yield();
            }

            // charge complete
            if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.HookChargeComplete;
                NotifyAllPlayers($"{playerIndex}-{hand}-ChargeComplete");
                AudioManager.Instance.PlayChargeComplete();
            }
        }

        // handle land a punch
        if(action == "Punch"){
            // if charge is not complete, do straight punch
            if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge || playerStates[playerIndex].punchStates[handIndex] == PunchState.Idle){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.StraightPunch;
                Debug.Log($"玩家 {playerIndex} 的 {handIndex} 手发动了直拳");
                AudioManager.Instance.PlayWave();
                NotifyAllPlayers($"{playerIndex}-{hand}-Straight", straightPunchWindup * 0.9f);
                AudioManager.Instance.StopCharge();

                await UniTask.Delay((int)(straightPunchWindup * 1000));
                opponentPunchState = playerStates[opponentIndex].punchStates[opponentHandIndex];
                Debug.Log($"对手状态：{opponentPunchState}");
                // dealing damage
                if(opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry){
                    // take damage
                    playerStates[opponentIndex].damageTaken += straightPunchDamage;
                    AudioManager.Instance.PlayPunch();
                }else if(opponentPunchState == PunchState.Parry){
                    // parry
                    _= SetToRecovery(playerIndex, hand, parryRecovery);
                    AudioManager.Instance.PlayParry();
                    return;
                }else if(opponentPunchState == PunchState.Block){
                    // block
                    playerStates[opponentIndex].damageTaken += (straightPunchDamage - blockDamageReduction);
                    Debug.Log($"====blockdamage===={blockDamageReduction}");
                    AudioManager.Instance.PlayPunchBlocked();
                }

                //_= Interrupt(opponentIndex, opponentHandIndex == 0 ? "l" : "r");

                // recovery
                _= SetToRecovery(playerIndex, hand, straightPunchRecovery);

            }else if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookChargeComplete){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.HookPunch;
                Debug.Log($"玩家 {playerIndex} 的 {hand} 手发动了钩拳");
                AudioManager.Instance.PlayWave();
                NotifyAllPlayers($"{playerIndex}-{hand}-Hook", hookPunchWindup * 0.9f);

                // windup
                await UniTask.Delay((int)(hookPunchWindup * 1000));

                // dealing damage
                opponentPunchState = playerStates[opponentIndex].punchStates[opponentHandIndex];
                if(opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry){
                    // take damage
                    playerStates[opponentIndex].damageTaken += hookPunchDamage;
                    AudioManager.Instance.PlayPunch();
                }else if(opponentPunchState == PunchState.Parry){
                    // parry
                    _= SetToRecovery(playerIndex, hand, parryRecovery);
                    AudioManager.Instance.PlayParry();
                    return;
                }else if(opponentPunchState == PunchState.Block){
                    // block
                    playerStates[opponentIndex].damageTaken += (hookPunchDamage - blockDamageReduction);
                    Debug.Log($"====blockdamage===={blockDamageReduction}");
                    AudioManager.Instance.PlayPunchBlocked();
                }

                _ = SetToRecovery(playerIndex, hand, hookPunchRecovery);
            }
        }

        // handle block
        else if(punchState == PunchState.Idle && action == "Block"){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Parry;
            NotifyAllPlayers($"{playerIndex}-{hand}-Parry");
            Debug.Log($"玩家 {playerIndex} 的 {hand} 手举起了防御");
            _= StartParry(playerIndex, hand);
        }

        // handle end charge
        else if((punchState == PunchState.HookCharge || punchState == PunchState.HookChargeComplete) && 
            action == "CancelCharge"
        ){
            AudioManager.Instance.StopCharge();
            _ = SetToRecovery(playerIndex, hand, blockRecovery);
        }

        // handle end block
        else if((punchState == PunchState.Block || punchState == PunchState.Parry) && action == "CancelBlock"){
            _ = SetToRecovery(playerIndex, hand, blockRecovery);
        }
    }

    public void AddDamageToPlayer(int playerIndex, float damage){
        playerStates[playerIndex].damageTaken += damage;
        foreach (var pair in playerStates)
        {
            playerStateTextsDict[pair.Key].text = $"Player {pair.Key + 1} - {pair.Value.punchStates[0]} - {pair.Value.punchStates[1]}\n DamageTaken: {pair.Value.damageTaken}";
        }
    }

    public int GetPlayerWithLessDamageTaken(){
        if(playerStates[0].damageTaken < playerStates[1].damageTaken){
            return 0;
        }else if(playerStates[0].damageTaken > playerStates[1].damageTaken){
            return 1;
        }else{
            return -1;
        }
    }

    // **通知所有玩家（广播事件）**
    public void NotifyAllPlayers(string message, float d = 0)
    {
        foreach (var player in players.Values)
        {
            _= player.ReceiveGameEvent(message, d);
        }

        // put game state on tmp text
        foreach (var pair in playerStates)
        {
            playerStateTextsDict[pair.Key].text = $"Player {pair.Key + 1} - {pair.Value.punchStates[0]} - {pair.Value.punchStates[1]}\n DamageTaken: {pair.Value.damageTaken}";
        }
    }


    ////////////
    /// 以下为游戏逻辑
    
    private async UniTaskVoid StartParry(int player, string hand)
    {
        var handIndex = hand == "l" ? 0 : 1;
        await UniTask.Delay((int)(parryDuration * 1000));

        if (playerStates[player].punchStates[handIndex] == PunchState.Parry)
        {
            playerStates[player].punchStates[handIndex] = PunchState.Block;
            NotifyAllPlayers($"{player}-{hand}-Block");
        }
    }

    private async UniTask SetToRecovery(int player, string hand, float duration)
    {
        int handIndex = hand == "l" ? 0 : 1;
        // set recovery state
        playerStates[player].punchStates[handIndex] = PunchState.Recovery;
        NotifyAllPlayers($"{player}-{hand}-Recovery", duration * 0.9f);
        await UniTask.Delay((int)(duration * 1000));

        // set to idle
        playerStates[player].punchStates[handIndex] = PunchState.Idle;
        NotifyAllPlayers($"{player}-{hand}-Idle");

    }

    private async UniTaskVoid Interrupt(int player, string hand){
        int handIndex = hand == "l" ? 0 : 1;
        // interrupt player's hook charge
        if(playerStates[player].punchStates[handIndex] == PunchState.HookCharge || playerStates[player].punchStates[handIndex] == PunchState.HookChargeComplete){
            await SetToRecovery(player, hand, straightInterruptTime);
        }
        
        await UniTask.Yield();
    }
}
