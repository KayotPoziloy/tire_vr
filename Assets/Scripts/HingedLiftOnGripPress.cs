using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HingedLiftOnGripPress : MonoBehaviour
{
    [Header("Настройки вращения")]
    [SerializeField] private float maxAngle = 90f; 
    [SerializeField] private float minAngle = 0f;  
    [SerializeField] private float rotationSpeed = 50f;

    [Header("Ось вращения (мировая)")]
    [SerializeField] private Vector3 rotationAxis = Vector3.right;

    private float currentAngle = 0f; 
    private float targetAngle = 0f;
    private bool isMoving = false;

    private XRSimpleInteractable interactable;

    void Start()
    {
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
                targetAngle = maxAngle; 
                break;
            }
            else if (name.Contains("Left"))
            {
                targetAngle = minAngle; 
                break;
            }

            controllerParent = controllerParent.parent;
        }

        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;
        
        float deltaAngle = rotationSpeed * Time.deltaTime;
        float newAngle = Mathf.MoveTowards(currentAngle, targetAngle, deltaAngle);
        float angleStep = newAngle - currentAngle;

        transform.Rotate(rotationAxis, -angleStep, Space.World);

        currentAngle = newAngle;

        if (Mathf.Approximately(currentAngle, targetAngle))
            isMoving = false;
    }
}
