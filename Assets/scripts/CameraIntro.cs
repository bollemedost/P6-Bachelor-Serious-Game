using UnityEngine;
using Cinemachine;
using System.Collections;

public class CameraIntro : MonoBehaviour
{
    public CinemachineVirtualCamera cinematicCam;
    public CinemachineVirtualCamera gameplayCam;
    public float introDuration = 2f;

    void Start()
    {
        StartCoroutine(SwitchCamera());
    }

    IEnumerator SwitchCamera()
    {
        yield return new WaitForSeconds(introDuration);

        cinematicCam.Priority = 5;
        gameplayCam.Priority = 20;
    }
}