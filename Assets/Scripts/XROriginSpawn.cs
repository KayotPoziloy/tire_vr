using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class XROriginSpawn : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool matchYaw = true;

    private IEnumerator Start()
    {
        if (!xrOrigin) xrOrigin = FindFirstObjectByType<XROrigin>();
        if (!xrOrigin || !spawnPoint) yield break;

        // Даем OpenXR один кадр (иногда два) чтобы он выставил tracking origin
        yield return null;
        yield return null;

        // Ставим КАМЕРУ в точку спавна (это важно, не сам объект XR Origin "в лоб")
        xrOrigin.MoveCameraToWorldLocation(spawnPoint.position);

        if (matchYaw)
        {
            var flatForward = Vector3.ProjectOnPlane(spawnPoint.forward, Vector3.up).normalized;
            if (flatForward.sqrMagnitude > 0.001f)
                xrOrigin.MatchOriginUpCameraForward(Vector3.up, flatForward);
        }
    }
}
