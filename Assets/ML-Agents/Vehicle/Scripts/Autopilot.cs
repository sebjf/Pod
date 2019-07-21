﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autopilot : MonoBehaviour
{
    public Vector3 target;
    public float speed;

    private Vehicle vehicle;

    private void Awake()
    {
        vehicle = GetComponent<Vehicle>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var target = transform.InverseTransformPoint(this.target);
        target.y = 0;
        target.Normalize();
        var angle = Mathf.Atan2(target.x, target.z);

        var extrema = vehicle.maxSteerAngle * Mathf.Deg2Rad;

        vehicle.steeringAngle = Mathf.Clamp(angle, -extrema, extrema) / extrema;

        vehicle.throttle = speed;
    }

    private void Update()
    {

    }
}
