using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class VulkanizerDock : MonoBehaviour
{
    [Header("Socket (WheelDock)")]
    [SerializeField] private XRSocketInteractor wheelSocket; // XR Socket Interactor на WheelDock

    [Header("Audio (plays exactly at tyre disappearance)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip tyrePopClip;
    [Range(0f, 1f)] [SerializeField] private float volume = 1f;


    private bool busy;

    public bool IsBusy => busy;

    public bool HasWheelInSocket()
    {
        return GetDockedWheel() != null;
    }

    // Вызывай это из рычага/кнопки
    public void StartSwap()
    {
        if (busy) return;

        var wheel = GetDockedWheel();
        if (wheel == null) return;

        StartCoroutine(SwapRoutine(wheel));
    }

    private WheelQuickService GetDockedWheel()
    {
        if (wheelSocket == null) return null;

        var selected = wheelSocket.GetOldestInteractableSelected();
        if (selected == null) return null;

        return selected.transform.GetComponentInParent<WheelQuickService>();
    }

    private IEnumerator SwapRoutine(WheelQuickService wheel)
    {
        busy = true;

        if (!audioSource) audioSource = GetComponent<AudioSource>();

        XRGrabInteractable wheelGrab = wheel.GetComponent<XRGrabInteractable>();


        // На колесе должен быть TyreVisualPulse (который "прячет/показывает" ТУ ЖЕ шину)
        var pulse = wheel.GetComponentInChildren<TyreVisualPulse>(true);

        if (pulse != null)
        {
            // В момент исчезновения шины - звук
            yield return pulse.DoTyrePulse(() =>
            {
                if (audioSource && tyrePopClip)
                    audioSource.PlayOneShot(tyrePopClip, volume);
            });
        }
        else
        {
            // если pulse нет — просто звук и пауза
            if (audioSource && tyrePopClip)
                audioSource.PlayOneShot(tyrePopClip, volume);

            yield return new WaitForSeconds(1.0f);
        }


        busy = false;
    }
}
