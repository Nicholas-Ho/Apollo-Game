using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverCraftScript : MonoBehaviour
{
    Rigidbody rigidBody;
    InputScript inputManager;
    public enum HorizontalMovementMode {Turning, Strafing, TurningEndless, StrafingEndless};
    public Transform HoverPointCentre;
    [Header("Drive Settings")]
    public HorizontalMovementMode horizontalMovementMode;
    public float forwardAccel;
    public float reverseAccel;
    public float lateralAccel;
    public float turnThreshold = 10f;
    public float turnResponsiveness = 1f;
    public float drag;
    [Range(0,1)]
    public float slowFactor = 0.9f;
    public float groundRealignResponsiveness = 10f;
    public float angleOfRoll = 30f;
    public PIDController anglePID;

    [Header("Hover Settings")]
    public float hoverForce;
    public float hoverHeight = 0.5f;
    public float onGroundHeight = 2f;
    public float raycastDist = 1f;
    public PIDController hoverPID;

    [Header("Physics Settings")]
    public Transform shipBody;
    public float hoverGravity = 5f;
    public float fallGravity = 10f;

    // Forward is positive
    float straightSpeed;
    // Right is positive
    float lateralSpeed;
    bool onGround;
    float yEulerAngle;
    
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        inputManager = GetComponent<InputScript>();

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

        if(Physics.Raycast(HoverPointCentre.position, Vector3.down, out hitInfo, raycastDist)){
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

        if(horizontalMovementMode == HorizontalMovementMode.Strafing || horizontalMovementMode == HorizontalMovementMode.StrafingEndless) {
            // To face forward
            float angleCorrection = (transform.eulerAngles.y - yEulerAngle > 180) ? 360 : 0;

            Quaternion yRotation = Quaternion.Euler(-transform.up * (transform.eulerAngles.y - yEulerAngle - angleCorrection));

            // Final Rotation
            rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, rotation * yRotation, Time.deltaTime * groundRealignResponsiveness));

        } else if(horizontalMovementMode == HorizontalMovementMode.TurningEndless){
            CalibrateRotationEndless(rotation);
        } else if(horizontalMovementMode == HorizontalMovementMode.Turning) {
            // Final Rotation
            rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, rotation, Time.deltaTime * groundRealignResponsiveness));
        }
    }

    void CalibrateRotationEndless(Quaternion rotation){
        if(Mathf.Abs(inputManager.horizontal) < inputManager.horizontalInputThresh + 0.1 && onGround){
            // To face forward
            float angleCorrection = (transform.eulerAngles.y - yEulerAngle > 180) ? 360 : 0;
            Quaternion yRotation = Quaternion.Euler(-transform.up * (transform.eulerAngles.y - yEulerAngle - angleCorrection));

            // Final Rotation
            rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, rotation * yRotation, Time.deltaTime * groundRealignResponsiveness));
        } else if(!onGround){
            float angleCorrection = (transform.eulerAngles.y - yEulerAngle > 180) ? 360 : 0;
            float rotateBy = anglePID.Seek(0, transform.eulerAngles.y - yEulerAngle - angleCorrection);
            Quaternion yRotation = Quaternion.Euler(transform.up * rotateBy * Time.deltaTime);

            rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, rotation * yRotation, Time.deltaTime * groundRealignResponsiveness));
        } else {
            rigidBody.MoveRotation(Quaternion.Lerp(rigidBody.rotation, rotation, Time.deltaTime * groundRealignResponsiveness));
        }
    }

    void CalculateMovement(){
        // Forwards and Backwards
        if(horizontalMovementMode == HorizontalMovementMode.TurningEndless || horizontalMovementMode == HorizontalMovementMode.StrafingEndless){
            StraightMovementEndless();
        } else {
            CalculateStraightMovement();
        }

        // Horizontal
        if(horizontalMovementMode == HorizontalMovementMode.Strafing || horizontalMovementMode == HorizontalMovementMode.StrafingEndless){
            CalculateLateralMovement();
        } else if(horizontalMovementMode == HorizontalMovementMode.Turning){
            CalculateTurning();
        } else if(horizontalMovementMode == HorizontalMovementMode.TurningEndless){
            CalculateTurningEndless();
        }
    }

    void StraightMovementEndless(){
        if(onGround){
            float force = forwardAccel - drag * Mathf.Abs(straightSpeed);
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

    void CalculateTurning(){
        if(onGround){
            // Calculate Torque
            float rotationTorque = inputManager.horizontal * turnResponsiveness - rigidBody.angularVelocity.y;
            rigidBody.AddRelativeTorque(new Vector3(0f, rotationTorque, 0f), ForceMode.VelocityChange);

            // Prevent centrifugal force from swinging the craft outwards
            Vector3 lateralFriction = -transform.right * (lateralSpeed / Time.deltaTime);
            rigidBody.AddForce(lateralFriction, ForceMode.Acceleration);

            // Cosmetic Roll
            float angle = angleOfRoll * -inputManager.horizontal;
            Quaternion bodyRotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            shipBody.rotation = Quaternion.Lerp(shipBody.rotation, bodyRotation, Time.deltaTime * 10f);
        } else {
            shipBody.rotation = Quaternion.Lerp(shipBody.rotation, transform.rotation, Time.deltaTime * 5f);
        }
    }

    void CalculateTurningEndless(){
        if(onGround){
            if(Mathf.Abs(inputManager.horizontal) > inputManager.horizontalInputThresh - 0.1){
                float angleCorrection = (transform.eulerAngles.y - yEulerAngle > 180) ? 360 : 0;
                float angleDifference = transform.eulerAngles.y - yEulerAngle - angleCorrection;

                if(Mathf.Abs(angleDifference) < turnThreshold || (angleDifference) * inputManager.horizontal < 0){ // Takes into account if the player wants to move in the opposite dir over threshold
                    // Calculate Torque
                    float rotationTorque = inputManager.horizontal * turnResponsiveness - rigidBody.angularVelocity.y;
                    rigidBody.AddRelativeTorque(new Vector3(0f, rotationTorque, 0f), ForceMode.VelocityChange);
                } else {
                    rigidBody.angularVelocity = Vector3.zero;
                }
            }
            // Code for rotating back is found in "CalibrateRotation()"

            // Prevent centrifugal force from swinging the craft outwards
            Vector3 lateralFriction = -transform.right * (lateralSpeed / Time.deltaTime);
            rigidBody.AddForce(lateralFriction, ForceMode.Acceleration);

            

            // Cosmetic Roll
            float angle = angleOfRoll * -inputManager.horizontal;
            Quaternion bodyRotation = transform.rotation * Quaternion.Euler(0f, 0f, angle);
            shipBody.rotation = Quaternion.Lerp(shipBody.rotation, bodyRotation, Time.deltaTime * 10f);
        } else {
            float angleCorrection = (transform.eulerAngles.y - yEulerAngle > 180) ? 360 : 0;
            float angleDifference = transform.eulerAngles.y - yEulerAngle - angleCorrection;

            // Preventing the ship from spinning in the air
            if(angleDifference * rigidBody.angularVelocity.y > 0){ // If the angular velocity is in the same direction as angular displacement
                float rotationTorque = -rigidBody.angularVelocity.y;
                rigidBody.AddRelativeTorque(new Vector3(0f, rotationTorque, 0f), ForceMode.VelocityChange);
            }
            shipBody.rotation = Quaternion.Lerp(shipBody.rotation, transform.rotation, Time.deltaTime * 5f);
        }
    }
}
