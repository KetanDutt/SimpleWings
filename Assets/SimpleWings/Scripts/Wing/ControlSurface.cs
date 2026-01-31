//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;

public class ControlSurface : MonoBehaviour
{
	[Header("Deflection")]

	[Tooltip("Deflection with max positive input."), Range(0, 90)]
	public float max = 15f;

	[Tooltip("Deflection with max negative input"), Range(0, 90)]
	public float min = 15f;

	[Tooltip("Speed of the control surface deflection.")]
	public float moveSpeed = 90f;

	[Tooltip("Requested deflection of the control surface normalized to [-1, 1]. "), Range(-1, 1)]
	public float targetDeflection = 0f;

	[Header("Speed Stiffening")]

	[Tooltip("Wing to use for deflection forces. Deflection limited based on " +
		"airspeed will not function without a reference wing.")]
	[SerializeField] private SimpleWing wing = null;

	[Tooltip("How much force the control surface can exert. The lower this is, " +
		"the more the control surface stiffens with speed.")]
	public float maxTorque = 6000f;

	private Rigidbody rigid = null;
	private float angle = 0f;

	private void Awake()
	{
		// If the wing has been referenced, then control stiffening will want to be used.
		if (wing != null)
            rigid = GetComponentInParent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		// Different angles depending on positive or negative deflection.
		float targetAngle = targetDeflection > 0f ? targetDeflection * max : targetDeflection * min;

		// How much you can deflect, depends on how much force it would take
		float sqrVelocity = (rigid != null) ? rigid.velocity.sqrMagnitude : 0f;
		if (rigid != null && wing != null && sqrVelocity > 1f)
		{
			float torqueAtMaxDeflection = sqrVelocity * wing.WingArea;
			float asinArg = maxTorque / torqueAtMaxDeflection;

			// Asin(x) where x > 1 or x < -1 is not a number.
			// If we have enough torque (asinArg >= 1), we are not limited.
			if (asinArg < 1.0f && asinArg > -1.0f)
			{
				float maxAvailableDeflection = Mathf.Asin(asinArg) * Mathf.Rad2Deg;
				targetAngle = Mathf.Clamp(targetAngle, -maxAvailableDeflection, maxAvailableDeflection);
			}
		}

		// Move the control surface.
		float oldAngle = angle;
		angle = Mathf.MoveTowards(angle, targetAngle, moveSpeed * Time.fixedDeltaTime);

		// Apply rotation
		transform.Rotate(angle - oldAngle, 0f, 0f, Space.Self);
	}

}
