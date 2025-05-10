using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using System.Threading;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class GameStateManager : NetworkBehaviour
{
    public enum GameState
    {
        Matching,
        Fighting,
        Break,
        End
    }

    public static GameStateManager Instance { get; private set; }

    public bool isGameModeLocal = true;
    // [SyncVar(OnChange = nameof(OnGameStateChange))]
    public GameState gameState = GameState.Matching;

    public int playerCount = 0;
    public int p1KOCount = 0; // Number of times player1 has been KOed
    public int p2KOCount = 0; // Number of times player2 has been KOed
    public int koWrongAnswer = -50; // Stun removal for wrong answer during break phase when player is KOed
    public int koCorrectAnswer = -80; // Stun removal for correct answer during break phase when player is KOed
    public int notKoWrongAnswer = -20; // Stun removal for wrong answer during break phase when player is not KOed
    public int notKoCorrectAnswer = -30; // Stun removal for correct answer during break phase when player is not KOed

    [Header("Phase Length")]
    public float fightingPhaseLength = 30f;
    public float breakPhaseLength = 15f;

    [Header("UI")]
    public TMP_Text timerText;

    [Header("Fighting phase")]
    private int koPlayer;

    [Header("Break phase")]
    public GameObject questionBoard;
    public JudgeController judgeController;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RunGameStateLoop().Forget();
    }

    public override void OnStartServer()
    {
        if (Instance == null) Instance = this;
        base.OnStartServer();
        gameState = GameState.Matching;
        RunGameStateLoop().Forget();
    }

    // Main game loop
    private async UniTaskVoid RunGameStateLoop()
    {
        // Audio
        AudioManager.Instance.PlayIngameBGM();

        // Delay before starting the loop
        await UniTask.Delay(500);
        await PlayerGatheringPhase();

        // Main game loop
        while (true)
        {
            // Create a new CancellationTokenSource for the fighting phase
            using (CancellationTokenSource battleCTS = new CancellationTokenSource())
            {
                await StartFightingPhase(battleCTS.Token);
            }

            // Check if game should end based on KO counts
            if (p1KOCount >= 3 || p2KOCount >= 3)
                break;

            // Break phase runs for full duration without cancellation
            await StartBreakPhase();

            await StartRoundEndPhase();
        }

        await StartEndPhase();
    }

    public void PlayerJoined()
    {
        playerCount++;
    }

    private async UniTask PlayerGatheringPhase()
    {
        // Pre-phase logic: matching state
        gameState = GameState.Matching;
        Debug.Log("Player gathering phase started");

        // Wait until two players have joined
        await UniTask.WaitUntil(() => playerCount == 2);

        // Post-phase logic: reset player input caches and disable inputs during transition
        Debug.Log("Player gathering phase ended");
        foreach (var player in LocalModeGameManager.Instance.playerInputs)
        {
            Debug.Log($"Resetting input cache for player {player.Key}");
            player.Value.GetComponent<InputCache>().ResetHold();
        }
        LocalModeGameManager.Instance.DisablePlayersInput();

        // Small delay to prevent instant phase switch issues
        await UniTask.Delay(500);
    }

    // Fighting phase: either lasts for the full duration or ends early if a player is KOed.
    private async UniTask StartFightingPhase(CancellationToken token)
    {
        // Pre-phase logic
        gameState = GameState.Fighting;
        Debug.Log("Fighting phase started");
        GameStateClientHandler.Instance.timerText.text = "Fighting!";
        AudioManager.Instance.PlayStartEnd();

        // Delay before starting the actual phase
        await UniTask.Delay(1500);
        LocalModeGameManager.Instance.EnablePlayersInput();
        ChangeState(GameState.Fighting, fightingPhaseLength);

        // Wait for either the phase duration to complete or a KO event to occur
        await WaitForPhaseEnd(fightingPhaseLength, GameState.Fighting, token);

        // Post-phase logic
        Debug.Log("Fighting phase ended");
        LocalModeGameManager.Instance.DisablePlayersInput();
        AudioManager.Instance.PlayStartEnd();
        // Additional delay before transitioning to the next phase
        await UniTask.Delay(1500);
    }

    // Break phase: always waits until the countdown finishes.
    private async UniTask StartBreakPhase()
    {
        // Pre-phase logic
        gameState = GameState.Break;
        // Play zebra animation (or similar) before counting
        await UniTask.Delay(500);
        _ = judgeController.StartCounting(LocalModeGameManager.Instance.GetPlayer(koPlayer));
        await UniTask.Delay(500);

        Debug.Log("Break phase started");
        ShowQuestionBoard(true);
        GamepadAnswerSelector.Instance.ResetSelections();
        Button correctAnswer = questionBoard.GetComponent<QuestionGenerator>().GenerateQuestion();
        ChangeState(GameState.Break, breakPhaseLength);

        // Wait for the break phase duration to complete (no cancellation)
        await WaitForPhaseEnd(breakPhaseLength, GameState.Break, CancellationToken.None);

        // Post-phase logic
        Debug.Log("Break phase ended");
        ShowQuestionBoard(false);

        Tuple<bool, bool> result = GamepadAnswerSelector.Instance.CheckAnswerCorrectness(correctAnswer);
        bool player1Correct = result.Item1;
        bool player2Correct = result.Item2;
        Debug.Log($"Player 1: {player1Correct}, Player 2: {player2Correct}");

        // Apply damage based on answer correctness and KO status
        LocalModeGameManager.Instance.AddDamageToPlayer(0,
            koPlayer == 0 ? (player1Correct ? koCorrectAnswer : koWrongAnswer)
                          : (player1Correct ? notKoCorrectAnswer : notKoWrongAnswer));
        LocalModeGameManager.Instance.AddDamageToPlayer(1,
            koPlayer == 1 ? (player2Correct ? koCorrectAnswer : koWrongAnswer)
                          : (player2Correct ? notKoCorrectAnswer : notKoWrongAnswer));

        // Resume judge watching animation
        _ = judgeController.StartWatching();

        // Delay before transitioning to next phase
        await UniTask.Delay(1500);
    }

    private async UniTask StartRoundEndPhase()
    {
        // Update KO count and reset KO player status based on which player was KOed
        if (koPlayer == 0)
        {
            p1KOCount++;
            LocalModeGameManager.Instance.ResetKOPlayer(0);
            koPlayer = -1;
        }
        else if (koPlayer == 1)
        {
            p2KOCount++;
            LocalModeGameManager.Instance.ResetKOPlayer(1);
            koPlayer = -1;
        }

        Debug.Log($"Player 1 KO count: {p1KOCount}, Player 2 KO count: {p2KOCount}");
        // Delay for any transition animations (e.g., zebra animation)
        await UniTask.Delay(2000);
    }

    private async UniTask StartEndPhase()
    {
        // Pre-phase logic for the end phase
        gameState = GameState.End;
        Debug.Log("End phase started");

        // Determine the winner based on KO counts
        int winner = (p1KOCount > p2KOCount) ? 2 : (p1KOCount < p2KOCount) ? 1 : 0;
        string resultText = (winner == 0) ? "It's a draw!" : $"Player {winner} wins!";

        // Stop any active phase timer to prevent further UI updates
        GameStateClientHandler.Instance.StopPhaseTimer();

        // Show end game screen with the final text
        GameStateClientHandler.Instance.ShowEndGameScreen(resultText);
        Debug.Log(resultText);

        // Wait for a period before returning to the main menu
        await UniTask.Delay(5000);
        SceneLoader.Instance.StartLoadingSceneAsync("MainMenu");
        AudioManager.Instance.PlayMainmenuBGM();
    }

    // WaitForPhaseEnd: For Fighting phase, it exits early if a KO is detected; for Break phase, waits for full duration.
    private async UniTask WaitForPhaseEnd(float duration, GameState phaseName, CancellationToken token)
    {
        float elapsedTime = 0f;
        while (true)
        {
            if (phaseName == GameState.Fighting)
            {
                // Check if any player is KOed
                koPlayer = LocalModeGameManager.Instance.CheckKOPlayer();
                if (koPlayer != -1)
                {
                    // Exit early if a player is KOed
                    break;
                }
            }
            else if (phaseName == GameState.Break)
            {
                // Increase elapsed time for break phase countdown
                elapsedTime += Time.deltaTime;
                if (elapsedTime >= duration)
                {
                    break;
                }
            }

            // If a cancellation token (for fighting phase) is provided and is canceled, exit early
            if (token != CancellationToken.None && token.IsCancellationRequested)
            {
                break;
            }
            await UniTask.Yield();
        }
    }

    // Local UI and state change methods
    public void ChangeState(GameState state, float phaseLength)
    {
        GameStateClientHandler.Instance.StartNewPhase(phaseLength, state.ToString());
    }

    public void ShowQuestionBoard(bool show)
    {
        UIManager.Instance.ToggleQuestionBoard(show);
    }

    public void ShowEndGameScreen(string txt)
    {
        GameStateClientHandler.Instance.ShowEndGameScreen(txt);
    }

    // Networked RPC calls to update clients
    [ObserversRpc]
    public void ChangeStateRPC(GameState state, float phaseLength)
    {
        GameStateClientHandler.Instance.StartNewPhase(phaseLength, state.ToString());
    }

    [ObserversRpc]
    public void ShowQuestionBoardRPC(bool show)
    {
        UIManager.Instance.ToggleQuestionBoard(show);
    }

    [ObserversRpc]
    public void ShowEndGameScreenRPC(string txt)
    {
        GameStateClientHandler.Instance.ShowEndGameScreen(txt);
    }
}
