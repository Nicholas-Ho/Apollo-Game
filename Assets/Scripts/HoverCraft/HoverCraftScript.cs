using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCraftScript : MonoBehaviour
{
    Rigidbody rigidBody;
    InputScript inputManager;
    PauseScript pauseScript;
    public enum HorizontalMovementMode {Strafing, StrafingEndless};
    public Transform hoverPointCentre;
    [Header("Drive Settings")]
    public HorizontalMovementMode horizontalMovementMode;
    public float forwardAccel;
    public float reverseAccel;
    public float lateralAccel; // If lateral movement is acceleration-based (Strafing)
    public float lateralRatio = 0.5f; // If lateral movement is velocity-based (Strafing Endless)
    public float drag;
    [Range(0,1)]
    public float slowFactor = 0.9f;
    public float groundRealignResponsiveness = 10f;
    public float angleOfRoll = 30f;
    public PIDControllerSettings anglePIDSettings;
    PIDController anglePID;

    [Header("Hover Settings")]
    public float hoverForce;
    public float hoverHeight = 0.5f;
    public float onGroundHeight = 2f;
    public float raycastDist = 1f;
    public PIDControllerSettings hoverPIDSettings;
    PIDController hoverPID;

    [Header("Physics Settings")]
    public Transform shipBody;
    public float hoverGravity = 5f;
    public float fallGravity = 10f;

    // Forward is positive
    float straightSpeed;
    // Right is positive
    float lateralSpeed;
    bool onGround;
    Vector3 initialForward;
    float yEulerAngle;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        inputManager = GetComponent<InputScript>();
        pauseScript = FindObjectOfType<PauseScript>();

        anglePID = new PIDController(anglePIDSettings);
        hoverPID = new PIDController(hoverPIDSettings);

        initialForward = transform.forward;
        yEulerAngle = transform.eulerAngles.y;
    }

    void FixedUpdate(){
        straightSpeed = Vector3.Dot(rigidBody.velocity, transform.forward);
        lateralSpeed = Vector3.Dot(rigidBody.velocity, transform.right);

        CalculateHover();
        CalculateMovement();
    }

    void CalculateHover(){
        RaycastHit hitInfo;
        Vector3 gravity;
        float height;
        Vector3 groundNormal;

        onGround = false;

        if(Physics.Raycast(hoverPointCentre.position, Vector3.down, out hitInfo, raycastDist)){
            height = hitInfo.distance;
            groundNormal = hitInfo.normal.normalized;
            float hoverPercent = hoverPID.Seek(hoverHeight, height);
            //Debug.Log(height);

            Vector3 force = Vector3.up * hoverForce * hoverPercent;
            gravity = Vector3.down * hoverGravity * height;
            rigidBody.AddForce(force, ForceMode.Force);

            onGround = (height < onGroundHeight) ? true : false;
        }  else {
            groundNormal = Vector3.up;
            gravity = Vector3.down * fallGravity;
        }
        
        rigidBody.AddForce(gravity, ForceMode.Force);


        CalibrateRotation(groundNormal);
    }

    void CalibrateRotation(Vector3 groundNormal){
        // Calculate Rotation to be (roughly) parallel to the ground
        Vector3 projection = Vector3.ProjectOnPlane(transform.forward, groundNormal);
		Quaternion rotation = Quaternion.LookRotation(projection, groundNormal);

        // To face forward
        float angleCorrection = (transform.eulerAngles.y - yEulerAngle > 180) ? 360 : 0;

        Quaternion yRotation = Quaternion.Euler(-transform.up * (transform.eulerAngles.y - yEulerAngle - angleCorrection));

        // Final Rotation
        rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, rotation * yRotation, Time.deltaTime * groundRealignResponsiveness));
    }

    void CalculateMovement(){
        // Forwards and Backwards
        if(horizontalMovementMode == HorizontalMovementMode.StrafingEndless){
            StraightMovementEndless();
        } else {
            CalculateStraightMovement();
        }

        // Horizontal
        if(horizontalMovementMode == HorizontalMovementMode.Strafing){
            CalculateLateralMovement();
        } else if(horizontalMovementMode == HorizontalMovementMode.StrafingEndless){
            CalculateLateralMovementEndless();
        }
    }

    void StraightMovementEndless(){
        if(onGround){
            float forwardRatio = Vector3.Dot(initialForward, transform.forward);
            float force = forwardAccel / forwardRatio - drag * Mathf.Abs(straightSpeed);
            rigidBody.AddForce(transform.forward * force, ForceMode.Force);
        }
    }

    void CalculateStraightMovement(){
        // Forwards and Backwards
        if(inputManager.forward > 0){
            if(straightSpeed >= 0){
                float force = inputManager.forward * forwardAccel - drag * Mathf.Abs(straightSpeed);
                rigidBody.AddForce(transform.forward * force, ForceMode.Force);
            } else {
                rigidBody.velocity -= transform.forward * (1f - slowFactor) * straightSpeed;

                float force = reverseAccel * 0.5f;
                rigidBody.AddForce(transform.forward * force, ForceMode.Force);
            }
        } else if(inputManager.backward > 0){
            if(straightSpeed <= 0){
                float force = inputManager.backward * reverseAccel - drag * Mathf.Abs(straightSpeed);
                rigidBody.AddForce(-transform.forward * force, ForceMode.Force);
            } else {
                rigidBody.velocity -= transform.forward * (1f - slowFactor) * straightSpeed;

                float force = forwardAccel * 0.5f;
                rigidBody.AddForce(-transform.forward * force, ForceMode.Force);
            }
        } else {
            rigidBody.velocity -= transform.forward * (1f - slowFactor) * straightSpeed;
        }
    }

    void CalculateLateralMovement(){
        // Lateral Movement
        // Right is positive
        if(inputManager.horizontal > 0){ // Right
            if(lateralSpeed >= 0){
                float force = Mathf.Abs(inputManager.horizontal) * lateralAccel - drag * Mathf.Abs(lateralSpeed);
                rigidBody.AddForce(transform.right * force, ForceMode.Force);
            } else {
                rigidBody.velocity -= transform.right * (1f - slowFactor) * lateralSpeed;

                float force = lateralAccel * 0.5f;
                rigidBody.AddForce(transform.right * force, ForceMode.Force);
            }
        } else if(inputManager.horizontal < 0){ // Left
            if(lateralSpeed <= 0){
                float force = Mathf.Abs(inputManager.horizontal) * lateralAccel - drag * Mathf.Abs(lateralSpeed);
                rigidBody.AddForce(-transform.right * force, ForceMode.Force);
            } else {
                rigidBody.velocity -= transform.right * (1f - slowFactor) * lateralSpeed;

                float force = lateralAccel * 0.5f;
                rigidBody.AddForce(-transform.right * force, ForceMode.Force);
            }
        } else {
            rigidBody.velocity -= transform.right * (1f - slowFactor) * lateralSpeed;
        }

        // Cosmetic Roll
        if(onGround){
            float angle = angleOfRoll * -inputManager.horizontal;
            Quaternion bodyRotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            shipBody.rotation = Quaternion.Lerp(shipBody.rotation, bodyRotation, Time.deltaTime * 10f);
        } else {
            shipBody.rotation = Quaternion.Lerp(shipBody.rotation, transform.rotation, Time.deltaTime * 2.5f);
        }
    }

    void CalculateLateralMovementEndless(){
        // Lateral Movement
        // Right is positive
        float currentLateralSpeed = Vector3.Dot(rigidBody.velocity, transform.right);
        float lateralSpeedChange = inputManager.horizontal * straightSpeed * lateralRatio - currentLateralSpeed;
        rigidBody.AddForce(transform.right * lateralSpeedChange, ForceMode.VelocityChange);

        // Cosmetic Roll
        if(onGround){
            float angle = angleOfRoll * -inputManager.horizontal;
            Quaternion bodyRotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            shipBody.rotation = Quaternion.Lerp(shipBody.rotation, bodyRotation, Time.deltaTime * 10f);
        } else {
            float angle = angleOfRoll * -inputManager.horizontal / 2;
            Quaternion bodyRotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            shipBody.rotation = Quaternion.Lerp(shipBody.rotation, bodyRotation, Time.deltaTime * 2.5f);
        }
    }
}

[System.Serializable]
public struct PIDControllerSettings{
    public float pCoeff;
    public float iCoeff;
    public float dCoeff;
    public float minimum;
    public float maximum;
}

[System.Serializable]
public struct HoverPoints{
    public Transform point;
    [HideInInspector]
    public PIDController controller;
}