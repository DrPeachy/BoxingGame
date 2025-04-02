using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // 引入 DOTween 命名空间

public class MainMenuCamera : MonoBehaviour
{
    private Camera selfCam;

    // 旋转中心点（例如太阳）
    public Transform pointToOscillate;
    
    // 最大摆动角度（单位：度）
    public float swingAngle = 30f;
    
    // 从 -swingAngle 到 swingAngle 所需时间（单位：秒）
    public float swingDuration = 2f;
    
    // 当前补间角度
    private float currentAngle;
    
    // 相机与中心点之间的初始偏移量
    private Vector3 initialOffset;
    
    // 相机的初始旋转
    private Quaternion originalRotation;
    
    // 轨道旋转轴——默认使用 Vector3.up
    public Vector3 orbitAxis = Vector3.up;

    void Start()
    {
        selfCam = GetComponent<Camera>();
        if (selfCam == null)
        {
            Debug.LogError("MainMenuCamera: Camera component not found!");
            return;
        }
        selfCam.clearFlags = CameraClearFlags.SolidColor;
        selfCam.backgroundColor = Color.black;
        
        if (pointToOscillate == null)
        {
            Debug.LogError("MainMenuCamera: pointToOscillate not set!");
            return;
        }
        
        // 记录初始偏移量和旋转
        originalRotation = transform.rotation;
        initialOffset = transform.position - pointToOscillate.position;
        
        // 初始角度设为 -swingAngle，后续在 [-swingAngle, swingAngle] 范围内往返
        currentAngle = -swingAngle;
        
        // 利用 DOTween 对 currentAngle 进行正弦缓动往返动画
        DOTween.To(() => currentAngle, x => { currentAngle = x; UpdateCameraTransform(); }, swingAngle, swingDuration)
               .SetEase(Ease.InOutSine)
               .SetLoops(-1, LoopType.Yoyo);
    }
    
    // 更新相机位置与旋转
    void UpdateCameraTransform()
    {
        // 计算当前的旋转量 Q（将初始偏移量旋转 currentAngle）
        Quaternion Q = Quaternion.AngleAxis(currentAngle, orbitAxis);
        
        // 新位置：中心点位置加上旋转后的偏移量
        transform.position = pointToOscillate.position + Q * initialOffset;
        
        // 更新相机旋转：将原始旋转按同样的 Q 旋转，施加了从中心向量的旋转变化
        transform.rotation = Q * originalRotation;
    }
    
}
        