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
    public int p1KOCount = 0;
    public int p2KOCount = 0;

    [Header("phase length")]
    public float fightingPhaseLength = 30f;
    public float breakPhaseLength = 15f;

    [Header("UI")]
    public TMP_Text timerText;

    [Header("Fighting phase")]
    private int koPlayer;

    [Header("Break phase")]
    public GameObject questionBoard;
    private CancellationTokenSource cts;
    public JudgeController judgeController;

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
            if(koPlayer == 0){
                p1KOCount++;
                koPlayer = -1;
            }else if(koPlayer == 1){
                p2KOCount++;
                koPlayer = -1;
            }
            if(p1KOCount >= 3 || p2KOCount >= 3) break;
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
        foreach(var player in LocalModeGameManager.Instance.playerInputs){
            Debug.Log($"Resetting input cache for player {player.Key}");
            player.Value.GetComponent<InputCache>().ResetHold(); // reset input cache for all players
        }
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
        await WaitForPhaseEnd(fightingPhaseLength, GameState.Fighting, token);
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
        // play zebra animation and shit
        await UniTask.Delay(500); // delay for zebra animation
        _= judgeController.StartCounting(LocalModeGameManager.Instance.GetPlayer(koPlayer));
        await UniTask.Delay(500); // delay for zebra animation
        

        Debug.Log("Break phase started");
        //questionBoard.SetActive(true);
        ShowQuestionBoard(true);
        GamepadAnswerSelector.Instance.ResetSelections();
        Button correctAnswer = questionBoard.GetComponent<QuestionGenerator>().GenerateQuestion();
        ChangeState(GameState.Break, breakPhaseLength);



        // wait for phase end
        await WaitForPhaseEnd(breakPhaseLength, GameState.Break, token);


        /// post phase logic
        // question board
        Debug.Log("Break phase ended");
        ShowQuestionBoard(false);
        Tuple<bool, bool> result = GamepadAnswerSelector.Instance.CheckAnswerCorrectness(correctAnswer);
        bool player1Correct = result.Item1;
        bool player2Correct = result.Item2;
        Debug.Log($"Player 1: {player1Correct}, Player 2: {player2Correct}");
        if (!player1Correct) LocalModeGameManager.Instance.AddDamageToPlayer(0, 50);
        if (!player2Correct) LocalModeGameManager.Instance.AddDamageToPlayer(1, 50);

        // zebra animation
        _= judgeController.StartWatching();

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

    private async UniTask WaitForPhaseEnd(float duration, GameState phaseName, CancellationToken token){
        float elapsedTime = 0f;
        while(!token.IsCancellationRequested){

            if(phaseName == GameState.Fighting){
                // Call the external singleton function to check if the fighting phase should end
                koPlayer = LocalModeGameManager.Instance.CheckKOPlayer();
                if(koPlayer != -1){
                    break;
                }

                // Optionally update timer text or other UI elements here if needed
            }else if(phaseName == GameState.Break){
                // counting duration
                elapsedTime += Time.deltaTime;
                if(elapsedTime >= duration){
                    break;
                }
            }

            await UniTask.Yield();
        }
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
