using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public sealed class NightMenu : MonoBehaviour
{
    private enum Page { Playing, Title, Paused, Controls, Dead, Escaped, Loading }

    private static readonly Color Ink = new Color(0.037f, 0.046f, 0.047f);
    private static readonly Color Paper = new Color(0.94f, 0.90f, 0.80f);
    private static readonly Color Amber = new Color(0.96f, 0.67f, 0.36f);
    private static readonly Color Dim = new Color(0.70f, 0.72f, 0.68f);
    private static readonly string[] TitleChoices = { "Enter the house", "Controls", "Quit" };
    private static readonly string[] PauseChoices = { "Resume", "Controls", "Restart the night", "Return to title" };
    private static readonly string[] EndChoices = { "Play again", "Return to title" };
    private static readonly string[] BackChoice = { "Back" };
    private static readonly string[] Bindings = { "W A S D", "Mouse", "Left Shift", "Left Ctrl", "E", "H", "Left mouse", "Esc" };
    private static readonly string[] Actions = { "Move", "Look around", "Run", "Crouch and move quietly", "Interact / hold to turn the crank", "Hide by a bed / get out", "Hold to carry an object; release to drop", "Pause or resume" };

    private Page page, returnPage;
    private int selection, returnSelection;
    private bool paused;
    private GUIStyle title, eyebrow, copy, choice;
    private Texture2D shade;

    public static void OpenSceneMenu()
    {
        if (FindObjectOfType<NightMenu>() != null) return;
        new GameObject("Night menu").AddComponent<NightMenu>();
    }

    private void Awake()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "menu": page = Page.Title; break;
            case "dead": page = Page.Dead; break;
            case "win": page = Page.Escaped; break;
            default: page = Page.Playing; break;
        }
        if (page != Page.Playing)
        {
            foreach (Canvas canvas in FindObjectsOfType<Canvas>()) canvas.enabled = false;
            if (EventSystem.current != null) EventSystem.current.enabled = false;
        }
        SetCursor(page != Page.Playing);
        shade = new Texture2D(96, 1, TextureFormat.RGBA32, false);
        for (int x = 0; x < shade.width; x++)
        {
            float t = x / (float)(shade.width - 1);
            Color c = Ink;
            c.a = Mathf.Lerp(0.98f, 0.22f, Mathf.SmoothStep(0f, 1f, (t - 0.40f) / 0.60f));
            shade.SetPixel(x, 0, c);
        }
        shade.wrapMode = TextureWrapMode.Clamp;
        shade.Apply();
    }

    private static void SetCursor(bool show)
    {
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
    }

    private void SetPause(bool value)
    {
        paused = value;
        AudioListener.pause = value;
        Time.timeScale = value ? 0f : 1f;
        SetCursor(value);
        Show(value ? Page.Paused : Page.Playing);
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused && page == Page.Playing) SetPause(true);
    }

    private void OnDestroy()
    {
        if (shade != null) Destroy(shade);
        if (!paused) return;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Show(Page next)
    {
        page = next;
        selection = 0;
    }

    private string[] Choices()
    {
        if (page == Page.Controls) return BackChoice;
        if (page == Page.Title) return TitleChoices;
        if (page == Page.Paused) return PauseChoices;
        return EndChoices;
    }

    private void Load(string scene)
    {
        if (page == Page.Loading) return;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        paused = false;
        Show(Page.Loading);
        SceneManager.LoadSceneAsync(scene);
    }

    private void Back()
    {
        Show(returnPage);
        selection = returnSelection;
    }

    private void Choose(int index)
    {
        if (page == Page.Controls) { Back(); return; }
        if (index == 1 && (page == Page.Title || page == Page.Paused))
        {
            returnPage = page;
            returnSelection = index;
            Show(Page.Controls);
            return;
        }

        if (page == Page.Paused && index == 0) SetPause(false);
        else if (page == Page.Title && index == 2) Application.Quit();
        else if ((page == Page.Paused && index == 3) || ((page == Page.Escaped || page == Page.Dead) && index == 1)) Load("menu");
        else Load("SampleScene");
    }

    private void Update()
    {
        if (page == Page.Loading) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (page == Page.Playing || page == Page.Paused) SetPause(!paused);
            else if (page == Page.Controls) Back();
            return;
        }
        if (page == Page.Playing) return;
        int count = Choices().Length;
        if (Input.GetKeyDown(KeyCode.UpArrow)) selection = (selection + count - 1) % count;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.Tab)) selection = (selection + 1) % count;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) Choose(selection);
    }

    private static GUIStyle Text(int size, FontStyle weight, Color color)
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight, alignment = TextAnchor.MiddleLeft, wordWrap = false };
        style.normal.textColor = color;
        style.padding = new RectOffset();
        return style;
    }

    private static void Fill(Rect area, Color color)
    {
        var previous = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(area, Texture2D.whiteTexture);
        GUI.color = previous;
    }

    private void OnGUI()
    {
        if (page == Page.Playing) return;
        GUI.depth = -100;
        if (title == null)
        {
            title = Text(49, FontStyle.Bold, Paper);
            eyebrow = Text(14, FontStyle.Bold, Amber);
            copy = Text(19, FontStyle.Normal, Dim);
            choice = Text(20, FontStyle.Normal, Paper);
        }
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), shade);
        float scale = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
        Matrix4x4 previous = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 960f * scale) / 2f, (Screen.height - 540f * scale) / 2f, 0), Quaternion.identity, Vector3.one * scale);
        Fill(new Rect(53, 43, 36, 3), Amber);
        GUI.Label(new Rect(53, 61, 750, 25), page == Page.Title ? "DEEP DATTA CHAUDHURY" : "THE MASK WITHIN", eyebrow);
        string heading = "The Mask Within";
        string line = "Someone is moving through your house.";
        switch (page)
        {
            case Page.Paused: heading = "Paused"; line = "Your run will resume from this moment."; break;
            case Page.Controls: heading = "Controls"; line = ""; break;
            case Page.Dead: heading = "He found you."; line = "Break his line of sight before trying to hide."; break;
            case Page.Escaped: heading = "You got out."; line = "Behind you, the house falls silent."; break;
            case Page.Loading: heading = "Loading…"; line = ""; break;
        }
        GUI.Label(new Rect(50, 88, 840, 72), heading, title);
        if (line.Length > 0) GUI.Label(new Rect(54, 161, 820, 39), line, copy);
        if (page == Page.Controls)
        {
            for (int i = 0; i < Bindings.Length; i++)
            {
                GUI.Label(new Rect(55, 164 + 30 * i, 135, 27), Bindings[i], eyebrow);
                GUI.Label(new Rect(195, 164 + 30 * i, 650, 27), Actions[i], copy);
            }
        }
        if (page != Page.Loading)
        {
            float y = page == Page.Controls ? 413 : 231;
            string[] labels = Choices();
            for (int i = 0; i < labels.Length; i++)
            {
                Rect rect = new Rect(53, y + i * 45, 330, 40);
                if (selection == i)
                {
                    Fill(rect, new Color(0.22f, 0.23f, 0.20f, 0.80f));
                    Fill(new Rect(rect.x, rect.y, 3, rect.height), Amber);
                }
                GUI.Label(new Rect(rect.x + 14, rect.y, rect.width - 16, rect.height), labels[i], choice);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    Choose(i);
                    break;
                }
            }
            GUI.Label(new Rect(55, 480, 800, 28), "Arrow keys to select   /   Enter to confirm", copy);
        }
        GUI.matrix = previous;
    }
}
