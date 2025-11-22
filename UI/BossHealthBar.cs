using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    private Image fillImage;
    private Image backImage;
    private Image[] otherImages;
    private float targetFill;
    void Awake()
    {
        fillImage = GetComponent<Image>();
        backImage = transform.parent.GetComponent<Image>();
        otherImages = transform.parent.GetComponentsInChildren<Image>();
    }

    void Start()
    {
        SetVisible(false);
    }

    void Update()
    {
        // Smoothly move the fill amount toward the target
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * 8);
    }

    public void UpdateBossHealth(float Health, float MaxHealth)
    {

        targetFill = Health / MaxHealth;
    }

    public void SetVisible(bool visible)
    {
        backImage.enabled = visible;
        fillImage.enabled = visible;
        foreach (Image im in otherImages)
        {
            im.enabled = visible;
        }
    }
}
