using UnityEngine;
using TMPro;

public class TVSystem : MonoBehaviour, IInteractable
{
    public string[] channels = {
        "...(정적)...",
        "뉴스: 실종자 수 증가...",
        "정신적으로 문제 있는 거 아니야?",
        "애가 어떻게 저렇게 잔인할 수가 있어.",
        "[도로 영상]"
    };
    public Color[] channelColors = {
        Color.black,
        new Color(0.15f, 0.15f, 0.15f),
        new Color(0.1f, 0.08f, 0.06f),
        new Color(0.08f, 0.05f, 0.05f),
        new Color(0.05f, 0.12f, 0.05f)
    };

    public MeshRenderer screenRenderer;
    public TextMeshProUGUI screenText;

    private int ch = 0;
    public string InteractPrompt { get { return "채널 돌리기"; } }

    void OnEnable() { SetCh(0); }

    public void OnInteract()
    {
        ch = (ch + 1) % channels.Length;
        SetCh(ch);
        if (ch == channels.Length - 1) Invoke("GoToRoad", 2f);
    }

    void SetCh(int c)
    {
        if (screenRenderer != null)
        {
            var mat = screenRenderer.material;
            mat.color = channelColors[c];
            mat.SetColor("_EmissionColor", channelColors[c] * 3f);
            mat.EnableKeyword("_EMISSION");
        }
        DialogueManager.Instance?.ShowDialogue(channels[c]);
    }

    void GoToRoad() { GameManager.Instance?.SetState(HighwayState.WalkToBody); }
}
