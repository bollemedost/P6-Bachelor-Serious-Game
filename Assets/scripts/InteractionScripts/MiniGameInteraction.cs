using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGameInteraction : MonoBehaviour
{
    [Header("Common Settings")]
    public GameObject canvas;              // optional (generic UI)
    public float interactDistance = 3f;
    public string playerTag = "Player";

    [Header("Event Settings")]
    public GameEvent miniGameEvent;        // the event that will be completed after minigame finished
    public List<GameEvent> prerequisiteEvents = new List<GameEvent>(); // must be completed to allow start

    [Header("UI Canvases")]
    public GameObject lockedCanvas;        // shown if locked
    public GameObject interactCanvas;      // shown when in range and unlocked

    [Header("Minigame Scene")]
    public string miniGameSceneName = "MemoryGame";

    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Debug")]
    public bool debugLogs = false;

    private Transform player;
    private bool isLoading = false;
    private EventManager eventManager;

    private void Start()
    {
        FindPlayer();
        eventManager = FindObjectOfType<EventManager>();

        // Hide UI at start
        if (interactCanvas != null) interactCanvas.SetActive(false);
        if (lockedCanvas != null) lockedCanvas.SetActive(false);
        if (canvas != null) canvas.SetActive(false);
    }

    private void Update()
    {
        if (isLoading) return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        float dist = Vector3.Distance(player.position, transform.position);
        bool inRange = dist <= interactDistance;

        bool unlocked = ArePrerequisitesCompleted();

        // Show the right UI
        if (interactCanvas != null) interactCanvas.SetActive(inRange && unlocked);
        if (lockedCanvas != null) lockedCanvas.SetActive(inRange && !unlocked);

        // Optional generic canvas
        if (canvas != null) canvas.SetActive(inRange);

        if (!inRange) return;

        // Press E to start (only if unlocked)
        if (unlocked && Input.GetKeyDown(interactKey))
        {
            isLoading = true;

            // Save return point BEFORE loading minigame
            ReturnToPreviousSceneT.SaveReturnPoint(player);

            if (debugLogs)
                Debug.Log($"[MiniGameInteraction] Loading '{miniGameSceneName}' for event '{(miniGameEvent ? miniGameEvent.name : "null")}'");

            // Load the minigame scene
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.FadeToScene(miniGameSceneName);
            }
            else
            {
                SceneManager.LoadScene(miniGameSceneName);
            }
        }
    }

    private bool ArePrerequisitesCompleted()
    {
        // If no EventManager exists, assume unlocked (so your game doesn't hard-break)
        if (eventManager == null) return true;

        if (prerequisiteEvents == null || prerequisiteEvents.Count == 0) return true;

        for (int i = 0; i < prerequisiteEvents.Count; i++)
        {
            var req = prerequisiteEvents[i];
            if (req == null) continue;

            if (!eventManager.IsEventCompleted(req))
                return false;
        }
        return true;
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag(playerTag);
        if (p != null) player = p.transform;
    }
}