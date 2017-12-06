using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPAC : MonoBehaviour
{

	public static List<CarPheremonMap> carPheremoneMapsList = null;
	// list of pheremon map for each car

	public static int antCount = 50;
	// total numbers of ants for each car
	[HideInInspector]
	public Vector3 startPoint = Vector3.zero;
	// starting point of ants
	//[HideInInspector]
	//public Vector3 endPoint = Vector3.zero;			// ending point of ants

	public static int pheromoneRate;
	// Segregate pheromone every n frams TODO: change to seconds.
	public static int pheromoneEvaporationTime;
	// Time until a pheromone evaporates.
	public static float antSpeed;
	// The speed of the ants.
	public static float carryCapacity;
	// maximum item each ant can carry.


	public static Node userNode;
	public static List<Road> MPACColonyRoadList = null;
	public static List<Node> MPACColonyNodeList = null;

	public static List<GameObject> carlist = new List<GameObject> ();
	public static List<GameObject> selectedCarlist = null;


	public GameObject MPACantPrefab;
	[HideInInspector]
	public List<GameObject> antList = new List<GameObject> ();
	[HideInInspector]
	public List<GameObject> rideResourceList = new List<GameObject> ();


	public void startMPAC ()
	{
		selectedCarlist = new List<GameObject> ();
		carlist = transform.GetComponent<CarGenerator> ().getCarList ();

		startPoint = UserSelector.selectedNode.transform.position;
		Color color = Color.white;
		for (int i = 0; i < antCount; i++) {
			GameObject ant = Instantiate (MPACantPrefab, startPoint, Quaternion.Euler (transform.eulerAngles), transform);
			foreach (Transform child in ant.transform) {
				child.GetComponent<Renderer> ().material.SetColor ("_Color", color);
			}
			antList.Add (ant);
		}
		addCarToRoadAndNodeList ();
		initCarPheremoneMaps ();
	}

	void addCarToRoadAndNodeList ()
	{
		MPACColonyRoadList = new List<Road> ();
		MPACColonyNodeList = new List<Node> ();
		foreach (Road road in SimController.roadList) {
			MPACColonyRoadList.Add (road);
		}

		//// NODE LIST
		foreach (Node node in SimController.nodeList) {
			Node n = new Node (node.location);
			Vector3[] ways = (Vector3[])node.ways.ToArray ().Clone ();
			n.ways.AddRange (ways);
			MPACColonyNodeList.Add (n);
		}

		// add all the cars on the map as seperate roads...
		foreach (GameObject car in CarGenerator.carList) {
			Vector3 carTarget = car.transform.GetComponent<carController_new> ().targetNode.location;
			Vector3 carCurrent = car.transform.GetComponent<carController_new> ().currentNode.location; 
			// add the road from car to target node....
			Road r = new Road (car.transform.position, carTarget);
			float trafficLoad = SimController.getRoadTraffic (carTarget, carCurrent);
			r.TrafficLoad = trafficLoad;
			MPACColonyRoadList.Add (r);

			// add a new node for car and set only one way to the target location.
			Node carNode = new Node (car.transform.position);
			carNode.ways.Add (carTarget);
			MPACColonyNodeList.Add (carNode);
			// add a new way to car in the target node...
			int carTargetIndex = MPACColonyNodeList.FindIndex (node => node.location == carTarget);
			MPACColonyNodeList [carTargetIndex].ways.Add (car.transform.position);
		}

	}

	void initCarPheremoneMaps ()
	{
		carPheremoneMapsList = new List<CarPheremonMap> ();
		
		foreach (GameObject car in CarGenerator.carList) {
			CarPheremonMap cpm = new CarPheremonMap ();
			cpm.car = car;
			cpm.pheremoneList = new List<Pheremone> ();

			Color color = Random.ColorHSV (0, 1, 1, 1, 1, 1);

			float height = UnityEngine.Random.Range (0.05f, 0.7f);

			List<Pheremone> pheremoneList = new List<Pheremone> ();
			foreach (Road road in MPACColonyRoadList) {
				Pheremone p = new Pheremone (road.End_A, road.End_B, road, color, height);
				cpm.pheremoneList.Add (p);
			}

			carPheremoneMapsList.Add (cpm);
		}
	}

	public void updatePathPheremones (List<Node> pathTrace, Color color, GameObject car)
	{
		float TotalPheremone = 5f;

		float totalDistance = 0f;
		for(int i = 1; i<pathTrace.Count;i ++){
			totalDistance = totalDistance + Vector3.Magnitude (pathTrace [i - 1].location - pathTrace [i].location);
		}


		int index = carPheremoneMapsList.FindIndex (carPheremoneMap => carPheremoneMap.car.transform.position == car.transform.position);

		for (int i = 1; i < pathTrace.Count; i++) {
			Vector3 a = pathTrace [i - 1].location;
			Vector3 b = pathTrace [i].location;
			foreach (Pheremone p in carPheremoneMapsList[index].pheremoneList) {
				if ((p.EndA == a && p.EndB == b) || (p.EndB == a && p.EndA == b)) {
					p.C = color;
					p.addPheremone (TotalPheremone /totalDistance); ////////////<c><c><c>========= updat thsis
					p.UpdateColor ();
				}
			}
		}
	}

	public float computePathDistace (List<Node>pathTrace)
	{
		float totalDistace = 0;

		for (int i = 1; i < pathTrace.Count; i++) {
			totalDistace = totalDistace + Vector3.Magnitude (pathTrace [i].location - pathTrace [i - 1].location);	
		}
		return totalDistace;
	}

	public float getPheremoneValue (Vector3 a, Vector3 b, GameObject car)
	{

		int index = carPheremoneMapsList.FindIndex (carPheremoneMap => carPheremoneMap.car.transform.position == car.transform.position);
		foreach (Pheremone p in carPheremoneMapsList[index].pheremoneList) {
			if ((p.EndA == a && p.EndB == b) || (p.EndB == a && p.EndA == b)) {
				return p.pheremone;
			}
		}
		return 0;
	}

	void Update ()
	{
		if (carPheremoneMapsList != null) {
			foreach (CarPheremonMap carPheremoneMap in carPheremoneMapsList) {
				foreach (Pheremone p in carPheremoneMap.pheremoneList) {
					p.UpdatePheremoneState ();
				}
			}
		}
	}

	public class CarPheremonMap
	{
		public List<Pheremone> pheremoneList = null;
		public GameObject car;
		
	}

	public static float getRoadTraffic(Vector3 Point1, Vector3 Point2){
		foreach( Road road in MPACColonyRoadList){
			if ((Point1 == road.End_A || Point1 == road.End_B) && (Point2 == road.End_A || Point2 == road.End_B)) {
				return road.TrafficLoad;
			}
		}
		return 0.0f;
	}

	public static void clearMapPheremones(){
		if (carPheremoneMapsList != null) {
			carPheremoneMapsList.Clear ();
			carPheremoneMapsList = null;
		}
	}
}
