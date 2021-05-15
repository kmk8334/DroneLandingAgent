using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputUIManager : MonoBehaviour
{
    public SpriteRenderer leftTorqueUp;
    public SpriteRenderer leftTorqueDown;
    public SpriteRenderer rightTorqueUp;
    public SpriteRenderer rightTorqueDown;
    public GameObject windIndicator;

    // Start is called before the first frame update
    void Start()
    {
        leftTorqueUp = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        leftTorqueDown = gameObject.transform.GetChild(1).GetComponent<SpriteRenderer>();
        rightTorqueUp = gameObject.transform.GetChild(2).GetComponent<SpriteRenderer>();
        rightTorqueDown = gameObject.transform.GetChild(3).GetComponent<SpriteRenderer>();
        windIndicator = gameObject.transform.GetChild(4).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        // UI is updated each frame by DroneFlight.cs
    }

    /// <summary>
    /// Updates the UI arrows for left torque
    /// </summary>
    /// <param name="thrust">+1 for positive thrust, 0 for neutral, -1 for negative thrust</param>
    public void DisplayLeftTorque(int thrust)
    {
        switch (thrust)
        {
            case 1:
                UpdateSpriteColor(leftTorqueUp, true);
                UpdateSpriteColor(leftTorqueDown, false);
                break;
            case 0:
                UpdateSpriteColor(leftTorqueUp, false);
                UpdateSpriteColor(leftTorqueDown, false);
                break;
            case -1:
                UpdateSpriteColor(leftTorqueUp, false);
                UpdateSpriteColor(leftTorqueDown, true);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Updates the UI arrows for right torque
    /// </summary>
    /// <param name="thrust">+1 for positive thrust, 0 for neutral, -1 for negative thrust</param>
    public void DisplayRightTorque(int thrust)
    {
        switch (thrust)
        {
            case 1:
                UpdateSpriteColor(rightTorqueUp, true);
                UpdateSpriteColor(rightTorqueDown, false);
                break;
            case 0:
                UpdateSpriteColor(rightTorqueUp, false);
                UpdateSpriteColor(rightTorqueDown, false);
                break;
            case -1:
                UpdateSpriteColor(rightTorqueUp, false);
                UpdateSpriteColor(rightTorqueDown, true);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Helper function for updating the color filter of a given UI element
    /// </summary>
    /// <param name="obj">Component of the UI object</param>
    /// <param name="enabled">true to display with red, false to display pure black</param>
    private void UpdateSpriteColor(SpriteRenderer obj, bool enabled)
    {
        obj.color = enabled ? new Color(1, 1, 1) : new Color(0, 0, 0);
    }


    public void UpdateWindIndicator(Vector2 windDir, float windSpeed)
    {
        // Calculate the direction the wind is pointing in from [-90 deg, +90 deg],
        // and if X is pointing in the negative direction, flip it by adding 180 degrees
        windIndicator.transform.rotation = Quaternion.Euler(
            0, 
            0, 
            Mathf.Atan(windDir.y / windDir.x) * Mathf.Rad2Deg + (windDir.x < 0 ? 180 : 0));

        windIndicator.transform.localScale = new Vector3(
            windSpeed,
            windSpeed,
            windIndicator.transform.localScale.z);
    }
}
