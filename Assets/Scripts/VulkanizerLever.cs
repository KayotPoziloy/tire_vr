using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VulkanizerLever : MonoBehaviour
{
    [SerializeField] private VulkanizerDock dock;
    [SerializeField] private XRSimpleInteractable interactable;

    private void Awake()
    {
        if (!interactable) interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
            interactable.selectEntered.AddListener(_ => Pull());
    }

    private void Pull()
    {
        if (dock == null) return;
        dock.StartSwap();
    }
}
