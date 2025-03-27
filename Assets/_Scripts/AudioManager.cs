using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AudioEffectsPlayer{
    public GameObject audioEffectsPlayer;
    public AudioSource wave;
    public AudioSource punch;
    public AudioSource charge;
    public AudioSource parry;
    public AudioSource punchBlocked;
    public AudioSource getHit;

    public AudioEffectsPlayer(GameObject audioEffectsPlayer){
        this.audioEffectsPlayer = audioEffectsPlayer;
        wave = audioEffectsPlayer.transform.Find("Wave").GetComponent<AudioSource>();
        punch = audioEffectsPlayer.transform.Find("Punch").GetComponent<AudioSource>();
        charge = audioEffectsPlayer.transform.Find("Charge").GetComponent<AudioSource>();
        parry = audioEffectsPlayer.transform.Find("Parry").GetComponent<AudioSource>();
        punchBlocked = audioEffectsPlayer.transform.Find("PunchBlocked").GetComponent<AudioSource>();
        getHit = audioEffectsPlayer.transform.Find("GetHit").GetComponent<AudioSource>();
    }
}


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource general;
    // [SerializeField] private AudioSource wave;
    // [SerializeField] private AudioSource punch;
    // [SerializeField] private AudioSource charge;
    // [SerializeField] private AudioSource parry;
    // [SerializeField] private AudioSource punchBlocked;
    // [SerializeField] private AudioSource getHit;

    public Dictionary<int, AudioEffectsPlayer> audioEffectsPlayers = new Dictionary<int, AudioEffectsPlayer>();


    [Header("Audio Clips")]
    public AudioClip startEndClip;
    public List<AudioClip> punchClipsList;
    public List<AudioClip> punchBlockedClipsList;
    public AudioClip chargingClip;
    public AudioClip chargeCompleteClip;
    public AudioClip questionBoardSelectClip;
    public List<AudioClip> getHitClipsList; // light to heavy
    public AudioClip generalButtonClickedClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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


    public void PlayWave(int playerID)
    {
        audioEffectsPlayers[playerID].wave.Play();
    }

    public void PlayPunch(int playerID)
    {
        audioEffectsPlayers[playerID].punch.clip = punchClipsList[Random.Range(0, punchClipsList.Count)];
        audioEffectsPlayers[playerID].punch.Play();
    }

    public void PlayPunchBlocked(int playerID)
    {
        audioEffectsPlayers[playerID].punchBlocked.clip = punchBlockedClipsList[Random.Range(0, punchBlockedClipsList.Count)];
        audioEffectsPlayers[playerID].punchBlocked.Play();
    }

    public void PlayCharge(int playerID)
    {
        audioEffectsPlayers[playerID].charge.clip = chargingClip;
        audioEffectsPlayers[playerID].charge.volume = 0.8f;
        audioEffectsPlayers[playerID].charge.Play();
    }

    public void PlayChargeComplete(int playerID)
    {
        // stop charging sound
        audioEffectsPlayers[playerID].charge.Stop();
        audioEffectsPlayers[playerID].charge.clip = chargeCompleteClip;
        audioEffectsPlayers[playerID].charge.volume = 0.3f;
        audioEffectsPlayers[playerID].charge.Play();
    }

    public void StopCharge(int playerID)
    {
        audioEffectsPlayers[playerID].charge.Stop();
    }

    public void PlayParry(int playerID)
    {
        audioEffectsPlayers[playerID].parry.Play();
    }

    public void PlayGetHit(int playerID, int hitType = 0)
    {
        audioEffectsPlayers[playerID].getHit.clip = getHitClipsList[hitType];
        audioEffectsPlayers[playerID].getHit.Play();
    }

    public void PlayGeneralButtonClicked()
    {
        PlayGeneral(generalButtonClickedClip);
    }
}
