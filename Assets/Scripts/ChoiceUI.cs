using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ChoiceUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI questionText;
    public Button btnYes;
    public Button btnNo;
    public TextMeshProUGUI btnYesText;
    public TextMeshProUGUI btnNoText;

    private Action onYes, onNo;

    void Start() { if (panel != null) panel.SetActive(false); }

    public void Show(string question, string yesLabel, string noLabel, Action yes, Action no)
    {
        onYes = yes; onNo = no;
        if (questionText != null) questionText.text = question;
        if (btnYesText != null) btnYesText.text = yesLabel;
        if (btnNoText != null) btnNoText.text = noLabel;
        if (panel != null) panel.SetActive(true);
        btnYes.onClick.RemoveAllListeners();
        btnNo.onClick.RemoveAllListeners();
        btnYes.onClick.AddListener(() => { Hide(); onYes?.Invoke(); });
        btnNo.onClick.AddListener(() => { Hide(); onNo?.Invoke(); });
    }

    void Hide() { if (panel != null) panel.SetActive(false); }
}
