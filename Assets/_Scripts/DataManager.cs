using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    // PLAYER DATA
    public int money;
    public HashSet<int> purchasedEquipmentIds = new HashSet<int>();
    public HashSet<int> purchasedCharacterIds = new HashSet<int>();
    public int[] equippedEquipmentIds = { -1, -1, -1, -1 };
    public int[] equippedCharacterIds = { -1, -1 };

    // CONFIG DATA
    public List<Equipment> equipments = new List<Equipment>();
    public List<Character> characters = new List<Character>();



    private string assetPath = "_Prefabs/Equipments/";

    private void Awake() {
        if(Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Destroy(gameObject);
        }

        Load();
    }

    public void Save(){
        SaveData saveData = new SaveData();
        saveData.money = money;
        // 将 HashSet 转换为 List 以便序列化
        saveData.purchasedEquipmentIds = new List<int>(purchasedEquipmentIds);

        string json = JsonUtility.ToJson(saveData);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
    }

    public void Load(){
        string path = Application.persistentDataPath + "/savefile.json";
        if(System.IO.File.Exists(path)){
            string json = System.IO.File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);
            money = saveData.money;
            // 将加载的 List 转换回 HashSet
            purchasedEquipmentIds = new HashSet<int>(saveData.purchasedEquipmentIds);
        }else{
            money = 0;
            purchasedEquipmentIds = new HashSet<int>();
            Debug.LogError("Save file not found");
        }
    }


    void OnValidate()
    {
        for(int i = 0; i < equipments.Count; i++){
            // check for existing equipment
            if(i != equipments.Count - 1){
                if(equipments[i].id != i){
                    Debug.LogError("Equipment id must be unique");
                }
                if(equipments[i].name == ""){
                    Debug.Log("Add default name for equipment " + i);
                    equipments[i].name = "Default_Equipment_" + i;
                }
                if(equipments[i].price < 0){
                    Debug.LogError("Price must be greater than or equal to 0, setting price to 0");
                    equipments[i].price = 0;
                }
            }
            // set default values for new equipment
            else{
                equipments[i].id = i;
                if(equipments[i].name == ""){
                    equipments[i].name = "Default_Equipment";
                }
                if(equipments[i].price < 0){
                    equipments[i].price = 0;
                }
            }
        }
    }
}

[System.Serializable]
public struct SaveData{
    public int money;
    public List<int> purchasedEquipmentIds;
    public List<int> purchasedCharacterIds;
    // P1: left-0, right-1, P2: left-2, right-3
    public List<int> equippedEquipmentIds;
    // P1: 0, P2: 1
    public List<int> equippedCharacterIds;
}
