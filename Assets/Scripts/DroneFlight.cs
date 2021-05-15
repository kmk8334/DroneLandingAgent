using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DroneFlight : MonoBehaviour
{
    public GameObject trainingArea;
    private Rigidbody2D rigidBody;
    public Collider2D[] collisionChecks;
    public Vector2 leftRotorOffset;
    public Vector2 rightRotorOffset;
    public Transform leftFoot;
    public Transform rightFoot;
    public float gravityScalar = 1.0f;
    public float timeScalar = 10.0f;
    public float torqueScalar = 0.25f;
    public Vector2 windDir = new Vector2(1, 0);
    public float windSpeed = 0.2f;
    public bool collisionDetected;

    public GameObject UIManagerObject;
    private InputUIManager UIManager;

    // Start is called before the first frame update
    void Start()
    {
        // Get the physics object that handles position/velocity
        rigidBody = GetComponent<Rigidbody2D>();

        // Get a reference to each rotor's position relative to the drone object's position.
        // The torque scalar affects how much rotational inertia will occur compared to the
        // rotor's true position. A torque scalar of 0 will be 100% vertical thrust, and
        // a torque scalar approaching infinity will be 100% rotational thrust.
        leftRotorOffset = new Vector2(
            transform.GetChild(0).transform.localPosition.x * torqueScalar,
            transform.GetChild(0).transform.localPosition.y);
        rightRotorOffset = new Vector2(
            transform.GetChild(1).transform.localPosition.x * torqueScalar,
            transform.GetChild(1).transform.localPosition.y);

        // Store a reference to the Transform points for the landing pads
        leftFoot = transform.GetChild(2);
        leftFoot = transform.GetChild(3);

        // Get a reference to necessary external resources
        UIManager = UIManagerObject.GetComponent<InputUIManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// When the drone collides with an object, inform DroneRLAgent that the episode is complete.
    /// </summary>
    /// <param name="collider">Trigger that the drone collided with</param>
    void OnTriggerEnter2D(Collider2D collider)
    {
        collisionDetected = true;
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        collisionDetected = false;
    }

    /// <summary>
    /// Initialize an episode with a starting position, rotation, velocity and wind value
    /// </summary>
    public void ResetDrone()
    {
        // Update the RLAgent tracker value
        collisionDetected = false;

        // Set the drone's starting position
        rigidBody.MovePosition(new Vector3(
            Random.Range(trainingArea.transform.position.x - 5f, 
                         trainingArea.transform.position.x + 5f),
            Random.Range(trainingArea.transform.position.y + 0f, 
                         trainingArea.transform.position.y + 3f), 
            0f));

        // Slightly rotate the drone so it's not as easy
        rigidBody.MoveRotation(Random.Range(-10f, 10f));

        // Choose a wind speed that has a high bias towards horizontal speed
        float horizontalSpeed = Random.Range(-1.0f, 1.0f);
        float verticalSpeed = horizontalSpeed * Random.Range(-1.0f, 1.0f);
        windDir = new Vector2(horizontalSpeed, verticalSpeed).normalized;
        windSpeed = Random.Range(0.05f, 0.2f);

        // Set the starting velocity to a small random value close to 0
        rigidBody.velocity = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-0.5f, 1.0f));
        rigidBody.angularVelocity = 0f;

        // Update the UI with the new wind speed
        UIManager.UpdateWindIndicator(windDir, windSpeed);
    }

    /// <summary>
    /// Apply a thrust force (acceleration) at the left rotor's position
    /// </summary>
    /// <param name="thrust">Amount of force [-1, +1] to apply (negative for inverse thrust)</param>
    public void ActivateLeftRotor(int thrust)
    {
        // Update the UI element
        UIManager.DisplayLeftTorque(thrust);

        // Skip calculations if the rotor is off
        if (thrust == 0) return;

        // Calculate the up vector, adjusted for the drone's rotation
        Vector2 up = (transform.rotation * new Vector2(0, 1)).normalized;

        // Apply the parent's transform.position and rotation to the stored offset
        Vector2 leftRotorTruePos = transform.position + transform.rotation * leftRotorOffset;

        // Apply the force to the rigidbody
        rigidBody.AddForceAtPosition(up * thrust * timeScalar, leftRotorTruePos);
    }

    /// <summary>
    /// Apply a thrust force (acceleration) at the right rotor's position
    /// </summary>
    /// <param name="thrust">Amount of force [-1, +1] to apply (negative for inverse thrust)</param>
    public void ActivateRightRotor(int thrust)
    {
        // Update the UI
        UIManager.DisplayRightTorque(thrust);

        // Skip calculations if the rotor is off
        if (thrust == 0) return;

        // Calculate the up vector, adjusted for the drone's rotation
        Vector2 up = (transform.rotation * new Vector2(0, 1)).normalized;

        // Apply the parent's transform.position and rotation to the stored offset
        Vector2 rightRotorTruePos = transform.position + transform.rotation * rightRotorOffset;

        // Apply the force to the rigidbody
        rigidBody.AddForceAtPosition(up * thrust * timeScalar, rightRotorTruePos);
    }

    /// <summary>
    /// Apply the force of gravity to the drone's body
    /// </summary>
    public void ApplyGravity()
    {
        rigidBody.AddForce(new Vector2(0, -gravityScalar * timeScalar));
    }

    /// <summary>
    /// Apply the force of wind to the drone's body
    /// </summary>
    public void ApplyWind()
    {
        rigidBody.AddForce(windDir * windSpeed * timeScalar);
    }
}
