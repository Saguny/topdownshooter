using UnityEngine;

public class SecretBossHallucinationUI : MonoBehaviour
{
    public Animator animator;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void PlayHallucination()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Play");
    }

    // Animation event at the end
    public void OnFinished()
    {
        gameObject.SetActive(false);
    }
}
