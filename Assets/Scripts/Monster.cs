using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Check if target is in range.
/// Check if monster can see target.
/// Start charging up "excitement." Charges faster if target is moving "playfully"
/// Charge with speed based on distance and "excitement" factor
/// 
/// </summary>
public class Monster : MonoBehaviour
{
    [SerializeField]
    private float sightRange = 12f;

    [SerializeField]
    private float attackRange = 5;

    [SerializeField]
    private float attackLungeForce = 5;

    [SerializeField]
    private float chargeAcceleration = 15f;

    [SerializeField]
    private float chargePastTargetDistance = 5f;

    [SerializeField]
    private float maxChargeTime = 4f;

    [SerializeField]
    private Rigidbody2D targetRigidbody;

    [SerializeField]
    private Slider chargeSlider;

    [SerializeField]
    private GameObject exclamationPoint;

    private Vector2 DistanceToTarget => targetRigidbody.position - rigidbody2D.position;

    private new Rigidbody2D rigidbody2D;
    private float currentChargeTime = 0f;
    private float currentExcitementBonus = 1f;
    private float maxExcitementBonus = 2f;
    private Vector2 previousPosition;
    private Animator animator;
    private bool isFacingRight;
    private bool canSeeTarget;

    private enum MonsterState { Idle, Charging }
    private MonsterState currentState = MonsterState.Idle;

    private const string horizontalSpeedAnimationParameter = "horizontalSpeed", 
        canSeeTargetAnimationParameter = "canSeeTarget", attackAnimationTriggerParameter = "attack";

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        exclamationPoint.SetActive(false);
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimationParameters();
       
    }

    private void FixedUpdate()
    {
        canSeeTarget = CanSeeTarget();
        float directionTowardTarget = Mathf.Sign(DistanceToTarget.x);
        exclamationPoint.SetActive(canSeeTarget);

        if (currentState == MonsterState.Idle)
        {
            if (currentChargeTime >= maxChargeTime)
            {
                currentState = MonsterState.Charging;
                currentChargeTime = 0f;
            }
            else
            {
                //rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
                FillChargeMeter(canSeeTarget);
                if (canSeeTarget) UpdateDirectionFacingBasedOnTargetPosition(directionTowardTarget);
            }
        }
        else if (currentState == MonsterState.Charging)
        {
            float previousDirectionTowardTarget =
                Mathf.Sign((targetRigidbody.position - previousPosition).x);           
            bool shouldStop = previousDirectionTowardTarget != directionTowardTarget;

            if (shouldStop)
                currentState = MonsterState.Idle;
            else
            {
                ChargeTowardTarget(directionTowardTarget);
                UpdateDirectionFacingBasedOnVelocity();
                if (IsInAttackRange())
                    Attack(DistanceToTarget.normalized);
            }
        }

        previousPosition = rigidbody2D.position;

    }

    private void Attack(Vector2 directionTowardTarget)
    {
        animator.SetTrigger(attackAnimationTriggerParameter);
        //rigidbody2D.velocity = new Vector2(0, rigidbody2D.velocity.y);
        rigidbody2D.AddForce(directionTowardTarget * attackLungeForce, ForceMode2D.Impulse);
    }

    private void FillChargeMeter(bool canSeeTarget)
    {
        if (canSeeTarget)
            currentChargeTime += Time.deltaTime;

        chargeSlider.value = currentChargeTime / maxChargeTime;
    }

    private bool CanSeeTarget()
    {
        bool canSee = false;

        if (DistanceToTarget.magnitude <= sightRange)
            canSee = true;

        return canSee;
    }

    private bool IsInAttackRange()
    {
        bool isInRange = false;
        if (DistanceToTarget.magnitude <= attackRange)
            isInRange = true;
        return isInRange;
    }

    // TODO: flip sprite when turning
    // TODO: add some to distance so monster charges past target
    // TODO: check for position change, if it's not much, try jumping?
    // TODO: if X is close to target, but target Y is higher, try jumping?

    /// <summary>
    /// Charge at target based on acceleration.
    /// </summary>
    private void ChargeTowardTarget(float directionTowardTarget)
    {
        float acceleration =
            Mathf.Clamp(DistanceToTarget.magnitude + chargePastTargetDistance, 1, sightRange) *
            chargeAcceleration * directionTowardTarget * Time.deltaTime;

        rigidbody2D.AddForce(Vector2.right * acceleration, ForceMode2D.Impulse);
    }

    private void UpdateAnimationParameters()
    {
        animator.SetFloat(horizontalSpeedAnimationParameter, Mathf.Abs(rigidbody2D.velocity.x));
        animator.SetBool(canSeeTargetAnimationParameter, canSeeTarget);
    }

    private void FlipCharacter()
    {
        Vector2 flippedScale = transform.localScale;
        flippedScale.x *= -1;
        transform.localScale = flippedScale;
        isFacingRight = !isFacingRight;
    }

    private void UpdateDirectionFacingBasedOnVelocity()
    {
        if (rigidbody2D.velocity.x < 0 && isFacingRight)
            FlipCharacter();
        else if (rigidbody2D.velocity.x > 0 && !isFacingRight)
            FlipCharacter();
    }

    private void UpdateDirectionFacingBasedOnTargetPosition(float directionTowardTarget)
    {
        if (directionTowardTarget < 0 && isFacingRight)
            FlipCharacter();
        else if (directionTowardTarget > 0 && !isFacingRight)
            FlipCharacter();
    }
}


