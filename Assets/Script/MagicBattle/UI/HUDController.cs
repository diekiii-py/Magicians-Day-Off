using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple world-space HUD: shows the cat's taunt lines, the live miss counter, and the
/// win/lose screens. If the fields below are left empty, a basic world-space Canvas with
/// TMP texts and panels is generated automatically at startup - no manual UI building
/// needed. Assign the fields yourself only if you want a custom-designed HUD instead.
/// </summary>
public class HUDController : MonoBehaviour
{
    public TMP_Text messageText;
    public TMP_Text missCounterText;
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Auto-build placeholder HUD")]
    [Tooltip("If every field above is left empty, a simple world-space Canvas is built automatically.")]
    public bool autoBuildIfEmpty = true;
    [Tooltip("Optional - assign CenterEyeAnchor so the auto-built HUD always faces the player.")]
    public Transform faceTarget;
    public Vector3 localOffset = new Vector3(0f, 0f, 1.2f);
    public float worldScale = 0.0015f;

    public string[] tauntLines =
    {
        "THAT'S MY HAT!",
        "GIVE IT BACK, MAGICIAN!",
        "YOU'LL REGRET THIS!",
    };

    void Awake()
    {
        if (autoBuildIfEmpty && messageText == null && missCounterText == null && winPanel == null && losePanel == null)
        {
            BuildPlaceholderHud();
        }
    }

    void BuildPlaceholderHud()
    {
        var canvasGO = new GameObject("HUDCanvas_AutoBuilt", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.localPosition = localOffset;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one * worldScale;

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        var rootRect = canvasGO.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(800, 500);

        if (faceTarget != null)
        {
            var face = canvasGO.AddComponent<FaceCamera>();
            face.cameraToFace = faceTarget;
        }

        messageText = CreateText(canvasGO.transform, "MessageText", new Vector2(0, 170), 42, Color.white);
        missCounterText = CreateText(canvasGO.transform, "MissCounterText", new Vector2(0, -170), 30, new Color(1f, 0.45f, 0.45f));

        winPanel = CreatePanel(canvasGO.transform, "WinPanel", "YOU WIN!\nThe cat backs off.", new Color(0.35f, 0.85f, 0.4f));
        losePanel = CreatePanel(canvasGO.transform, "LosePanel", "THE CAT GOT IN...", new Color(0.9f, 0.3f, 0.3f));
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    static TMP_Text CreateText(Transform parent, string name, Vector2 anchoredPosition, float fontSize, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(760, 140);
        rect.anchoredPosition = anchoredPosition;

        var text = go.AddComponent<TextMeshProUGUI>();
        text.text = string.Empty;
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.color = color;
        text.enableWordWrapping = true;
        return text;
    }

    static GameObject CreatePanel(Transform parent, string name, string message, Color accentColor)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(760, 420);

        var image = go.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.65f);

        var label = CreateText(go.transform, "Label", Vector2.zero, 50, accentColor);
        label.text = message;
        return go;
    }

    void OnEnable()
    {
        GameEvents.OnCatRevealed += HandleCatRevealed;
        GameEvents.OnNoteMissed += HandleMissChanged;
        GameEvents.OnPlayerWin += HandleWin;
        GameEvents.OnPlayerLose += HandleLose;
    }

    void OnDisable()
    {
        GameEvents.OnCatRevealed -= HandleCatRevealed;
        GameEvents.OnNoteMissed -= HandleMissChanged;
        GameEvents.OnPlayerWin -= HandleWin;
        GameEvents.OnPlayerLose -= HandleLose;
    }

    void HandleCatRevealed()
    {
        if (messageText == null) return;
        messageText.text = tauntLines.Length > 0 ? tauntLines[Random.Range(0, tauntLines.Length)] : string.Empty;
    }

    void HandleMissChanged(int misses, int maxMisses)
    {
        if (missCounterText != null) missCounterText.text = $"Misses: {misses}/{maxMisses}";
    }

    void HandleWin()
    {
        if (winPanel != null) winPanel.SetActive(true);
    }

    void HandleLose()
    {
        if (losePanel != null) losePanel.SetActive(true);
    }
}
