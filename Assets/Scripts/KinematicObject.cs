using System.Collections;
using Configuration;
using UnityEngine;
using static Utils.LayerUtils;

public class KinematicObject : MonoBehaviour {
    public float minGroundNormalY = .65f;

    [Range(0, 2)]
    public float gravityModifier;
    public Vector2 velocity;
    public bool IsGrounded { get; set; }

    public Vector2 targetVelocity;
    protected Vector2 groundNormal;
    protected Rigidbody2D body;
    protected ContactFilter2D contactFilter;
    protected readonly RaycastHit2D[] hitBuffer = new RaycastHit2D[16];

    protected const float MinMoveDistance = 0.001f;
    protected const float ShellRadius = 0.01f;

    public void Bounce(Vector2 force) {
        BounceX(force.x);
        BounceY(force.y);
    }

    public void BounceX(float value) {
        if (value == 0) {
            return;
        }

        float validatedForce = ValidateBounceForceX(value);
        if (Mathf.Approximately(validatedForce, 0f)) {
            return;
        }

        velocity.x = validatedForce;
        body.velocity = new Vector2(validatedForce, body.velocity.y);
        StopCoroutine(DecelerateBounce());
        StartCoroutine(DecelerateBounce(Mathf.Abs(value) / 10));
    }

    public void BounceY(float value) {
        if (value == 0) {
            return;
        }

        float validatedForce = ValidateBounceForceY(value);
        if (Mathf.Approximately(validatedForce, 0f)) {
            return;
        }

        velocity.y = validatedForce;
        body.velocity = new Vector2(body.velocity.x, validatedForce);
        StopCoroutine(DecelerateBounce());
        StartCoroutine(DecelerateBounce(Mathf.Abs(value) / 10));
    }

    private float ValidateBounceForceX(float bounceForce) {
        Vector2 direction = bounceForce < 0 ? Vector2.left : Vector2.right;
        return ValidateBounceForce(bounceForce, direction);
    }

    private float ValidateBounceForceY(float bounceForce) {
        Vector2 direction = bounceForce < 0 ? Vector2.down : Vector2.up;
        return ValidateBounceForce(bounceForce, direction);
    }

    private float ValidateBounceForce(float bounceForce, Vector2 direction) {
        float checkDistance = Mathf.Abs(bounceForce);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDistance, Ground);
        if (hit.collider != null) {
            return Mathf.Sign(bounceForce) * hit.distance;
        }

        return bounceForce;
    }

    private IEnumerator DecelerateBounce(float time = 0.2f) {
        yield return new WaitForSeconds(time);
        velocity.x = 0f;
        body.velocity = Vector2.zero;
    }

    public void Teleport(Vector3 position) {
        body.position = position;
        velocity *= 0;
        body.velocity *= 0;
    }

    protected virtual void OnEnable() {
        body = GetComponent<Rigidbody2D>();
        body.isKinematic = true;
    }

    protected virtual void OnDisable() {
        body.isKinematic = false;
    }

    protected virtual void Start() {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;

        gravityModifier = GlobalConfiguration.GravityScale;
    }

    protected virtual void Update() {
        targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity() {
    }

    protected virtual void FixedUpdate() {
        ApplyGravity();
        velocity.x = targetVelocity.x;
        IsGrounded = false;
        PerformMovement();
    }

    protected virtual void ApplyGravity() {
        if (gravityModifier == 0) {
            return;
        }

        if (velocity.y < 0)
            velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
        else
            velocity += Physics2D.gravity * Time.deltaTime;
    }

    protected void PerformMovement() {
        var deltaPosition = velocity * Time.deltaTime;
        var moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);
        var move = moveAlongGround * deltaPosition.x;
        PerformMovement(move, false);
        move = Vector2.up * deltaPosition.y;
        PerformMovement(move, true);
    }

    void PerformMovement(Vector2 move, bool yMovement) {
        var distance = move.magnitude;

        if (distance > MinMoveDistance) {
            var count = body.Cast(move, contactFilter, hitBuffer, distance + ShellRadius);
            for (var i = 0; i < count; i++) {
                var currentNormal = hitBuffer[i].normal;

                if (currentNormal.y > minGroundNormalY) {
                    IsGrounded = true;
                    if (yMovement) {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                if (IsGrounded) {
                    var projection = Vector2.Dot(velocity, currentNormal);
                    if (projection < 0) {
                        velocity -= projection * currentNormal;
                    }
                }
                else {
                    // Object is on air, but hit something so vertical up and horizontal velocity is 0
                    velocity.x *= 0;
                    velocity.y = Mathf.Min(velocity.y, 0);
                }

                var modifiedDistance = hitBuffer[i].distance - ShellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }
        }

        body.position += move.normalized * distance;
    }
}