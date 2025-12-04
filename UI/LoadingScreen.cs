using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Fade()
    {
        anim.Play("LoadingScreenFade");
    }
}
