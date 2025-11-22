using UnityEngine;
using UnityEngine.UI;

public class OverheatBar : MonoBehaviour
{
    private Image fillImage;
    private bool isFull;
    private float targetFill;
    private Animator anim;
    private Color originalColor; // store the Inspector color
    void Awake()
    {
        fillImage = GetComponent<Image>();
        originalColor = fillImage.color; // save the original orange
    }

    void Update()
    {
        // Smoothly move the fill amount toward the target
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * 8);

        // Lerp the current color toward red based on fill amount
        fillImage.color = Color.Lerp(originalColor, Color.red, fillImage.fillAmount);
    }

    public void UpdateOverHeat(float OverHeatValue, float MaxOverheat)
    {
        anim = transform.parent.GetComponent<Animator>();
        if(targetFill < OverHeatValue / MaxOverheat) anim.Play("OHPulse");
        
        targetFill = OverHeatValue / MaxOverheat;
        isFull = OverHeatValue >= MaxOverheat / 2;
    }
}
