using UnityEngine;

public class VaryEnemies : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float variation = 0.25f;
    [SerializeField] private bool varySize = true;
    [SerializeField] private bool varyColor = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if(varyColor) VaryColor();
        if(varySize) VarySize();
    }

    private void VarySize()
    {
        transform.localScale *= 1 + Random.Range(-variation, variation);
    }

    private void VaryColor()
    {
        float r = Mathf.Clamp01(spriteRenderer.color.r + Random.Range(-variation, variation));
        float g = Mathf.Clamp01(spriteRenderer.color.g + Random.Range(-variation, variation));
        float b = Mathf.Clamp01(spriteRenderer.color.b + Random.Range(-variation, variation));

        spriteRenderer.color = new Color(r, g, b);
    }
}
