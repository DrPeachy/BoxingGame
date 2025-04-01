using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIMerchantSlot : MonoBehaviour
{
    string merchantName;
    string merchantDescription;
    int merchantPrice;
    bool isPurchased;

    // UI elements
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;
    public TMP_Text isPurchaseText;
    public Button purchaseButton;
    public int merchantID;
    public string merchantType;

    public void SetMerchantData(string name, string description, int price, bool purchased, int id, string type){
        merchantName = name;
        merchantDescription = description;
        merchantPrice = price;
        isPurchased = purchased;
        merchantID = id;
        merchantType = type;

        nameText.text = merchantName;
        //descriptionText.text = merchantDescription;
        priceText.text = "Price: " + merchantPrice.ToString();
        isPurchaseText.text = isPurchased ? "Purchased" : "Buy";
        purchaseButton.interactable = !isPurchased;

    }

    // callback function for purchase button
    public void OnClickPurchase(){
        if (merchantType == "Equipment")
        {
            isPurchased = StoreManager.Instance.PurchaseEquipmentById(merchantID);
        }
        else if (merchantType == "Character")
        {
            isPurchased = StoreManager.Instance.PurchaseCharacterById(merchantID);
        }
        else
        {
            Debug.LogError("Invalid merchant type: " + merchantType);
            return;
        }
        
        isPurchaseText.text = isPurchased ? "Purchased" : "Buy";
        purchaseButton.interactable = !isPurchased;
    }
}
