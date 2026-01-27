using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HubMagnetTrigger : MonoBehaviour
{
    [SerializeField] private Transform hubAttach; // точка Attach на ступице
    [SerializeField] private float snapDistance = 0.18f; // чтобы не притягивало издалека
    [SerializeField] private float snapAngle = 25f;      // и не цепляло "в бок"

    private WheelQuickService candidate;

    private void OnTriggerStay(Collider other)
    {
        var wheel = other.GetComponentInParent<WheelQuickService>();
        if (!wheel) return;

        // колесо уже на месте
        if (wheel.transform.parent == hubAttach) return;

        // запоминаем кандидата (снап сделаем после отпускания)
        candidate = wheel;
    }

    private void OnTriggerExit(Collider other)
    {
        var wheel = other.GetComponentInParent<WheelQuickService>();
        if (!wheel) return;

        if (candidate == wheel)
            candidate = null;
    }

    private void Update()
    {
        if (candidate == null) return;
        if (hubAttach == null) return;

        // если колесо всё ещё в руке — ждём отпускания
        var grab = candidate.GetComponent<XRGrabInteractable>();
        if (grab != null && grab.isSelected) return;

        // доп. условия: близко и примерно правильно повернуто
        float dist = Vector3.Distance(candidate.transform.position, hubAttach.position);
        if (dist > snapDistance) return;

        float ang = Quaternion.Angle(candidate.transform.rotation, hubAttach.rotation);
        if (ang > snapAngle) return;

        // снап
        candidate.TrySnapTo(hubAttach);

        // чтобы не спамило — очищаем
        candidate = null;
    }
}
