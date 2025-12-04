using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas HUDUI;
    [SerializeField] private UIDocument Settings;
    [SerializeField] private Canvas LoadingScreen;
    private LoadingScreen LoadingScreenScript;
    private VisualElement settingsRoot;
    private bool finishedLoading;

    void Start()
    {
        if (LoadingScreen != null) LoadingScreenScript = LoadingScreen.GetComponent<LoadingScreen>();
        // rootVisualElement is guaranteed to be ready in Start
        settingsRoot = Settings.rootVisualElement;

        // Start hidden
        HideSettings();
    }

    void Update()
    {
        GameObject lastArea = GameObject.FindGameObjectWithTag("LastArea");
        if (lastArea == null)
        {
            return;
        }
        else
        {
            if (GameSession.loadSavedGame && !finishedLoading) SaveManager.LoadPlayer();
            finishedLoading = true;
            HideLoadingScreen();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && finishedLoading)
        {
            if (settingsRoot.style.display == DisplayStyle.None)
                ShowSettings();
            else
                HideSettings();
        }
    }

    public void ShowSettings()
    {
        settingsRoot.style.display = DisplayStyle.Flex;
        if(HUDUI != null) HUDUI.enabled = false;
        Time.timeScale = 0f;
    }

    public void HideSettings()
    {
        settingsRoot.style.display = DisplayStyle.None;
        if(HUDUI != null) HUDUI.enabled = true;
        Time.timeScale = 1f;
    }

    public void HideLoadingScreen()
    {
        LoadingScreenScript.Fade();
    }
}
