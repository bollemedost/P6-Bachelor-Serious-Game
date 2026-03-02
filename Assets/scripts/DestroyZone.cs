using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Woman"))
        {
            Destroy(other.gameObject);
        }
    }
}