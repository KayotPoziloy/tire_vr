using UnityEngine;

public class WrenchProximityAuto : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WheelQuickService wheel;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;   // на гайковёрте
    [SerializeField] private AudioClip unscrewClip;     // звук откручивания
    [SerializeField] private float volume = 1f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 0.8f;

    private float nextTime;

    private void Awake()
    {
        if (!audioSource) audioSource = GetComponentInParent<AudioSource>();
    }

    private void Update()
    {
        if (Time.time < nextTime) return;
        if (wheel == null) return;

        // твоя логика “открутить болты”
        bool didSomething = wheel.UseWrenchAllBolts(); // <-- сделай чтобы метод вернул bool

        if (didSomething && audioSource && unscrewClip)
            audioSource.PlayOneShot(unscrewClip, volume);

        if (didSomething)
            nextTime = Time.time + cooldown;
    }

    private void OnTriggerEnter(Collider other)
    {
        wheel = other.GetComponentInParent<WheelQuickService>();
    }

    private void OnTriggerExit(Collider other)
    {
        var w = other.GetComponentInParent<WheelQuickService>();
        if (w == wheel) wheel = null;
    }
}
