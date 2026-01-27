using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LiftHoldButton : MonoBehaviour
{
    public enum Direction { Up, Down }

    [SerializeField] private LiftControllerHold lift;
    [SerializeField] private Direction direction;

    private XRSimpleInteractable interactable;

    private void Awake()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<XRSimpleInteractable>();

        interactable.selectEntered.AddListener(_ => SetHeld(true));
        interactable.selectExited.AddListener(_ => SetHeld(false));
    }

    private void OnDisable()
    {
        // на всякий: если объект выключился во время удержания
        SetHeld(false);
    }

    private void SetHeld(bool held)
    {
        if (lift == null) return;

        if (direction == Direction.Up) lift.SetUpHeld(held);
        else lift.SetDownHeld(held);
    }
}
