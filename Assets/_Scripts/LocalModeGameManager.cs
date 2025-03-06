using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FishNet.Demo.AdditiveScenes;
using UnityEngine;

public class LocalModeGameManager : MonoBehaviour
{
    public static LocalModeGameManager Instance { get; private set; }
    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    private Dictionary<int, PlayerState> playerStates = new Dictionary<int, PlayerState>();
    public int PlayerCount => players.Count;
    public bool isGameModeLocal = true;

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
            Debug.Log($"玩家 {player.PlayerIndex} 加入游戏");

            // 初始化玩家状态
            playerStates[player.PlayerIndex] = new PlayerState(PunchState.Idle, PunchState.Idle);

            // update game state
            GameStateManager.Instance.PlayerJoined();
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
            }
        }

        // handle land a punch
        if(action == "Punch"){
            // if charge is not complete, do straight punch
            if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge || playerStates[playerIndex].punchStates[handIndex] == PunchState.Idle){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.StraightPunch;
                Debug.Log($"玩家 {playerIndex} 的 {hand} 手发动了直拳");

                NotifyAllPlayers($"{playerIndex}-{hand}-Straight", straightPunchWindup * 0.9f);

                await UniTask.Delay((int)(straightPunchWindup * 1000));

                // dealing damage
                if(opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry){
                    // take damage
                    playerStates[opponentIndex].damageTaken += straightPunchDamage;
                }else if(opponentPunchState == PunchState.Parry){
                    // parry
                    _= Interrupt(opponentIndex, handIndex);
                }else{
                    // block
                    playerStates[opponentIndex].damageTaken += straightPunchDamage - blockDamageReduction;
                }

                _= Interrupt(opponentIndex, opponentHandIndex);

                // recovery
                NotifyAllPlayers($"{playerIndex}-{hand}-Recovery", straightPunchRecovery * 0.9f);
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
                await UniTask.Delay((int)(straightPunchRecovery * 1000));

                // idle
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
            }else if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookChargeComplete){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.HookPunch;
                Debug.Log($"玩家 {playerIndex} 的 {hand} 手发动了钩拳");

                NotifyAllPlayers($"{playerIndex}-{hand}-Hook", hookPunchWindup * 0.9f);

                // windup
                await UniTask.Delay((int)(hookPunchWindup * 1000));

                // dealing damage
                if(opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry){
                    // take damage
                    playerStates[opponentIndex].damageTaken += hookPunchDamage;
                }else if(opponentPunchState == PunchState.Parry){
                    // parry
                    _= Interrupt(opponentIndex, handIndex);
                }else{
                    // block
                    playerStates[opponentIndex].damageTaken += hookPunchDamage - blockDamageReduction;
                }

                // recovery
                NotifyAllPlayers($"{playerIndex}-{hand}-Recovery", hookPunchRecovery * 0.9f);
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
                await UniTask.Delay((int)(hookPunchRecovery * 1000));

                // idle
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
            }
        }

        // handle block
        else if(punchState == PunchState.Idle && action == "Block"){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Block;
            NotifyAllPlayers($"{playerIndex}-{hand}-Block");
            Debug.Log($"玩家 {playerIndex} 的 {hand} 手举起了防御");
            _= StartParry(playerIndex, handIndex);
            // holding block
            while(playerStates[playerIndex].punchStates[handIndex] == PunchState.Block){
                await UniTask.Yield();
            }
        }

        // handle end charge
        else if((punchState == PunchState.HookCharge || punchState == PunchState.HookChargeComplete) && 
            action == "CancelCharge"
        ){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
            NotifyAllPlayers($"{playerIndex}-{hand}-Recovery", blockRecovery * 0.9f);
            await UniTask.Delay((int)(blockRecovery * 1000));
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
        }

        // handle end block
        else if((punchState == PunchState.Block || punchState == PunchState.Parry) && action == "CancelBlock"){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
            NotifyAllPlayers($"{playerIndex}-{hand}-Recovery", blockRecovery * 0.9f);
            await UniTask.Delay((int)(blockRecovery * 1000));
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
        }


    }

    // **通知所有玩家（广播事件）**
    public void NotifyAllPlayers(string message, float d = 0)
    {
        foreach (var player in players.Values)
        {
            _= player.ReceiveGameEvent(message, d);
        }
    }


    ////////////
    /// 以下为游戏逻辑
    
    private async UniTaskVoid StartParry(int player, int hand)
    {
        var state = playerStates[player];
        var handState = state.punchStates[hand];

        if (handState != PunchState.Block) return;

        playerStates[player].punchStates[hand] = PunchState.Parry;
        await UniTask.Delay((int)(parryDuration * 1000));

        if (playerStates[player].punchStates[hand] == PunchState.Parry)
        {
            playerStates[player].punchStates[hand] = PunchState.Block;
        }
    }

    private async UniTask SetToRecovery(int player, int hand, float duration)
    {
        // set recovery state
        playerStates[player].punchStates[hand] = PunchState.Recovery;
        NotifyAllPlayers($"{player}-{hand}-Recovery", duration * 0.9f);
        await UniTask.Delay((int)(duration * 1000));

        // set to idle
        playerStates[player].punchStates[hand] = PunchState.Idle;
        NotifyAllPlayers($"{player}-{hand}-Idle");

    }

    private async UniTaskVoid Interrupt(int player, int hand){
        // interrupt player's hook charge
        if(playerStates[player].punchStates[hand] == PunchState.HookCharge || playerStates[player].punchStates[hand] == PunchState.HookChargeComplete){
            await SetToRecovery(player, hand, straightInterruptTime);
        }

        // target player's being interrupted by parring
        else if(playerStates[player].punchStates[hand] == PunchState.HookPunch || playerStates[player].punchStates[hand] == PunchState.StraightPunch){
            await SetToRecovery(player, hand, parryRecovery);
        }
        
        await UniTask.Yield();
    }
}
