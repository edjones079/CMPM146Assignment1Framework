using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Security.Cryptography;
using System;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    // you can use this label to show debug information,
    // like the distance to the (next) target
    public TextMeshProUGUI label;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;
    }

    // Update is called once per frame
    void Update()
    {
        // Assignment 1: If a single target was set, move to that target
        //                If a path was set, follow that path ("tightly")

        // you can use kinematic.SetDesiredSpeed(...) and kinematic.SetDesiredRotationalVelocity(...)
        //    to "request" acceleration/decceleration to a target speed/rotational velocity
        float arrivalRadius = 5.0f;
        float nextTurn = 0.0f;

        if (path != null && path.Count > 0)
        {
            target = path[0];
        }

        if (path != null && path.Count > 1)
        {
            nextTurn = Vector3.SignedAngle(path[0] - transform.position, path[1] - path[0], Vector3.up);
            UnityEngine.Debug.Log(nextTurn);
        }

        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;

        if (distance < arrivalRadius)
        {
            if (path != null && path.Count > 0)
            {
                path.RemoveAt(0);

                if (path.Count == 0)
                {
                    path = null;
                    kinematic.SetDesiredSpeed(0);
                    kinematic.SetDesiredRotationalVelocity(0);
                }
            }
            else {
                kinematic.SetDesiredSpeed(0);
                kinematic.SetDesiredRotationalVelocity(0);
            }
            return;
        }

        Vector3 forward = transform.forward;

        float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

        float angleAbs = Mathf.Abs(angle);

        float maxSpeed = kinematic.max_speed;
        float minSpeedFactor = 0.2f;

        float angleFactor = Mathf.Exp(-angleAbs / 45f);
        angleFactor = Mathf.Clamp(angleFactor, minSpeedFactor, 1f);

        float baseSpeed = Mathf.Min(maxSpeed, distance);

        float adjustedSpeed = baseSpeed;

        

        if (Math.Abs(nextTurn) > 90f)
        {
            adjustedSpeed = baseSpeed * angleFactor;
        }

        kinematic.SetDesiredSpeed(adjustedSpeed);
        kinematic.SetDesiredRotationalVelocity(angle);
    }

    public void SetTarget(Vector3 target)
    {
        this.target = target;
        EventBus.ShowTarget(target);
    }

    public void SetPath(List<Vector3> path)
    {
        this.path = path;
    }

    public void SetMap(List<Wall> outline)
    {
        this.path = null;
        this.target = transform.position;
    }
}
