using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    private UIDocument settingsUI;
    private VisualElement root;
    private Slider volumeSlider;
    private Button closeButton;
    private Button respawnButton;
    private Button mainmenuButton;
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
    private AudioManager audioManager;
    public float initialSliderValue = 80;
    

    void Start()
    {
        settingsUI = GetComponent<UIDocument>();
        uiManager = FindAnyObjectByType<UIManager>();   
        audioManager = FindAnyObjectByType<AudioManager>();

        // Get the root VisualElement of the UIDocument
        root = settingsUI.rootVisualElement;

        // Access the volume slider
        volumeSlider = root.Q<Slider>();
        if (volumeSlider != null)
        {
            volumeSlider.value = initialSliderValue;
            audioManager.SetMasterVolume(volumeSlider.value / 100);
            // Listen for value changes
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                float sliderValue = evt.newValue / 100; // 0 to 1
                audioManager.SetMasterVolume(sliderValue);
                // Debug.Log("Volume changed to : " + sliderValue);
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

        // Access the Respawn button
        respawnButton = root.Q<Button>("RespawnButton");
        if (respawnButton != null)
        {
            respawnButton.clicked += () =>
            {
                // Hide the entire menu
                PlayerController playerController = FindAnyObjectByType<PlayerController>();
                if (playerController != null) playerController.Respawn();
                uiManager.HideSettings();
            };
        }
        

        mainmenuButton = root.Q<Button>("MainMenuButton");
        if (mainmenuButton != null)
        {
            mainmenuButton.clicked += () =>
            {
                SceneManager.LoadScene(0);
            };
        }

        

        // Access the stats labels
        var statItemsContainer = root.Q<VisualElement>("StatItemContainer");
        if (statItemsContainer != null)
        {
            var statItems = statItemsContainer.Children(); // four StatItem elements

            // Assuming order is: Time, Enemies, Damage Dealt, Damage Taken
            timeSpentLabel = statItems.ElementAt(0).Children().ElementAt(1) as Label;
            enemiesDefeatedLabel = statItems.ElementAt(1).Children().ElementAt(1) as Label;
            damageDealtLabel = statItems.ElementAt(2).Children().ElementAt(1) as Label;
            damageTakenLabel = statItems.ElementAt(3).Children().ElementAt(1) as Label;
        }

        // totalTimeSpent = 0;
        // totalEnemiesDefeated = 0;
        // totalDamageDealt = 0;
        // totalDamageTaken = 0;
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
