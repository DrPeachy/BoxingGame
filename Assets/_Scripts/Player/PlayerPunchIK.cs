using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerPunchIK : MonoBehaviour
{
    public float leftHandPositionWeight;
    public float leftHandRotationWeight;
    public float rightHandPositionWeight;
    public float rightHandRotationWeight;

    public Animator animator;
    public Transform leftHandIKTarget;
    public Transform rightHandIKTarget;
    public Quaternion rotationOffset;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        leftHandIKTarget = new GameObject().transform;
        rightHandIKTarget = new GameObject().transform;
        leftHandIKTarget.name = "LeftHandIKTarget";
        rightHandIKTarget.name = "RightHandIKTarget";
        leftHandIKTarget.parent = transform;
        rightHandIKTarget.parent = transform;
    }

    void OnAnimatorIK(int layerIndex)
    {
        Debug.Log("OnAnimatorIK");
        Quaternion leftHandRotationOffset = rotationOffset;
        Quaternion rightHandRotationOffset = new Quaternion(rotationOffset.x, -rotationOffset.y, -rotationOffset.z, rotationOffset.w);
        
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftHandPositionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftHandRotationWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandPositionWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandRotationWeight);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
        animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
        animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
    }
}
