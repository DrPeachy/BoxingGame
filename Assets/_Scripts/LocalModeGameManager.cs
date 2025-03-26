using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    [Header("Metrices")]
    private string logFilePath;
    private bool sessionLogged = false; // 是否已记录本局会话的开始时间

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        logFilePath = Path.Combine(Application.persistentDataPath, $"combat_log_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.txt");
    }

    // 玩家加入时注册
    public void RegisterPlayer(PlayerController player)
    {
        if (!players.ContainsKey(player.PlayerIndex))
        {
            players[player.PlayerIndex] = player;
            playerInputs[player.PlayerIndex] = player.GetComponent<PlayerInput>();
            CursorController.Instance.AddPlayerInput(player.PlayerIndex, playerInputs[player.PlayerIndex]);
            // register player audio sources
            AudioManager.Instance.audioEffectsPlayers[player.PlayerIndex] = new AudioEffectsPlayer(player.transform.Find("AudioSources").gameObject);
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

            // 如果还没有写入会话开始时间，则写入
            if (!sessionLogged)
            {
                LogSessionHeader();
                sessionLogged = true;
            }
        }
    }

    // 新增方法：写入会话开始时间（日期-开始游戏时间）
    private void LogSessionHeader()
    {
        string header = $"{DateTime.Now.ToString("yyyy-MM-dd")}-开始游戏时间: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}";
        AppendLogLine(header);
    }

    // 新增方法：写入单条日志记录
    private void LogCombatEvent(string message)
    {
        // 解析消息格式，假定格式为 "playerIndex-hand-action"
        string[] parts = message.Split('-');
        if(parts.Length < 3) return;
        int playerIndex;
        if(!int.TryParse(parts[0], out playerIndex)) return;
        string handStr = parts[1] == "l" ? "左拳" : "右拳";
        string action = parts[2];
        string logLine = $"p{playerIndex+1}-{handStr}-{action}-{DateTime.Now.ToString("HH:mm:ss.fff")}";
        AppendLogLine(logLine);
    }

    // 新增方法：将文本行追加到日志文件中
    private void AppendLogLine(string line)
    {
        try
        {
            File.AppendAllText(logFilePath, line + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Debug.LogError("写入日志失败: " + ex.Message);
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
            AudioManager.Instance.PlayCharge(playerIndex); // play charge sound

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
                AudioManager.Instance.PlayChargeComplete(playerIndex);
            }
        }

        // handle land a punch
        if(action == "Punch"){
            // if charge is not complete, do straight punch
            if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookCharge || playerStates[playerIndex].punchStates[handIndex] == PunchState.Idle){
                // debug , print both hand punch state
                Debug.Log($"kkkkk玩家 {playerIndex} 的左手状态：{playerStates[playerIndex].punchStates[0]} ${handIndex}");
                Debug.Log($"kkkkk玩家 {playerIndex} 的右手状态：{playerStates[playerIndex].punchStates[1]} ${handIndex}");
                playerStates[playerIndex].punchStates[handIndex] = PunchState.StraightPunch;
                Debug.Log($"玩家 {playerIndex} 的 {handIndex} 手发动了直拳");
                AudioManager.Instance.PlayWave(playerIndex);
                NotifyAllPlayers($"{playerIndex}-{hand}-Straight", straightPunchWindup * 0.9f);
                AudioManager.Instance.StopCharge(playerIndex);

                await UniTask.Delay((int)(straightPunchWindup * 1000));
                opponentPunchState = playerStates[opponentIndex].punchStates[opponentHandIndex];
                Debug.Log($"对手状态：{opponentPunchState}");
                // dealing damage
                if(opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry){
                    // take damage
                    playerStates[opponentIndex].damageTaken += straightPunchDamage;
                    AudioManager.Instance.PlayPunch(playerIndex);
                }else if(opponentPunchState == PunchState.Parry){
                    // parry
                    _= SetToRecovery(playerIndex, hand, parryRecovery);
                    AudioManager.Instance.PlayParry(playerIndex);
                    return;
                }else if(opponentPunchState == PunchState.Block){
                    // block
                    playerStates[opponentIndex].damageTaken += (straightPunchDamage - blockDamageReduction);
                    Debug.Log($"====blockdamage===={blockDamageReduction}");
                    AudioManager.Instance.PlayPunchBlocked(playerIndex);
                }

                //_= Interrupt(opponentIndex, opponentHandIndex == 0 ? "l" : "r");

                // recovery
                _= SetToRecovery(playerIndex, hand, straightPunchRecovery);

            }else if(playerStates[playerIndex].punchStates[handIndex] == PunchState.HookChargeComplete){
                playerStates[playerIndex].punchStates[handIndex] = PunchState.HookPunch;
                Debug.Log($"玩家 {playerIndex} 的 {hand} 手发动了钩拳");
                AudioManager.Instance.PlayWave(playerIndex);
                NotifyAllPlayers($"{playerIndex}-{hand}-Hook", hookPunchWindup * 0.9f);

                // windup
                await UniTask.Delay((int)(hookPunchWindup * 1000));

                // dealing damage
                opponentPunchState = playerStates[opponentIndex].punchStates[opponentHandIndex];
                if(opponentPunchState != PunchState.Block && opponentPunchState != PunchState.Parry){
                    // take damage
                    playerStates[opponentIndex].damageTaken += hookPunchDamage;
                    AudioManager.Instance.PlayPunch(playerIndex);
                }else if(opponentPunchState == PunchState.Parry){
                    // parry
                    _= SetToRecovery(playerIndex, hand, parryRecovery);
                    AudioManager.Instance.PlayParry(playerIndex);
                    return;
                }else if(opponentPunchState == PunchState.Block){
                    // block
                    playerStates[opponentIndex].damageTaken += (hookPunchDamage - blockDamageReduction);
                    Debug.Log($"====blockdamage===={blockDamageReduction}");
                    AudioManager.Instance.PlayPunchBlocked(playerIndex);
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
            AudioManager.Instance.StopCharge(playerIndex);
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
        LogCombatEvent(message);

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
        // pop buffered action from input cache
        players[player].inputCache.PopBufferedAction(player, hand);
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
