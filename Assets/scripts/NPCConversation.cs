using UnityEngine;
using System.Collections;

public class NPCConversation : MonoBehaviour
{
    public Animator animator;
    public string[] talkAnimations;

    public float minTalkTime = 4f;
    public float maxTalkTime = 8f;

    public Transform player;

    public float hearingDistance = 15f;
    public float fadeSpeed = 2f;

    AudioSource audioSource;
    float targetVolume;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;

        audioSource.pitch = Random.Range(0.9f, 1.1f);

        StartCoroutine(TalkLoop());
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        // Decide what the target volume should be
        if (distance <= hearingDistance)
        {
            targetVolume = 1f;

            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else
        {
            targetVolume = 0f;
        }

        // Smoothly move volume toward the target
        audioSource.volume = Mathf.MoveTowards(
            audioSource.volume,
            targetVolume,
            fadeSpeed * Time.deltaTime
        );

        // Stop audio once fully faded out
        if (audioSource.volume == 0 && targetVolume == 0 && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    IEnumerator TalkLoop()
    {
        while(true)
        {
            int rand = Random.Range(0, talkAnimations.Length);
            animator.Play(talkAnimations[rand]);

            float waitTime = Random.Range(minTalkTime, maxTalkTime);
            yield return new WaitForSeconds(waitTime);
        }
    }
}