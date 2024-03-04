using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusicWithDelaySolo : MonoBehaviour
{

    public float delay = 1;

    // Start is called before the first frame update
    void Awake()
    {
        Invoke("PlayMusic", delay);
    }

    void PlayMusic()
    {
        GetComponent<AudioSource>().Play();
    }
}
