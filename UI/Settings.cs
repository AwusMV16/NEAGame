using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
    private Label deathsLabel;
    private int totalTimeSpent;
    private int totalEnemiesDefeated;
    private int totalDamageDealt;
    private int totalDeaths;
    private float playTimeSeconds;
    private UIManager uiManager;
    private AudioManager audioManager;
    public float initialSliderValue = 80;
    

    void Start()
    {
        LoadStats();
        settingsUI = GetComponent<UIDocument>();
        uiManager = FindAnyObjectByType<UIManager>();   
        audioManager = FindAnyObjectByType<AudioManager>();

        // Get the root VisualElement of the UIDocument
        root = settingsUI.rootVisualElement;

        // Access the volume slider
        volumeSlider = root.Q<Slider>();
        if (volumeSlider != null && audioManager != null)
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
            deathsLabel = statItems.ElementAt(3).Children().ElementAt(1) as Label;
        }

        playTimeSeconds = 0f;
    }

    void Update()
    {
        // Increment time every frame
        playTimeSeconds += Time.deltaTime;

        // Once a full second has passed
        if (playTimeSeconds >= 1f)
        {
            int secondsPassed = Mathf.FloorToInt(playTimeSeconds);
            playTimeSeconds -= secondsPassed;

            // Use IncrementStats instead of modifying totals directly
            IncrementStats(timeSpent: secondsPassed);

            // Update only the time label (optional optimisation)
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

    public void IncrementStats(int timeSpent = 0, int enemies = 0, int damageDealt = 0, int deaths = 0)
    {
        // Add each parameter to its respective total
        totalTimeSpent += timeSpent;
        totalEnemiesDefeated += enemies;
        totalDamageDealt += damageDealt;
        totalDeaths += deaths;

        // Update all labels with the new totals
        UpdateDisplay();
        SaveStats(); // Save immediately after update
    }

    private void UpdateDisplay()
    {
        if (timeSpentLabel != null) timeSpentLabel.text = FormatTime(totalTimeSpent);
        if (enemiesDefeatedLabel != null) enemiesDefeatedLabel.text = totalEnemiesDefeated.ToString();
        if (damageDealtLabel != null) damageDealtLabel.text = totalDamageDealt.ToString();
        if (deathsLabel != null) deathsLabel.text = totalDeaths.ToString();
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

    public void SaveStats()
    {
        var data =  SaveManager.Load();
        data["TotalTimeSpent"] = totalTimeSpent.ToString();
        data["TotalEnemiesDefeated"] = totalEnemiesDefeated.ToString();
        data["TotalDamageDealt"] = totalDamageDealt.ToString();
        data["TotalDeaths"] = totalDeaths.ToString();

        SaveManager.Save(data);
    }

    public void LoadStats()
    {
        var data = SaveManager.Load();

        if (data.TryGetValue("TotalTimeSpent", out var time)) totalTimeSpent = int.Parse(time);
        if (data.TryGetValue("TotalEnemiesDefeated", out var enemies)) totalEnemiesDefeated = int.Parse(enemies);
        if (data.TryGetValue("TotalDamageDealt", out var damage)) totalDamageDealt = int.Parse(damage);
        if (data.TryGetValue("TotalDeaths", out var deaths)) totalDeaths = int.Parse(deaths);

        UpdateDisplay(); // Update labels to reflect loaded stats
    }
}
