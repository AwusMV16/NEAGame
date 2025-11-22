using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    private Image fillImage;
    private bool isFull;
    private float targetFill;
    private Animator anim;
    void Awake()
    {
        fillImage = GetComponent<Image>();
    }

    void Update()
    {
        // Smoothly move the fill amount toward the target
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * 8);
    }

    public void UpdateEnergy(float Energy, float MaxEnergy)
    {
        anim = transform.parent.GetComponent<Animator>();

        anim.SetTrigger("Pulse");
        targetFill = Energy / MaxEnergy;

        isFull = Energy >= MaxEnergy / 2;
        anim.SetBool("Shake", isFull);
    }
}
