using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;

    public List<Equipment> equipments = new List<Equipment>();
    public int index;

    void Awake()
    {
        if(Instance == null){
            Instance = this;
        }else{
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // read from data manager
        equipments = DataManager.Instance.equipments;
        index = 0;   
    }

    public void NextEquipment()
    {
        index++;
        if(index >= equipments.Count){
            index = 0;
        }
    }

    public void PreviousEquipment()
    {
        index--;
        if(index < 0){
            index = equipments.Count - 1;
        }
    }


}
