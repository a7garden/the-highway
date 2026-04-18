using UnityEngine;
using Polyperfect.Common;

/// <summary>
/// 캐릭터가 이동할 때 발걸음 소리를 랜덤으로 출력합니다.
/// Footsteps - Essentials/Footsteps_Rock 오디오 파일을 사용합니다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepAudio : MonoBehaviour
{
    [Header("Footstep Settings")]
    [Range(0f, 1f)] public float volume = 0.8f;
    public bool useSpatialAudio = false;

    [Header("Walk")]
    public float walkInterval = 0.5f;
    public AudioClip[] walkClips; // 자동 할당 또는手动

    [Header("Run")]
    public float runInterval = 0.3f;
    public AudioClip[] runClips;

    [Header("Jump / Land")]
    public AudioClip[] jumpStartClips;
    public AudioClip[] jumpLandClips;

    private AudioSource _audioSource;
    private CharacterController _cc;
    private HorrorPlayerController _player;
    private float _stepTimer = 0f;
    private bool _wasGrounded = true;
    private Vector3 _lastPosition;
    private float _distanceTraveled = 0f;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.playOnAwake = false;

        // 런타임 및 에디터에서 Resources 폴더로부터 자동 로드
        LoadClipsFromResources("Footsteps_Rock/Walk", ref walkClips);
        LoadClipsFromResources("Footsteps_Rock/Run", ref runClips);
        LoadClipsFromResources("Footsteps_Rock/Jump", ref jumpStartClips, "Start");
        LoadClipsFromResources("Footsteps_Rock/Jump", ref jumpLandClips, "Land");
    }

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _player = GetComponent<HorrorPlayerController>();
    }

    void Update()
    {
        if (_cc == null) return;

        bool grounded = _cc.isGrounded;

        // 위치 기반 이동 감지 (CharacterController.velocity는 kinematic 이동에서 0임)
        Vector3 currentPos = transform.position;
        float distDelta = Vector3.Distance(currentPos, _lastPosition);
        _distanceTraveled += distDelta;
        _lastPosition = currentPos;

        bool movementAllowed = _player == null || !_player.cameraLocked;
        bool isMoving = distDelta > 0.001f && grounded && movementAllowed;
        bool isRunning = distDelta > 0.015f; // 한 프레임에 0.015 이상 이동 시 런으로 판단

        // 착지 감지
        if (!_wasGrounded && grounded)
        {
            PlayRandomClip(jumpLandClips);
            _stepTimer = 0f;
        }

        if (isMoving)
        {
            _stepTimer += Time.deltaTime;
            float interval = isRunning ? runInterval : walkInterval;

            if (_stepTimer >= interval)
            {
                _stepTimer = 0f;
                PlayRandomClip(isRunning ? runClips : walkClips);
            }
        }
        else
        {
            _stepTimer = 0f;
        }

        _wasGrounded = grounded;
    }

    /// <summary>점프 시작 시 외부에서 호출</summary>
    public void PlayJumpStart()
    {
        PlayRandomClip(jumpStartClips);
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            return;
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        if (useSpatialAudio)
        {
            Common_AudioManager.PlaySound(clip, transform.position);
        }
        else
        {
            _audioSource.PlayOneShot(clip, volume);
        }
    }

    private void LoadClipsFromResources(string path, ref AudioClip[] target, string nameFilter = null)
    {
        if (target != null && target.Length > 0)
            return;

        AudioClip[] allClips = Resources.LoadAll<AudioClip>(path);

        if (allClips == null || allClips.Length == 0) return;

        var clips = new System.Collections.Generic.List<AudioClip>();
        foreach (var clip in allClips)
        {
            if (string.IsNullOrEmpty(nameFilter) || clip.name.Contains(nameFilter))
            {
                clips.Add(clip);
            }
        }

        if (clips.Count > 0)
            target = clips.ToArray();
    }
}