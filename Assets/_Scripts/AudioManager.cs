using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource wave;
    [SerializeField] private AudioSource punch;
    [SerializeField] private AudioSource charge;
    [SerializeField] private AudioSource parry;
    [SerializeField] private AudioSource punchBlocked;


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
