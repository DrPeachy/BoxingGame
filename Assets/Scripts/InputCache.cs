using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputCache : MonoBehaviour
{
    public bool leftPunchBuffered;
    public bool leftBlockBuffered;
    public bool leftChargeBuffered;
    public bool rightPunchBuffered;
    public bool rightBlockBuffered;
    public bool rightChargeBuffered;

    public float bufferTime = 0.1f;

    [Header("Hold")]
    public bool leftPunchHold;
    public bool leftBlockHold;
    public bool leftChargeHold;
    public bool rightPunchHold;
    public bool rightBlockHold;
    public bool rightChargeHold;

    private float leftBufferTimer;
    private float rightBufferTimer;
    

    void Update()
    {
        if(leftBufferTimer > 0){
            leftBufferTimer -= Time.deltaTime;
            if(leftBufferTimer <= 0){
                leftPunchBuffered = false;
                leftBlockBuffered = false;
                leftChargeBuffered = false;
            }
        }
        if(rightBufferTimer > 0){
            rightBufferTimer -= Time.deltaTime;
            if(rightBufferTimer <= 0){
                rightPunchBuffered = false;
                rightBlockBuffered = false;
                rightChargeBuffered = false;
            }
        }
    }

    public void HoldAction(string hand, string action){
        if(hand == "l"){
            if(action == "Punch"){
                leftPunchHold = true;
            }
            else if(action == "Block"){
                leftBlockHold = true;
            }
            else if(action == "Charge"){
                leftChargeHold = true;
            }
        }
        else if(hand == "r"){
            if(action == "Punch"){
                rightPunchHold = true;
            }
            else if(action == "Block"){
                rightBlockHold = true;
            }
            else if(action == "Charge"){
                rightChargeHold = true;
            }
        }
    }

    public void ReleaseAction(string hand, string action){
        if(hand == "l"){
            if(action == "Punch"){
                leftPunchHold = false;
            }
            else if(action == "Block"){
                leftBlockHold = false;
            }
            else if(action == "Charge"){
                leftChargeHold = false;
            }
        }
        else if(hand == "r"){
            if(action == "Punch"){
                rightPunchHold = false;
            }
            else if(action == "Block"){
                rightBlockHold = false;
            }
            else if(action == "Charge"){
                rightChargeHold = false;
            }
        }
    }

    public void ResetHold(string hand = "both"){
        if(hand == "l"){
            leftPunchHold = false;
            leftBlockHold = false;
            leftChargeHold = false;
        }
        else if(hand == "r"){
            rightPunchHold = false;
            rightBlockHold = false;
            rightChargeHold = false;
        }
        else if(hand == "both"){
            leftPunchHold = false;
            leftBlockHold = false;
            leftChargeHold = false;
            rightPunchHold = false;
            rightBlockHold = false;
            rightChargeHold = false;
        }
    }

    public void PushAction(string hand, string action){
        if(hand == "l"){
            if(action == "Punch"){
                ResetHand(hand);
                leftPunchBuffered = true;
                leftBufferTimer = bufferTime;
            }
            else if(action == "Block"){
                ResetHand(hand);
                leftBlockBuffered = true;
                leftBufferTimer = bufferTime;
            }
            else if(action == "Charge"){
                ResetHand(hand);
                leftChargeBuffered = true;
                leftBufferTimer = bufferTime;
            }
        }
        else if(hand == "r"){
            if(action == "Punch" && !rightPunchBuffered){
                ResetHand(hand);
                rightPunchBuffered = true;
                rightBufferTimer = bufferTime;
            }
            else if(action == "Block" && !rightBlockBuffered){
                ResetHand(hand);
                rightBlockBuffered = true;
                rightBufferTimer = bufferTime;
            }
            else if(action == "Charge" && !rightChargeBuffered){
                ResetHand(hand);
                rightChargeBuffered = true;
                rightBufferTimer = bufferTime;
            }
        }
        else if(action == "Cancel"){
            ResetHand(hand);
        }
    }

    public void PopBufferedAction(int playerIndex, string hand){
        if(hand == "l"){
            if(leftPunchBuffered){
                _ = LocalModeGameManager.Instance.HandlePlayerAction(playerIndex, hand, "Punch");
            }
            else if(leftBlockBuffered || leftBlockHold){
                _ = LocalModeGameManager.Instance.HandlePlayerAction(playerIndex, hand, "Block");
            }
            else if(leftChargeBuffered){
                _ = LocalModeGameManager.Instance.HandlePlayerAction(playerIndex, hand, "Charge");
            }
            ResetHand(hand);
        }
        else if(hand == "r"){
            if(rightPunchBuffered){
                _ = LocalModeGameManager.Instance.HandlePlayerAction(playerIndex, hand, "Punch");
            }
            else if(rightBlockBuffered || rightBlockHold){
                _ = LocalModeGameManager.Instance.HandlePlayerAction(playerIndex, hand, "Block");
            }
            else if(rightChargeBuffered){
                _ = LocalModeGameManager.Instance.HandlePlayerAction(playerIndex, hand, "Charge");
            }
            ResetHand(hand);
        }
    }

    private void ResetHand(string hand)
    {
        if(hand == "l"){
            leftPunchBuffered = false;
            leftBlockBuffered = false;
            leftChargeBuffered = false;
            leftBufferTimer = 0;
        }
        else if(hand == "r"){
            rightPunchBuffered = false;
            rightBlockBuffered = false;
            rightChargeBuffered = false;
            rightBufferTimer = 0;
        }
    }


}
