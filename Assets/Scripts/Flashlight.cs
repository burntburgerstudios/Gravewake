using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flashlight : MonoBehaviour
{
    public Transform sprite;
    public float range = 10f;
    public float innerAngle = 25f;
    public float outerAngle = 60f;
    public float intensity = 1.2f;
    public Color color = new Color(1f, 0.93f, 0.75f);
    public float flicker = 0.05f;
    public float sway = 8f; 

    Light2D beam;

    void Awake()
    {
        GameObject go = new GameObject("Flashlight");
        go.transform.SetParent(transform, false);
        go.transform.SetPositionAndRotation(sprite.position, sprite.rotation);

        beam = go.AddComponent<Light2D>();
        beam.lightType = Light2D.LightType.Point;
        beam.pointLightInnerRadius = 0f;
        beam.pointLightOuterRadius = range;
        beam.pointLightInnerAngle = innerAngle;
        beam.pointLightOuterAngle = outerAngle;
        beam.falloffIntensity = 0.6f;
        beam.color = color;
        beam.intensity = intensity;
        beam.shadowsEnabled = true;
        beam.shadowIntensity = 0.9f;
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * 15f, 0f) - 0.5f;
        beam.intensity = intensity + noise * flicker * 2f;
    }

    void LateUpdate()
    {
        beam.transform.position = sprite.position;
        beam.transform.rotation = Quaternion.Slerp(beam.transform.rotation, sprite.rotation, sway * Time.deltaTime);
    }
}