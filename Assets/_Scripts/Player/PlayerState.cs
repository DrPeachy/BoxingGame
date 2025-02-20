using System.Collections.Generic;

public enum PunchState
{
    Idle,
    HookCharge,
    HookChargeComplete,
    HookPunch,
    StraightPunch,
    Block,
    Parry,
    Recovery
}

public class PlayerState{
    public List<PunchState> punchStates;
    public float chargeTime;
    public float damageTaken;
    public PlayerState(PunchState lPunch, PunchState rPunch, float chargeTime = 0){
        punchStates = new List<PunchState>(){lPunch, rPunch};
        this.chargeTime = chargeTime;
        this.damageTaken = 0;
    }
}