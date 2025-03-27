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

    public void SetMerchantData(string name, string description, int price, bool purchased, int id){
        merchantName = name;
        merchantDescription = description;
        merchantPrice = price;
        isPurchased = purchased;
        merchantID = id;

        nameText.text = merchantName;
        //descriptionText.text = merchantDescription;
        priceText.text = "Price: " + merchantPrice.ToString();
        isPurchaseText.text = isPurchased ? "Purchased" : "Buy";
        purchaseButton.interactable = !isPurchased;

    }

    // callback function for purchase button
    public void OnClickPurchase(){
        isPurchased = StoreManager.Instance.PurchaseEquipmentById(merchantID);
        isPurchaseText.text = isPurchased ? "Purchased" : "Buy";
        purchaseButton.interactable = !isPurchased;
    }
}
