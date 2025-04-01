using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
    public Transform mainPanel;
    public Transform settingPanel;
    public Transform storePanel;
    public Transform lockerPanel;
    void Start()
    {
        
    }

    private void DisableAllPanel()
    {
        mainPanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
        storePanel.gameObject.SetActive(false);
        lockerPanel.gameObject.SetActive(false);
    }

    public void OnClickSetting()
    {
        DisableAllPanel();
        settingPanel.gameObject.SetActive(true);
    }

    public void OnClickBack()
    {
        DisableAllPanel();
        mainPanel.gameObject.SetActive(true);
    }

    public void OnClickStore()
    {
        DisableAllPanel();
        storePanel.gameObject.SetActive(true);
    }

    public void OnClickLocker()
    {
        DisableAllPanel();
        lockerPanel.gameObject.SetActive(true);
    }
}
