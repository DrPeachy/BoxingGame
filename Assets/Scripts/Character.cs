using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

[System.Serializable]
public class CharacterSetting{
    [Header("Straight Punch Settings")]
    public NC_Float straightPunchWindup;
    public NC_Float straightPunchRecovery;
    public NC_Float straightBlockedRecovery;
    public NC_Float straightPunchDamage;
    public NC_Float straightInterruptTime;

    [Header("Hook Punch Settings")]
    public NC_Float hookChargeDuration;
    public NC_Float hookPunchWindup;
    public NC_Float hookPunchRecovery;
    public NC_Float hookPunchDamage;

    [Header("Block Settings")]
    public NC_Float blockRecovery;
    public NC_Float parryDuration;
    public NC_Float parryRecovery;
    public NC_Float blockDamageReduction;

    public CharacterSetting(){}

    public CharacterSetting(Character character){
        straightPunchWindup = character.straightPunchWindup;
        straightPunchRecovery = character.straightPunchRecovery;
        straightBlockedRecovery = character.straightBlockedRecovery;
        straightPunchDamage = character.straightPunchDamage;
        straightInterruptTime = character.straightInterruptTime;

        hookChargeDuration = character.hookChargeDuration;
        hookPunchWindup = character.hookPunchWindup;
        hookPunchRecovery = character.hookPunchRecovery;
        hookPunchDamage = character.hookPunchDamage;

        blockRecovery = character.blockRecovery;
        parryDuration = character.parryDuration;
        parryRecovery = character.parryRecovery;
        blockDamageReduction = character.blockDamageReduction;
    }

    public CharacterSetting(CharacterSetting other){
        straightPunchWindup = other.straightPunchWindup;
        straightPunchRecovery = other.straightPunchRecovery;
        straightBlockedRecovery = other.straightBlockedRecovery;
        straightPunchDamage = other.straightPunchDamage;
        straightInterruptTime = other.straightInterruptTime;

        hookChargeDuration = other.hookChargeDuration;
        hookPunchWindup = other.hookPunchWindup;
        hookPunchRecovery = other.hookPunchRecovery;
        hookPunchDamage = other.hookPunchDamage;

        blockRecovery = other.blockRecovery;
        parryDuration = other.parryDuration;
        parryRecovery = other.parryRecovery;
        blockDamageReduction = other.blockDamageReduction;
    }

    public void ApplyEquipmentEffect(CharacterSetting equipmentEffect){
        straightPunchWindup.addEffect(equipmentEffect.straightPunchWindup);
        straightPunchRecovery.addEffect(equipmentEffect.straightPunchRecovery);
        straightBlockedRecovery.addEffect(equipmentEffect.straightBlockedRecovery);
        straightPunchDamage.addEffect(equipmentEffect.straightPunchDamage);
        straightInterruptTime.addEffect(equipmentEffect.straightInterruptTime);

        hookChargeDuration.addEffect(equipmentEffect.hookChargeDuration);
        hookPunchWindup.addEffect(equipmentEffect.hookPunchWindup);
        hookPunchRecovery.addEffect(equipmentEffect.hookPunchRecovery);
        hookPunchDamage.addEffect(equipmentEffect.hookPunchDamage);

        blockRecovery.addEffect(equipmentEffect.blockRecovery);
        parryDuration.addEffect(equipmentEffect.parryDuration);
        parryRecovery.addEffect(equipmentEffect.parryRecovery);
        blockDamageReduction.addEffect(equipmentEffect.blockDamageReduction);
    }
}


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
    public float straightBlockedRecovery = 0.6f;
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

    //public CharacterSetting characterDefaultSetting;


    // effect
    // NC_Float: base, additional, percentage
    // final value = base + additional + base * percentage
    // the effect is stored in this NC_Float
    // should avoid using operator directly between a NC_Float and this effect value
    //public NC_Float effect;

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