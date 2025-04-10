using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class JudgeController : MonoBehaviour
{
    public enum JudgeState
    {
        Watching,
        Counting,
        Ending
    }
    public JudgeState judgeState = JudgeState.Counting;
    public GameObject judgeObject;
    public Animator judgeAnimator;

    [Header("Watching")]
    public List<Transform> judgePositions;
    private int currentPositionIndex = 0;
    public Transform moveCenter;
    public float stopDuration = 2.5f;

    [Header("Counting")]
    public Transform countTarget;
    public float playerPosOffset = 0.5f;
    public float countDuration = 10f;


    // Update is called once per frame

    void Start()
    {
        _= StartGameLoop();
    }

    public async UniTask StartGameLoop(){
        while(true){
            _= StartWatching();
            await UniTask.Delay((int)1000 * 10);
            _= StartCounting(countTarget);
            await UniTask.Delay((int)1000 * 10);   
        }
    }

    public async UniTask StartWatching()
    {
        // If already in Watching state, exit
        if (judgeState == JudgeState.Watching) return;
        judgeState = JudgeState.Watching;

        // Main loop: continue moving as long as judgeState is Watching
        while (judgeState == JudgeState.Watching)
        {
            // Move to the next position
            Vector3 targetPosition = judgePositions[currentPositionIndex].position;
            targetPosition.y = judgeObject.transform.position.y; // Keep the y position the same
            judgeObject.transform.DOMove(targetPosition, 1f).SetEase(Ease.Linear).OnUpdate(() =>
            {
                judgeObject.transform.LookAt(new Vector3(moveCenter.position.x, judgeObject.transform.position.y, moveCenter.position.z));
            }).OnComplete(() =>
            {
                judgeAnimator?.SetTrigger("Stop");
            });
            judgeAnimator?.SetTrigger("Walk");
            await UniTask.Delay((int)(1000 * stopDuration)); // Wait for 1 second

            // Increment the index and loop back if necessary
            currentPositionIndex = (currentPositionIndex + 1) % judgePositions.Count;
        }
    }

    public async UniTaskVoid StartCounting(Transform player){
        // If already in Counting state, exit
        if (judgeState == JudgeState.Counting) return;
        judgeState = JudgeState.Counting;

        // Move to the player's position
        Vector3 targetPosition = player.forward * playerPosOffset + player.position;
        targetPosition.y = judgeObject.transform.position.y; // Keep the y position the same
        judgeObject.transform.DOMove(targetPosition, 1f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            judgeObject.transform.LookAt(player.position);
        }).OnComplete(() =>
        {
            judgeAnimator?.SetTrigger("Stop");
        });
        judgeAnimator?.SetTrigger("Walk");

        // start counting
        // AudioManager.Instance?.PlayZebraJudgeCountdown(0);
        await UniTask.Delay((int)(1000 * countDuration)); // Wait for 10 second

        // Move back to the next position
        Vector3 targetPosition2 = judgePositions[currentPositionIndex].position;
        targetPosition2.y = judgeObject.transform.position.y; // Keep the y position the same
        judgeObject.transform.DOMove(targetPosition2, 1f).SetEase(Ease.Linear).OnUpdate(() =>
        {
            judgeObject.transform.LookAt(targetPosition2);
        }).OnComplete(() =>
        {
            judgeAnimator?.SetTrigger("Stop");
        });
    }
}
