using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FishNet;
using Cysharp.Threading.Tasks;
using System.Threading;

public class GameStateClientHandler : MonoBehaviour
{
    public static GameStateClientHandler Instance { get; private set; }

    [Header("UI")]
    public TMP_Text timerText;

    [Header("Break phase")]
    public GameObject questionBoard;

    private CancellationTokenSource cts;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Starts a new phase timer by canceling any previous timer
    public void StartNewPhase(float phaseLength, string phaseName = "")
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }

        cts = new CancellationTokenSource();
        UpdatePhaseTimer(phaseLength, phaseName, cts.Token).Forget();
    }

    // Asynchronously updates the phase timer UI and checks for cancellation
    public async UniTask UpdatePhaseTimer(float phaseLength, string phaseName = "", CancellationToken token = default)
    {
        float elapsedTime = 0f;

        while (elapsedTime < phaseLength)
        {
            // Check if cancellation is requested, then exit early
            if (token.IsCancellationRequested)
                return;

            if (timerText != null)
                timerText.text = $"{phaseName} {phaseLength - elapsedTime:0.0}";

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        if (timerText != null)
            timerText.text = $"{phaseName} ended!";
    }

    // Stops and disposes the current phase timer
    public void StopPhaseTimer()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    // Shows or hides the question board UI element
    public void ShowQuestionBoard(bool show)
    {
        questionBoard.SetActive(show);
    }

    // Updates the end game screen text and cancels any active timer
    public void ShowEndGameScreen(string txt)
    {
        // Ensure any active timer is stopped so it does not override the final text
        StopPhaseTimer();

        if (timerText != null)
            timerText.text = txt;
    }
}
