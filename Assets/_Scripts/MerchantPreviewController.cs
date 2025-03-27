using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MerchantPreviewController : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Camera merchantCam;
    Vector2 textureResolution;
    public RectTransform merchantUINode;
    public Transform merchantModelParent;
    public Vector2 numRowCol = new Vector2(3, 4);
    int pageIndex = 0;
    public float modelScaleFactor = 0.5f;

    void Start()
    {
        InitializeRenderTexture();
        InstantiateMerchantModel();
    }

    void InitializeRenderTexture()
    {
        // check null
        if (!merchantCam || !merchantUINode || !merchantModelParent)
        {
            Debug.LogError("MerchantController: missing components");
            return;
        }

        // get resolution from UI node
        textureResolution = merchantUINode.sizeDelta;
        // change renderTextureture resolution
        ChangerenderTexturetureResolution((int)textureResolution.x, (int)textureResolution.y);
        // set renderTextureture to camera
        merchantCam.targetTexture = renderTexture;
        // adjust camera aspect
        AdjustCameraAspect();
    }

    void ChangerenderTexturetureResolution(int width, int height)
    {
        if (renderTexture != null)
        {
            // 释放当前的 renderTextureture
            renderTexture.Release();
            // 修改分辨率
            renderTexture.width = width;
            renderTexture.height = height;
            // 重新创建 renderTextureture
            renderTexture.Create();
        }
    }

    void AdjustCameraAspect() {
    if (renderTexture != null && merchantCam != null) {
        merchantCam.aspect = (float)renderTexture.width / renderTexture.height;
    }
}
    public void InstantiateMerchantModel()
    {
        // 清理旧的模型，防止重复实例化
        foreach (Transform child in merchantModelParent)
        {
            Destroy(child.gameObject);
        }

        int rows = (int)numRowCol.x;     // 行数
        int columns = (int)numRowCol.y;  // 列数
        int itemsPerPage = rows * columns;

        // 根据 pageIndex 计算当前页的起始索引
        int startIndex = pageIndex * itemsPerPage;
        var equipments = DataManager.Instance.equipments;
        if (startIndex >= equipments.Count)
        {
            Debug.LogWarning("当前页没有足够的装备数据");
            return;
        }

        // 计算正交摄像机视野在世界单位下的宽高
        float cameraHeight = merchantCam.orthographicSize * 2f;
        float cameraWidth = cameraHeight * merchantCam.aspect;

        Debug.Log("cameraWidth: " + cameraWidth + ", cameraHeight: " + cameraHeight);

        // 每个格子在世界单位下的尺寸
        float cellWidth = cameraWidth / columns;
        float cellHeight = cameraHeight / rows;

        Debug.Log("cellWidth: " + cellWidth + ", cellHeight: " + cellHeight);

        // 设置视野左上角在 merchantModelParent 坐标系中的位置
        // 假设 merchantModelParent 的局部原点和摄像机视野中心对齐
        Vector3 origin = new Vector3(-cameraWidth / 2f, cameraHeight / 2f, 0);

        // 遍历当前页的装备数据，按照网格中心位置实例化模型
        for (int i = startIndex; i < Mathf.Min(startIndex + itemsPerPage, equipments.Count); i++)
        {
            int relativeIndex = i - startIndex;  // 当前页内的索引
            int col = relativeIndex % columns;
            int row = relativeIndex / columns;

            // X 坐标：从左向右，每个单元格居中
            float posX = origin.x + col * cellWidth + cellWidth / 2f;
            // Y 坐标：从上向下，每个单元格居中
            float posY = origin.y - row * cellHeight - cellHeight / 2f;
            Vector3 pos = new Vector3(posX, posY, 0);

            Debug.Log("pos: " + pos);

            // 实例化模型，并设置为 merchantModelParent 的子对象，同时设置局部位置
            GameObject model = Instantiate(equipments[i].model, merchantModelParent);
            model.transform.localScale = Vector3.one * modelScaleFactor;
            model.transform.localPosition = pos;

            // set model to rotate using DOTween in constant speed
            model.transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart);
        }
    }


}
