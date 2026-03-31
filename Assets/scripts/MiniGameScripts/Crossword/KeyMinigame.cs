using UnityEngine;
using System.Collections.Generic;

public class KeyMinigame : MonoBehaviour
{
    [Header("Key Audio")]
    public List<KeyAudio> keyAudios;

    private AudioSource audioSource;

    [System.Serializable]
    public class KeyAudio
    {
        public KeyCode mainKey;
        public KeyCode altKey;
        public AudioClip clip;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    private void Update()
    {
        foreach (var ka in keyAudios)
        {
            if ((Input.GetKeyDown(ka.mainKey) || Input.GetKeyDown(ka.altKey)) && ka.clip != null)
            {
                audioSource.Stop();          // stop current sound
                audioSource.clip = ka.clip;  // set new clip
                audioSource.Play();          // play new sound
            }
        }
    }
}