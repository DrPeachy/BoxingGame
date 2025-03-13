using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;

public class PlayerView : MonoBehaviour
{
    private Transform lGlove;
    private Transform rGlove;
    private Vector3 lGloveOrgPos;
    private Vector3 rGloveOrgPos;
    private Quaternion lGloveOrgRot;
    private Quaternion rGloveOrgRot;
    private Sequence leftGloveSequence;
    private Sequence rightGloveSequence;
    private Transform selfCamera;

    [Header("Punch states Transform")]
    public Transform lBlockPos;
    public Transform rBlockPos;
    public Transform lPunchPos;
    public Transform rPunchPos;
    public Transform lChargePos;
    public Transform rChargePos;
    [Header("Punch states Rotation")]
    public Quaternion lBlockRot;
    public Quaternion rBlockRot;
    public Quaternion lPunchRot;
    public Quaternion rPunchRot;
    public Quaternion lChargeRot;
    public Quaternion rChargeRot;

    void Awake()
    {
        // get player controller
    }

    void Start()
    {
        // get glove transform
        lGlove = transform.Find("l");
        rGlove = transform.Find("r");

        lGloveOrgPos = lGlove.localPosition;
        rGloveOrgPos = rGlove.localPosition;
        lGloveOrgRot = lGlove.localRotation;
        rGloveOrgRot = rGlove.localRotation;

        // dotween sequence
        leftGloveSequence = DOTween.Sequence();
        rightGloveSequence = DOTween.Sequence();

        selfCamera = transform.Find("playerCam");
    }

    private Sequence GetOrResetSequence(string hand)
    {
        Sequence sequence = hand == "l" ? leftGloveSequence : rightGloveSequence;

        if (sequence.IsActive() && !sequence.IsComplete())
        {
            sequence.Kill(); // 终止当前的动画序列
        }

        return DOTween.Sequence(); // 创建新的序列
    }



    public void SetPlayerTransform(int playerIndex)
    {
        Debug.Log("SetPlayerTransform");
        if (playerIndex == 0)
        {
            transform.position = new Vector3(0f, 0, 1);
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (playerIndex == 1)
        {
            transform.position = new Vector3(0f, 0, -1);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void DisableCamera()
    {
        selfCamera.gameObject.SetActive(false);
    }

    public void AnimateCharge(string hand, float duration)
    {
        Sequence sequence = GetOrResetSequence(hand);
        Transform glove = hand == "l" ? lGlove : rGlove;
        Vector3 targetPos = hand == "l" ? lChargePos.localPosition : rChargePos.localPosition;
        Quaternion targetRot = hand == "l" ? lChargeRot : rChargeRot;

        sequence.Append(
            glove.DOLocalMove(
                targetPos, 
                duration
            ).SetEase(Ease.InBack));
        sequence.Join(
            glove.DOLocalRotateQuaternion(
                targetRot, 
                duration
            ).SetEase(Ease.InBack));
    
    }

    public void AnimatePunch(string hand, float duration)
    {
        Sequence sequence = GetOrResetSequence(hand);
        Transform glove = hand == "l" ? lGlove : rGlove;
        Vector3 targetPos = hand == "l" ? lPunchPos.localPosition : rPunchPos.localPosition;
        Quaternion targetRot = hand == "l" ? lPunchRot : rPunchRot;

        sequence.Append(
            glove.DOLocalMove(
                targetPos, 
                duration
            ).SetEase(Ease.InBack));
        sequence.Join(
            glove.DOLocalRotateQuaternion(
                targetRot, 
                duration
            ).SetEase(Ease.InBack));
    }

    public void AnimateBlock(string hand)
    {
        Sequence sequence = GetOrResetSequence(hand);
        Transform glove = hand == "l" ? lGlove : rGlove;
        Vector3 targetPos = hand == "l" ? lBlockPos.localPosition : rBlockPos.localPosition;
        Quaternion targetRot = hand == "l" ? lBlockRot : rBlockRot;
        
        // set block position and rotation without animation
        glove.localPosition = targetPos;
        glove.localRotation = targetRot;

    }

    public void AnimateRecovery(string hand, float duration)
    {
        Sequence sequence = GetOrResetSequence(hand);
        Transform glove = hand == "l" ? lGlove : rGlove;
        Vector3 targetPos = hand == "l" ? lGloveOrgPos : rGloveOrgPos;
        Quaternion targetRot = hand == "l" ? lGloveOrgRot : rGloveOrgRot;

        sequence.Append(
            glove.DOLocalMove(
                targetPos, 
                duration
            ).SetEase(Ease.InBack));
        sequence.Join(
            glove.DOLocalRotateQuaternion(
                targetRot, 
                duration
            ).SetEase(Ease.InBack));
    }

    public void ResetGloves(string hand)
    {
        if(hand == "l")
        {
            lGlove.localPosition = lGloveOrgPos;
            lGlove.localRotation = lGloveOrgRot;
        }
        else
        {
            rGlove.localPosition = rGloveOrgPos;
            rGlove.localRotation = rGloveOrgRot;
        }
    }


}
