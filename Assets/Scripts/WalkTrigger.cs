using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WalkTrigger : MonoBehaviour
{
    public HighwayState targetState;
    public bool oneShot = true;
    private bool fired = false;

    void Awake() { GetComponent<BoxCollider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (fired && oneShot) return;
        if (!other.CompareTag("Player")) return;
        fired = true;
        GameManager.Instance?.SetState(targetState);
    }
}
