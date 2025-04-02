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

    public Camera lockerCam;
    public Camera storeCam;
    public Transform lockerPreview;
    public Transform storePreview;

    // Post processing
    public Volume volume;
    private DepthOfField depthOfField;
    public float focusDistanceWithBlurOn = 0.1f;
    public float focusDistanceWithBlurOff = 7.68f;
    public float apertureWithBlurOn = 0.5f;
    public float apertureWithBlurOff = 7.1f;


    void Start()
    {
        InitializePostProcessingProperties();
    }

    private void DisableAllPanel()
    {
        mainPanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
        storePanel.gameObject.SetActive(false);
        lockerPanel.gameObject.SetActive(false);

        lockerCam.gameObject.SetActive(false);
        storeCam.gameObject.SetActive(false);
        lockerPreview.gameObject.SetActive(false);
        storePreview.gameObject.SetActive(false);

        DisableVolumeBlur();
    }

    public void InitializePostProcessingProperties()
    {
        if (volume != null)
        {
            if (volume.profile.TryGet<DepthOfField>(out var depthOfField))
            {
                this.depthOfField = depthOfField;
                this.depthOfField.active = true;
            }
        }
    }

    private void EnablVolumneBlur()
    {
        if(depthOfField != null)
        {
            // depthOfField.active = true;
            depthOfField.focusDistance.value = focusDistanceWithBlurOn;
            depthOfField.aperture.value = apertureWithBlurOn;
        }
    }
    private void DisableVolumeBlur()
    {
        if (depthOfField != null)
        {
            depthOfField.focusDistance.value = focusDistanceWithBlurOff;
            depthOfField.aperture.value = apertureWithBlurOff;
        }
    }

    public void OnClickSetting()
    {
        DisableAllPanel();
        settingPanel.gameObject.SetActive(true);
        EnablVolumneBlur();
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
        EnablVolumneBlur();
    }

    public void OnClickLocker()
    {
        DisableAllPanel();
        lockerPanel.gameObject.SetActive(true);
        lockerCam?.gameObject.SetActive(true);
        lockerPreview?.gameObject.SetActive(true);
        EnablVolumneBlur();
    }
}
