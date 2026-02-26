using UnityEngine;

public class Coin : Interactable
{
    [Header("Coin Settings")]
    public int coinValue = 1;

    [Header("Pickup Animation")]
    public PlayerTalkingAnimations.BodyAnimationType animationToPlay;

    [Tooltip("How long the pickup animation should play (seconds)")]
    public float animationDuration = 0.5f;

    [Header("Effects")]
    public AudioSource audioSource;
    public ParticleSystem collectParticles;

    private bool isCollected = false;

    public override void Interact()
    {
        if (isCollected) return;
        isCollected = true;

        // Add coin to manager
        if (CoinManager.Instance != null)
            CoinManager.Instance.AddCoin(coinValue);

        // Play pickup animation once
        PlayPickupAnimationOnce();

        // Play sound
        if (audioSource != null)
            audioSource.Play();

        // Play particles
        if (collectParticles != null)
        {
            collectParticles.transform.parent = null;
            collectParticles.Play();
            Destroy(collectParticles.gameObject, 2f);
        }

        // Hide canvas immediately
        if (canvas != null)
            canvas.SetActive(false);

        // Destroy coin after sound finishes (or immediately if no sound)
        float delay = (audioSource != null && audioSource.clip != null)
            ? audioSource.clip.length
            : 0f;

        Destroy(gameObject, delay);
    }

    private void PlayPickupAnimationOnce()
    {
        PlayerTalkingAnimations playerAnim = FindObjectOfType<PlayerTalkingAnimations>();
        if (playerAnim == null) return;

        // Stop any currently playing sequence
        playerAnim.StopSequence();

        // Play pickup animation
        playerAnim.PlayBodyAnimation(animationToPlay);

        // Return to Idle after custom duration
        playerAnim.StartCoroutine(ReturnToIdleAfterDelay(playerAnim, animationDuration));
    }

    private System.Collections.IEnumerator ReturnToIdleAfterDelay(
        PlayerTalkingAnimations playerAnim, float delay)
    {
        yield return new WaitForSeconds(delay);
        playerAnim.PlayBodyAnimation(PlayerTalkingAnimations.BodyAnimationType.Idle);
    }

    protected override bool IsCurrentlyInteracting()
    {
        return false;
    }
}