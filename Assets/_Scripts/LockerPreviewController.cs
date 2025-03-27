using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockerPreviewController : MonoBehaviour
{
    public Transform p1;
    public Transform p2;
    public LayerMask layerMask;
    public Camera previewCam;
    public RectTransform previewUINode;
    public RectTransform previewP1ModelNode;
    public RectTransform previewP2ModelNode;
    public RenderTexture renderTexture;

    void Start()
    {
        InitializePlayerPreview();
        SetPreviewModelPosition();
    }

    void InitializePlayerPreview()
    {
        // set layer
        int layerIndex = (int)Mathf.Log(layerMask.value, 2);
        p1.gameObject.layer = layerIndex;
        p2.gameObject.layer = layerIndex;
    }

    void RemoveLayer()
    {
        p1.gameObject.layer = 0;
        p2.gameObject.layer = 0;
    }


    void SetPreviewModelPosition()
    {
        // 获取 UI 区域的尺寸
        Vector2 uiSize = previewUINode.sizeDelta;

        // 计算相机视野在世界单位下的尺寸
        float worldHeight = previewCam.orthographicSize * 2f;
        float worldWidth = worldHeight * previewCam.aspect;

        // 计算 UI 到世界的比例因子
        float scaleX = worldWidth / uiSize.x;
        float scaleY = worldHeight / uiSize.y;

        // 获取预览 UI 节点的局部位置（假设 pivot 为中心）
        Vector2 p1UIPos = previewP1ModelNode.anchoredPosition;
        Vector2 p2UIPos = previewP2ModelNode.anchoredPosition;

        // 将 UI 坐标转换为世界坐标
        Vector3 p1WorldPos = new Vector3(p1UIPos.x * scaleX, p1UIPos.y * scaleY, 0);
        Vector3 p2WorldPos = new Vector3(p2UIPos.x * scaleX, p2UIPos.y * scaleY, 0);

        // 设置 p1 和 p2 的位置（确保它们在正交相机视野内）
        p1.position = p1WorldPos;
        p2.position = p2WorldPos;

        // 可选：调试输出
        Debug.Log("p1 world pos: " + p1WorldPos + ", p2 world pos: " + p2WorldPos);
    }
}
