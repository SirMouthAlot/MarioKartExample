using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDriving : MonoBehaviour
{
    [SerializeField] float accelerationPower;
    [SerializeField] float steeringPower;
    [SerializeField] float driftPower;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxSpeedWithBoost;
    [SerializeField] float boostForce = 25f;
    [SerializeField] GameObject raceManager;
    [SerializeField ]ParticleSystem driftParticles;

    [SerializeField] Transform finishLinePos;

    float accelerationInput = 0;
    float steeringInput = 0;
    
    float horizontalInput = 0;

    float rotationAngle = 0;

    float velocityVsUp;

    bool canDrive = true;
    bool offroad = false;
    bool isDrifting = false;
    bool driftActivated = false;
    bool checkpointReached = true;

    Rigidbody2D body;
    LapCounter lapCounter;
    Vector2 triggerPosition;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        lapCounter = raceManager.GetComponent<LapCounter>();

        driftParticles.gameObject.SetActive(false);

        triggerPosition = finishLinePos.position;
    }
    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        accelerationInput = Input.GetAxis("Vertical");

        //If the race is over don't allow the car to drive anymore
        if (lapCounter.GetIsCompleted())
            canDrive = false;

        //Check for drift input
        if (Input.GetKeyDown(KeyCode.Space))
            isDrifting = true;
        if (Input.GetKeyUp(KeyCode.Space))
            isDrifting = false;
    }

    private void FixedUpdate()
    {
        //if the race is not completed yet the player can drive
        if (canDrive)
        {
            ApplySteering();

            ApplyEngineForce();

            KillOrthogonalVelocity();

            if (offroad)
                AdjustSpeedForOffroad();
            else
                maxSpeed = 35;

            //If the car is moving faster than the current maximum speed, the car will slow down until it reaches its maximum speed again
            if (velocityVsUp > maxSpeed)
                SlowDownToMaxSpeed();

            //Activates and Deactivates drifting
            if (isDrifting && !driftActivated)
                ActivateDrift();
            else if (!isDrifting & driftActivated)
                DeactivateDrift();
        }
        else
            body.velocity = new Vector2(0, 0);
    }

    void ActivateDrift()
    {
        //Changes the turning values to allow for a drifting effect
        driftPower = 0.9f;
        steeringPower = 2.5f;
        driftActivated = true;
        driftParticles.gameObject.SetActive(true);
    }

    void DeactivateDrift()
    {
        //Changes the turning values back to normal when the player is no longer drifting
        driftPower = 0.5f;
        steeringPower = 1.5f;
        driftActivated = false;
        driftParticles.gameObject.SetActive(false);
    }

    void AdjustSpeedForOffroad()
    {
        //Changed the maximum driving speed when the car is off of the track
        maxSpeed = 20;
    }

    private void ApplySteering()
    {
        //Makes steering smoother when transitioning between forward and backward movement
        if (horizontalInput < 0 && steeringInput > 0 && ((accelerationInput <= 0 && velocityVsUp > 0) || (accelerationInput >= 0 && velocityVsUp < 0)))
            steeringInput = Mathf.Lerp(steeringInput, -1, Time.fixedDeltaTime);
        else if (horizontalInput > 0 && steeringInput < 0 && ((accelerationInput <= 0 && velocityVsUp > 0) || (accelerationInput >= 0 && velocityVsUp < 0)))
            steeringInput = Mathf.Lerp(steeringInput, 1, Time.fixedDeltaTime);
        else
            steeringInput = horizontalInput;

        //Limits turning based on speed (Can't turn when not moving forward)
        float minSpeedForTurn = (body.velocity.magnitude / 8);

        minSpeedForTurn = Mathf.Clamp01(minSpeedForTurn);

        //Stops the car from still turning in the same direction when changing from forward input to backward input
        if (accelerationInput <= 0 && velocityVsUp > 0 || accelerationInput >= 0 && velocityVsUp < 0)
            steeringPower = Mathf.Lerp(steeringPower, 0.0f, Time.fixedDeltaTime * 2);
        else if (driftActivated)
            steeringPower = 2.5f;
        else
            steeringPower = 1.5f;

        //Turns Car, Direction changes if they are moving backward
        if (velocityVsUp <= 0)
            rotationAngle += steeringInput * steeringPower * minSpeedForTurn;
        else
            rotationAngle -= steeringInput * steeringPower * minSpeedForTurn;

        body.rotation = rotationAngle;
    }

    private void ApplyEngineForce()
    { 
        //Our Forward Speed
        velocityVsUp = Vector2.Dot(transform.up, body.velocity);

        //Limits Forward Speed
        if (velocityVsUp >= maxSpeed && accelerationInput > 0)
            return;

        //Limits Reverse Speed
        if (velocityVsUp <= -maxSpeed * 0.5f && accelerationInput < 0)
            return;
        
        //Creates a Drag when no accelerating so the car stops on its own
        if (accelerationInput == 0 || (accelerationInput < 0 && velocityVsUp > 0))
            body.drag = Mathf.Lerp(body.drag, 5.0f, Time.fixedDeltaTime * 3);
        else
            body.drag = 0;

        //Makes the car accelerate
        Vector2 engineForce = transform.up * accelerationInput * accelerationPower;

        body.AddForce(engineForce, ForceMode2D.Force);
    }

    void SlowDownToMaxSpeed()
    {
        //Adds a backwards force that is twice as strong as the forward driving force to slow the car down if it is over maximum speed
        Vector2 slowDownForce = -transform.up * (accelerationPower * 2.0f);

        body.AddForce(slowDownForce, ForceMode2D.Force);
    }

    void KillOrthogonalVelocity()
    {
        //Limits Sideways movement when turning (Less Space Like)
        Vector2 forwardVelocity = transform.up * Vector2.Dot(body.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(body.velocity, transform.right);

        body.velocity = forwardVelocity + rightVelocity * driftPower;
    }

    void ApplySpeedBoost()
    {
        //if the speed of the car is already more than the max allowed speed with boost then dont add the boost
        if (velocityVsUp >= maxSpeedWithBoost && accelerationInput > 0)
            return;

        body.AddForce(transform.up * boostForce, ForceMode2D.Impulse);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Vector2 playerPos = transform.position;

        Vector2 triggerDirection = (triggerPosition - playerPos).normalized;

        //If the car drives over a boost pad, apply boost
        if (collision.tag == "BoostPad")
            ApplySpeedBoost();

        //Allows the lap to increase when the car passes the finish line
        if (collision.tag == "FinishLine" && triggerDirection.y > 0 && checkpointReached)
        {
            lapCounter.IncrementLap();
            checkpointReached = false;
        }
        
        //Checks if the car is on the track
        if (collision.tag == "Track")
            offroad = false;

        //Checks if the car has passed the checkpoint to allow for a lap increase
        if (collision.tag == "Checkpoint")
            checkpointReached = true;

    }

    void OnTriggerExit2D(Collider2D collision)
    {
        //Checks if the car is off of the track
        if (collision.tag == "Track")
            offroad = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Creates a small bounce off of the wall when hit
        body.AddForce(collision.contacts[0].normal * (velocityVsUp / 2), ForceMode2D.Impulse);
    }
}
