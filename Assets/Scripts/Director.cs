using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Director : MonoBehaviour
{
    public static Director Instance { get; private set; }

    private Image _overlay;
    private AudioSource _sfxSource;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.spatialBlend = 0f;
        _sfxSource.playOnAwake = false;
    }

    void BuildOverlay()
    {
        // Reuse existing FadeCanvas child if present
        Transform existing = transform.Find("FadeCanvas");
        if (existing != null) { _overlay = existing.GetComponentInChildren<Image>(); return; }

        var canvasGO = new GameObject("FadeCanvas");
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        var cr = canvasGO.AddComponent<CanvasScaler>();
        cr.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        var gr = canvasGO.AddComponent<GraphicRaycaster>();
        gr.enabled = false; // ignore raycasts

        var imgGO = new GameObject("Overlay");
        imgGO.transform.SetParent(canvasGO.transform, false);

        var rect = imgGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        _overlay = imgGO.AddComponent<Image>();
        _overlay.color = new Color(0f, 0f, 0f, 0f); // start fully transparent
        _overlay.raycastTarget = false;
    }

    // Fades overlay TO given color at alpha=1
    public IEnumerator FadeOut(float duration, Color? color = null)
    {
        Color target = color ?? Color.black;
        target.a = 1f;
        Color start = _overlay.color;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            _overlay.color = Color.Lerp(start, target, t / duration);
            yield return null;
        }
        _overlay.color = target;
    }

    // Fades overlay alpha back to 0
    public IEnumerator FadeIn(float duration)
    {
        Color start = _overlay.color;
        Color target = new Color(start.r, start.g, start.b, 0f);
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            _overlay.color = Color.Lerp(start, target, t / duration);
            yield return null;
        }
        _overlay.color = target;
    }

    // Snap to opaque color, hold, then fade out
    public IEnumerator Flash(Color color, float holdSeconds = 0.05f, float fadeOutSeconds = 0.35f)
    {
        color.a = 1f;
        _overlay.color = color;
        yield return new WaitForSecondsRealtime(holdSeconds);
        yield return FadeIn(fadeOutSeconds);
    }

    // Instant black → hold → fade back
    public IEnumerator HardCut(float blackHold = 0.6f)
    {
        _overlay.color = Color.black;
        yield return new WaitForSecondsRealtime(blackHold);
        yield return FadeIn(0.4f);
    }

    public void SetOverlayColor(Color c, float alpha)
    {
        if (_overlay == null) return;
        _overlay.color = new Color(c.r, c.g, c.b, alpha);
    }

    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        _sfxSource.PlayOneShot(clip, volume);
    }

    public IEnumerator CrossfadeBGM(AudioClip next, float duration = 2f)
    {
        if (GameManager.Instance == null || GameManager.Instance.bgmSource == null) yield break;
        var src = GameManager.Instance.bgmSource;
        float startVol = src.volume;
        float t = 0f;
        while (t < duration * 0.5f)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / (duration * 0.5f));
            yield return null;
        }
        src.clip = next;
        src.Play();
        t = 0f;
        while (t < duration * 0.5f)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(0f, startVol, t / (duration * 0.5f));
            yield return null;
        }
        src.volume = startVol;
    }
}
