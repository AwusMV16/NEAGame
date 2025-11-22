using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image fillImage;
    private bool isLow;
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

    public void UpdateHealth(float Health, float MaxHealth)
    {
        targetFill = Health / MaxHealth;

        isLow = Health <= MaxHealth * 0.15;
        transform.parent.GetComponent<Animator>().SetBool("Throb", isLow);
    }
}
