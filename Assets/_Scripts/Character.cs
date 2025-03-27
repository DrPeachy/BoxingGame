using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character{
    public int id;
    public string name;
    public string description;
    public int price;

    // stats
    [Header("Straight Punch Settings")]
    public float straightPunchWindup = 0.5f;
    public float straightPunchRecovery = 0.3f;
    public float straightPunchDamage = 5f;
    public float straightInterruptTime = 0.5f;

    [Header("Hook Punch Settings")]
    public float hookChargeDuration = 0.8f;
    public float hookPunchWindup = 0.7f;
    public float hookPunchRecovery = 0.4f;
    public float hookPunchDamage = 7f;

    [Header("Block Settings")]
    public float blockRecovery = 0.25f;
    public float parryDuration = 0.25f;
    public float parryRecovery = 0.9f;
    public float blockDamageReduction = 4f;


    // effect
    // NC_Float: base, additional, percentage
    // final value = base + additional + base * percentage
    // the effect is stored in this NC_Float
    // should avoid using operator directly between a NC_Float and this effect value
    public NC_Float effect;

    // to do: add 3D model for the equipment
    public GameObject model;

    // public Equipment(int id, string name, string description, int price, NC_Float effect){
    //     this.id = id;
    //     this.name = name;
    //     this.description = description;
    //     this.price = price;
    //     this.effect = effect;
    // }


}