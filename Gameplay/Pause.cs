using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Button resumeButton;
    public GameObject pl;

    private void Start()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        NightMenu.OpenSceneMenu();
    }
}
