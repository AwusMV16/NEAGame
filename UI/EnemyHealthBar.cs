using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
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

    public void UpdateHealth(float Health, float MaxHealth)
    {
        targetFill = Health / MaxHealth;
    }

    public void HideHealth()
    {
        transform.parent.gameObject.SetActive(false);
    }
    public void ShowHealth()
    {
        transform.parent.gameObject.SetActive(true);
    }
}
