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
    private Transform selfCamera;
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

        selfCamera = transform.Find("playerCam");
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
        glove.DOLocalMoveZ(-1f, duration).SetEase(Ease.InBack);
    }

    public void AnimatePunch(string hand, float duration)
    {
        Transform glove = hand == "l" ? lGlove : rGlove;
        glove.DOLocalMoveZ(2f, duration).SetEase(Ease.InBack);
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
        // reset all glove properties
        if(hand == "l")
        {
            lGlove.DOLocalMove(lGloveOrgPos, duration).SetEase(Ease.OutBack);
            lGlove.DOLocalRotateQuaternion(lGloveOrgRot, duration).SetEase(Ease.OutBack);
        }
        else
        {
            rGlove.DOLocalMove(rGloveOrgPos, duration).SetEase(Ease.OutBack);
            rGlove.DOLocalRotateQuaternion(rGloveOrgRot, duration).SetEase(Ease.OutBack);
        }
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
