using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class PlayerEffect : MonoBehaviour
{
    [Header("References")]
    public CameraShake cameraShake;
    public Image flashImage;

    [Header("Camera Shake")]
    public float cameraShakeDuration = 0.1f;
    public float cameraShakeMagnitude = 0.1f;


    [Header("Flash")]
    public float flashDuration = 0.1f;
    public float flashStartAlpha = 1f;


    void Start()
    {
        if(cameraShake == null){
            cameraShake = GetComponent<PlayerView>().selfCamera.GetComponent<CameraShake>();
        }
    }




    public void TriggerCameraShake(float duration, float magnitude){
        _ = cameraShake.Shake(duration, magnitude);
    }

    public void TriggerCameraShake(){
        _ = cameraShake.Shake(cameraShakeDuration, cameraShakeMagnitude);
    }

    public void TriggerFlash(float duration, float startAlpha){
        _ = TriggerFlashEffect(duration, startAlpha);
    }

    public void TriggerFlash(float startAlpha){
        _ = TriggerFlashEffect(flashDuration, startAlpha);
    }

    public void TriggerFlash(){
        _ = TriggerFlashEffect(flashDuration, flashStartAlpha);
    }




    public async UniTaskVoid TriggerFlashEffect(float flashDuration, float flashStartAlpha){
        flashStartAlpha = Mathf.Clamp(flashStartAlpha, 0, 1);
        Vector3 flashColorRGB = new Vector3(flashImage.color.r, flashImage.color.g, flashImage.color.b);
        flashImage.color = new Color(flashColorRGB.x, flashColorRGB.y, flashColorRGB.z, flashStartAlpha);

        float elapsedTime = 0f;
        while(elapsedTime < flashDuration){
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(flashStartAlpha, 0, elapsedTime / flashDuration);
            flashImage.color = new Color(flashColorRGB.x, flashColorRGB.y, flashColorRGB.z, alpha);
            await UniTask.Yield();
        }
        flashImage.color = new Color(flashColorRGB.x, flashColorRGB.y, flashColorRGB.z, 0);
    }

}
