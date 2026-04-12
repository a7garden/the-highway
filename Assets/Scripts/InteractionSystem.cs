using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Raycast")]
    public float interactRange = 2.5f;
    public LayerMask interactableLayer;

    [Header("Prompt UI")]
    public TextMeshProUGUI promptText;

    private Camera _cam;
    private IInteractable _currentTarget;

    void Start()
    {
        _cam = GetComponentInChildren<Camera>();
        if (promptText != null) promptText.text = "";
    }

    void Update()
    {
        CheckForInteractable();

        if (_currentTarget != null && Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame)
        {
            _currentTarget.OnInteract();
        }
    }

    void CheckForInteractable()
    {
        if (_cam == null) return;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                _currentTarget = interactable;
                if (promptText != null) promptText.text = interactable.InteractPrompt;
                return;
            }
        }

        _currentTarget = null;
        if (promptText != null) promptText.text = "";
    }
}
