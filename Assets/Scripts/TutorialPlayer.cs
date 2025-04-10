using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TutorialPlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Awake()
    {
        if(videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        if(videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
        Debug.Log("TutorialPlayer enabled");
        // reset to the beginning
        videoPlayer.frame = 0;
        videoPlayer.Play();
    }


}
