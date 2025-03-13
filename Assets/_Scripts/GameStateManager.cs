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
using System;
using Cysharp.Threading.Tasks.CompilerServices;
using System.Threading;

public class GameStateManager : NetworkBehaviour
{
    public enum GameState{
        Matching,
        Fighting,
        Break,
        End
    }

    public static GameStateManager Instance { get; private set; }

    public bool isGameModeLocal = true;

    //[SyncVar(OnChange = nameof(OnGameStateChange))]
    public GameState gameState = GameState.Matching;

    public int playerCount = 0;
    public int roundCount = 0;

    [Header("phase length")]
    public float fightingPhaseLength = 30f;
    public float breakPhaseLength = 15f;

    [Header("UI")]
    public TMP_Text timerText;

    [Header("Break phase")]
    public GameObject questionBoard;

    private CancellationTokenSource cts;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start(){
        //this.enabled = true;
        RunGameStateLoop().Forget();
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
        cts = new CancellationTokenSource();

        await PlayerGatheringPhase();
        
        // DebugEndGame().Forget(); // end game after 5 seconds

        while(!cts.Token.IsCancellationRequested){
            await StartFightingPhase(cts.Token);
            if(cts.Token.IsCancellationRequested) break;
            await StartBreakPhase(cts.Token);
            if(cts.Token.IsCancellationRequested) break;
            roundCount++;
            if(roundCount >= 3) break;
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
        LocalModeGameManager.Instance.DisablePlayersInput(); // disable input while transitioning to next phase

        // delay before next phase, prevent instant phase switch that cause crash
        await UniTask.Delay(500);
    }

    private async UniTask StartFightingPhase(CancellationToken token){
        // pre phase logic
        gameState = GameState.Fighting;
        Debug.Log("Fighting phase started");
        GameStateClientHandler.Instance.timerText.text = "Fighting!";
        AudioManager.Instance.PlayStartEnd();
        await UniTask.Delay(1500); // delay before starting the phase
        LocalModeGameManager.Instance.EnablePlayersInput();
        ChangeState(GameState.Fighting, fightingPhaseLength);
        

        // wait for phase end
        await WaitForPhaseEnd(fightingPhaseLength, "Fighting", token);
        //if(token.IsCancellationRequested) return;

        // post phase logic
        Debug.Log("Fighting phase ended");
        LocalModeGameManager.Instance.DisablePlayersInput();
        AudioManager.Instance.PlayStartEnd();

        // delay before next phase, prevent instant phase switch that cause crash
        await UniTask.Delay(1500); // longer delay for creating a gap between phases
    }

    private async UniTask StartBreakPhase(CancellationToken token){
        // pre phase logic
        gameState = GameState.Break;
        Debug.Log("Break phase started");
        //questionBoard.SetActive(true);
        ShowQuestionBoard(true);
        CursorController.Instance.Reset();
        Button correctAnswer = questionBoard.GetComponent<QuestionGenerator>().GenerateQuestion();
        ChangeState(GameState.Break, breakPhaseLength);

        // wait for phase end
        await WaitForPhaseEnd(breakPhaseLength, "Break", token);
        //if(token.IsCancellationRequested) return;

        // post phase logic
        Debug.Log("Break phase ended");
        //questionBoard.SetActive(false);
        ShowQuestionBoard(false);
        Tuple<bool, bool> result = CursorController.Instance.CheckAnswerCorrectness(correctAnswer);
        bool player1Correct = result.Item1;
        bool player2Correct = result.Item2;
        Debug.Log($"Player 1: {player1Correct}, Player 2: {player2Correct}");
        if (!player1Correct) LocalModeGameManager.Instance.AddDamageToPlayer(0, 50);
        if (!player2Correct) LocalModeGameManager.Instance.AddDamageToPlayer(1, 50);

        // delay before next phase, prevent instant phase switch that cause crash
        await UniTask.Delay(1500);
    }

    private async UniTask StartEndPhase(){
        // pre phase logic
        gameState = GameState.End;
        Debug.Log("End phase started");

        //ShowEndGameScreen($"Player {OnlineModeGameManager.Instance.GetWinner() + 1} wins!");
        int winner = LocalModeGameManager.Instance.GetPlayerWithLessDamageTaken() + 1;
        if(winner == 0) ShowEndGameScreen("It's a draw!");
        else ShowEndGameScreen($"Player {winner} wins!");

        await UniTask.Delay(5000);
        SceneLoader.Instance.StartLoadingSceneAsync("MainMenu");
    }

    private async UniTask WaitForPhaseEnd(float phaseLength, string phaseName, CancellationToken token){
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

            if(token.IsCancellationRequested) return;

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        // if (timerText != null)
        //     timerText.text = $"{phaseName} ended!";
    }

    public void EndGame(){
        if(cts != null && !cts.Token.IsCancellationRequested){
            cts.Cancel();
            Debug.Log("Game ended");
        }
    }

    public async UniTaskVoid DebugEndGame(){
        await UniTask.Delay(5000);
        EndGame();
    }

    // local
    public void ChangeState(GameState state, float phaseLength){
        GameStateClientHandler.Instance.StartNewPhase(phaseLength, state.ToString());
    }

    public void ShowQuestionBoard(bool show){
        UIManager.Instance.ToggleQuestionBoard(show);
    }

    public void ShowEndGameScreen(string txt){
        GameStateClientHandler.Instance.ShowEndGameScreen(txt);
    }


    // networked
    [ObserversRpc]
    public void ChangeStateRPC(GameState state, float phaseLength){
        GameStateClientHandler.Instance.StartNewPhase(phaseLength, state.ToString());
    }

    [ObserversRpc]
    public void ShowQuestionBoardRPC(bool show){
        UIManager.Instance.ToggleQuestionBoard(show);
    }

    [ObserversRpc]
    public void ShowEndGameScreenRPC(string txt){
        GameStateClientHandler.Instance.ShowEndGameScreen(txt);
    }


}
