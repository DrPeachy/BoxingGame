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
    public List<float> chargeTimes;
    public float damageTaken;
    public PlayerState(PunchState lPunch, PunchState rPunch, float chargeTime = 0){
        punchStates = new List<PunchState>(){lPunch, rPunch};
        this.chargeTimes = new List<float>(){chargeTime, chargeTime};
        this.damageTaken = 0;
    }
}