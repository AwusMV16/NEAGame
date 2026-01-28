using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class SettingsMainMenu : MonoBehaviour
{
    private UIDocument settingsUI;
    private VisualElement root;
    private Slider volumeSlider;
    private Button closeButton;
    private Label timeSpentLabel;
    private Label enemiesDefeatedLabel;
    private Label damageDealtLabel;
    private Label damageTakenLabel;
    private int totalTimeSpent;
    private int totalEnemiesDefeated;
    private int totalDamageDealt;
    private int totalDamageTaken;
    private float playTimeSeconds;
    private UIManager uiManager;

    void Start()
    {
        settingsUI = GetComponent<UIDocument>();
        uiManager = FindAnyObjectByType<UIManager>();

        // Get the root VisualElement of the UIDocument
        root = settingsUI.rootVisualElement;

        // Access the volume slider
        volumeSlider = root.Q<Slider>();
        if (volumeSlider != null)
        {
            // Listen for value changes
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                Debug.Log("Volume changed to: " + evt.newValue);
            });
        }

        // Access the Close button
        closeButton = root.Q<Button>("CloseButton");
        if (closeButton != null)
        {
            closeButton.clicked += () =>
            {
                // Hide the entire menu
                uiManager.HideSettings();
            };
        }

        // Access the stats labels
        var statsContainer = root.Q<VisualElement>("StatsContainer");
        statsContainer.style.display = DisplayStyle.None;

        var respawnButton = root.Q<Button>("RespawnButton");
        respawnButton.style.display = DisplayStyle.None;

        var mainmenuButton = root.Q<Button>("MainMenuButton");
        mainmenuButton.style.display = DisplayStyle.None;

        playTimeSeconds = 0f;
    }

    void Update()
    {
        // Increment time every frame
        playTimeSeconds += Time.deltaTime;

        // Update display once per second to avoid excessive updates
        if(Time.frameCount % 60 == 0)
        {
            totalTimeSpent = Mathf.FloorToInt(playTimeSeconds);
            
            // Only update the time label
            if (timeSpentLabel != null)
            {
                timeSpentLabel.text = FormatTime(totalTimeSpent);
            }
        }
    }

    public float GetVolume()
    {
        return volumeSlider.value;
    }

    public void IncrementStats(int timeSpent = 0, int enemies = 0, int damageDealt = 0, int damageTaken = 0)
    {
        // Add each parameter to its respective total
        totalTimeSpent += timeSpent;
        totalEnemiesDefeated += enemies;
        totalDamageDealt += damageDealt;
        totalDamageTaken += damageTaken;

        // Update all labels with the new totals
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        // Updates all stat labels to display the current totals
        timeSpentLabel.text = totalTimeSpent.ToString();
        enemiesDefeatedLabel.text = totalEnemiesDefeated.ToString();
        damageDealtLabel.text = totalDamageDealt.ToString();
        damageTakenLabel.text = totalDamageTaken.ToString();
    }
    
    private string FormatTime(int seconds)
    {
        if (seconds < 3600) // Less than 1 hour
        {
            int minutes = seconds / 60;
            int secs = seconds % 60;
            return $"{minutes}:{secs:D2}";
        }
        else // 1 hour or more
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;
            return $"{hours}:{minutes:D2}:{secs:D2}";
        }
    }
}
