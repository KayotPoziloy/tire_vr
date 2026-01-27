using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BalancerDock : MonoBehaviour
{
    [Header("Wheel socket (XR Socket on WheelDock)")]
    [SerializeField] private XRSocketInteractor wheelSocket;

    [Header("Wheel spin")]
    [SerializeField] private Transform spinPivot;          // пустышка: ось вращения колеса (обычно центр дока)
    [SerializeField] private Vector3 spinAxisLocal = new Vector3(0, 0, 1);
    [SerializeField] private float spinRpm = 240f;         // скорость вращения
    [SerializeField] private float spinDuration = 3.0f;    // сколько "крутим"
    [SerializeField] private float settleNeedleTime = 1.0f;// сколько "успокаиваем стрелку в центр"
    [SerializeField] private float stopTime = 0.6f;        // плавная остановка

    [Header("Lid")]
    [SerializeField] private Transform lid;
    [SerializeField] private Vector3 openLocalEuler;
    [SerializeField] private Vector3 closedLocalEuler;
    [SerializeField] private float lidMoveTime = 0.5f;

    [Header("Gauge needle (reuse from compressor)")]
    [SerializeField] private GaugeNeedleSway needle;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spinLoop;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;

    [Header("Locking while balancing")]
    [SerializeField] private bool disableGrabWhileBusy = true;

    private bool busy;
    private WheelQuickService dockedWheel;
    private XRGrabInteractable dockedGrab;
    private Rigidbody dockedRb;

    private float currentRpm; // для плавной остановки

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (wheelSocket)
        {
            wheelSocket.selectEntered.AddListener(OnWheelDocked);
            wheelSocket.selectExited.AddListener(OnWheelUndocked);
        }

        if (lid) lid.localEulerAngles = openLocalEuler;
        if (needle) needle.SetCenter();
    }

    private void OnDestroy()
    {
        if (wheelSocket)
        {
            wheelSocket.selectEntered.RemoveListener(OnWheelDocked);
            wheelSocket.selectExited.RemoveListener(OnWheelUndocked);
        }
    }

    private void OnWheelDocked(SelectEnterEventArgs args)
    {
        dockedWheel = args.interactableObject.transform.GetComponentInParent<WheelQuickService>();
        if (!dockedWheel) return;

        dockedGrab = dockedWheel.GetComponent<XRGrabInteractable>();
        dockedRb = dockedWheel.GetComponent<Rigidbody>();

        // Как только колесо встало — можно сразу закрыть крышку (или оставить открытой до старта)
        // Тут оставлю открытой, а закроем при запуске балансировки.
    }

    private void OnWheelUndocked(SelectExitEventArgs args)
    {
        if (busy) return; // во время работы не даём снять (на всякий)
        dockedWheel = null;
        dockedGrab = null;
        dockedRb = null;

        if (needle) needle.SetCenter();
        if (lid) lid.localEulerAngles = openLocalEuler;
    }

    public bool HasWheel() => dockedWheel != null;

    // Вызывай этот метод из кнопки/рычага (UnityEvent -> BalancerDock.StartBalance)
    public void StartBalance()
    {
        if (busy) return;
        if (!HasWheel()) return;

        StartCoroutine(BalanceRoutine());
    }

    private IEnumerator BalanceRoutine()
    {
        busy = true;

        // 1) фиксируем колесо (чтобы XR не пытался его "увезти")
        if (disableGrabWhileBusy && dockedGrab) dockedGrab.enabled = false;
        if (dockedRb)
        {
            dockedRb.linearVelocity = Vector3.zero;
            dockedRb.angularVelocity = Vector3.zero;
            dockedRb.isKinematic = true;
        }

        // 2) закрываем крышку
        yield return RotateLid(closedLocalEuler, lidMoveTime);

        // 3) звук
        if (audioSource && spinLoop)
        {
            audioSource.clip = spinLoop;
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.Play();
        }

        // 4) вращаем + стрелка качается
        currentRpm = spinRpm;
        float t = 0f;
        while (t < spinDuration)
        {
            t += Time.deltaTime;

            SpinWheel(currentRpm);

            if (needle)
            {
                // качание
                float phase01 = t * 1.0f; // просто "время", внутри SetSway синус
                needle.SetSway(phase01);
            }

            yield return null;
        }

        // 5) стрелка в центр, колесо ещё крутится
        float s = 0f;
        float startAngle = needle ? needle.transform.localRotation.eulerAngles.z : 0f;

        while (s < settleNeedleTime)
        {
            s += Time.deltaTime;

            SpinWheel(currentRpm);

            if (needle)
            {
                float k = Mathf.Clamp01(s / Mathf.Max(settleNeedleTime, 0.001f));
                // Плавно к центру:
                float target = needle.CenterAngle;
                // Нормализуем углы, чтобы не прыгало
                float a = Mathf.DeltaAngle(0f, startAngle);
                float to = target;
                float angle = Mathf.Lerp(a, to, k);
                needle.SetAngle(angle);
            }

            yield return null;
        }

        // 6) теперь останавливаем колесо (плавно)
        float stop = 0f;
        while (stop < stopTime)
        {
            stop += Time.deltaTime;

            float k = 1f - Mathf.Clamp01(stop / Mathf.Max(stopTime, 0.001f));
            currentRpm = spinRpm * k;

            SpinWheel(currentRpm);

            yield return null;
        }

        // стоп звук
        if (audioSource) audioSource.Stop();

        // 7) открываем крышку
        yield return RotateLid(openLocalEuler, lidMoveTime);

        // 8) снова можно снимать колесо
        if (disableGrabWhileBusy && dockedGrab) dockedGrab.enabled = true;
        if (dockedRb) dockedRb.isKinematic = false;

        busy = false;
    }

    private void SpinWheel(float rpm)
    {
        if (!dockedWheel) return;

        // крутим именно объект колеса (WheelQuickService), но лучше через pivot, если он задан
        Transform tr = dockedWheel.transform;

        if (spinPivot != null)
        {
            // Вращение вокруг pivot, чтобы было красиво "по оси"
            float degPerSec = rpm * 360f / 60f;
            tr.RotateAround(spinPivot.position, spinPivot.TransformDirection(spinAxisLocal.normalized), degPerSec * Time.deltaTime);
        }
        else
        {
            float degPerSec = rpm * 360f / 60f;
            tr.Rotate(tr.TransformDirection(spinAxisLocal.normalized), degPerSec * Time.deltaTime, Space.World);
        }
    }

    private IEnumerator RotateLid(Vector3 targetEuler, float duration)
    {
        if (!lid) yield break;

        Vector3 start = lid.localEulerAngles;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(duration, 0.001f));

            // чтобы не прыгало по 0/360:
            float x = Mathf.LerpAngle(start.x, targetEuler.x, k);
            float y = Mathf.LerpAngle(start.y, targetEuler.y, k);
            float z = Mathf.LerpAngle(start.z, targetEuler.z, k);

            lid.localEulerAngles = new Vector3(x, y, z);
            yield return null;
        }

        lid.localEulerAngles = targetEuler;
    }
}
