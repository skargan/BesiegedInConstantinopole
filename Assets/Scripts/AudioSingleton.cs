using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSingleton : MonoBehaviour
{
    static private AudioSingleton _instance;

    static public AudioSingleton Instance
    {
        get => _instance;
        private set => _instance = value;
    }


    // Music

    AudioSource myPlayer;
    [SerializeField] AudioClip backgroundMusic;



    private void Awake()
    {
        _instance = this;
        myPlayer = GetComponent<AudioSource>();
    }

}
