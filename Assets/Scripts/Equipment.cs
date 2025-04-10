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
    public CharacterSetting equipmentEffect; // effect applied to the character



    // to do: add 3D model for the equipment
    public GameObject model;

    public Equipment(int id, string name, string description, int price){
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
    }

    public Equipment(int id, string name, string description, int price, CharacterSetting effect){
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.equipmentEffect = effect;
    }




  

}