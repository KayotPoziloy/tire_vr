using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LiftObjectsOnGrip : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float moveAmount = 0.2f; 
    [SerializeField] private float moveSpeed = 2f;  

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private XRSimpleInteractable interactable;

    void Start()
    {
        startPosition = transform.position;
        targetPosition = startPosition;

        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
            interactable = gameObject.AddComponent<XRSimpleInteractable>();

        interactable.selectEntered.AddListener(OnGripPressed);
    }

    private void OnGripPressed(SelectEnterEventArgs args)
    {
        var interactor = args.interactorObject.transform;
        Transform controllerParent = interactor.parent;

        
        while (controllerParent != null)
        {
            string name = controllerParent.name;

            if (name.Contains("Right"))
            {
                targetPosition = startPosition + Vector3.up * moveAmount;
                break;
            }
            else if (name.Contains("Left"))
            {
                targetPosition = startPosition + Vector3.down * moveAmount;
                break;
            }

            controllerParent = controllerParent.parent;
        }

        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            isMoving = false;
            startPosition = transform.position; 
        }
    }
}