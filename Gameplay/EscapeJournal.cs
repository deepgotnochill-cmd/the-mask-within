using UnityEngine;

// Lives on the front-entry trigger for this run; the gameplay scene owns its lifetime.
public sealed class EscapeJournal : MonoBehaviour
{
    public enum Stage { FrontDoor, Crank, Key, Attic, Escape }
    public static EscapeJournal Current { get; private set; }

    private static readonly string[] Goals = {
        "Find the front door.",
        "The front door is blocked. Find the stair crank.",
        "The stairs are down. Find the key near the front door.",
        "Take the key up the stairs to the attic door.",
        "Go through the open attic door."
    };

    private PlayerController player;
    public static bool PlayerHidden => Current != null && Current.player != null && !Current.player.enabled;

    private Stage stage;
    private string prompt;
    private int promptFrame;
    private float progress;
    private GUIStyle textStyle;

    private void Awake() { Current = this; }
    private void Start() { player = FindObjectOfType<PlayerController>(); }
    private void OnDestroy() { if (Current == this) Current = null; }

    public void Advance(Stage next)
    {
        if (next > stage) stage = next;
    }

    public void Prompt(string text, float fraction = -1f)
    {
        prompt = text;
        promptFrame = Time.frameCount;
        progress = fraction;
    }

    private void OnGUI()
    {
        if (Time.timeScale <= 0f || Event.current.type != EventType.Repaint) return;
        if (textStyle == null)
            textStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontSize = 17, wordWrap = false };

        float scale = Mathf.Min(Screen.width / 900f, Screen.height / 640f);
        scale = Mathf.Min(scale, 2f);
        Matrix4x4 saved = GUI.matrix;
        Color savedColor = GUI.color;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
        float width = Screen.width / scale;
        float height = Screen.height / scale;
        DrawStrip(new Rect(18f, 18f, 0f, 36f), Goals[(int)stage], false, -1f);
        if (!string.IsNullOrEmpty(prompt) && promptFrame == Time.frameCount)
            DrawStrip(new Rect(width / 2f, height - 76f, 0f, 40f), prompt, true, progress);
        GUI.color = savedColor;
        GUI.matrix = saved;
    }

    private void DrawStrip(Rect bounds, string text, bool centered, float meter)
    {
        bounds.width = textStyle.CalcSize(new GUIContent(text)).x + 30f;
        if (centered) bounds.x -= bounds.width * 0.5f;
        GUI.color = new Color(0.05f, 0.055f, 0.06f, 0.92f);
        GUI.DrawTexture(bounds, Texture2D.whiteTexture);
        GUI.color = new Color(0.86f, 0.65f, 0.42f);
        GUI.DrawTexture(new Rect(bounds.x, bounds.y, 3f, bounds.height), Texture2D.whiteTexture);
        if (meter >= 0f)
            GUI.DrawTexture(new Rect(bounds.x, bounds.yMax - 3f, bounds.width * Mathf.Clamp01(meter), 3f), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(bounds.x + 12f, bounds.y, bounds.width - 14f, bounds.height), text, textStyle);
    }
}
