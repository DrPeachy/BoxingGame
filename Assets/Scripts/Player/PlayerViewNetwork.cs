using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using FishNet.Object;

public class PlayerViewNetwork : NetworkBehaviour
{
    private Transform lGlove;
    private Transform rGlove;
    private Vector3 lGloveOrgPos;
    private Vector3 rGloveOrgPos;
    private Quaternion lGloveOrgRot;
    private Quaternion rGloveOrgRot;
    [SerializeField]private Transform selfCamera;
    void Awake()
    {
        // get player controller
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // get glove transform
        lGlove = transform.Find("l");
        rGlove = transform.Find("r");

        lGloveOrgPos = lGlove.localPosition;
        rGloveOrgPos = rGlove.localPosition;
        lGloveOrgRot = lGlove.localRotation;
        rGloveOrgRot = rGlove.localRotation;

        //selfCamera = transform.Find("playerCam");
    }


    public void SetPlayerTransform(int playerIndex)
    {
        Debug.Log("SetPlayerTransform");
        if (playerIndex == 0)
        {
            transform.position = new Vector3(0f, 0, 2);
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (playerIndex == 1)
        {
            transform.position = new Vector3(0f, 0, -2);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void DisableCamera()
    {
        selfCamera.gameObject.SetActive(false);
    }

    public void AnimateCharge(string hand, float duration)
    {
        Transform glove = hand == "l" ? lGlove : rGlove;
        glove.transform.localPosition = glove.transform.localPosition - new Vector3(0, 0, 0.1f);
        // shake in harmony mode
        glove.DOShakePosition(0.05f, 0.1f).SetLoops(-1);
    }

    public void AnimatePunch(string hand, float duration)
    {
        Transform glove = hand == "l" ? lGlove : rGlove;
        glove.DOLocalMoveZ(2f, duration).SetEase(Ease.InBack).OnComplete(() =>
        {
            // kill the shake
            glove.DOKill();
        });
    }

    public void AnimateBlock(string hand)
    {
        Transform glove = hand == "l" ? lGlove : rGlove;
        // rotate around y axis without using dotween
        glove.RotateAround(glove.position, Vector3.up, (hand == "l" ? 1 : -1) * 90);

    }

    public void AnimateRecovery(string hand, float duration)
    {
        Transform glove = hand == "l" ? lGlove : rGlove;
        Vector3 targetPos = hand == "l" ? lGloveOrgPos : rGloveOrgPos;
        Quaternion targetRot = hand == "l" ? lGloveOrgRot : rGloveOrgRot;

        glove.DOLocalMove(targetPos, duration).SetEase(Ease.OutBack);
        glove.DOLocalRotateQuaternion(targetRot, duration).SetEase(Ease.OutBack).OnComplete(() =>
        {
            // kill the shake
            glove.DOKill();
        });
    }

    public void ResetGloves(string hand)
    {
        Transform glove = hand == "l" ? lGlove : rGlove;
        glove.localPosition = hand == "l" ? lGloveOrgPos : rGloveOrgPos;
        glove.localRotation = hand == "l" ? lGloveOrgRot : rGloveOrgRot;
    }


}
