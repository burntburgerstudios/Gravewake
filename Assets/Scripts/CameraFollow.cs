using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Camera cam;
    public float smoothTime = 0.25f; 

    Vector3 basePos;
    Vector3 velocity;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        basePos = cam.transform.position;
    }

    void LateUpdate()
    {
        Vector3 target = new Vector3(transform.position.x, transform.position.y, basePos.z);
        basePos = Vector3.SmoothDamp(basePos, target, ref velocity, smoothTime);
        cam.transform.position = basePos + CameraShake.Offset();
    }
}