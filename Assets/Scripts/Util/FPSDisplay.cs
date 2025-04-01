using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;

    void Awake()
    {
        // 保证该对象在切换场景时不被销毁
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // 使用指数加权平均平滑帧率
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();

        // 在屏幕左上角显示 FPS 信息
        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = Color.green;

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} FPS)", msec, fps);
        GUI.Label(rect, text, style);
    }
}
