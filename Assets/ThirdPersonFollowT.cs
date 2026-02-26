using UnityEngine;

public class ThirdPersonFollow : MonoBehaviour
{
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 5f, -8f);

    [Header("Smoothing")]
    public float followSmooth = 8f;

    [Header("Look At")]
    public Vector3 lookAtOffset = new Vector3(0f, 1.5f, 0f);
    public float rotationSmooth = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Smooth position follow
        Vector3 desiredPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSmooth * Time.deltaTime);

        // Smooth look at player
        Vector3 lookTarget = target.position + lookAtOffset;
        Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationSmooth * Time.deltaTime);
    }
}