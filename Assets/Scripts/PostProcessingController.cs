using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class PostProcessingController : MonoBehaviour
{
    PostProcessingData data;
    [SerializeField] private Volume volume;
    [SerializeField] private VolumeProfile volumeProfile;

    // Profile Properties
    private DepthOfField depthOfField;
    private Bloom bloom;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;

    // Effects Properties
    public float focusDistanceWithBlurOn = 0.1f;
    public float focusDistanceWithBlurOff = 7.68f;
    public float apertureWithBlurOn = 0.5f;
    public float apertureWithBlurOff = 7.1f;

    public void InitializePostProcessingProperties()
    {
        if (volumeProfile != null)
        {
            if (volumeProfile.TryGet<DepthOfField>(out var depthOfField))
            {
                this.depthOfField = depthOfField;
                this.depthOfField.active = true;
            }
            if (volumeProfile.TryGet<Bloom>(out var bloom))
            {
                this.bloom = bloom;
                this.bloom.active = true;
            }
            if (volumeProfile.TryGet<Vignette>(out var vignette))
            {
                this.vignette = vignette;
                this.vignette.active = true;
            }
            if (volumeProfile.TryGet<ChromaticAberration>(out var chromaticAberration))
            {
                this.chromaticAberration = chromaticAberration;
                this.chromaticAberration.active = true;
            }
        }
    }

    // Post Processing
    public bool SetPostProcessingVolumeProfile(string assetPath){
        VolumeProfile profile = Resources.Load<VolumeProfile>(assetPath);
        if(profile == null)
        {
            Debug.LogWarning("未能在路径 " + assetPath + " 加载到 VolumeProfile 资源");
            return false;
        }
        // assign the loaded profile to the volume
        volumeProfile = profile;
        volume.profile = volumeProfile;

        // initialize the post processing properties
        InitializePostProcessingProperties();
        return true;
    }

    public bool SetPostProcessingVolume(Volume volume)
    {
        if (volume == null) return false;

        // assign the loaded profile to the volume
        this.volume = volume;
        volumeProfile = volume.profile;

        // initialize the post processing properties
        InitializePostProcessingProperties();
        return true;
    }

    public bool SetPostProcessingVolumeProfile(VolumeProfile profile)
    {
        if (profile == null) return false;

        // assign the loaded profile to the volume
        volumeProfile = profile;
        volume.profile = volumeProfile;

        // initialize the post processing properties
        InitializePostProcessingProperties();
        return true;
    }

    public void EnableVolumeBlur()
    {
        if(depthOfField != null)
        {
            // depthOfField.active = true;
            depthOfField.focusDistance.value = focusDistanceWithBlurOn;
            depthOfField.aperture.value = apertureWithBlurOn;
        }
    }
    public void DisableVolumeBlur()
    {
        if (depthOfField != null)
        {
            depthOfField.focusDistance.value = focusDistanceWithBlurOff;
            depthOfField.aperture.value = apertureWithBlurOff;
        }
    }
}
