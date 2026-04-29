using UnityEngine;
using System.Collections;

public class DrawerRoom : MonoBehaviour
{
    public void StartScene() { StartCoroutine(DrawerRoutine()); }

    IEnumerator DrawerRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        if (DialogueManager.Instance != null)
        {
            yield return DialogueManager.Instance.PlayLinesCoroutine(
                "...",
                "<i>밖에서 싸우는 소리가 들린다.</i>"
            );
        }

        DialogueManager.Instance?.ShowDialogue("[ 서랍을 열 수 있다 ]", 4f);
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance?.EnableMovement(true);
    }
}
