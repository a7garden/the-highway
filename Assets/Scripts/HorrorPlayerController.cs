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

    private CharacterController _cc;
    private Vector3 _velocity;
    private float _verticalRot = 0f;
    private bool _cursorLocked = true;

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
            _verticalRot = Mathf.Clamp(_verticalRot, -verticalClampAngle, verticalClampAngle);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(_verticalRot, 0f, 0f);
        }

        // 중력
        if (_cc.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        // WASD 이동
        float h = 0f, v = 0f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) h += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)  h -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)    v += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)  v -= 1f;

        Vector3 move = transform.right * h + transform.forward * v;
        if (move.magnitude > 1f) move.Normalize();
        _cc.Move(move * moveSpeed * Time.deltaTime);

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    public void SetCursorLock(bool locked)
    {
        _cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }
}
