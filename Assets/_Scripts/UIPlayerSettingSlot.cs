using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPlayerSettingSlot : MonoBehaviour
{
    public string slotType;
    public int playerIndex;
    public int handIndex;
    public Button leftArrow;
    public Button rightArrow;
    public TextMeshProUGUI slotName;
    public UILockerPanel lockerPanel;
    [SerializeField]private List<int> playerOwnedCharIds;
    [SerializeField]private List<int> playerOwnedEquipIds;
    [SerializeField]private int playerPreviewCharIndex;
    [SerializeField]private int playerPreviewEquipLeftIndex;
    [SerializeField]private int playerPreviewEquipRightIndex;

    void Start()
    {
        leftArrow.onClick.AddListener(OnLeftArrowClick);
        rightArrow.onClick.AddListener(OnRightArrowClick);
    }

    void OnEnable()
    {
        // every time the slot is enabled, update the list of player owned characters and equipments
        if(slotType == "Character") UpdatePlayerOwnedChar();
        if(slotType == "Equipment") UpdatePlayerOwnedEquip();
    }


    void UpdatePlayerOwnedChar()
    {
        // transfer the player owned character ids from DataManager to this script(from HashSet to List in increasing order)
        playerOwnedCharIds = new List<int>(DataManager.Instance.purchasedCharacterIds);
        playerOwnedCharIds.Sort();

        // set the slot name to the current equipped character/equipment name
        if (slotType == "Character")
        {
            int currentCharId = DataManager.Instance.equippedCharacterIds[playerIndex];

            slotName.text = currentCharId == -1 ? "Default" : DataManager.Instance.characters[currentCharId].name;

            // set the player preview character index to the current equipped character index
            playerPreviewCharIndex = playerOwnedCharIds.IndexOf(currentCharId);
        }
    }

    void UpdatePlayerOwnedEquip()
    {
        // transfer the player owned equipment ids from DataManager to this script(from HashSet to List in increasing order)
        playerOwnedEquipIds = new List<int>(DataManager.Instance.purchasedEquipmentIds);
        playerOwnedEquipIds.Sort();
        if (slotType == "Equipment")
        {
            int currentEquipIdLeft = DataManager.Instance.equippedEquipmentIds[2 * playerIndex];
            int currentEquipIdRight = DataManager.Instance.equippedEquipmentIds[2 * playerIndex + 1];

            // slot for hand
            if (handIndex == 0)
            {
                slotName.text = currentEquipIdLeft == -1 ? "Default" : DataManager.Instance.equipments[currentEquipIdLeft].name;

                // set the player preview equipment index to the current equipped equipment index
                playerPreviewEquipLeftIndex = playerOwnedEquipIds.IndexOf(currentEquipIdLeft);
            }
            else if (handIndex == 1)
            {
                slotName.text = currentEquipIdRight == -1 ? "Default" : DataManager.Instance.equipments[currentEquipIdRight].name;

                // set the player preview equipment index to the current equipped equipment index
                playerPreviewEquipRightIndex = playerOwnedEquipIds.IndexOf(currentEquipIdRight);
            }
        }
    }

    void OnLeftArrowClick()
    {
        if (slotType == "Character")
        {
            // early return if the player doesn't own any character
            if(playerOwnedCharIds.Count == 0) return;

            playerPreviewCharIndex = (playerPreviewCharIndex - 1 + playerOwnedCharIds.Count) % playerOwnedCharIds.Count;

            slotName.text = playerOwnedCharIds[playerPreviewCharIndex] == -1 ? "Default" : DataManager.Instance.characters[playerOwnedCharIds[playerPreviewCharIndex]].name;


        }
        else if (slotType == "Equipment")
        {
            // early return if the player doesn't own any equipment
            if(playerOwnedEquipIds.Count == 0) return;

            if (handIndex == 0)
            {
                playerPreviewEquipLeftIndex = (playerPreviewEquipLeftIndex - 1 + playerOwnedEquipIds.Count) % playerOwnedEquipIds.Count;

                // update the slot name
                slotName.text = playerOwnedEquipIds[playerPreviewEquipLeftIndex] == -1 ? "Default" : DataManager.Instance.equipments[playerOwnedEquipIds[playerPreviewEquipLeftIndex]].name;

                // update the data in cpu
                DataManager.Instance.equippedEquipmentIds[2 * playerIndex] = playerOwnedEquipIds[playerPreviewEquipLeftIndex];

            }
            else if (handIndex == 1)
            {
                playerPreviewEquipRightIndex = (playerPreviewEquipRightIndex - 1 + playerOwnedEquipIds.Count) % playerOwnedEquipIds.Count;

                // update the slot name
                slotName.text = playerOwnedEquipIds[playerPreviewEquipRightIndex] == -1 ? "Default" : DataManager.Instance.equipments[playerOwnedEquipIds[playerPreviewEquipRightIndex]].name;

                // update the data in cpu
                DataManager.Instance.equippedEquipmentIds[2 * playerIndex + 1] = playerOwnedEquipIds[playerPreviewEquipRightIndex];
            }


            // call corresponding playerview's method to update the player's equipment
            lockerPanel.p1View.UpdatePlayerEquipment(playerIndex);

        }
    }

    void OnRightArrowClick()
    {
        if (slotType == "Character")
        {
            // early return if the player doesn't own any character
            if(playerOwnedCharIds.Count == 0) return;
            
            playerPreviewCharIndex = (playerPreviewCharIndex + 1) % playerOwnedCharIds.Count;

            slotName.text = playerOwnedCharIds[playerPreviewCharIndex] == -1 ? "Default" : DataManager.Instance.characters[playerOwnedCharIds[playerPreviewCharIndex]].name;

        }
        else if (slotType == "Equipment")
        {
            // early return if the player doesn't own any equipment
            if(playerOwnedEquipIds.Count == 0) return;

            if (handIndex == 0)
            {
                playerPreviewEquipLeftIndex = (playerPreviewEquipLeftIndex + 1) % playerOwnedEquipIds.Count;

                // update the slot name
                slotName.text = playerOwnedEquipIds[playerPreviewEquipLeftIndex] == -1 ? "Default" : DataManager.Instance.equipments[playerOwnedEquipIds[playerPreviewEquipLeftIndex]].name;

                // update the data in cpu
                DataManager.Instance.equippedEquipmentIds[2 * playerIndex] = playerOwnedEquipIds[playerPreviewEquipLeftIndex];

            }
            else if (handIndex == 1)
            {
                playerPreviewEquipRightIndex = (playerPreviewEquipRightIndex + 1) % playerOwnedEquipIds.Count;

                // update the slot name
                slotName.text = playerOwnedEquipIds[playerPreviewEquipRightIndex] == -1 ? "Default" : DataManager.Instance.equipments[playerOwnedEquipIds[playerPreviewEquipRightIndex]].name;

                // update the data in cpu
                DataManager.Instance.equippedEquipmentIds[2 * playerIndex + 1] = playerOwnedEquipIds[playerPreviewEquipRightIndex];
            }

            // call corresponding playerview's method to update the player's equipment
            lockerPanel.p1View.UpdatePlayerEquipment(playerIndex);
        }
    }
}
