using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class HorrorPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.15f;
    public float verticalClampAngle = 80f;

    [Header("References")]
    public Transform cameraTransform;

    // 카메라 잠금 (개미 구간용)
    [HideInInspector] public bool cameraLocked = false;
    [Header("Ant View Look")]
    [Range(0f, 30f)] public float antLookRange = 10f; // 개미 구간에서 허용할 카메라 움직임 범위(도)

    // 이동 잠금 전용 (Update() 전체를 비활성화하지 않기 위해)
    [HideInInspector] public bool movementLocked = false;

    private CharacterController _cc;
    private Vector3 _velocity;
    private float _verticalRot = 0f;
    private bool _cursorLocked = true;

    public float VerticalRotation => _verticalRot;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        SetCursorLock(true);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse    = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // ESC 커서 토글
        if (keyboard.escapeKey.wasPressedThisFrame)
            SetCursorLock(!_cursorLocked);

        // 마우스 시점
        if (_cursorLocked)
        {
            Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;
            transform.Rotate(Vector3.up * delta.x);
            _verticalRot -= delta.y;

            if (cameraLocked)
            {
                // 개미 구간: 기본 90도에서 antLookRange 내에서만 카메라 움직임 허용
                float basePitch = 90f;
                _verticalRot = Mathf.Clamp(_verticalRot, basePitch - antLookRange, basePitch + antLookRange);
                if (cameraTransform != null)
                    cameraTransform.localRotation = Quaternion.Euler(_verticalRot, 0f, 0f);
            }
            else
            {
                _verticalRot = Mathf.Clamp(_verticalRot, -verticalClampAngle, verticalClampAngle);
                if (cameraTransform != null)
                    cameraTransform.localRotation = Quaternion.Euler(_verticalRot, 0f, 0f);
            }
        }

        // 중력
        if (_cc.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        // WASD 이동 (movementLocked이면 막음)
        if (!movementLocked)
        {
            float h = 0f, v = 0f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  h -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)    v += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)  v -= 1f;

            Vector3 move = transform.right * h + transform.forward * v;
            if (move.magnitude > 1f) move.Normalize();
            _cc.Move(move * moveSpeed * Time.deltaTime);
        }

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    public void SetCursorLock(bool locked)
    {
        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }

    /// <summary>카메라를 아래 방향으로 고정 (개미 구간)</summary>
    public void LockCameraDown()
    {
        cameraLocked = true;
        _verticalRot = 90f;
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    /// <summary>카메라를 아래 방향으로 부드럽게 전환</summary>
    public void SmoothLockCameraDown(float duration = 0.5f)
    {
        // 즉시 잠금 플래그 설정 (enabled=false 이후에도 카메라 회전 허용하기 위해)
        cameraLocked = true;
        _verticalRot = 90f;
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        StartCoroutine(SmoothCameraDownRoutine(duration));
    }

    System.Collections.IEnumerator SmoothCameraDownRoutine(float duration)
    {
        cameraLocked = true;
        Quaternion startRot = cameraTransform != null ? cameraTransform.localRotation : Quaternion.identity;
        Quaternion targetRot = Quaternion.Euler(90f, 0f, 0f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        if (cameraTransform != null)
            cameraTransform.localRotation = targetRot;
        _verticalRot = 90f;
    }

    /// <summary>카메라 잠금 해제</summary>
    public void UnlockCamera()
    {
        cameraLocked = false;
        _verticalRot = 0f;
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    /// <summary>카메라 잠금을 부드럽게 해제</summary>
    public void SmoothUnlockCamera(float duration = 0.5f)
    {
        StartCoroutine(SmoothCameraUnlockRoutine(duration));
    }

    System.Collections.IEnumerator SmoothCameraUnlockRoutine(float duration)
    {
        Quaternion startRot = cameraTransform != null ? cameraTransform.localRotation : Quaternion.Euler(90f, 0f, 0f);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
        if (cameraTransform != null)
            cameraTransform.localRotation = targetRot;
        cameraLocked = false;
        _verticalRot = 0f;
    }

    /// <summary>특정 위치를 부드럽게 바라보도록 카메라 회전</summary>
    public void SmoothLookAt(Vector3 worldPos, float duration = 1f)
    {
        StartCoroutine(SmoothLookAtRoutine(worldPos, duration));
    }

    System.Collections.IEnumerator SmoothLookAtRoutine(Vector3 worldPos, float duration)
    {
        cameraLocked = true;
        Quaternion startRot = cameraTransform != null ? cameraTransform.localRotation : Quaternion.identity;

        // worldPos를 로컬 공간으로 변환하여 pitch/yaw 계산
        Vector3 dir = worldPos - cameraTransform.position;
        float pitch = 0f;
        bool success = false;

        if (dir.sqrMagnitude > 0.001f && cameraTransform.parent != null)
        {
            // 플레이어 기준 로컬 방향
            Vector3 localDir = Quaternion.Inverse(cameraTransform.parent.rotation) * dir;
            localDir.Normalize();

            // pitch (상하) = Y 방향, yaw (좌우) = XZ 평면에서
            pitch = Mathf.Asin(Mathf.Clamp(localDir.y, -1f, 1f)) * Mathf.Rad2Deg;
            float yaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

            pitch = Mathf.Clamp(pitch, -verticalClampAngle, verticalClampAngle);
            Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (cameraTransform != null)
                    cameraTransform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                yield return null;
            }
            if (cameraTransform != null)
                cameraTransform.localRotation = targetRot;

            _verticalRot = pitch;
            success = true;
        }

        cameraLocked = false;
        if (!success) _verticalRot = 0f;
    }
}
