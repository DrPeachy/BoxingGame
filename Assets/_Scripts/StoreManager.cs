using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using TMPro.EditorUtilities;


public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance;

    public List<Equipment> equipments = new List<Equipment>();
    public int index;

    public Transform prefabDisplayTransform;
    private GameObject currentPrefab;

    [Header("Buttons")]
    public GameObject nextButton;
    public GameObject previousButton;
    public GameObject purchaseButton;
    [Header("Texts")]
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text priceText;


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
        if(prefabDisplayTransform == null){
            prefabDisplayTransform.position = new Vector3(0, 0, 0);
            prefabDisplayTransform.rotation = Quaternion.identity;
            prefabDisplayTransform.localScale = new Vector3(1, 1, 1);
        }
        LoadPrefabToDisplay();
    }

    public void NextEquipment()
    {
        index++;
        if(index >= equipments.Count){
            index = 0;
        }
        LoadPrefabToDisplay();
    }

    public void PreviousEquipment()
    {
        index--;
        if(index < 0){
            index = equipments.Count - 1;
        }
        LoadPrefabToDisplay();
    }

    public void PurchaseEquipment(){
        if(DataManager.Instance.money >= equipments[index].price){
            DataManager.Instance.money -= equipments[index].price;
            DataManager.Instance.purchasedEquipmentIds.Add(equipments[index].id);

            // make sure the id is unique
            if(DataManager.Instance.purchasedEquipmentIds.Count != DataManager.Instance.purchasedEquipmentIds.Distinct().Count()){
                Debug.LogError("Redundant equipment id in purchased list");
            }

            purchaseButton.SetActive(false);
            priceText.text = "Purchased";
        }else{
            Debug.Log("Not enough money");
        }
    }

    public void LoadPrefabToDisplay(){
        if(currentPrefab != null){
            Destroy(currentPrefab);
        }
        currentPrefab = Instantiate(equipments[index].model, prefabDisplayTransform.position, prefabDisplayTransform.rotation, prefabDisplayTransform);
        nameText.text = equipments[index].name;
        // descriptionText.text = equipments[index].description;
        
        // check if the equipment is already purchased
        if(DataManager.Instance.purchasedEquipmentIds.Contains(equipments[index].id)){
            purchaseButton.SetActive(false);
            priceText.text = "Purchased";
        }else{
            purchaseButton.SetActive(true);
            // set price text
            priceText.text = "Price: " + equipments[index].price.ToString();
        }

    }


}
