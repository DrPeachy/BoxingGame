using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIMainMenu : MonoBehaviour
{
    public Transform mainPanel;
    public Transform settingPanel;
    public Transform storePanel;
    public Transform lockerPanel;
    public Transform tutorialPanel;

    public Camera lockerCam;
    public Camera storeCam;
    public Transform lockerPreview;
    public Transform storePreview;

    // Post processing
    public PostProcessingController ppController;
    public Volume volume;
    public bool isPPLoaded = false;



    void Start()
    {
        InitializePostProcessing();
    }

    private void InitializePostProcessing()
    {
        isPPLoaded = ppController.SetPostProcessingVolume(volume);
    }

    private void DisableAllPanel()
    {
        mainPanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
        storePanel.gameObject.SetActive(false);
        lockerPanel.gameObject.SetActive(false);
        tutorialPanel.gameObject.SetActive(false);

        lockerCam.gameObject.SetActive(false);
        storeCam.gameObject.SetActive(false);
        lockerPreview.gameObject.SetActive(false);
        storePreview.gameObject.SetActive(false);

        if(isPPLoaded) ppController.DisableVolumeBlur();
    }

    public void OnClickSetting()
    {
        DisableAllPanel();
        settingPanel.gameObject.SetActive(true);
        
        if(isPPLoaded) ppController.EnableVolumeBlur();
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
        storeCam?.gameObject.SetActive(true);
        storePreview?.gameObject.SetActive(true);
        
        if(isPPLoaded) ppController.EnableVolumeBlur();
    }

    public void OnClickLocker()
    {
        DisableAllPanel();
        lockerPanel.gameObject.SetActive(true);
        lockerCam?.gameObject.SetActive(true);
        lockerPreview?.gameObject.SetActive(true);
        
        if(isPPLoaded) ppController.EnableVolumeBlur();
    }

    public void OnClickTutorial(){
        DisableAllPanel();
        tutorialPanel.gameObject.SetActive(true);
    }
}
