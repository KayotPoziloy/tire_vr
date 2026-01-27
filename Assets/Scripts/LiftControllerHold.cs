using UnityEngine;

public class LiftControllerHold : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform platform; // lift_tracks (двигаем его)
    [SerializeField] private Transform carRoot;  // r34

    [Header("Auto mount (car is already above the arms)")]
    [SerializeField] private bool mountOnStart = true;

    [Header("Limits (relative to start local Y)")]
    [SerializeField] private float downDelta = 0.0f;  // обычно 0 — исходное
    [SerializeField] private float upDelta = 0.8f;    // насколько вверх

    [Header("Speed")]
    [SerializeField] private float moveSpeed = 0.7f;  // м/с

    private float startLocalY;
    private float minY;
    private float maxY;

    private bool upHeld;
    private bool downHeld;

    private Transform mountedCar;
    private Transform carOriginalParent;
    private Rigidbody carRb;

    private void Awake()
    {
        startLocalY = platform.localPosition.y;
        minY = startLocalY + downDelta;
        maxY = startLocalY + upDelta;
    }

    private void Start()
    {
        if (mountOnStart)
            TryMountCar();
    }

    private void Update()
    {
        float dir = 0f;
        if (upHeld) dir += 1f;
        if (downHeld) dir -= 1f;

        if (Mathf.Abs(dir) < 0.01f)
            return;

        TryMountCar();

        var lp = platform.localPosition;
        lp.y = Mathf.Clamp(lp.y + dir * moveSpeed * Time.deltaTime, minY, maxY);
        platform.localPosition = lp;
    }

    public void SetUpHeld(bool held)   => upHeld = held;
    public void SetDownHeld(bool held) => downHeld = held;

    private void TryMountCar()
    {
        if (mountedCar != null) return;
        if (carRoot == null) return;

        mountedCar = carRoot;
        carOriginalParent = carRoot.parent;

        carRb = carRoot.GetComponent<Rigidbody>();
        if (carRb != null)
        {
            carRb.linearVelocity = Vector3.zero;
            carRb.angularVelocity = Vector3.zero;
            carRb.isKinematic = true;
        }

        carRoot.SetParent(platform, true);
    }


    public void UnmountCar()
    {
        if (mountedCar == null) return;

        mountedCar.SetParent(carOriginalParent, true);
        if (carRb != null) carRb.isKinematic = false;

        mountedCar = null;
        carOriginalParent = null;
        carRb = null;
    }

    public float Lift01
    {
        get
        {
            float y = platform.localPosition.y;
            return Mathf.InverseLerp(minY, maxY, y);
        }
    }

    public bool IsLiftUpEnough(float threshold01 = 0.95f)
    {
        return Lift01 >= threshold01;
    }

}
