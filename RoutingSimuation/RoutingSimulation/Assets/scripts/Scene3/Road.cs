using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road
{

	public Vector3 End_A;
	public Vector3 End_B;
	public float Length;
	public float RoadWidth;
	public bool IsTwoWay;
	public float TrafficLoad;
	public float RoadRisk;
	public float TrafficLights;

	public GameObject path;

	public Road (Vector3 A, Vector3 B, GameObject path = null)
	{
		this.End_A = A;
		this.End_B = B;
		this.path = path;
		if (path != null) {
			float dx = A.x - B.x;
			float dy = A.z - B.z;
			float dz = A.y - B.y;
			Length = Mathf.Sqrt (dx * dx + dy * dy + dz * dz);
			RoadWidth = 2;
			IsTwoWay = true;
			TrafficLoad = UnityEngine.Random.Range (0.0f, 0.9f); // 0 = no Traffic - 5 means very heavy traffic.
			TrafficLoad = (Mathf.Round (TrafficLoad * 10)) / 10.0f; // 10 possible levels of traffic data.
			RoadRisk = 0; // 0 = no road risk.
			TrafficLights = UnityEngine.Random.Range (0, 3); 

			this.path.GetComponent<Renderer> ().material.SetColor ("_Color", new Color (TrafficLoad, 1.0f - TrafficLoad, 0));
		}

	}
		
}
