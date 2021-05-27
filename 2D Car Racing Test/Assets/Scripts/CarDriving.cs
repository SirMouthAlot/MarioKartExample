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

    float accelerationInput = 0;
    float steeringInput = 0;
    
    float horizontalInput = 0;

    float rotationAngle = 0;

    float velocityVsUp;

    Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

    }
    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        accelerationInput = Input.GetAxis("Vertical");
    }

    private void FixedUpdate()
    {
        ApplySteering();

        ApplyEngineForce();

        KillOrthogonalVelocity();
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
        else
            steeringPower = 2f; 

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If the car drives over a boost pad, apply boost
        if (collision.tag == "BoostPad")
            ApplySpeedBoost();
    }
}
