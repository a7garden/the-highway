using UnityEngine;

public class DoorInteraction : MonoBehaviour, IInteractable
{
    public HighwayState nextState = HighwayState.BloodyRoad;
    public string InteractPrompt { get { return "문 열기"; } }
    public void OnInteract() { GameManager.Instance?.SetState(nextState); }
}
