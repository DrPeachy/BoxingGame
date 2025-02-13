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
        }
    }

    // **处理玩家输入**
    public async UniTaskVoid HandlePlayerAction(int playerIndex, string hand, string action)
    {
        Debug.Log($"接收到 玩家 {playerIndex} 的输入：{hand} 手 {action}");
        // **处理游戏逻辑（例如：攻击、移动等）**+
        // get punch state
        int handIndex = hand == "left" ? 0 : 1;
        PlayerState playerState = playerStates[playerIndex];
        PunchState punchState = playerState.punchStates[handIndex];
        // get opponent's punch state
        int opponentIndex = playerIndex == 0 ? 1 : 0;
        if (!playerStates.ContainsKey(opponentIndex)) return;
        PunchState opponentPunchState = playerStates[opponentIndex].punchStates[handIndex];


        /// handle action
        if(punchState != PunchState.Idle && (action == "Punch" || action == "Charge")) return;

        // handle punch charge
        if(punchState == PunchState.Idle && action == "Charge"){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.HookCharge;
            playerStates[playerIndex].chargeTime = 0;

            while(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge && playerStates[playerIndex].chargeTime < hookChargeDuration){
                playerStates[playerIndex].chargeTime += Time.deltaTime;
                await UniTask.Yield();
            }

            if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.HookChargeComplete;
            }
        }

        // handle land a punch
        if(punchState == PunchState.Idle && action == "Punch"){
            // if charge is not complete, do straight punch
            if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.StraightPunch;
                Debug.Log($"玩家 {playerIndex} 的 {hand} 手发动了直拳");
                await UniTask.Delay((int)(straightPunchWindup * 1000));

                // dealing damage
                if(opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry){
                    // take damage
                }else if(opponentPunchState == PunchState.Parry){
                    // parry
                }else{
                    // block
                }

                // recovery
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
                await UniTask.Delay((int)(straightPunchRecovery * 1000));

                // idle
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
            }else if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookChargeComplete){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.HookPunch;
                Debug.Log($"玩家 {playerIndex} 的 {hand} 手发动了钩拳");
                await UniTask.Delay((int)(hookPunchWindup * 1000));

                // dealing damage
                if(opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry){
                    // take damage
                }else if(opponentPunchState == PunchState.Parry){
                    // parry
                }else{
                    // block
                }

                // recovery
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
                await UniTask.Delay((int)(hookPunchRecovery * 1000));

                // idle
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
            }
        }

        // handle block
        if(punchState == PunchState.Idle && action == "Block"){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Block;
            Debug.Log($"玩家 {playerIndex} 的 {hand} 手举起了防御");
            _= StartParry(playerIndex, handIndex);
            // holding block
            while(playerStates[playerIndex].punchStates[handIndex] == PunchState.Block){
                await UniTask.Yield();
            }
        }

        // handle end charge
        if((punchState == PunchState.HookCharge || punchState == PunchState.HookChargeComplete) && 
            action == "EndCharge"
        ){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
            await UniTask.Delay((int)(blockRecovery * 1000));
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
        }

        // handle end block
        if((punchState == PunchState.Block || punchState == PunchState.Parry) && action == "EndBlock"){
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
            await UniTask.Delay((int)(blockRecovery * 1000));
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
        }


    }

    // **通知所有玩家（广播事件）**
    public void NotifyAllPlayers(string message)
    {
        foreach (var player in players.Values)
        {
            //player.ReceiveGameEvent(message);
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
}
