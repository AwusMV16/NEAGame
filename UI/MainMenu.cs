using UnityEngine;
using UnityEngine.UIElements;
// using System.Linq;

public static class GameSession
{
    public static bool loadSavedGame = false;
    public static bool playerSaveLoaded = false;
}

public class MainMenu : MonoBehaviour
{
    private UIDocument mainMenuUI;
    private VisualElement root;
    private Button continueButton;
    private Button LoadButton;
    private Button NewGameButton;
    private Button settingsButton;
    private Button ExitButton;
    private UIManager UIManager;
    
    void Start()
    {
        mainMenuUI = GetComponent<UIDocument>();
        UIManager = FindAnyObjectByType<UIManager>();
        root = mainMenuUI.rootVisualElement;

        continueButton = root.Q<Button>("Continue", "Row");
        NewGameButton = root.Q<Button>("NewGame", "Row");
        settingsButton = root.Q<Button>("Settings");
        ExitButton = root.Q<Button>("Exit");

        if (continueButton != null)
        {
            continueButton.clicked += () =>
            {
                GameSession.loadSavedGame = true; // tell game scene to load save
                UnityEngine.SceneManagement.SceneManager.LoadScene("Main Game");
            };
        }

        if (NewGameButton != null)
        {
            NewGameButton.clicked += () =>
            {
                GameSession.loadSavedGame = false; // tell game scene not to load save
                UnityEngine.SceneManagement.SceneManager.LoadScene("Main Game");
            };
        }

        if (settingsButton != null)
        {
            settingsButton.clicked += () =>
            {
                // Hide the entire menu
                UIManager.ShowSettings();
            };
        }

        if (ExitButton != null)
        {
            ExitButton.clicked += () =>
            {
                // Quit
                Application.Quit();
            };
        }
    }
}
