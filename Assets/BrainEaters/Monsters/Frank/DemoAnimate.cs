using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoAnimate : MonoBehaviour
{
    public Animator animator;

    string[] allTriggers = new string[] { "idle", "attack", "die", "walk" };
    int currentAnimationIndex = 0;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            // Move to next animation
            currentAnimationIndex++;

            // Loop back to start
            if (currentAnimationIndex >= allTriggers.Length)
                currentAnimationIndex = 0;

            // Trigger animation
            string triggerName = allTriggers[currentAnimationIndex];
            animator.SetTrigger(triggerName);

            Debug.Log("Triggering: " + triggerName);
        }
    }
}