using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

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

        continueButton = root.Q<Button>(null, "Row"); // gets first button with class Row
        NewGameButton = root.Q<Button>("NewGame");
        settingsButton = root.Q<Button>("Settings");
        ExitButton = root.Q<Button>("Exit");
        continueButton = root.Q<Button>("Continue");
        LoadButton = root.Q<Button>("LoadSave");

        if (settingsButton != null)
        {
            settingsButton.clicked += () =>
            {
                // Hide the entire menu
                UIManager.ShowSettings();
            };
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
