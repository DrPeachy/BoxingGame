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
        punch.Play();
    }

    public void PlayCharge()
    {
        charge.Play();
    }

    public void PlayParry()
    {
        parry.Play();
    }
}
