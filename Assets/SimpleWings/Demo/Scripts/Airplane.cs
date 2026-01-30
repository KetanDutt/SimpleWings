//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using System;

public class Airplane : MonoBehaviour
{
	public ControlSurface elevator;
	public ControlSurface aileronLeft;
	public ControlSurface aileronRight;
	public ControlSurface rudder;
	public Engine engine;

	public WeaponDropper[] weapons;

	public Rigidbody Rigidbody { get; internal set; }

	private float throttle = 1.0f;
	private bool yawDefined = false;

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
	}

	private void Start()
	{
		if (elevator == null)
			Debug.LogWarning(name + ": Airplane missing elevator!");
		if (aileronLeft == null)
			Debug.LogWarning(name + ": Airplane missing left aileron!");
		if (aileronRight == null)
			Debug.LogWarning(name + ": Airplane missing right aileron!");
		if (rudder == null)
			Debug.LogWarning(name + ": Airplane missing rudder!");
		if (engine == null)
			Debug.LogWarning(name + ": Airplane missing engine!");

		try
		{
			Input.GetAxis("Yaw");
			yawDefined = true;
		}
		catch (ArgumentException e)
		{
			Debug.LogWarning(e);
			Debug.LogWarning(name + ": \"Yaw\" axis not defined in Input Manager. Rudder will not work correctly!");
		}
	}

	// Update is called once per frame
	void Update()
	{
		if (elevator != null)
		{
			elevator.targetDeflection = -Input.GetAxis("Vertical");
		}
		if (aileronLeft != null)
		{
			aileronLeft.targetDeflection = -Input.GetAxis("Horizontal");
		}
		if (aileronRight != null)
		{
			aileronRight.targetDeflection = Input.GetAxis("Horizontal");
		}
		if (rudder != null && yawDefined)
		{
			// YOU MUST DEFINE A YAW AXIS FOR THIS TO WORK CORRECTLY.
			// Imported packages do not carry over changes to the Input Manager, so
			// to restore yaw functionality, you will need to add a "Yaw" axis.
			rudder.targetDeflection = Input.GetAxis("Yaw");
		}

		if (engine != null)
		{
			// Fire 1 to speed up, Fire 2 to slow down. Make sure throttle only goes 0-1.
			throttle += Input.GetAxis("Fire1") * Time.deltaTime;
			throttle -= Input.GetAxis("Fire2") * Time.deltaTime;
			throttle = Mathf.Clamp01(throttle);

			engine.throttle = throttle;
		}

		if (weapons.Length > 0)
		{
			if (Input.GetButtonDown("Fire3"))
			{
				foreach (WeaponDropper dropper in weapons)
				{
					dropper.Fire(Rigidbody.GetPointVelocity(dropper.transform.position));
				}
			}
		}
	}

	private float CalculatePitchG()
	{
		// Angular velocity is in radians per second.
		Vector3 localVelocity = transform.InverseTransformDirection(Rigidbody.velocity);
		Vector3 localAngularVel = transform.InverseTransformDirection(Rigidbody.angularVelocity);

		// Local pitch velocity (X) is positive when pitching down.
		// Centripetal acceleration = v * omega.
		float centripetalAccel = localVelocity.z * localAngularVel.x;

		// Express in G (Always relative to Earth G)
		float verticalG = centripetalAccel / -9.81f;

		// Add the planet's gravity in. When the up is facing directly up, then the full
		// force of gravity will be felt in the vertical.
		verticalG += transform.up.y * (Physics.gravity.y / -9.81f);

		return verticalG;
	}

	private float guiTimer = 0f;
	private string guiSpeed;
	private string guiThrottle;
	private string guiG;

	private void OnGUI()
	{
		const float msToKnots = 1.94384f;

		if (Time.time > guiTimer)
		{
			guiSpeed = string.Format("Speed: {0:0.0} knots", Rigidbody.velocity.magnitude * msToKnots);
			guiThrottle = string.Format("Throttle: {0:0.0}%", throttle * 100.0f);
			guiG = string.Format("G Load: {0:0.0} G", CalculatePitchG());

			guiTimer = Time.time + 0.2f;
		}

		GUI.Label(new Rect(10, 40, 300, 20), guiSpeed);
		GUI.Label(new Rect(10, 60, 300, 20), guiThrottle);
		GUI.Label(new Rect(10, 80, 300, 20), guiG);
	}
}
