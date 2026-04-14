using UnityEngine;
using System.Collections;

public class DrawerRoom : MonoBehaviour
{
    public void StartScene() { StartCoroutine(DrawerRoutine()); }

IEnumerator DrawerRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        DialogueManager.Instance?.ShowDialogue("...");
        yield return new WaitForSeconds(1.5f);
        DialogueManager.Instance?.ShowDialogue("밖에서 싸우는 소리가 들린다.");
        yield return new WaitForSeconds(2f);
        DialogueManager.Instance?.ShowDialogue("[ 서랍을 열 수 있다 ]");
        yield return new WaitForSeconds(1.5f);
        GameManager.Instance?.EnableMovement(true);
    }
}
