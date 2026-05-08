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

    void Start()
    {
        SnapInstant();
    }

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

    public void SnapInstant()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = desiredPos;

        Vector3 lookTarget = target.position + lookAtOffset;
        Vector3 dir = lookTarget - transform.position;

        if (dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}