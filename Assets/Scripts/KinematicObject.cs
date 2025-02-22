using System.Collections;
using Configuration;
using UnityEngine;

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
        StartCoroutine(BounceDynamic(force));
    }

    public void BounceX(float value) {
        Bounce(new Vector2(value, 0f));
    }

    public void BounceY(float value) {
        Bounce(new Vector2(0f, value));
    }

    private IEnumerator BounceDynamic(Vector2 force) {
        float maxForce = Mathf.Max(Mathf.Abs(force.x), Mathf.Abs(force.y));
        body.isKinematic = false;
        body.AddForce(force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(maxForce / 10);
        body.velocity = Vector2.zero;
        body.isKinematic = true;
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
                } else {
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