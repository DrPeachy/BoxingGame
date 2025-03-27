using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStorePanel : MonoBehaviour
{
    public Transform merchantSlotPrefab;
    public Transform merchantParent;
    // Start is called before the first frame update
    void Start()
    {
        InitializePanel();
    }

    public void InitializePanel(){
        foreach(Equipment m in StoreManager.Instance.equipments){
            var slot = Instantiate(merchantSlotPrefab, merchantParent);
            slot.SetParent(merchantParent);
            // set equipment data
            slot.GetComponent<UIMerchantSlot>().SetMerchantData(m.name, m.description, m.price, false, m.id);
        }
    }
}
