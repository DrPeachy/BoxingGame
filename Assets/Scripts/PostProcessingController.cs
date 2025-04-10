using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using Cysharp.Threading.Tasks;


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
    private LensDistortion lensDistortion;

    /// Effects Properties
    // Depth of Field
    [Header("Effects Property - Depth of Field")]
    public float blurOnFocusDistance = 0.1f;
    public float blurOffFocusDistance = 7.68f;
    public float blurOnAperture = 0.5f;
    public float blurOffAperture = 7.1f;
    public float injuryFocalLength = 80f;
    public float normalFocalLength = 33f;

    // vignette
    [Header("Effects Property - Vignette")]
    public Color injuryVignetteColor = new Color(1f, 0.5f, 0f, 1f);
    public Color normalVignetteColor = new Color(0.1f, 0.1f, 0.2f, 1f);
    public float injuryVignetteInt = 0.7f;
    public float normalVignetteInt = 0.2f;

    // bloom dirty lens
    [Header("Effects Property - Bloom")]
    public float injuryBloomSweatValue = 100f;
    public float normalBloomSweatValue = 0f;

    // chromatic aberration
    [Header("Effects Property - Chromatic Aberration")]
    public float injuryChromaticAberrationInt = 1.0f;
    public float normalChromaticAberrationInt = 0.0f;

    // lens distortion
    [Header("Effects Property - Lens Distortion")]
    public float injuryLensDistortionValue = 0.5f;
    public float normalLensDistortionValue = 0.0f;

    private float currentInjury = 0f;
    private Tween injuryTween;
    public float injuryDecayTime = 5f;

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
            if (volumeProfile.TryGet<LensDistortion>(out var lensDistortion))
            {
                this.lensDistortion = lensDistortion;
                this.lensDistortion.active = true;
            }
        }
    }

    // Post Processing
    public bool SetPostProcessingVolumeProfile(string assetPath)
    {
        VolumeProfile profile = Resources.Load<VolumeProfile>(assetPath);
        if (profile == null)
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
        if (depthOfField != null)
        {
            // depthOfField.active = true;
            depthOfField.focusDistance.value = blurOnFocusDistance;
            depthOfField.aperture.value = blurOnAperture;
        }
    }
    public void DisableVolumeBlur()
    {
        if (depthOfField != null)
        {
            depthOfField.focusDistance.value = blurOffFocusDistance;
            depthOfField.aperture.value = blurOffAperture;
        }
    }


    public void AddInjury(float amount)
    {
        currentInjury = Mathf.Clamp01(currentInjury + amount);
        UpdatePostProcessingEffects();

        // kill existing tween if it exists
        if (injuryTween != null)
        {
            injuryTween.Kill();
        }
        // launch tween
        // 被揍得越多，回复得越慢
        injuryTween = DOTween.To(() => currentInjury, x =>
        {
            currentInjury = x;
            UpdatePostProcessingEffects();
        }, 0f, injuryDecayTime * currentInjury * currentInjury).SetEase(Ease.OutQuad);
    }

    // update post processing effects
    private void UpdatePostProcessingEffects()
    {
        if (vignette != null)
        {
            vignette.color.value = Color.Lerp(normalVignetteColor, injuryVignetteColor, currentInjury);
            vignette.intensity.value = Mathf.Lerp(normalVignetteInt, injuryVignetteInt, currentInjury);
        }
        if (bloom != null)
        {
            bloom.dirtIntensity.value = Mathf.Lerp(normalBloomSweatValue, injuryBloomSweatValue, currentInjury);
        }
        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(normalChromaticAberrationInt, injuryChromaticAberrationInt, currentInjury);
        }
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = Mathf.Lerp(normalLensDistortionValue, injuryLensDistortionValue, currentInjury);
        }
        if (depthOfField != null)
        {
            depthOfField.focalLength.value = Mathf.Lerp(normalFocalLength, injuryFocalLength, currentInjury);
        }
    }


}
