using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    Rigidbody2D body;

    [SerializeField] float accelerationPower;
    [SerializeField] float steeringPower;
    [SerializeField] float boostForce = 25f;
    float steerAmount;
    float speed;
    float direction;


    void Start()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        ApplyDriveForce();

        ApplySteerForce();
    }

    void ApplySteerForce()
    {
        steerAmount = -Input.GetAxis("Horizontal");

        direction = Mathf.Sign(Vector2.Dot(body.velocity, body.GetRelativeVector(Vector2.up)));

        body.rotation += steerAmount * steeringPower * body.velocity.magnitude * direction;

        body.AddRelativeForce(-Vector2.right * body.velocity.magnitude * steerAmount / 2);
    }

    void ApplyDriveForce()
    {
        speed = Input.GetAxis("Vertical") * accelerationPower;

        body.AddRelativeForce(Vector2.up * speed);
    }

    void ApplySpeedBoost()
    {
        body.AddForce(body.GetRelativeVector(Vector2.up) * boostForce, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "BoostPad")
            ApplySpeedBoost();
    }
}
