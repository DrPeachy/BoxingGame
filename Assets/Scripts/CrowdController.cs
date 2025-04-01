using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrowdController : MonoBehaviour
{
    void Start()
    {
        // make an animation to control the crowd to keep jumping
        transform.DOJump(transform.position, 0.1f, 1, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }
}
