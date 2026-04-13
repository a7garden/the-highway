using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public string itemName = "담배";
    public HighwayState nextState = HighwayState.Room1;
    public bool triggerState = true;

    public string InteractPrompt { get { return itemName + " 줍기"; } }

    public void OnInteract()
    {
        DialogueManager.Instance?.ShowDialogue(itemName + "을 주웠다.");
        gameObject.SetActive(false);
        if (triggerState) GameManager.Instance?.SetState(nextState);
    }
}
