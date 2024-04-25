using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class Jumper : Agent
{
    [SerializeField] private float jumpForce = 10f;

    private bool jumpIsReady = true;
    private bool requestJump = false; 
    private Rigidbody rBody;
    private Vector3 startingPosition;
    private int score = 0;
    public event Action OnReset;

    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        startingPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        Reset();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
      
        sensor.AddObservation(rBody.velocity.y);
        sensor.AddObservation(transform.position.y);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
        if (actionBuffers.DiscreteActions[0] == 1)
        {
            Jump();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = requestJump ? 1 : 0;
    }

    private void Update()
    {
       
        if (Input.GetKey(KeyCode.UpArrow))
        {
            requestJump = true;
        }
    }

    private void FixedUpdate()
    {
      
        if (requestJump)
        {
            Jump();
            requestJump = false;
        }
    }

    private void Jump()
    {
        if (jumpIsReady)
        {
            rBody.AddForce(new Vector3(0, jumpForce, 0), ForceMode.VelocityChange);
            jumpIsReady = false;
            AddReward(-0.05f); 
        }
    }

    private void Reset()
    {
        score = 0;
        jumpIsReady = true;

        transform.position = startingPosition;
        rBody.velocity = Vector3.zero;

        OnReset?.Invoke();
    }

    private void OnCollisionEnter(Collision collidedObj)
    {
        if (collidedObj.gameObject.CompareTag("Street"))
        {
            jumpIsReady = true;
        }
        else if (collidedObj.gameObject.CompareTag("Mover") || collidedObj.gameObject.CompareTag("DoubleMover"))
        {
            AddReward(-1.0f);  
            EndEpisode();      
        }
    }

    private void OnTriggerEnter(Collider collidedObj)
    {
        if (collidedObj.gameObject.CompareTag("score"))
        {
            score++;
            AddReward(0.5f);
            ScoreCollector.Instance.AddScore(score);
        }
    }
}
