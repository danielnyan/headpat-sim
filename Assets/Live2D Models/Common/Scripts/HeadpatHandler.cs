using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadpatHandler : MonoBehaviour
{
    // Serializable variables (but not really)
    // Rate where model goes from idle to headpat mode
    private float headpatHeadpatDamp = 0.16f;
    // Rate of look direction adjustment in headpat mode
    private float headpatLookDamp = 0.04f;
    // Rate where model goes from headpat to idle mode
    private float idleHeadpatDamp = 0.04f;
    // Rate of look direction adjustment in idle mode
    private float idleLookDamp = 0.01f;
    // Shell radius where model's X or Y value will be maximum
    private float headColliderBuffer = 0.2f;
    // Radius where the hand will appear and the model will look at hand
    private float effectiveRadius = 0.6f;
    // Probability of playing a random idle animation per frame
    private float idleAnimationProbability = 0.001f;
    // Number of idle animations
    private int numberOfIdleAnimations = 5;

    // Runtime variables
    private GameObject headpatHand;
    private Animator animator;
    private CircleCollider2D headCollider;
    private Vector2 targetPosition;
    private int currentHeadpatMode = 0;
    private float headpatWeightTarget;
    private float headpatDamp;
    private float lookDamp;

    // Start is called before the first frame update
    private void Start()
    {
        headpatHand = GameObject.Find("Headpat Hand");
        animator = GetComponent<Animator>();
        headCollider = GetComponent<CircleCollider2D>();
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        EnableHandByDistance();

        if (Input.GetMouseButton(0) && IsHeadpatting())
        {
            animator.SetBool("Is Headpatting", true);
            HandleHeadpatProcedure();
            headpatDamp = headpatHeadpatDamp;
            headpatWeightTarget = 1f;
            lookDamp = headpatLookDamp;
        }
        else
        {
            animator.SetBool("Is Headpatting", false);
            targetPosition = Vector2.zero;
            SetMode(0);
            headpatDamp = idleHeadpatDamp;
            headpatWeightTarget = 0f;
            lookDamp = idleLookDamp;
            HandleIdleAnimation();
        }

        Vector2 currentPosition = new Vector2(
            animator.GetFloat("Headpat X"),
            animator.GetFloat("Headpat Y")
        );
        float newWeight = Mathf.Lerp(animator.GetLayerWeight(1), headpatWeightTarget, headpatDamp);
        animator.SetLayerWeight(1, newWeight);

        currentPosition = Vector2.Lerp(currentPosition, targetPosition, lookDamp);
        animator.SetFloat("Headpat X", currentPosition.x);
        animator.SetFloat("Headpat Y", currentPosition.y);
    }

    private void HandleIdleAnimation()
    {
        if (animator.GetLayerWeight(1) < 0.05f)
        {
            if (UnityEngine.Random.Range(0f,1f) < idleAnimationProbability)
            {
                animator.SetInteger("Random Idle",
                    UnityEngine.Random.Range(0, numberOfIdleAnimations));
                animator.SetTrigger("Play Idle Animation");
            }
        }
    }

    private void HandleHeadpatProcedure()
    {
        if (animator.GetLayerWeight(1) < 0.05f)
        {
            animator.SetTrigger("Interrupt Idle");
        }

        // Sets the target X and Y headpat rotation, subject to smoothing
        Vector2 basicPosition = (Vector2)headpatHand.transform.position
                - (Vector2)headCollider.bounds.center;
        float radiusMultiplier
            = basicPosition.magnitude / (headCollider.radius - headColliderBuffer);
        radiusMultiplier = Mathf.Min(1, radiusMultiplier);
        basicPosition /= Mathf.Max(Mathf.Abs(basicPosition.x),
            Mathf.Abs(basicPosition.y));
        targetPosition = basicPosition * radiusMultiplier;

        // Sets the direction of the headpat hand
        if (radiusMultiplier > 0.7f)
        {
            if (basicPosition.x > 0.95f && basicPosition.y < 0.8f)
            {
                SetMode(1);
            }
            else if (basicPosition.x < -0.95f && basicPosition.y < 0.8f)
            {
                SetMode(2);
            }
            else if (basicPosition.y < -0.9f)
            {
                SetMode(3);
            }
            else
            {
                SetMode(0);
            }
        }
        else
        {
            SetMode(0);
        }
    }

    private void EnableHandByDistance()
    {
        float distance = ((Vector2)transform.position -
            (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition)
            ).magnitude;
        if (distance > effectiveRadius)
        {
            headpatHand.SetActive(false);
            Cursor.visible = true;
            headpatHand.transform.position = Vector3.zero;
        }
        else if (distance <= effectiveRadius)
        {
            headpatHand.SetActive(true);
            Cursor.visible = false;
            headpatHand.transform.position
                = Camera.main.ScreenToWorldPoint(Input.mousePosition)
                + new Vector3(0, 0, 10);
        }
    }

    private bool IsHeadpatting()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
        if (hit.collider != null && hit.collider.transform == transform)
        {
            return true;
        }
        return false;
    }

    private void SetMode(int mode)
    {
        if (mode == currentHeadpatMode)
        {
            return;
        }

        currentHeadpatMode = mode;
        for (int i = 0; i < headpatHand.transform.childCount; i++)
        {
            if (i == mode)
            {
                headpatHand.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                headpatHand.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
