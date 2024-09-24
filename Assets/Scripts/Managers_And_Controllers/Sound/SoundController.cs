using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    public List<AudioPair> audioPairs;  // Lista de pares de clips

    private void Start()
    {
        if (audioPairs.Count > 0)
        {
            int randomIndex = Random.Range(0, audioPairs.Count);
            AudioPair randomPair = audioPairs[randomIndex];

            AudioClip normalTheme = randomPair.normalTheme;
            AudioClip hurryTheme = randomPair.hurryTheme;

            audioSource.clip = normalTheme;

            audioSource.Play();

            Debug.Log("Playing: " + audioSource.clip.name);

            // Puedes hacer algo con los clips, como reproducirlos
            // por ejemplo: AudioSource.PlayClipAtPoint(firstClipA, transform.position);
        }
    }

    [System.Serializable]
    public class AudioPair
    {
        public AudioClip normalTheme;  // Primer AudioClip
        public AudioClip hurryTheme;  // Segundo AudioClip
    }
}
