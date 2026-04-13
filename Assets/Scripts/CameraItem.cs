using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class CameraItem : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    public float zoomFOV = 22f;
    public float zoomSpeed = 8f;

    private Camera cam;
    private float normalFOV = 60f;
    private bool inInventory = false;
    private bool zoomed = false;
    private bool used = false;
    private InteractionSystem interSys;

    public string InteractPrompt { get { return inInventory ? "" : "카메라 줍기"; } }

    public void OnInteract()
    {
        inInventory = true;
        DialogueManager.Instance?.ShowDialogue("카메라를 집었다.");
        DialogueManager.Instance?.ShowDialogue("[ 우클릭: 줌 / 좌클릭: 촬영 ]");
        gameObject.SetActive(false); // hide world object, keep script on player
    }

    void Start()
    {
        cam = Camera.main;
        if (cam != null) normalFOV = cam.fieldOfView;
        interSys = FindObjectOfType<InteractionSystem>();
    }

    void Update()
    {
        if (!inInventory || used) return;
        var mouse = Mouse.current;
        if (mouse == null) return;

        bool rHeld = mouse.rightButton.isPressed;
        zoomed = rHeld;
        if (cam != null)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, rHeld ? zoomFOV : normalFOV, Time.deltaTime * zoomSpeed);

        // Block other interactions while zooming
        if (interSys != null) interSys.enabled = !rHeld;

        if (zoomed && mouse.leftButton.wasPressedThisFrame && !used)
        {
            used = true;
            if (interSys != null) interSys.enabled = true;
            StartCoroutine(TakePhoto());
        }
    }

    IEnumerator TakePhoto()
    {
        DialogueManager.Instance?.ShowDialogue("찰칵.");
        if (cam != null) cam.fieldOfView = normalFOV;
        yield return new WaitForSeconds(0.8f);
        GameManager.Instance?.SetState(HighwayState.HospitalScene);
    }
}
