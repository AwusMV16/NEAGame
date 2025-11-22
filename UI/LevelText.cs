using TMPro;
using UnityEngine;

public class LevelText : MonoBehaviour
{
    private TextMeshProUGUI text;
    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    public void updateLevelText(int level)
    {
        transform.parent.GetComponent<Animator>().SetTrigger("Pulse");
        text.SetText(level.ToString());
    }
}
