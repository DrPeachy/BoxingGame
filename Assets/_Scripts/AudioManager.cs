using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource general;
    [SerializeField] private AudioSource wave;
    [SerializeField] private AudioSource punch;
    [SerializeField] private AudioSource charge;
    [SerializeField] private AudioSource parry;
    [SerializeField] private AudioSource punchBlocked;


    [Header("Audio Clips")]
    public AudioClip startEndClip;
    public List<AudioClip> punchClipsList;
    public List<AudioClip> punchBlockedClipsList;
    public AudioClip chargingClip;
    public AudioClip chargeCompleteClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void PlayGeneral(AudioClip clip, float volume = 1f)
    {
        general.volume = volume;
        general.clip = clip;
        general.Play();
    }

    public void PlayStartEnd()
    {
        PlayGeneral(startEndClip, 0.2f);
    }


    public void PlayWave()
    {
        wave.Play();
    }

    public void PlayPunch()
    {
        punch.clip = punchClipsList[Random.Range(0, punchClipsList.Count)];
        punch.Play();
    }

    public void PlayPunchBlocked()
    {
        punchBlocked.clip = punchBlockedClipsList[Random.Range(0, punchBlockedClipsList.Count)];
        punchBlocked.Play();
    }

    public void PlayCharge()
    {
        charge.clip = chargingClip;
        charge.volume = 0.8f;
        charge.Play();
    }

    public void PlayChargeComplete()
    {
        // stop charging sound
        charge.Stop();
        charge.clip = chargeCompleteClip;
        charge.volume = 0.3f;
        charge.Play();
    }

    public void StopCharge()
    {
        charge.Stop();
    }

    public void PlayParry()
    {
        parry.Play();
    }
}
