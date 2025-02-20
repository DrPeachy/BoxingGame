using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using FishNet.Managing;
using FishNet.Object;

public class OnlineModeGameManager : NetworkBehaviour
{
    public static OnlineModeGameManager Instance { get; private set; }
    private Dictionary<int, PlayerControllerNetwork> players = new Dictionary<int, PlayerControllerNetwork>();
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
    public void RegisterPlayer(PlayerControllerNetwork player)
    {
        if (!players.ContainsKey(player.PlayerIndex))
        {
            players[player.PlayerIndex] = player;
            Debug.Log($"Server: Player[{player.PlayerIndex}] joined the game");

            // 初始化玩家状态
            playerStates[player.PlayerIndex] = new PlayerState(PunchState.Idle, PunchState.Idle);

            // update game state
            if(IsServerInitialized) GameStateManager.Instance.PlayerJoined();
        }
    }

    // enable player's input
    public void EnablePlayersInput()
    {
        // loop over player controllers and enable input
        foreach (PlayerControllerNetwork player in players.Values)
        {
            player.enabled = true;
        }
    }

    // disable player's input
    public void DisablePlayersInput()
    {
        // loop over player controllers and disable input
        foreach (PlayerControllerNetwork player in players.Values)
        {
            player.enabled = false;
        }
    }


    // **处理玩家输入**
    [ServerRpc(RequireOwnership = false)]
    public void HandlePlayerAction(int playerIndex, string hand, string action){
        ProcessHandlePlayerAction(playerIndex, hand, action).Forget();
    }

    private async UniTaskVoid ProcessHandlePlayerAction(int playerIndex, string hand, string action)
    {
        Debug.Log($"Server: Player[{playerIndex}] hand[{hand}] action[{action}]");
        // **处理游戏逻辑（例如：攻击、移动等）**+
        // get punch state
        int handIndex = hand == "l" ? 0 : 1;
        PlayerState playerState = playerStates[playerIndex];
        PunchState punchState = playerState.punchStates[handIndex];
        // get opponent's punch state
        int opponentIndex = playerIndex == 0 ? 1 : 0;
        if (!playerStates.ContainsKey(opponentIndex)) return;
        PunchState opponentPunchState = playerStates[opponentIndex].punchStates[handIndex];


        /// handle action
        if (punchState != PunchState.Idle && (action == "Charge")) return;

        // handle punch charge
        if (punchState == PunchState.Idle && action == "Charge")
        {
            playerStates[playerIndex].punchStates[handIndex] = PunchState.HookCharge;
            playerStates[playerIndex].chargeTime = 0;

            while (playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge && playerStates[playerIndex].chargeTime < hookChargeDuration)
            {
                playerStates[playerIndex].chargeTime += Time.deltaTime;
                await UniTask.Yield();
            }

            if (playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge)
            {
                playerStates[playerIndex].punchStates[handIndex] = PunchState.HookChargeComplete;
            }
        }

        // handle land a punch
        if (action == "Punch")
        {
            // if charge is not complete, do straight punch
            if (playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge || playerStates[playerIndex].punchStates[handIndex] == PunchState.Idle)
            {
                playerStates[playerIndex].punchStates[handIndex] = PunchState.StraightPunch;
                Debug.Log($"Server: Player[{playerIndex}] hand[{hand}] landed a straight punch");

                NotifyAllPlayers($"{playerIndex}-{hand}-Straight", straightPunchWindup * 0.9f);

                await UniTask.Delay((int)(straightPunchWindup * 1000));

                // dealing damage
                if (opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry)
                {
                    // take damage
                }
                else if (opponentPunchState == PunchState.Parry)
                {
                    // parry
                }
                else
                {
                    // block
                }

                // recovery
                NotifyAllPlayers($"{playerIndex}-{hand}-Recovery", straightPunchRecovery * 0.9f);
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
                await UniTask.Delay((int)(straightPunchRecovery * 1000));

                // idle
                playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
            }
            else if (playerStates[playerIndex].punchStates[handIndex] == PunchState.HookChargeComplete)
            {
                playerStates[playerIndex].punchStates[handIndex] = PunchState.HookPunch;
                Debug.Log($"Server: Player[{playerIndex}] hand[{hand}] landed a hook punch");

                NotifyAllPlayers($"{playerIndex}-{hand}-Hook", hookPunchWindup * 0.9f);

                // windup
                await UniTask.Delay((int)(hookPunchWindup * 1000));

                // dealing damage
                if (opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry)
                {
                    // take damage
                }
                else if (opponentPunchState == PunchState.Parry)
                {
                    // parry
                }
                else
                {
                    // block
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
        else if (action == "Block" && punchState == PunchState.Idle)
        {
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Block;
            NotifyAllPlayers($"{playerIndex}-{hand}-Block");
            Debug.Log($"Server: Player[{playerIndex}]'s hand[{hand}] is blocking");
            StartParry(playerIndex, handIndex).Forget();
            // holding block
            while (playerStates[playerIndex].punchStates[handIndex] == PunchState.Block)
            {
                await UniTask.Yield();
            }
        }

        // handle end charge
        else if (action == "CancelCharge" &&
            (punchState == PunchState.HookCharge || punchState == PunchState.HookChargeComplete)
        )
        {
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
            NotifyAllPlayers($"{playerIndex}-{hand}-Recovery", blockRecovery * 0.9f);
            await UniTask.Delay((int)(blockRecovery * 1000));
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
        }

        // handle end block
        else if ((punchState == PunchState.Block || punchState == PunchState.Parry) && action == "CancelBlock")
        {
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Recovery;
            NotifyAllPlayers($"{playerIndex}-{hand}-Recovery", blockRecovery * 0.9f);
            await UniTask.Delay((int)(blockRecovery * 1000));
            playerStates[playerIndex].punchStates[handIndex] = PunchState.Idle;
        }


    }

    // **通知所有玩家（广播事件）**
    [ServerRpc(RequireOwnership = false)]
    public void NotifyAllPlayers(string message, float d = 0)
    {
        foreach (var player in players.Values)
        {
            player.ReceiveGameEvent(message, d);
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

    private async UniTaskVoid Interrupt(int player, int hand)
    {
        // TODO: interrupt the punch
        await UniTask.Yield();
    }
}
