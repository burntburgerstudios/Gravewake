using UnityEngine;

public static class CameraShake
{
    static float power;

    public static void Shake(float amount)
    {
        power = amount;
    }

    public static Vector3 Offset()
    {
        power = Mathf.MoveTowards(power, 0f, Time.deltaTime);
        return (Vector3)Random.insideUnitCircle * power;
    }
}