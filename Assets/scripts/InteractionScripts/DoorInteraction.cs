using UnityEngine;
using System.Collections;

public class DoorInteraction : Interactable
{
    [System.Serializable]
    public class TimedDialogueUI
    {
        public float timeStamp;   
        public GameObject uiObject; 
    }

    [Header("Event Settings")]
    public GameEvent doorEvent;                
    public GameEvent[] prerequisiteEvents;     
    private EventManager eventManager;

    [Header("UI Canvases")]
    public GameObject lockedCanvas;            
    public GameObject interactCanvas;          

    [Header("Scene Transition")]
    public string sceneToLoad;                 

    [Header("Audio")]
    public AudioSource audioSource;            
    public AudioClip doorAudioClip;            
    public float audioDelay = 0f;              

    [Header("Optional Dialogue UI")]
    public GameObject dialogueCanvas;           
    public TimedDialogueUI[] timedUI;

    private bool isUnlocked = false;
    private bool isInteracting = false;
    private bool hasInteracted = false; // New: prevents interact UI from popping back
    private float interactionTimer = 0f;
    private int currentUIIndex = 0;

    protected override void Start()
    {
        base.Start();
        eventManager = Object.FindFirstObjectByType<EventManager>();
        if (eventManager == null)
            Debug.LogError("No EventManager found in scene!");

        if (lockedCanvas != null) lockedCanvas.SetActive(false);
        if (interactCanvas != null) interactCanvas.SetActive(false);

        if (audioSource == null && doorAudioClip != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactDistance && !hasInteracted)
            UpdateCanvasState();
        else
        {
            if (lockedCanvas != null) lockedCanvas.SetActive(false);
            if (interactCanvas != null) interactCanvas.SetActive(false);
        }

        // Timed dialogue UI
        if (isInteracting && timedUI != null && currentUIIndex < timedUI.Length)
        {
            interactionTimer += Time.deltaTime;
            if (interactionTimer >= timedUI[currentUIIndex].timeStamp)
            {
                if (currentUIIndex > 0)
                {
                    var previous = timedUI[currentUIIndex - 1];
                    if (previous.uiObject != null)
                        previous.uiObject.SetActive(false);
                }

                var current = timedUI[currentUIIndex];
                if (current.uiObject != null)
                    current.uiObject.SetActive(true);

                currentUIIndex++;
            }
        }
    }

    private void UpdateCanvasState()
    {
        if (eventManager == null) return;

        isUnlocked = true;
        foreach (var prereq in prerequisiteEvents)
        {
            if (!eventManager.IsEventCompleted(prereq))
            {
                isUnlocked = false;
                break;
            }
        }

        if (isUnlocked)
        {
            if (!isInteracting && !hasInteracted && interactCanvas != null)
                interactCanvas.SetActive(true);

            if (lockedCanvas != null) lockedCanvas.SetActive(false);
        }
        else
        {
            if (lockedCanvas != null) lockedCanvas.SetActive(true);
            if (interactCanvas != null) interactCanvas.SetActive(false);
        }
    }

    public override void Interact()
    {
        if (!isUnlocked || eventManager == null || isInteracting) return;

        hasInteracted = true; // prevent interact UI from coming back
        if (interactCanvas != null) interactCanvas.SetActive(false);

        StartCoroutine(HandleDoorInteraction());
    }

    private IEnumerator HandleDoorInteraction()
    {
        isInteracting = true;

        // Reset dialogue UI
        interactionTimer = 0f;
        currentUIIndex = 0;

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        if (timedUI != null)
        {
            foreach (var entry in timedUI)
            {
                if (entry.uiObject != null)
                    entry.uiObject.SetActive(false);
            }
        }

        if (audioDelay > 0f)
            yield return new WaitForSeconds(audioDelay);

        if (audioSource != null && doorAudioClip != null)
        {
            audioSource.PlayOneShot(doorAudioClip);
            yield return new WaitForSeconds(doorAudioClip.length);
        }

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (doorEvent != null)
            eventManager.CompleteEvent(doorEvent);

        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            if (SceneTransition.Instance != null)
                SceneTransition.Instance.FadeToScene(sceneToLoad);
            else
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }

        isInteracting = false;
    }

    protected override bool IsCurrentlyInteracting()
    {
        return isInteracting;
    }
}