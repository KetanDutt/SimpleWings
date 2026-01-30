//
// Copyright (c) Brian Hernandez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using UnityEngine;
using System.Collections.Generic;

public class GearVisuals : MonoBehaviour
{
	public WheelCollider[] wheels;
	public Transform wheelVisualizerPrefab;

	private struct WheelData
	{
		public Transform visual;
		public WheelCollider collider;
	}

	private List<WheelData> visualToWheelMap;

	private void Awake()
	{
		visualToWheelMap = new List<WheelData>();
	}

	// Use this for initialization
	private void Start()
	{
		if (wheelVisualizerPrefab != null)
		{
			// Create a cylinder and associate each cylinder with a wheel.
			foreach (WheelCollider wheel in wheels)
			{
				Transform visual = Instantiate(wheelVisualizerPrefab, wheel.transform);
				WheelData data = new WheelData { visual = visual, collider = wheel };
				visualToWheelMap.Add(data);
			}
		}
	}

	private void Update()
	{
		int count = visualToWheelMap.Count;
		if (count > 0)
		{
			Vector3 pos;
			Quaternion rot;

			for (int i = 0; i < count; i++)
			{
				WheelData data = visualToWheelMap[i];
				data.collider.GetWorldPose(out pos, out rot);
				data.visual.position = pos;
				data.visual.rotation = rot;
			}
		}
	}
}
