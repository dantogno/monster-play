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

    private enum MonsterState { Idle, Charging }
    private MonsterState currentState = MonsterState.Idle;

    private void Awake()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        exclamationPoint.SetActive(false);
    }

    private void FixedUpdate()
    {
        bool canSeeTarget = CheckForTarget();
        exclamationPoint.SetActive(canSeeTarget);

        if (currentState == MonsterState.Idle)
        {
            if (currentChargeTime >= maxChargeTime)
            {
                currentState = MonsterState.Charging;
                currentChargeTime = 0f;
            }
            else
                FillChargeMeter(canSeeTarget);
        }
        else if (currentState == MonsterState.Charging)
        {
            float previousDirectionTowardTarget =
                Mathf.Sign((targetRigidbody.position - previousPosition).x);
            float directionTowardTarget = Mathf.Sign(DistanceToTarget.x);

            bool shouldStop = previousDirectionTowardTarget != directionTowardTarget;

            //Debug.Log($"shouldStop: {shouldStop}");

            if (shouldStop)
                currentState = MonsterState.Idle;
            else
                ChargeTowardTarget(directionTowardTarget);            
        }

        previousPosition = rigidbody2D.position;

    }

    private void FillChargeMeter(bool canSeeTarget)
    {
        if (canSeeTarget)
            currentChargeTime += Time.deltaTime;

        chargeSlider.value = currentChargeTime / maxChargeTime;
    }

    private bool CheckForTarget()
    {
        bool canSee = false;

        if (DistanceToTarget.magnitude <= sightRange)
        {
            canSee = true;
        }

        return canSee;
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
}


