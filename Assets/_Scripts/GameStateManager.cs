using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GameStateManager : NetworkBehaviour
{
    public enum GameState{
        Matching,
        Fighting,
        Break,
        End
    }

    public static GameStateManager Instance { get; private set; }

    //[SyncVar(OnChange = nameof(OnGameStateChange))]
    public GameState gameState = GameState.Matching;

    public int playerCount = 0;

    [Header("phase length")]
    public float fightingPhaseLength = 30f;
    public float breakPhaseLength = 15f;

    [Header("UI")]
    public TMP_Text timerText;

    [Header("Break phase")]
    public GameObject questionBoard;

    private void Awake()
    {
        // if (Instance == null)
        // {
        //     Instance = this;
        //     DontDestroyOnLoad(gameObject);
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
    }

    private void Start(){
        //RunGameStateLoop().Forget();
    }

    public override void OnStartServer(){
        if(Instance == null) Instance = this;

        base.OnStartServer();
        gameState = GameState.Matching;
        RunGameStateLoop().Forget();
    }



    private async UniTaskVoid RunGameStateLoop(){
        // delay before starting the loop
        await UniTask.Delay(500);

        await PlayerGatheringPhase();

        while(!OnlineModeGameManager.Instance.CheckEndGame()){
            await StartFightingPhase();
            await StartBreakPhase();
        }

        await StartEndPhase();
    }
    public void PlayerJoined(){
        playerCount++;
    }

    private async UniTask PlayerGatheringPhase(){
        // pre phase logic
        gameState = GameState.Matching;
        Debug.Log("Player gathering phase started");
        //timerText.text = "Waiting for players..."; // debug

        // wait for phase end
        await UniTask.WaitUntil(() => playerCount == 2);

        // post phase logic
        Debug.Log("Player gathering phase ended");

        // delay before next phase, prevent instant phase switch that cause crash
        await UniTask.Delay(500);
    }

    private async UniTask StartFightingPhase(){
        // pre phase logic
        gameState = GameState.Fighting;
        Debug.Log("Fighting phase started");
        //LocalModeGameManager.Instance.EnablePlayersInput();
        ChangeState(GameState.Fighting, fightingPhaseLength);

        // wait for phase end
        await WaitForPhaseEnd(fightingPhaseLength, "Fighting");

        // post phase logic
        Debug.Log("Fighting phase ended");
        //LocalModeGameManager.Instance.DisablePlayersInput();

        // delay before next phase, prevent instant phase switch that cause crash
        await UniTask.Delay(500);
    }

    private async UniTask StartBreakPhase(){
        // pre phase logic
        gameState = GameState.Break;
        Debug.Log("Break phase started");
        //questionBoard.SetActive(true);
        ShowQuestionBoard(true);
        ChangeState(GameState.Break, breakPhaseLength);

        // wait for phase end
        await WaitForPhaseEnd(breakPhaseLength, "Break");

        // post phase logic
        Debug.Log("Break phase ended");
        //questionBoard.SetActive(false);
        ShowQuestionBoard(false);

        // delay before next phase, prevent instant phase switch that cause crash
        await UniTask.Delay(500);
    }

    private async UniTask StartEndPhase(){
        // pre phase logic
        gameState = GameState.End;
        Debug.Log("End phase started");

        ShowEndGameScreen($"Player {OnlineModeGameManager.Instance.GetWinner() + 1} wins!");
    }

    private async UniTask WaitForPhaseEnd(float phaseLength, string phaseName = ""){
        float elapsedTime = 0f;

        while(elapsedTime < phaseLength){
            // if (timerText != null)
            //     timerText.text = $"{phaseName} {phaseLength - elapsedTime:0.0}";

            // // debug - space key to skip phase
            // if(Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame){
            //     Debug.Log("Phase skipped");
            //     if (timerText != null)
            //         timerText.text = "Phase Skipped!";
            //     return;
            // }

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        // if (timerText != null)
        //     timerText.text = $"{phaseName} ended!";
    }

    [ObserversRpc]
    public void ChangeState(GameState state, float phaseLength){
        GameStateClientHandler.Instance.StartNewPhase(phaseLength, state.ToString());
    }

    [ObserversRpc]
    public void ShowQuestionBoard(bool show){
        GameStateClientHandler.Instance.ShowQuestionBoard(show);
    }

    [ObserversRpc]
    public void ShowEndGameScreen(string txt){
        GameStateClientHandler.Instance.ShowEndGameScreen(txt);
    }


}
