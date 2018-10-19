using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Contains functions for controling monster behaviour. 
/// Script uses Animation State Behaviours.
/// Charge with speed based on distance and "excitement" factor
/// 
/// </summary>
public class Monster : MonoBehaviour
{
    #region Serialized Editor Fields
    [SerializeField]
    private float sightRange = 12f;

    [SerializeField]
    private float attackRangeX = 5, attackRangeY = 10;

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
    private Collider2D groundCollider, groundDetectTrigger;

    [SerializeField]
    private ContactFilter2D groundContactFilter;

    [SerializeField]
    private PhysicsMaterial2D stoppingPhysicsMaterial, movingPhysicsMaterial;

    [SerializeField]
    private Slider chargeSlider;

    [SerializeField]
    private GameObject exclamationPoint;

    [SerializeField]
    private float attackLungeYAdjustment = 2;

    [SerializeField]
    private float attackCooldown = 2;

    [SerializeField]
    private float lungeAttackMaxY = 500, lungeAttackMinY = 100;
    #endregion

    #region Public Properties
    public PhysicsMaterial2D StoppingPhysicsMaterial => stoppingPhysicsMaterial;
    public PhysicsMaterial2D MovingPhysicsMaterial => movingPhysicsMaterial;
    public bool CanSeeTarget { get; private set; }
    public Vector2 DistanceToTarget => targetRigidbody.position - rigidbody2D.position;
    public float XDirectionTowardTarget => Mathf.Sign(DistanceToTarget.x);
    public int AttackTriggerAnimParam => attackTriggerAnimParam;
    #endregion


    #region Private Fields
    private new Rigidbody2D rigidbody2D;
    private float currentChargeTime = 0f;
    private float currentExcitementBonus = 1f;
    private float maxExcitementBonus = 2f;
    private Vector2 previousPosition;
    private Animator animator;
    private bool isFacingRight;
    private MonsterIdleBehaviour monsterIdleBehaviour;
    private MonsterAlertedBehaviour monsterAlertedBehaviour;
    private MonsterChargingBehaviour monsterChargingBehaviour;
    private MonsterStoppingBehaviour monsterStoppingBehaviour;
    private Collider2D[] groundHitDetectionResults = new Collider2D[16];
    private bool isOnGround;
    private bool attackIsOnCooldown;

    private int horizontalSpeedAnimPram = Animator.StringToHash("horizontalSpeed"),
        verticalSpeedAnimParam = Animator.StringToHash("verticalSpeed"),
        attackTriggerAnimParam = Animator.StringToHash("attack"),
        canSeeTargetAnimParam = Animator.StringToHash("canSeeTarget"),
        isOnGroundAnimParam = Animator.StringToHash("isOnGround"),
        chargeTriggerAnimParam = Animator.StringToHash("charge"),
        stopTriggerAnimParam = Animator.StringToHash("stop");
    #endregion



    #region Monobehaviour Message Functions
    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        exclamationPoint.SetActive(false);
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        InitializeAnimationStateBehaviours();
    }

    private void Update()
    {
        UpdateIsOnGround();
        UpdateAnimationParameters();
    }

    private void UpdateIsOnGround()
    {
        isOnGround = groundDetectTrigger.OverlapCollider(groundContactFilter, groundHitDetectionResults) > 0;
    }

    private void FixedUpdate()
    {
        CanSeeTarget = CheckIfTargetIsInSightRange();
       
    }
    #endregion

    #region Private Functions
    /// <summary>
    /// Find a reference to the state machine behaviours in Start since they might not exist yet in Awake.
    /// </summary>
    private void InitializeAnimationStateBehaviours()
    {
        monsterIdleBehaviour = animator.GetBehaviour<MonsterIdleBehaviour>();
        monsterIdleBehaviour.monster = this;

        monsterAlertedBehaviour = animator.GetBehaviour<MonsterAlertedBehaviour>();
        monsterAlertedBehaviour.monster = this;

        monsterChargingBehaviour = animator.GetBehaviour<MonsterChargingBehaviour>();
        monsterChargingBehaviour.monster = this;

        monsterStoppingBehaviour = animator.GetBehaviour<MonsterStoppingBehaviour>();
        monsterStoppingBehaviour.monster = this;
    }


    /// <summary>
    /// Returns true if the target is in sight range.
    /// </summary>
    private bool CheckIfTargetIsInSightRange()
    {
        bool canSee = false;

        if (DistanceToTarget.magnitude <= sightRange)
            canSee = true;

        return canSee;
    }

    /// <summary>
    /// Returns true if the target is in attack range.
    /// </summary>
    private bool CheckIfTargetIsInAttackRange()
    {
        bool isInRange = false;
        if (Mathf.Abs(DistanceToTarget.x) <= attackRangeX && Mathf.Abs(DistanceToTarget.y) <= attackRangeY)
            isInRange = true;
        return isInRange;
    }

    private void UpdateAnimationParameters()
    {
        animator.SetFloat(horizontalSpeedAnimPram, Mathf.Abs(rigidbody2D.velocity.x));
        animator.SetFloat(verticalSpeedAnimParam, rigidbody2D.velocity.y);
        animator.SetBool(isOnGroundAnimParam, isOnGround);
        animator.SetBool(canSeeTargetAnimParam, CanSeeTarget);
    }

    /// <summary>
    /// Flip character on X.
    /// </summary>
    private void FlipCharacter()
    {
        Vector2 flippedScale = transform.localScale;
        flippedScale.x *= -1;
        transform.localScale = flippedScale;
        isFacingRight = !isFacingRight;
    }

    private IEnumerator WaitForAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        attackIsOnCooldown = false;
    }
    #endregion

    #region Public Functions
    /// <summary>
    /// Displays or hides the exclamation point over the character's head.
    /// </summary>
    /// <param name="isVisible">Should the exlamation point be visible?</param>
    public void SetExclamationPointVisibility(bool makeVisible)
    {
        exclamationPoint.SetActive(makeVisible);
    }
    /// <summary>
    /// Changes the physics material of the character's collider that contacts the ground.
    /// </summary>
    /// <param name="newMaterial">Material to change to.</param>
    public void SetPhysicsMaterial(PhysicsMaterial2D newMaterial)
    {
        groundCollider.sharedMaterial = newMaterial;
    }

    /// <summary>
    /// Turns character to face the direction of its X velocity.
    /// </summary>
    public void FaceDirectionMoving()
    {
        if (rigidbody2D.velocity.x < 0 && isFacingRight)
            FlipCharacter();
        else if (rigidbody2D.velocity.x > 0 && !isFacingRight)
            FlipCharacter();
    }

    /// <summary>
    /// Turns the character to face the target.
    /// </summary>
    public void FaceTarget()
    {
        float directionTowardTarget = Mathf.Sign(DistanceToTarget.x);
        if (directionTowardTarget < 0 && isFacingRight)
            FlipCharacter();
        else if (directionTowardTarget > 0 && !isFacingRight)
            FlipCharacter();
    }

    /// <summary>
    /// The character should charge when it's charge meter is full.
    /// This fills the meter and sets an animation trigger to move to the charge state
    /// once the meter is full.
    /// 
    /// The meter is intended to fill while the character is in its Alerted state.
    /// </summary>
    public void FillChargeMeter()
    {
        currentChargeTime += Time.deltaTime;
        chargeSlider.value = currentChargeTime / maxChargeTime;

        if (currentChargeTime >= maxChargeTime)
        {
            currentChargeTime = 0;
            animator.SetTrigger(chargeTriggerAnimParam);
        }
    }

    /// <summary>
    /// Charge at target based on acceleration.
    /// </summary>
    public void ChargeTowardTarget()
    {
        float acceleration =
            Mathf.Clamp(DistanceToTarget.magnitude + chargePastTargetDistance, 1, sightRange) *
            chargeAcceleration * XDirectionTowardTarget * Time.deltaTime;

        rigidbody2D.AddForce(Vector2.right * acceleration, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Returns true if the monster should stop charging. Fires stop trigger if true.
    /// 
    /// Not sure about automatically firing the stop trigger here, but it saves
    /// making a bunch of public properties for the anim triggers.
    /// Is there ever a case where shouldStop is true and we DONT want to 
    /// fire the stop trigger?
    /// </summary>
    public bool CheckIfShouldStopCharging()
    {
        bool shouldStop = false;
        float xDirectionMoving = Mathf.Sign(rigidbody2D.velocity.x);
        bool movingWrongWay = XDirectionTowardTarget != xDirectionMoving;
        shouldStop = movingWrongWay && DistanceToTarget.magnitude > sightRange;
        if (shouldStop) animator.SetTrigger(stopTriggerAnimParam);
        return shouldStop;
    }

    public bool CheckIfShouldAttack()
    {
        bool shouldAttack = CheckIfTargetIsInAttackRange() && !attackIsOnCooldown;
        if (shouldAttack) animator.SetTrigger(attackTriggerAnimParam);
        return shouldAttack;
    }

    public void Attack()
    {
        animator.SetTrigger(attackTriggerAnimParam);
        Vector2 attackLungeDirection = DistanceToTarget.normalized;
        Vector2 force = attackLungeDirection;
        force.y += attackLungeYAdjustment + DistanceToTarget.y / 2;
        force *= attackLungeForce;
        force.y = Mathf.Clamp(force.y, lungeAttackMinY, lungeAttackMaxY);
        Debug.Log($"Attack force: {force}");
        rigidbody2D.AddForce(force, ForceMode2D.Impulse);
        FaceDirectionMoving();
        attackIsOnCooldown = true;
        StartCoroutine(WaitForAttackCooldown());
    }



    #endregion
    // TODO: flip sprite when turning
    // TODO: add some to distance so monster charges past target
    // TODO: check for position change, if it's not much, try jumping?
    // TODO: if X is close to target, but target Y is higher, try jumping?


}


