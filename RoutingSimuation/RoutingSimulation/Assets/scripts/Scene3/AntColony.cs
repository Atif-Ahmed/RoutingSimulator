using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntColony : MonoBehaviour {


	public static int antCount = 100;		// total numbers of ants for each car
	public Vector3 startPoint = Vector3.one;		// starting point of ants
	public Vector3 endPoint = Vector3.one;			// ending point of ants

	public static int pheromoneRate;						// Segregate pheromone every n frams TODO: change to seconds.
	public static int pheromoneEvaporationTime;			// Time until a pheromone evaporates.
	public static float antSpeed;							// The speed of the ants.
	public static float carryCapacity;						// maximum item each ant can carry.	



	public Color colonyColor;
	public bool foundUser = false;

	public static GameObject antPrefab;

	public List<GameObject> antList = new List<GameObject>();
	public List<Pheremone> pheremoneList = null;

	// ant colony road also have car locations as a seperate nodes on the map.....
	public List<Road> AntColonyRoadList = null;
	public List<Node> AntColonyNodeList = null;
	public Node carNode;
	public Node carTargetNode;

	public float shortestDistance=15e10f;
	public GameObject entry = null;

	void Start(){
		entry = new GameObject ();
		Color color = Random.ColorHSV (0, 1, 1, 1, 1, 1); 
		colonyColor = color;
		// color all the car and the ants........!!!!
		foreach (Transform child in transform.parent.transform) {
			if(child.GetComponent<Renderer> ()!= null)
				child.GetComponent<Renderer> ().material.SetColor ("_Color",color );
		}
		for(int i =0;i<antCount; i++){
			GameObject ant = Instantiate (antPrefab, startPoint, Quaternion.Euler(transform.eulerAngles),transform);
			foreach (Transform child in ant.transform) {
				child.GetComponent<Renderer> ().material.SetColor ("_Color",color );
			}
			antList.Add (ant);
		}
		addCarToRoadAndNodeList ();
		initPheremoneList ();
	}

	void addCarToRoadAndNodeList(){
		AntColonyRoadList = new List<Road> ();
		AntColonyNodeList = new List<Node> ();
		foreach (Road road in SimController.roadList) {
			AntColonyRoadList.Add (road);
		}
		// add all the cars on the map as seperate roads...
		Road r = new Road(transform.parent.position,transform.parent.GetComponent<carController_new> ().targetNode.location);
		AntColonyRoadList.Add (r);


		//// NODE LIST
		foreach (Node node in SimController.nodeList) {
			Node n = new Node (node.location);
			Vector3[] ways = (Vector3[])node.ways.ToArray ().Clone ();
			n.ways.AddRange(ways);
			AntColonyNodeList.Add (n);
		}

		Vector3 carTarget = transform.parent.GetComponent<carController_new> ().targetNode.location;
		carNode = new Node(transform.parent.position);
		carNode.ways.Add (carTarget);
		AntColonyNodeList.Add (carNode);
		carTargetNode = AntColonyNodeList.Find (node => (node.location == carTarget));
		carTargetNode.ways.Add (transform.parent.position);


	}

	void initPheremoneList ()
	{
		pheremoneList = new List<Pheremone> ();
		foreach (Road road in AntColonyRoadList) {
			Pheremone p = new Pheremone (road.End_A, road.End_B, road, colonyColor, UnityEngine.Random.Range(0.1f,0.6f));
			pheremoneList.Add (p);
		}
	}

	public void updatePathPheremones(List<Node> pathTrace){
		float TotalPheremone = 2.5f;

		float totalDistance = 0f;
		for(int i = 1; i<pathTrace.Count;i ++){
			totalDistance = totalDistance + Vector3.Magnitude (pathTrace [i - 1].location - pathTrace [i].location);
		}

		for (int i = 1; i < pathTrace.Count; i++) {
			Vector3 a = pathTrace [i - 1].location;
			Vector3 b = pathTrace [i].location;
			float distance = Vector3.Magnitude (b - a);
			foreach (Pheremone p in pheremoneList) {
				if((p.EndA == a && p.EndB == b) || (p.EndB == a && p.EndA == b)){

					Debug.Log ("Pheremone" + TotalPheremone/totalDistance );
					p.addPheremone (TotalPheremone / totalDistance); ////////////<c><c><c>========= updat thsis
					p.UpdateColor();
				}
			}
		}
	}

	public float computePathDistace(List<Node>pathTrace){
		float totalDistace = 0;

		for (int i = 1; i < pathTrace.Count; i++) {
			totalDistace = totalDistace + Vector3.Magnitude (pathTrace [i].location - pathTrace [i - 1].location);	
		}
		return totalDistace;
	}

	public float getPheremoneValue(Vector3 a, Vector3 b){
		foreach (Pheremone p in pheremoneList) {
			if((p.EndA == a && p.EndB == b) || (p.EndB == a && p.EndA == b)){
				return p.pheremone;
			}
		}
		return 0;
	}

	void Update(){
		foreach (Pheremone p in pheremoneList) {
			p.UpdatePheremoneState ();
		}
	}


}
