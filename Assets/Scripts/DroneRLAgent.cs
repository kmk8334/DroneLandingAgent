using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class DroneRLAgent : Agent
{
    // References to external objects
    private Transform trainingArea;
    private Rigidbody2D droneRB;
    private DroneFlight droneInterface;
    private Transform target;
    public GameObject targetPlatform;
    public int episodeCount = 0;
    public int episodeStep = 0;

    private float previousDistToTarget;

    public Vector2 debugDist;

    /// <summary>
    /// Standard on scene start function
    /// </summary>
    void Start()
    {
        trainingArea = gameObject.transform.parent;
        droneRB = GetComponent<Rigidbody2D>();
        droneInterface = GetComponent<DroneFlight>();
        target = targetPlatform.transform;
    }

    /// <summary>
    /// Run this code as the setup for each training episode
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Reset the drone's position/velocity/rotation and wind speed
        droneInterface.ResetDrone();

        // Move the target to a random position [-3, 3] on the X-axis
        target.position = new Vector3(
            Random.Range(trainingArea.position.x - 3.0f, trainingArea.position.x + 3.0f),
            target.position.y,
            0);

        episodeCount++;
        episodeStep = 0;

        previousDistToTarget = Vector2.Distance(target.position, droneRB.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Prepare calculations
        Vector2 trainingAreaPos = trainingArea.position; // Cast from Vector3 to Vector2
        Vector2 positionBetweenDroneFeet = (droneInterface.leftFoot.position + droneInterface.rightFoot.position) / 2; // Cast from Vector3 to Vector2
        Vector2 platformPosition = target.position; // Center of the platform, cast from Vector3 to Vector2

        // Information about the drone itself
        sensor.AddObservation(droneRB.position - trainingAreaPos); // Current drone position
        sensor.AddObservation(droneRB.rotation); // Current drone rotation
        sensor.AddObservation(droneRB.velocity); // Current drone velocity
        sensor.AddObservation(droneRB.angularVelocity); // Current drone rotational velocity

        // Information of the drone relative to its environment
        sensor.AddObservation(platformPosition - positionBetweenDroneFeet); // Distance FROM drone TO platform
        // sensor.AddObservation(droneInterface.windDir * droneInterface.windSpeed); // Speed of the wind, which most drones cannot read
    }

    /// <summary>
    /// Define the action and reward functions
    /// </summary>
    /// <param name="actionsOut">List of action: index #0 for left rotor, #1 for right motor</param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // ACTIONS, size = 2
        // Each action should be [+1, 0, -1] for both the left and right thrusters.

        // Apply physics simulations
        droneInterface.ActivateLeftRotor(actionBuffers.DiscreteActions[0] - 1);
        droneInterface.ActivateRightRotor(actionBuffers.DiscreteActions[1] - 1);
        droneInterface.ApplyWind();
        droneInterface.ApplyGravity();
        episodeStep++;

        // REWARDS
        Vector2 distanceToTarget = target.position - ((droneInterface.leftFoot.position + droneInterface.rightFoot.position) / 2); // Cast from Vec3 to Vec2
        // float approachVelocity = Vector2.Dot(distanceToTarget, droneRB.velocity); // Rate at which the drone is approaching the target
        float velocityToTarget = Vector2.Dot(droneRB.velocity, distanceToTarget);
        float rotationDegrees = Mathf.Abs(droneRB.rotation);
        float rbVelocity = droneRB.velocity.magnitude;

        // End the episode if the drone has strayed too far from the starting position
        if (droneRB.position.x < trainingArea.position.x - 7.0f
            || droneRB.position.x > trainingArea.position.x + 7.0f
            || droneRB.position.y > trainingArea.position.y + 6.0f)
        {
            AddReward(-1000.0f);
            EndEpisode();
        }

        // If the drone has collided with something, it reached the floor!
        // Due to a lingering collision bug, ensure the episode doesn't detect a phantom collision on the first frame
        if (droneInterface.collisionDetected && episodeStep > 5)
        {
            AddReward(1000f);
            AddReward(500.0f / (distanceToTarget.magnitude + 1f));
            AddReward(-0.25f * Mathf.Abs(rotationDegrees));
            AddReward(-3f * Mathf.Pow(droneRB.velocity.magnitude, 2));
            AddReward(-3f * Mathf.Pow(droneRB.angularVelocity * Mathf.Deg2Rad, 2));
            EndEpisode();
        }

        // The episode is still going, give guiding forces for rewards.
        AddReward(previousDistToTarget - Vector2.Distance(target.position, droneRB.position));
        previousDistToTarget = Vector2.Distance(target.position, droneRB.position);
        AddReward(-0.00001f * Mathf.Abs(rotationDegrees));
    }

    /// <summary>
    /// Allow human testing to ensure input actions are properly generating
    /// </summary>
    /// <param name="actionsOut">List of action outputs: index #0 for left rotor, #1 for right motor</param>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        // Left torque
        if (Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 2;
        }
        else if (!Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 0;
        }
        else
        {
            discreteActionsOut[0] = 1;
        }

        // Right torque
        if (Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 2;
        }
        else if (!Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 0;
        }
        else
        {
            discreteActionsOut[1] = 1;
        }
    }
}
