using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CompressorController : MonoBehaviour
{
    [Header("Socket (WheelDock)")]
    [SerializeField] private XRSocketInteractor wheelSocket; // XR Socket Interactor на WheelDock

    [Header("Needle (arrow)")]
    [SerializeField] private Transform needle;     // стрелка
    [SerializeField] private float leftZ = 180f;   // положение "влево" (подстрой под модель)
    [SerializeField] private float rightZ = 90f;   // положение "вправо" (подстрой)
    [SerializeField] private float needleMoveTime = 1.2f;

    [Header("Button")]
    [SerializeField] private XRSimpleInteractable pumpButton; // XRSimpleInteractable на кнопке

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // AudioSource на компрессоре
    [SerializeField] private AudioClip pumpClip;       // звук накачки
    [SerializeField] private float pumpTimeOverride = 0f; // 0 = длина клипа, иначе фикс

    [Header("Behavior")]
    [SerializeField] private bool lockWheelWhilePumping = true; // пока качаем - запретить граб колеса

    private bool pumping;

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        if (pumpButton != null)
            pumpButton.selectEntered.AddListener(OnPumpPressed);

        if (wheelSocket != null)
        {
            wheelSocket.selectEntered.AddListener(OnWheelDocked);
            wheelSocket.selectExited.AddListener(OnWheelUndocked);
        }
    }

    private void OnDisable()
    {
        if (pumpButton != null)
            pumpButton.selectEntered.RemoveListener(OnPumpPressed);

        if (wheelSocket != null)
        {
            wheelSocket.selectEntered.RemoveListener(OnWheelDocked);
            wheelSocket.selectExited.RemoveListener(OnWheelUndocked);
        }
    }

    private void Start()
    {
        SetNeedleLeft();
    }

    // Срабатывает когда колесо встало в сокет
    private void OnWheelDocked(SelectEnterEventArgs args)
    {
        SetNeedleLeft();
    }

    // Срабатывает когда колесо вынули из сокета
    private void OnWheelUndocked(SelectExitEventArgs args)
    {
        if (!pumping) SetNeedleLeft();
    }

    private void OnPumpPressed(SelectEnterEventArgs args)
    {
        TryStartPumping();
    }

    public void TryStartPumping()
    {
        if (pumping) return;

        var wheel = GetDockedWheel();
        if (wheel == null) return; // нет колеса в сокете

        StartCoroutine(PumpRoutine(wheel));
    }

    private WheelQuickService GetDockedWheel()
    {
        if (wheelSocket == null) return null;

        // В сокете может быть выбранный интерактибл (колесо)
        var selected = wheelSocket.GetOldestInteractableSelected();
        if (selected == null) return null;

        return selected.transform.GetComponentInParent<WheelQuickService>();
    }

    private IEnumerator PumpRoutine(WheelQuickService wheel)
    {
        pumping = true;

        XRGrabInteractable wheelGrab = null;
        if (wheel != null) wheelGrab = wheel.GetComponent<XRGrabInteractable>();

        

        float duration = pumpTimeOverride > 0f
            ? pumpTimeOverride
            : (pumpClip != null ? pumpClip.length : needleMoveTime);

        duration = Mathf.Max(0.05f, duration);

        // Старт звук
        if (audioSource != null && pumpClip != null)
            audioSource.PlayOneShot(pumpClip, 1f);

        // Двигаем стрелку: left -> right за duration
        Quaternion from = GetNeedleRotZ(leftZ);
        Quaternion to = GetNeedleRotZ(rightZ);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.Clamp01(t);

            if (needle != null)
                needle.localRotation = Quaternion.Slerp(from, to, k);

            yield return null;
        }

        
        pumping = false;
    }

    private void SetNeedleLeft()
    {
        if (needle == null) return;
        needle.localRotation = GetNeedleRotZ(leftZ);
    }

    private Quaternion GetNeedleRotZ(float z)
    {
        if (needle == null) return Quaternion.identity;
        var e = needle.localEulerAngles;
        return Quaternion.Euler(e.x, e.y, z);
    }
}
