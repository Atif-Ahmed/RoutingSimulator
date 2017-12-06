using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pheremone : MonoBehaviour {
	
	public float pheremone = 0.0f;
	public static float evaporationRate = 50f;

	public Vector3 EndA;
	public Vector3 EndB;
	public Road road;
	public Color C;
	public GameObject pheromonePlane;


	public Pheremone (Vector3 a, Vector3 b, Road r, Color C, float offset ){
		this.EndA = a;
		this.EndB = b;
		this.road = r;
		this.C = C;
		pheromonePlane = DrawLine (this.EndA, this.EndB, this.C, offset);
		UpdateColor ();
	}

	public void addPheremone(float amount){

		pheremone = pheremone + amount;

		//if(pheremone >= 1.0f)
		//	pheremone = 1.0f;
	}

	public void UpdatePheremoneState(){
		//decay pheremone overtime
		if (pheremone > 0.0f) {
			pheremone = pheremone - Time.deltaTime/evaporationRate;
			if (pheremone < 0)
				pheremone = 0;
		}
		UpdateColor ();
	}

	GameObject DrawLine (Vector3 start, Vector3 end, Color color, float offset)
	{
		float dx = start.x - end.x;
		float dy = start.z - end.z;
		float dz = start.y - end.y;
		float len = Mathf.Sqrt (dx * dx + dy * dy);
		float theta = (float)((Mathf.Atan2 (dx, dy) / Math.PI) * 180f); 

		float elevation = (float)((Mathf.Atan2 (end.y - start.y, len) / Math.PI) * 180f); 

		//compute the angle of road
		GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
		plane.transform.position = new Vector3 (
			(start.x + end.x) / 2, 
			(start.y + end.y) / 2+ offset,
			(start.z + end.z) / 2);
		plane.transform.rotation = Quaternion.Euler ( elevation,theta, 0);
		plane.transform.localScale = new Vector3 (0.25f/10.0f, 1.0f, Mathf.Sqrt (dx * dx + dy * dy + dz * dz)/10.0f);

		Material yourMaterial = (Material)Resources.Load("pheremone", typeof(Material));
		color.a = pheremone;
		yourMaterial.color = color;
		plane.GetComponent<Renderer> ().material = yourMaterial;
		plane.tag = "Pheremone";
		return plane;
	}

	public void UpdateColor(){
		Material material = pheromonePlane.GetComponent<Renderer> ().material;
		if (pheremone > 1.0f)
			this.C.a = 1f;
		else
			this.C.a = pheremone;
		material.color = C;
	}

	public void setPheremoneColor(Color color){
		C = color;
	}


}
