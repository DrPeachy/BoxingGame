using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine.UIElements;

public class PlayerView : MonoBehaviour
{
    public Transform lGlove;
    public Transform rGlove;
    private Vector3 lGloveOrgPos;
    private Vector3 rGloveOrgPos;
    private Quaternion lGloveOrgRot;
    private Quaternion rGloveOrgRot;
    private Vector3 playerOrgPos;
    private Quaternion playerOrgRot;
    [Header("Glove Animation")]
    private Sequence leftGloveSequence;
    private Sequence rightGloveSequence;
    private Tween leftChargeCompleteTween;
    private Tween rightChargeCompleteTween;
    public List<Material> gloveMaterials;
    private Material selfMaterial;
    private Renderer lgloveRenderer;
    private Renderer rgloveRenderer;

    [Header("Camera")]
    public Transform selfCamera;
    public Camera selfCameraComponent;
    public List<Transform> nodesToCull;

    [Header("IK Hands")]
    public Transform playerModel;
    private PlayerPunchIK playerPunchIK;

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
        // // get glove transform
        // lGlove = transform.Find("l");
        // rGlove = transform.Find("r");

        lGloveOrgPos = lGlove.localPosition;
        rGloveOrgPos = rGlove.localPosition;
        lGloveOrgRot = lGlove.localRotation;
        rGloveOrgRot = rGlove.localRotation;
        playerOrgPos = transform.position;
        playerOrgRot = transform.rotation;

        // dotween sequence
        leftGloveSequence = DOTween.Sequence();
        rightGloveSequence = DOTween.Sequence();



        //selfCamera = transform.Find("playerCam");
        if (selfCamera != null)
        {
            selfCameraComponent = selfCamera.GetComponent<Camera>();
        }

        // initialize ik target
        InitializeIKTarget();
    }

    void Update()
    {
        Vector3 ltargetWorldPos = lGlove.position;
        Quaternion ltargetWorldRot = lGlove.rotation;
        // update ik target with world position and rotation
        UpdateIKTarget("l", ltargetWorldPos, ltargetWorldRot);

        Vector3 rtargetWorldPos = rGlove.position;
        Quaternion rtargetWorldRot = rGlove.rotation;
        // update ik target with world position and rotation
        UpdateIKTarget("r", rtargetWorldPos, rtargetWorldRot);
    }

    void InitializeIKTarget(){
        // get the playerPunchIK component from the only active child from playerModel
        playerPunchIK = playerModel.GetComponentInChildren<PlayerPunchIK>();
        if (playerPunchIK == null)
        {
            Debug.LogError("PlayerPunchIK component not found in playerModel's children.");
            return;
        }
    }



    void UpdateIKTarget(string hand, Vector3 position, Quaternion rotation)
    {
        if (hand == "l" && playerPunchIK != null && playerPunchIK.leftHandIKTarget != null)
        {
            playerPunchIK.leftHandIKTarget.position = position;
            playerPunchIK.leftHandIKTarget.rotation = rotation;
        }
        else if(hand == "r" && playerPunchIK != null && playerPunchIK.rightHandIKTarget != null)
        {
            playerPunchIK.rightHandIKTarget.position = position;
            playerPunchIK.rightHandIKTarget.rotation = rotation;
        }
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
            transform.position = new Vector3(0f, 0, 0.5f);
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else if (playerIndex == 1)
        {
            transform.position = new Vector3(0f, 0, -0.5f);
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        SetGloveMaterial(playerIndex);
    }
    public void UpdatePlayerEquipment(int playerIndex)
    {
        int equipmentLeftId = DataManager.Instance.equippedEquipmentIds[2 * playerIndex];
        int equipmentRightId = DataManager.Instance.equippedEquipmentIds[2 * playerIndex + 1];

        // TODO: enable the equipment transform based on the equipment id
        // 根据装备 ID 确定要启用的子节点名称
        // 如果装备 ID 为 -1，则默认显示 "Default" 拳套
        string leftGloveEquipName = equipmentLeftId == -1 ? "Default" : "TE_" + DataManager.Instance.equipments[equipmentLeftId].name;
        string rightGloveEquipName = equipmentRightId == -1 ? "Default" : "TE_" + DataManager.Instance.equipments[equipmentRightId].name;

        Debug.Log("leftGloveEquipName: " + leftGloveEquipName);
        Debug.Log("rightGloveEquipName: " + rightGloveEquipName);
        // 遍历左拳套节点下的所有子节点，启用名称匹配的拳套，禁用其他
        foreach (Transform child in lGlove)
        {
            child.gameObject.SetActive(child.name == leftGloveEquipName);
        }

        // 遍历右拳套节点下的所有子节点，启用名称匹配的拳套，禁用其他
        foreach (Transform child in rGlove)
        {
            child.gameObject.SetActive(child.name == rightGloveEquipName);
        }
    }

    public void UpdatePlayerCharacter(int playerIndex){
        string characterName = DataManager.Instance.equippedCharacterIds[playerIndex] == -1 ? "Default" : DataManager.Instance.characters[DataManager.Instance.equippedCharacterIds[playerIndex]].name;

        foreach (Transform child in playerModel)
        {
            if (child.name == characterName)
            {
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }

        // update IK target
        InitializeIKTarget();
    }

    public void SetGloveMaterial(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= gloveMaterials.Count)
        {
            return;
        }
        Material gloveMaterial = playerIndex == 0 ? gloveMaterials[0] : gloveMaterials[1];
        selfMaterial = gloveMaterial;
        lgloveRenderer = lGlove.GetChild(0).GetComponent<Renderer>();
        rgloveRenderer = rGlove.GetChild(0).GetComponent<Renderer>();
        lgloveRenderer.material = gloveMaterial;
        rgloveRenderer.material = gloveMaterial;
    }

    public void SetCullLayer(int playerIndex)
    {
        // activate camera
        if (selfCamera != null) selfCamera.gameObject.SetActive(true);

        // set camera culling mask to exclude the layer of the other player
        int layerToRemove = playerIndex == 0 ? LayerMask.NameToLayer("P1") : LayerMask.NameToLayer("P2");
        foreach (Transform node in nodesToCull)
        {
            // loop over all child nodes and set the layer to the specified layer
            foreach (Transform child in node)
            {
                //Debug.Log("child name: " + child.name);
                //Debug.Log("child layer: " + child.gameObject.layer);
                child.gameObject.layer = layerToRemove;
            }
            //node.gameObject.layer = layerToRemove;
        }

        SetSelfCameraCullingMask(playerIndex);
    }

    private void SetSelfCameraCullingMask(int playerIndex)
    {
        if (selfCameraComponent == null)
        {
            Debug.LogWarning("selfCamera上未找到Camera组件");
            return;
        }

        int layerToRemove = -1;
        if (playerIndex == 0)
        {
            // 玩家0对应的layer名称为"P1"
            layerToRemove = LayerMask.NameToLayer("P1");
        }
        else if (playerIndex == 1)
        {
            // 玩家1对应的layer名称为"P2"
            layerToRemove = LayerMask.NameToLayer("P2");
        }
        else
        {
            Debug.LogError("无效的玩家索引，请传入0或1");
            return;
        }

        // 从摄像机的cullingMask中移除指定的Layer
        // 使用位运算将对应位清零
        selfCameraComponent.cullingMask &= ~(1 << layerToRemove);
    }

    public void DisableCamera()
    {
        selfCamera.gameObject.SetActive(false);
    }

    // ======================= Animation =======================
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

    public void AnimateChargeComplete(string hand)
    {
        if (hand == "l")
        {
            if (leftChargeCompleteTween != null)
            {
                leftChargeCompleteTween.Kill();
            }
            lgloveRenderer.material.color = Color.white;
            leftChargeCompleteTween = lgloveRenderer.material.DOColor(selfMaterial.color, 0.1f).SetLoops(-1, LoopType.Yoyo);

        }
        else
        {
            if (rightChargeCompleteTween != null)
            {
                rightChargeCompleteTween.Kill();
            }
            rgloveRenderer.material.color = Color.white;
            rightChargeCompleteTween = rgloveRenderer.material.DOColor(selfMaterial.color, 0.1f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void AnimatePunch(string hand, float duration)
    {
        Sequence sequence = GetOrResetSequence(hand);
        Transform glove = hand == "l" ? lGlove : rGlove;
        Vector3 targetPos = hand == "l" ? lPunchPos.localPosition : rPunchPos.localPosition;
        Quaternion targetRot = hand == "l" ? lPunchRot : rPunchRot;

        // kill color tween
        if (hand == "l")
        {
            if (leftChargeCompleteTween != null)
            {
                leftChargeCompleteTween.Kill();
                lgloveRenderer.material.color = selfMaterial.color;
            }
        }
        else
        {
            if (rightChargeCompleteTween != null)
            {
                rightChargeCompleteTween.Kill();
                rgloveRenderer.material.color = selfMaterial.color;
            }
        }

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
    
    public void AnimateKO(int playerIndex){
        //// player fall back
        Vector3 targetPos = playerOrgPos + new Vector3((-1) * (0.5f - playerIndex), 0.2f, playerIndex == 0 ? 0.7f : -0.7f);
        Quaternion targetRot = playerOrgRot * Quaternion.Euler(0, 0, 90);

        transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutBack);
        transform.DORotateQuaternion(targetRot, 0.5f).SetEase(Ease.OutBack);
    }

    public void AnimateKORecovery()
    {
        // player fall back
        Vector3 targetPos = playerOrgPos;
        Quaternion targetRot = playerOrgRot;

        transform.DOMove(targetPos, 0.5f).SetEase(Ease.OutBack);
        transform.DORotateQuaternion(targetRot, 0.5f).SetEase(Ease.OutBack);
    }


    public void ResetGloves(string hand)
    {
        if (hand == "l")
        {
            lGlove.localPosition = lGloveOrgPos;
            lGlove.localRotation = lGloveOrgRot;

            if (leftChargeCompleteTween != null)
            {
                leftChargeCompleteTween.Kill();
                lgloveRenderer.material.color = selfMaterial.color;
            }
        }
        else
        {
            rGlove.localPosition = rGloveOrgPos;
            rGlove.localRotation = rGloveOrgRot;

            if (rightChargeCompleteTween != null)
            {
                rightChargeCompleteTween.Kill();
                rgloveRenderer.material.color = selfMaterial.color;
            }
        }
    }


}
