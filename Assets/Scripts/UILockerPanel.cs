using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILockerPanel : MonoBehaviour
{
    public RectTransform p1Locker;
    public RectTransform p2Locker;
    public Transform playerSettingSlotPrefab;
    public UIPlayerSettingSlot p1CharSlot;
    public UIPlayerSettingSlot p1EquipLeftSlot;
    public UIPlayerSettingSlot p1EquipRightSlot;
    public UIPlayerSettingSlot p2CharSlot;
    public UIPlayerSettingSlot p2EquipLeftSlot;
    public UIPlayerSettingSlot p2EquipRightSlot;
    public PlayerView p1View;
    public PlayerView p2View;

    void Awake()
    {
        InitializeSlots();
    }

    void InitializeSlots()
    {
        // instantiate player setting slot
        p1CharSlot = Instantiate(playerSettingSlotPrefab, p1Locker).GetComponent<UIPlayerSettingSlot>();
        p2CharSlot = Instantiate(playerSettingSlotPrefab, p2Locker).GetComponent<UIPlayerSettingSlot>();
        p1EquipLeftSlot = Instantiate(playerSettingSlotPrefab, p1Locker).GetComponent<UIPlayerSettingSlot>();
        p2EquipLeftSlot = Instantiate(playerSettingSlotPrefab, p2Locker).GetComponent<UIPlayerSettingSlot>();
        p1EquipRightSlot = Instantiate(playerSettingSlotPrefab, p1Locker).GetComponent<UIPlayerSettingSlot>();
        p2EquipRightSlot = Instantiate(playerSettingSlotPrefab, p2Locker).GetComponent<UIPlayerSettingSlot>();

        // set slot type
        p1CharSlot.slotType = "Character";
        p2CharSlot.slotType = "Character";
        p1EquipLeftSlot.slotType = "Equipment";
        p2EquipLeftSlot.slotType = "Equipment";
        p1EquipRightSlot.slotType = "Equipment";
        p2EquipRightSlot.slotType = "Equipment";

        // set locker panel(parent) reference
        p1CharSlot.lockerPanel = this;
        p2CharSlot.lockerPanel = this;
        p1EquipLeftSlot.lockerPanel = this;
        p2EquipLeftSlot.lockerPanel = this;
        p1EquipRightSlot.lockerPanel = this;
        p2EquipRightSlot.lockerPanel = this;

        // set player index
        p1CharSlot.playerIndex = 0;
        p2CharSlot.playerIndex = 1;
        p1EquipLeftSlot.playerIndex = 0;
        p2EquipLeftSlot.playerIndex = 1;
        p1EquipRightSlot.playerIndex = 0;
        p2EquipRightSlot.playerIndex = 1;

        // set hand index
        p1EquipLeftSlot.handIndex = 0;
        p2EquipLeftSlot.handIndex = 0;
        p1EquipRightSlot.handIndex = 1;
        p2EquipRightSlot.handIndex = 1;
    }

}
