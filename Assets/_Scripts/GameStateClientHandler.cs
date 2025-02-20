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
        if (Instance == null) Instance = this;
    }

    public void StartNewPhase(float phaseLength, string phaseName = "")
    {
        if (cts != null){
            cts.Cancel();
            cts.Dispose();
        }

        cts = new CancellationTokenSource();
        UpdatePhaseTimer(phaseLength, phaseName, cts.Token).Forget();
    }

    public async UniTask UpdatePhaseTimer(float phaseLength, string phaseName = "", CancellationToken token = default){
        float elapsedTime = 0f;

        while(elapsedTime < phaseLength){
            if (timerText != null)
                timerText.text = $"{phaseName} {phaseLength - elapsedTime:0.0}";

            // debug - space key to skip phase
            

            elapsedTime += Time.deltaTime;
            await UniTask.Yield();
        }

        if (timerText != null)
            timerText.text = $"{phaseName} ended!";
    }

    // break phase ui logic
    public void ShowQuestionBoard(bool show){
        questionBoard.SetActive(show);
    }
}
