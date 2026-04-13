using UnityEngine;
using System.Collections;

public class HospitalRoom : MonoBehaviour
{
    public void StartScene() { StartCoroutine(HospitalRoutine()); }

    IEnumerator HospitalRoutine()
    {
        yield return new WaitForSeconds(1f);
        DialogueManager.Instance?.ShowDialogue("앨범에 사진이 들어 있다.");
        yield return new WaitForSeconds(2.5f);
        DialogueManager.Instance?.ShowDialogue("남자아이의 얼굴이다.");
        yield return new WaitForSeconds(2.5f);
        DialogueManager.Instance?.ShowDialogue("...어딘가에서 우는 소리가 들린다.");
        yield return new WaitForSeconds(3f);
        DialogueManager.Instance?.ShowDialogue("[ 문을 열 수 있다 ]");
    }
}
