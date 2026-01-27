using UnityEngine;

public class GaugeNeedleSway : MonoBehaviour
{
    [Header("Needle rotation (local Z by default)")]
    [SerializeField] private Vector3 localAxis = new Vector3(0, 0, 1);

    [Header("Angles (degrees)")]
    [SerializeField] private float centerAngle = 0f;
    [SerializeField] private float amplitude = 60f;   // влево/вправо от центра

    [Header("Motion")]
    [SerializeField] private float swayHz = 2.0f;     // частота качания

    public void SetCenter()
    {
        SetAngle(centerAngle);
    }

    public void SetSway(float t01)
    {
        // t01 0..1 -> синусное качание
        float angle = centerAngle + Mathf.Sin(t01 * Mathf.PI * 2f) * amplitude;
        SetAngle(angle);
    }

    public void SetAngle(float angle)
    {
        // вращаем вокруг localAxis
        var q = Quaternion.AngleAxis(angle, localAxis.normalized);
        transform.localRotation = q;
    }

    public float CenterAngle => centerAngle;
}
