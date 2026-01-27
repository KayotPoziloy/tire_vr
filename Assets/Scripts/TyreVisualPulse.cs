using System.Collections;
using UnityEngine;

public class TyreVisualPulse : MonoBehaviour
{
    [Header("Tyre object (same tyre, no duplicates)")]
    [SerializeField] private Transform tyreRoot;     // Tyre (один и тот же)
    [SerializeField] private float hideTime = 0.25f; // как быстро "исчезает"
    [SerializeField] private float waitHidden = 0.35f; // сколько лежит "без шины"
    [SerializeField] private float showTime = 0.25f; // как быстро "появляется"

    private Vector3 baseScale;
    private bool inited;

    public bool IsBusy { get; private set; }

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (inited) return;
        if (tyreRoot == null) return;

        baseScale = tyreRoot.localScale;
        inited = true;
    }

    public IEnumerator DoTyrePulse(System.Action onDisappearMoment = null)
    {
        Init();
        if (!inited || IsBusy) yield break;

        IsBusy = true;
        
        //  ЗВУК ТУТ
        onDisappearMoment?.Invoke();

        // 1) уменьшаем до почти нуля
        yield return ScaleRoutine(tyreRoot, baseScale, baseScale * 0.001f, hideTime);

        // можно физически скрыть рендереры (чтобы точно не мерцало)
        SetRenderersEnabled(tyreRoot, false);

        // 2) пауза "шины нет"
        yield return new WaitForSeconds(waitHidden);

        // 3) включаем обратно и увеличиваем
        SetRenderersEnabled(tyreRoot, true);
        tyreRoot.localScale = baseScale * 0.001f;

        yield return ScaleRoutine(tyreRoot, tyreRoot.localScale, baseScale, showTime);

        tyreRoot.localScale = baseScale;
        IsBusy = false;
    }

    private static IEnumerator ScaleRoutine(Transform t, Vector3 a, Vector3 b, float seconds)
    {
        if (t == null) yield break;

        float dur = Mathf.Max(seconds, 0.01f);
        float k = 0f;

        while (k < 1f)
        {
            k += Time.deltaTime / dur;
            t.localScale = Vector3.Lerp(a, b, Mathf.Clamp01(k));
            yield return null;
        }
        t.localScale = b;
    }

    private static void SetRenderersEnabled(Transform root, bool enabled)
    {
        if (root == null) return;
        var rends = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends) r.enabled = enabled;
    }
}
