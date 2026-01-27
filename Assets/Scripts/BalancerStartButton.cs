using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BalancerStartButton : MonoBehaviour
{
    [Header("Target balancer logic")]
    [SerializeField] private BalancerDock balancer;

    [Header("Button interactable (XRSimpleInteractable / XRBaseInteractable / etc.)")]
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    [Header("Which event to use")]
    [SerializeField] private bool useActivatedEvent = true; // как "Activate"
    [SerializeField] private bool fallbackToSelectEntered = true; // если Activated не срабатывает

    private void Reset()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
    }

    private void Awake()
    {
        if (!interactable) interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
        if (!balancer) balancer = GetComponentInParent<BalancerDock>();
    }

    private void OnEnable()
    {
        if (!interactable) return;

        if (useActivatedEvent)
            interactable.activated.AddListener(OnActivated);

        if (fallbackToSelectEntered)
            interactable.selectEntered.AddListener(OnSelectEntered);
    }

    private void OnDisable()
    {
        if (!interactable) return;

        if (useActivatedEvent)
            interactable.activated.RemoveListener(OnActivated);

        if (fallbackToSelectEntered)
            interactable.selectEntered.RemoveListener(OnSelectEntered);
    }

    private void OnActivated(ActivateEventArgs _)
    {
        TryStart();
    }

    private void OnSelectEntered(SelectEnterEventArgs _)
    {
        // На некоторых кнопках "нажатие" = selectEntered
        TryStart();
    }

    private void TryStart()
    {
        if (!balancer) return;

        // можно добавить защиту, если надо:
        // if (!balancer.HasWheel()) return;

        balancer.StartBalance();
    }
}
