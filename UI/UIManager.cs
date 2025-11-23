using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Canvas HUDUI;
    [SerializeField] private UIDocument Settings;
    private VisualElement settingsRoot;

    void Start()
    {
        // rootVisualElement is guaranteed to be ready in Start
        settingsRoot = Settings.rootVisualElement;

        // Start hidden
        HideSettings();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
}
