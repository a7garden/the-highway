using UnityEngine;
using UnityEngine.InputSystem;

public class GunSystem : MonoBehaviour
{
    private bool active = false;
    private Camera cam;

    void Start() { cam = Camera.main; }

    public void Enable()
    {
        active = true;
        DialogueManager.Instance?.ShowDialogue("총을 들었다.");
        DialogueManager.Instance?.ShowDialogue("[ 좌클릭: 발사 ]");
    }

    void Update()
    {
        if (!active) return;
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.leftButton.wasPressedThisFrame) Shoot();
    }

    void Shoot()
    {
        if (cam == null) return;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 60f))
        {
            var monster = hit.collider.GetComponentInParent<MonsterAI>();
            if (monster != null)
            {
                DialogueManager.Instance?.ShowDialogue("탕!");
                active = false;
                monster.GetShot();
            }
        }
    }
}
