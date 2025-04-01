using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;


public class CameraShake : MonoBehaviour
{
    public Vector3 originalPosition;

    void Awake()
    {
        originalPosition = transform.position;   
    }

    public async UniTask Shake(float duration, float magnitude){
        float elapsed = 0.0f;

        while(elapsed < duration){
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            await UniTask.Yield();

            transform.position = originalPosition;
        }
    }
}
