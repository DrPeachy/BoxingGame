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
    [SerializeField] private List<int> playerOwnedCharIds;
    [SerializeField] private List<int> playerOwnedEquipIds;
    [SerializeField] private int playerPreviewCharIndex;
    [SerializeField] private int playerPreviewEquipLeftIndex;
    [SerializeField] private int playerPreviewEquipRightIndex;

    void Start()
    {
        leftArrow.onClick.AddListener(OnLeftArrowClick);
        rightArrow.onClick.AddListener(OnRightArrowClick);

        if (slotType == "Character") UpdatePlayerOwnedChar();
        if (slotType == "Equipment") UpdatePlayerOwnedEquip();
    }

    void OnEnable()
    {
        // every time the slot is enabled, update the list of player owned characters and equipments
        if (slotType == "Character") UpdatePlayerOwnedChar();
        if (slotType == "Equipment") UpdatePlayerOwnedEquip();
    }


    void UpdatePlayerOwnedChar()
    {
        // transfer the player owned character ids from DataManager to this script(from HashSet to List in increasing order)
        playerOwnedCharIds = new List<int>(DataManager.Instance.purchasedCharacterIds);
        // ensure the default character -1 is always in the list
        if (!playerOwnedCharIds.Contains(-1))
        {
            playerOwnedCharIds.Add(-1);
        }
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
        // 从 DataManager 获取玩家拥有的装备 ID（HashSet转换为List）
        playerOwnedEquipIds = new List<int>(DataManager.Instance.purchasedEquipmentIds);
        // 确保默认装备 -1 始终存在于列表中
        if (!playerOwnedEquipIds.Contains(-1))
        {
            playerOwnedEquipIds.Add(-1);
        }
        playerOwnedEquipIds.Sort();

        if (slotType == "Equipment")
        {
            int currentEquipIdLeft = DataManager.Instance.equippedEquipmentIds[2 * playerIndex];
            int currentEquipIdRight = DataManager.Instance.equippedEquipmentIds[2 * playerIndex + 1];

            // 对于左拳槽
            if (handIndex == 0)
            {
                slotName.text = currentEquipIdLeft == -1 ? "Default" : DataManager.Instance.equipments[currentEquipIdLeft].name;
                // 将当前装备在列表中的索引赋给预览变量
                playerPreviewEquipLeftIndex = playerOwnedEquipIds.IndexOf(currentEquipIdLeft);
            }
            // 对于右拳槽
            else if (handIndex == 1)
            {
                slotName.text = currentEquipIdRight == -1 ? "Default" : DataManager.Instance.equipments[currentEquipIdRight].name;
                // 将当前装备在列表中的索引赋给预览变量
                playerPreviewEquipRightIndex = playerOwnedEquipIds.IndexOf(currentEquipIdRight);
            }
        }
    }

    void OnLeftArrowClick()
    {
        //// Character
        if (slotType == "Character")
        {
            // early return if the player doesn't own any character
            if (playerOwnedCharIds.Count == 0) return;

            playerPreviewCharIndex = (playerPreviewCharIndex - 1 + playerOwnedCharIds.Count) % playerOwnedCharIds.Count;

            // update the data in cpu
            slotName.text = playerOwnedCharIds[playerPreviewCharIndex] == -1 ? "Default" : DataManager.Instance.characters[playerOwnedCharIds[playerPreviewCharIndex]].name;

            // update the data in cpu
            DataManager.Instance.equippedCharacterIds[playerIndex] = playerOwnedCharIds[playerPreviewCharIndex];

            if (playerIndex == 0)
            {
                lockerPanel.p1View.UpdatePlayerCharacter(playerIndex);
            }
            else if (playerIndex == 1)
            {
                lockerPanel.p2View.UpdatePlayerCharacter(playerIndex);
            }
        }

        //// Equipment
        else if (slotType == "Equipment")
        {
            // early return if the player doesn't own any equipment
            if (playerOwnedEquipIds.Count == 0) return;

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
            if (playerIndex == 0)
            {
                lockerPanel.p1View.UpdatePlayerEquipment(playerIndex);
            }
            else if (playerIndex == 1)
            {
                lockerPanel.p2View.UpdatePlayerEquipment(playerIndex);
            }

        }
    }

    void OnRightArrowClick()
    {
        //// Character
        if (slotType == "Character")
        {
            // early return if the player doesn't own any character
            if (playerOwnedCharIds.Count == 0) return;

            playerPreviewCharIndex = (playerPreviewCharIndex + 1) % playerOwnedCharIds.Count;

            // update the slot name
            slotName.text = playerOwnedCharIds[playerPreviewCharIndex] == -1 ? "Default" : DataManager.Instance.characters[playerOwnedCharIds[playerPreviewCharIndex]].name;

            // update the data in cpu
            DataManager.Instance.equippedCharacterIds[playerIndex] = playerOwnedCharIds[playerPreviewCharIndex];

            if (playerIndex == 0)
            {
                lockerPanel.p1View.UpdatePlayerCharacter(playerIndex);
            }
            else if (playerIndex == 1)
            {
                lockerPanel.p2View.UpdatePlayerCharacter(playerIndex);
            }
        }

        //// Equipment
        else if (slotType == "Equipment")
        {
            // early return if the player doesn't own any equipment
            if (playerOwnedEquipIds.Count == 0) return;

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
            if (playerIndex == 0)
            {
                lockerPanel.p1View.UpdatePlayerEquipment(playerIndex);
            }
            else if (playerIndex == 1)
            {
                lockerPanel.p2View.UpdatePlayerEquipment(playerIndex);
            }
        }
    }
}
