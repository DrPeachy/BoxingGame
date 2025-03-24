using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Equipment{
    public int id;
    public string name;
    public string description;
    public int price;

    // effect
    // NC_Float: base, additional, percentage
    // final value = base + additional + base * percentage
    // the effect is stored in this NC_Float
    // should avoid using operator directly between a NC_Float and this effect value
    public NC_Float effect;

    // to do: add 3D model for the equipment
    public GameObject model;

    public Equipment(int id, string name, string description, int price, NC_Float effect){
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.effect = effect;
    }

    public Equipment(int id, string name, string description, int price, float baseValue, float additionalValue, float percentageValue){
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.effect = new NC_Float(baseValue, additionalValue, percentageValue);
    }




  

}