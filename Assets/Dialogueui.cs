using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Referencje UI")]
    public GameObject panel;        // DialoguePanel
    public TMP_Text nameText;       // imię NPC
    public TMP_Text lineText;       // treść kwestii

    [Header("Zachowanie")]
    public float autoHideSeconds = 4f;

    private float hideTimer = 0f;
    private bool visible = false;

    void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (!visible) return;

        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f || Input.GetKeyDown(KeyCode.Escape))
            Hide();
    }

    public void Show(string who, string line)
    {
        if (nameText != null) nameText.text = who;
        if (lineText != null) lineText.text = line;
        if (panel != null) panel.SetActive(true);
        visible = true;
        hideTimer = autoHideSeconds;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
        visible = false;
    }
}