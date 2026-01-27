using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class WheelQuickService : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private XRGrabInteractable grab;
    [SerializeField] private Rigidbody rb;

    [Header("Bolt visuals (just meshes)")]
    [SerializeField] private GameObject[] boltVisuals;

    [Header("Hub attach (snap point)")]
    [SerializeField] private Transform hubAttach; // ступица/Attach

    [Header("Start state")]
    [SerializeField] private bool startBolted = true;

    private bool bolted;

    // Запоминаем “свободные” настройки Rigidbody (когда колесо снято)
    private bool cached;
    private bool freeUseGravity;
    private RigidbodyInterpolation freeInterp;
    private CollisionDetectionMode freeCd;
    private RigidbodyConstraints freeConstraints;

    private void Awake()
    {
        if (!grab) grab = GetComponent<XRGrabInteractable>();
        if (!rb) rb = GetComponent<Rigidbody>();

        CacheRbFreeSettings();

        if (grab)
        {
            // Когда колесо взяли — оно точно “откручено” и должно отцепиться от ступицы
            grab.selectEntered.AddListener(_ =>
            {
                bolted = false;
                ApplyState();

                transform.SetParent(null, true);

                if (rb)
                {
                    rb.isKinematic = false;
                    rb.useGravity = freeUseGravity;

                    rb.interpolation = freeInterp;
                    rb.collisionDetectionMode = freeCd;
                    rb.constraints = freeConstraints;

                    rb.WakeUp();
                }
            });
        }
    }

    private void Start()
    {
        bolted = startBolted;
        ApplyState();

        if (bolted && hubAttach != null)
            SnapToHub(hubAttach);
    }

    /// <summary>
    /// Вызывается гайковёртом:
    /// bolted=true  -> открутить (болты пропали, колесо можно взять)
    /// bolted=false -> закрутить (только если колесо уже на ступице)
    /// </summary>
    public bool UseWrenchAllBolts()
    {
        // 1) Если сейчас прикручено — откручиваем всегда
        if (bolted)
        {
            bolted = false;
            ApplyState();
            return true; // было действие
        }

        // 2) Если не прикручено — закручивать можно только когда колесо на ступице
        if (hubAttach == null) return false;
        if (transform.parent != hubAttach) return false;

        bolted = true;
        SnapToHub(hubAttach);
        ApplyState();
        return true; // было действие
    }


    /// <summary>
    /// Вызывается магнитом: ставим колесо на ступицу после отпускания.
    /// </summary>
    public void TrySnapTo(Transform attach)
    {
        if (attach == null) return;
        hubAttach = attach;

        // Если в руке — не снапим
        if (grab != null && grab.isSelected) return;

        SnapToHub(attach);

        // остаёмся “открученными”, чтобы можно было сразу закрутить гайковёртом
        ApplyState();
    }

    private void SnapToHub(Transform attach)
    {
        transform.SetParent(attach, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // ВАЖНО: НЕ выключаем detectCollisions, иначе XR может перестать видеть объект
            rb.isKinematic = true;
            rb.useGravity = false;

            // чтобы не было “отставания”
            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.Sleep();
        }
    }

    private void ApplyState()
    {
        // Болты показать/спрятать
        if (boltVisuals != null)
        {
            foreach (var b in boltVisuals)
                if (b) b.SetActive(bolted);
        }

        // Граб только когда НЕ прикручено
        if (grab) grab.enabled = !bolted;

        if (!rb) return;

        CacheRbFreeSettings();

        if (bolted)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
            rb.useGravity = false;

            rb.interpolation = RigidbodyInterpolation.None;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            rb.Sleep();
        }
        else
        {
            rb.isKinematic = false;
            rb.useGravity = freeUseGravity;

            rb.interpolation = freeInterp;
            rb.collisionDetectionMode = freeCd;
            rb.constraints = freeConstraints;

            rb.WakeUp();
        }
    }

    private void CacheRbFreeSettings()
    {
        if (!rb || cached) return;

        freeUseGravity = rb.useGravity;
        freeInterp = rb.interpolation;
        freeCd = rb.collisionDetectionMode;
        freeConstraints = rb.constraints;

        cached = true;
    }
}
