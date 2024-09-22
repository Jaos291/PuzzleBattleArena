using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;

    private void Start()
    {
        int randomClip = Random.Range(0, clips.Length);

        audioSource.clip = clips[randomClip];
        audioSource.playOnAwake = true;
        audioSource.Play();
    }
}
