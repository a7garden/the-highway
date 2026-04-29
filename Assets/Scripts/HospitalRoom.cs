using UnityEngine;
using System.Collections;

public class HospitalRoom : MonoBehaviour
{
    private Transform _album;
    private Transform _photo;
    private Transform _door;
    private Collider _doorCol;
    private AudioSource _weep;

    void Awake()
    {
        _album = transform.Find("HospAlbum");
        _photo = transform.Find("HospAlbumPhoto");
        _door  = transform.Find("HospDoor");
        if (_door != null) _doorCol = _door.GetComponent<Collider>();
        var weepGo = transform.Find("WeepingSound");
        if (weepGo != null) _weep = weepGo.GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // hide album until dialogue, lock door until dialogue ends
        if (_album != null) _album.gameObject.SetActive(false);
        if (_photo != null) _photo.gameObject.SetActive(false);
        if (_doorCol != null) _doorCol.enabled = false;
    }

    public void StartScene() { StartCoroutine(HospitalRoutine()); }

    IEnumerator HospitalRoutine()
    {
        yield return new WaitForSeconds(1.2f);

        // reveal the album on the table
        if (_album != null) _album.gameObject.SetActive(true);
        if (_photo != null) _photo.gameObject.SetActive(true);

        // tilt camera toward the photo
        var pc = GameManager.Instance != null ? GameManager.Instance.playerController : null;
        if (pc != null && _photo != null) pc.SmoothLookAt(_photo.position, 1.0f);
        yield return new WaitForSeconds(0.6f);

        if (DialogueManager.Instance != null)
        {
            yield return DialogueManager.Instance.PlayLinesCoroutine(
                "앨범에 사진이 들어 있다.",
                "남자아이의 얼굴이다.",
                "<i>...웃고 있다.</i>"
            );
        }

        // distant weeping
        if (_weep != null && _weep.clip != null) _weep.Play();
        if (DialogueManager.Instance != null)
        {
            yield return DialogueManager.Instance.PlayLinesCoroutine(
                "...어딘가에서 우는 소리가 들린다."
            );
        }

        DialogueManager.Instance?.ShowDialogue("[ 문을 열 수 있다 ]", 4f);

        // unlock door
        if (_doorCol != null) _doorCol.enabled = true;
    }
}
