using UnityEngine;
using UnityEngine.UI;

public class NextLevelRadial : MonoBehaviour
{
    private Image fillImage;
    private float targetFill;

    void Awake()
    {
        fillImage = GetComponent<Image>();
    }

    void Update()
    {
        // Smoothly move the fill amount toward the target
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * 8);
    }

    // Update is called once per frame
    public void UpdateRemainingXP(float XP, float XPtoNextLevel)
    {
        targetFill = XP / XPtoNextLevel;
    }
}
