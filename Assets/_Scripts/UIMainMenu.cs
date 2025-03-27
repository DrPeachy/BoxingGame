using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : MonoBehaviour
{
    public Transform mainPanel;
    public Transform settingPanel;
    void Start()
    {
        
    }

    private void DisableAllPanel()
    {
        mainPanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
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
}
