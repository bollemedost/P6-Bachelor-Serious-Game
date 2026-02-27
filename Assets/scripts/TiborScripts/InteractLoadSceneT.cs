using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractLoadSceneT : MonoBehaviour
{
    [Header("Interaction")]
    [Tooltip("Scene name exactly as in Build Settings (case sensitive).")]
    public string sceneToLoad = "SubwayT";

    [Tooltip("Player tag to detect.")]
    public string playerTag = "Player";

    [Tooltip("How close you need to be (works if you don't want a trigger).")]
    public float maxDistance = 2.0f;

    [Tooltip("Press key to interact.")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Optional")]
    [Tooltip("Assign the player transform here, or it will auto-find by tag.")]
    public Transform player;

    private bool playerInTrigger = false;
    private bool isLoading = false;

    private void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) player = p.transform;
        }
    }

    private void Update()
    {
        if (isLoading) return;
        if (player == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        bool closeEnough = dist <= maxDistance;

        if ((playerInTrigger || closeEnough) && Input.GetKeyDown(interactKey))
        {
            isLoading = true;

            // ✅ SAVE where we came from + exact player position BEFORE loading minigame
            ReturnToPreviousSceneT.SaveReturnPoint(player);

            // Load minigame scene
            if (SceneTransition.Instance != null)
            {
                SceneTransition.Instance.FadeToScene(sceneToLoad);
            }
            else
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInTrigger = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
            playerInTrigger = false;
    }
}