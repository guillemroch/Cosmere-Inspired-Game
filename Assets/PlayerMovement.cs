using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
/**
 * PlayerMovement:
 * Calculates player movement
 */
public class PlayerMovement : MonoBehaviour
{
    
    //Player values for movements
    
    [Header("Movement Variables")]
    public Vector3 moveDirection;

    //Movement status flags
    [Header("Movement Flags")] 
    public bool isSprinting;
    public bool isGrounded;
    public bool isJumping;
    public bool isGravitySwitching;

    
    //Falling and ground detection variables
    [Header("Falling")] 
    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public LayerMask groundLayer;
    public float rayCastHeightOffset = 0.5f;
    public float rayCastMaxDistance = 1;

    [Header("Gravity")] 
    public Vector3 gravityDirection = Vector3.down; //What is the current gravity orientation for the player
    
    //Speeds
    [Header("Speeds")] 
    public float movementSpeed ;
    public float walkingSpeed = 1.5f;
    public float runningSpeed = 6f;
    public float sprintingSpeed = 8f;
    public float rotationSpeed = 15;
    
    //Jump
    [Header("Jump Speeds")] 
    public float jumpHeight = 3;
    public float gravityIntensity = -15;
    
    //References
    [Header("References")] 
    public PlayerManager playerManager;
    public AnimatorManager animatorManager;
    public InputManager inputManager;
    public Transform cameraObject;
    public Rigidbody playerRigidbody;
   
    //------------ METHODS --------------------------------------------------------------------
    
    /**
     * Gets all the needed references in Awake()
     */
    private void Awake()
    {
        
        //Get references
        playerManager = GetComponent<PlayerManager>();
        animatorManager = GetComponent<AnimatorManager>();
        inputManager = GetComponent<InputManager>();
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    /**
     * Method called in the PlayerManager
     * Handles all of the movement function
     */
    public void HandleAllMovement()
    {
        //First Handle the Falling
        HandleFallingAndLanding();
        
        if (playerManager.isInteracting)
            return;
        if (isJumping)
            return;
        
        //If the player is interacting or jumping we do not want him to move
        HandleMovement();
        HandleRotation();
    }

    
    /**
     * Handles the movement vector, this is the speed and direction
     */
    private void HandleMovement()
    {
        moveDirection = cameraObject.forward * inputManager.movementInput.y + cameraObject.right * inputManager.movementInput.x;
        moveDirection.y = 0;
        moveDirection.Normalize();

        if (isSprinting)
        {
            movementSpeed = sprintingSpeed;
        }
        else
        {
            if (inputManager.moveAmount >= 0.5f)
            {
                movementSpeed = runningSpeed;
            }
            else
            {
                movementSpeed = walkingSpeed;
            }
        }
        
        moveDirection *= movementSpeed;

        
        Vector3 movementVelocity = moveDirection;
        playerRigidbody.velocity = movementVelocity;
    }

    /**
     * Handles the player rotation
     */
    private void HandleRotation()
    {
        
        
        Vector3 targetDirection = Vector3.zero; //Resets target direction

        //calculate orientation based on camera position
        
        targetDirection = cameraObject.forward * inputManager.movementInput.y +
                          cameraObject.right * inputManager.movementInput.x;
        targetDirection.Normalize();
        targetDirection.y = 0; //We are only rotating along the x and y axis, the y is fixed

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        
        //TODO: Testing rotation when changing gravity


        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation;
        
       
        playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        transform.rotation = playerRotation;
        transform.rotation = Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;

    }

    
    /**
     * Handles falling and landing
     */
    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position - rayCastHeightOffset * gravityDirection;
        //rayCastOrigin.y += rayCastHeightOffset;

        if (!isGrounded && !isJumping)
        {
            if (!playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Fall", true);
            }

            inAirTimer += Time.deltaTime;
            playerRigidbody.AddForce(transform.forward * leapingVelocity);
            playerRigidbody.AddForce(gravityDirection * (fallingVelocity * inAirTimer));
        }

        if (Physics.SphereCast(rayCastOrigin, 0.2f, gravityDirection, out hit ,rayCastMaxDistance, groundLayer))
        {
            if (!isGrounded && !playerManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Land", true);
                Debug.Log("Touch ground, lets land");
            }

            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    /**
     * Handles Jumping
     */
    public void HandleJumping()
    {
        if (isGrounded)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);

            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection;
            playerVelocity.y = jumpingVelocity;
            playerRigidbody.velocity = playerVelocity;
        }
        
    }

    public void HandleGravityPull()
    {
        animatorManager.animator.SetBool("isGravitySwitching", true);
        animatorManager.PlayTargetAnimation("GravitySwitch", false);

        gravityDirection = -gravityDirection;

        inAirTimer = 0;
        
        
    }

    
    /**
     * Debug Gizmos
     * Cyan Ray: Ray
     * Magenta cube: rayCastOrigin
     * Green Sphere: Raycast hit
     */
    private void OnDrawGizmos()
    {
        RaycastHit hit;

        Gizmos.color = Color.cyan;
        Vector3 rayCastOrigin = transform.position - rayCastHeightOffset * gravityDirection;

        
        Gizmos.DrawRay(rayCastOrigin, gravityDirection);
        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(rayCastOrigin, new Vector3(0.15f, 0.05f, 0.15f));
        Gizmos.color = Color.green;
        Physics.SphereCast(rayCastOrigin, 0.2f, gravityDirection, out hit, rayCastMaxDistance, groundLayer);
        Gizmos.DrawSphere(hit.point, 0.05f);


        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right);




    }
}
