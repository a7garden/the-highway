using System.Collections;
using UnityEngine;

public class CameraSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float positionAmount = 0.006f;
    public float rotationAmount = 0.15f;
    public float frequency      = 0.6f;

    private Vector3    _basePos;
    private float      _intensity   = 1f;
    private float      _targetIntensity = 1f;
    private float      _lerpSpeed   = 1f;

    private Coroutine  _pulseHandle;
    private HorrorPlayerController _controller;

    // Perlin noise time offsets — different per axis so they don't correlate
    private float _tx, _ty, _tz, _rx, _ry;

    void Start()
    {
        _basePos = transform.localPosition;
        _controller = GetComponentInParent<HorrorPlayerController>();

        // Seed offsets with random starting points so every session looks different
        float s = Random.Range(0f, 1000f);
        _tx = s;
        _ty = s + 31.7f;
        _tz = s + 67.3f;
        _rx = s + 113.1f;
        _ry = s + 179.5f;
    }

    void LateUpdate()
    {
        _intensity = Mathf.Lerp(_intensity, _targetIntensity, Time.deltaTime * _lerpSpeed);

        float t = Time.time * frequency;

        Vector3 posOff = new Vector3(
            Mathf.PerlinNoise(_tx + t, 0f) * 2f - 1f,
            Mathf.PerlinNoise(_ty + t, 1f) * 2f - 1f,
            Mathf.PerlinNoise(_tz + t, 2f) * 2f - 1f
        );

        float i = _intensity;
        transform.localPosition = _basePos + posOff * (positionAmount * i);

        // Rotation sway: only while controller is live + free-look. During cutscenes/cameraLocked,
        // coroutines drive localRotation directly — avoid stomping or cumulative drift.
        bool controllerLive = _controller != null && _controller.enabled && !_controller.cameraLocked;
        if (!controllerLive) return;

        Vector3 rotOff = new Vector3(
            Mathf.PerlinNoise(_rx + t, 3f) * 2f - 1f,
            0f,
            Mathf.PerlinNoise(_ry + t, 4f) * 2f - 1f
        );

        Quaternion baseRot = Quaternion.Euler(_controller.VerticalRotation, 0f, 0f);
        transform.localRotation = baseRot * Quaternion.Euler(rotOff * (rotationAmount * i));
    }

    public void SetIntensity(float multiplier, float lerpSeconds = 1f)
    {
        _targetIntensity = multiplier;
        _lerpSpeed = lerpSeconds > 0f ? 1f / lerpSeconds : float.MaxValue;
        if (lerpSeconds <= 0f) _intensity = multiplier;
    }

    // One-shot intensity envelope: rise to peak, sustain, fall back to 1
    public void Pulse(float peakMultiplier, float attackSeconds, float sustainSeconds, float releaseSeconds)
    {
        if (_pulseHandle != null) StopCoroutine(_pulseHandle);
        _pulseHandle = StartCoroutine(PulseRoutine(peakMultiplier, attackSeconds, sustainSeconds, releaseSeconds));
    }

    IEnumerator PulseRoutine(float peak, float attack, float sustain, float release)
    {
        float baseIntensity = _intensity;

        // Attack
        float t = 0f;
        while (t < attack)
        {
            t += Time.deltaTime;
            _intensity = Mathf.Lerp(baseIntensity, peak, t / attack);
            _targetIntensity = _intensity;
            yield return null;
        }
        _intensity = peak;
        _targetIntensity = peak;

        // Sustain
        yield return new WaitForSeconds(sustain);

        // Release back to 1
        t = 0f;
        float releaseFrom = _intensity;
        while (t < release)
        {
            t += Time.deltaTime;
            _intensity = Mathf.Lerp(releaseFrom, 1f, t / release);
            _targetIntensity = _intensity;
            yield return null;
        }
        _intensity = 1f;
        _targetIntensity = 1f;
        _pulseHandle = null;
    }
}
