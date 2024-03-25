using System.Collections;
using System.Collections.Generic;
using Player.StateMachine;
using UnityEngine;

public class PlayerLashState : PlayerBaseState
{
    private Transform _playerTemporaryTransform;
    public PlayerLashState(PlayerStateMachine currentCtx, PlayerStateFactory stateFactory) : base(currentCtx, stateFactory) { }
    public override void EnterState() {
        
        Ctx.InputManager.ResetLashInput();
        Ctx.GravityDirection = Ctx.PlayerTransform.up;
        Ctx.AnimatorManager.animator.SetBool(Ctx.AnimatorManager.IsHalfLashingHash ,false);
        Ctx.AnimatorManager.animator.SetBool(Ctx.AnimatorManager.IsLashingHash, true);
        
        float limit = Ctx.MaxAngle* Ctx.Precision;
        //Maximum time to reach the max angle
        Ctx.Offset = (Mathf.Log((-limit)/(-Ctx.MaxAngle + limit)) / Ctx.Damping); 
        Ctx.MaxTime = Ctx.Offset*2;
        
        _playerTemporaryTransform = Ctx.PlayerTransform;
    }

    public override void UpdateState() {
        HandleMovement();
        HandleRotation();
        HandleGroundDetection();
        HandleGravity();
        CheckSwitchStates();
        
    }

    public override void FixedUpdateState() {
    }

    public override void ExitState() {
        
    }

    public override void CheckSwitchStates() {
        if (Ctx.isGrounded) {
            SwitchStates(Factory.Air());
        }
        if (Ctx.InputManager.LashInput) {
            if (Ctx.LashingIntensity < PlayerStateMachine.MAX_LASHING_INTENSITY)
                Ctx.LashingIntensity *= PlayerStateMachine.LASHING_INTENSITY_INCREMENT;
            Ctx.InputManager.ResetLashInput();
        }
        if (Ctx.InputManager.UnLashInput) {
            if (Ctx.LashingIntensity > PlayerStateMachine.DEFAULT_LASHING_INTENSITY) { 
                Ctx.LashingIntensity /= PlayerStateMachine.LASHING_INTENSITY_INCREMENT;
            } else {
                Ctx.LashingIntensity -= PlayerStateMachine.LASHING_INTENSITY_INCREMENT;
            }
            Ctx.InputManager.ResetUnLashInput();
        }
        if (Ctx.InputManager.SmallLashInput > 0 && Ctx.LashCooldown <= 0) {
            Ctx.LashingIntensity += PlayerStateMachine.LASHING_INTENSITY_SMALL_INCREMENT * Ctx.InputManager.SmallLashInput;
            Ctx.StartCoroutine(SmallLashCooldown(0.1f));
        }
        if (Ctx.InputManager.SmallUnLashInput > 0 && Ctx.LashCooldown <= 0) {
            Ctx.LashingIntensity -= PlayerStateMachine.LASHING_INTENSITY_SMALL_INCREMENT * Ctx.InputManager.SmallUnLashInput;
            Ctx.StartCoroutine(SmallLashCooldown(0.1f));
        }
        if (Ctx.LashingIntensity <= 0) {
            SwitchStates(Factory.Halflash());
        }
        if (Ctx.InputManager.ChangeDirectionLashInput) {
            Ctx.InputManager.ResetChangeDirectionLashInput();
            ChangeDirectionLash();
        }
        
        
        
    }

    public override void InitializeSubState() {
    }
    
    private void HandleMovement() {
        //The player rotates the gravity direction it is falling towards
        
        Ctx.MoveDirection = Ctx.PlayerTransform.right * Ctx.InputManager.MovementInput.y;
                        //    + Ctx.PlayerTransform.forward * -Ctx.InputManager.MovementInput.x;
        

        Ctx.MoveDirection.Normalize();
        
        Quaternion rotation = Quaternion.Euler(Ctx.MoveDirection) * Quaternion.Euler(Ctx.GravityDirection);
        
        Ctx.GravityDirection = rotation * Ctx.GravityDirection;
    }

    private void HandleRotation() {
        
        //Rotate to face towards the gravity direction
        Quaternion targetRotation = Quaternion.FromToRotation(Ctx.PlayerTransform.up, Ctx.GravityDirection);
        Ctx.PlayerTransform.rotation = targetRotation * Ctx.PlayerTransform.rotation;
        
        //Roll along the up axis of the player to move the player using a Lerp depending on the input
      
        float moveAmount = -Time.deltaTime * Ctx.InputManager.MovementInput.x;

        Ctx.transform.rotation = Quaternion.Lerp(Ctx.transform.rotation, Ctx.transform.rotation * Quaternion.Euler(Ctx.RotationAxis * moveAmount * Ctx.MaxAngle), Ctx.LerpSpeed);
        
        
    }

    private void HandleGroundDetection() {
        
        //Vector3 rayCastOrigin = Ctx.transform.position - Ctx.RayCastHeightOffset * Ctx.GravityDirection;
        Vector3 rayCastOrigin = Ctx.PlayerRigidbody.worldCenterOfMass;
        
        if (Ctx.InAirTimer <= Ctx.MaxAirSpeed) Ctx.InAirTimer += Time.deltaTime;
        Ctx.PlayerRigidbody.AddForce(Ctx.GravityDirection * (Ctx.FallingVelocity * Ctx.InAirTimer), ForceMode.Force);
        
        if (Physics.SphereCast(rayCastOrigin, Ctx.RayCastRadius, Ctx.GravityDirection, out RaycastHit hit ,Ctx.RayCastMaxDistance, Ctx.GroundLayer))
        {
           
            Ctx.isGrounded = true;
            Ctx.GravityDirection = -hit.normal;  
            Ctx.StartCoroutine(TriggerLandingFromLashingCoroutine(hit.normal, hit.point, 0.25f));
            
            Ctx.InAirTimer = 0;
           
        }
        else
        {
            Ctx.isGrounded = false;
        }
    }
    
    private void HandleGravity() {
        
        Ctx.PlayerRigidbody.AddForce(Ctx.GravityDirection * (Ctx.GravityIntensity * Ctx.GravityMultiplier * Ctx.LashingIntensity * 0.1f), ForceMode.Acceleration);

    }
    
    public IEnumerator TriggerLandingFromLashingCoroutine(Vector3 targetNormal, Vector3 hitPoint, float duration)
    {
        //TODO: Make it work without a Coroutine
        Vector3 originalPosition = Ctx.transform.position;
        Vector3 centerOfMass = Ctx.PlayerRigidbody.worldCenterOfMass;
         
        Quaternion startRotation = Ctx.PlayerTransform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(Ctx.PlayerTransform.up, targetNormal) * Ctx.PlayerTransform.rotation;

        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            Ctx.transform.position = centerOfMass;
            Ctx.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / duration);
            Ctx.transform.position = originalPosition;
            
            Ctx.transform.position = Vector3.Lerp(originalPosition, hitPoint + targetNormal * 0.5f, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        Ctx.transform.position = hitPoint;
        Ctx.transform.rotation = targetRotation;
    }
    
    public IEnumerator SmallLashCooldown(float duration)
    {
        Ctx.LashCooldown = duration;
        while (Ctx.LashCooldown >= 0)
        {
            Ctx.LashCooldown -= Time.deltaTime;
            yield return null;
        }
    }
    
    private void ChangeDirectionLash() {
        Ctx.GravityDirection = (Ctx.PlayerTransform.position - Ctx.CameraObject.position).normalized;
    } 
    
    private float Sigmoid(float x) {
        return (1 / (1 + Mathf.Exp(-Ctx.Damping * x)));
    }
}
