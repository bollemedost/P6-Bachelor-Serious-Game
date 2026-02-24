using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Common Settings")]
    public GameObject canvas;          // the pop-up UI
    public float interactDistance = 3f; // distance to show canvas / allow interaction
    public string playerTag = "Player"; // tag of the player

    protected Transform player;

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null) player = playerObj.transform;

        if (canvas != null)
            canvas.SetActive(false); // hide at start
    }

    protected virtual void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Show or hide canvas based on proximity
        if (canvas != null)
            canvas.SetActive(distance <= interactDistance);

        // Check for interaction key
        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    // Abstract method for object-specific behavior
    public abstract void Interact();
}