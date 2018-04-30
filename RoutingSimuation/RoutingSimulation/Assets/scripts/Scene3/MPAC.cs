using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPAC : MonoBehaviour
{


	float W_rw = 1f;
	// Road width
	float W_wt = 1f;
	// Waiting time
	float W_r = 1f;
	// Rating
	float W_d = 0.6f;
	// Distance
	float W_t = 1f;
	// Traffic
	float W_tl = 1f;
	// Traffic Lights
	float W_rr = 1f;
	// Road Risk
	float W_l = 1f;
	// Length of Road Patch
	float W_ugp = 0.25f;
	// Gender Preference of user
	float W_uap = 0.25f;
	// Age Preference of User


	public static List<CarPheremonMap> carPheremoneMapsList = null;
	// list of pheremon map for each car

	public static int antCount = 300;
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



	//local copy of user preferences....
	bool user_gender_pref = false;
	bool user_age_pref = false;
	bool user_gender = false;
	int user_ageGroup = 0;



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

		saveUserPrefsLocally ();
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
		for (int i = 1; i < pathTrace.Count; i++) {
			totalDistance = totalDistance + Vector3.Magnitude (pathTrace [i - 1].location - pathTrace [i].location);
		}


		int index = carPheremoneMapsList.FindIndex (carPheremoneMap => carPheremoneMap.car.transform.position == car.transform.position);

		for (int i = 1; i < pathTrace.Count; i++) {
			Vector3 a = pathTrace [i - 1].location;
			Vector3 b = pathTrace [i].location;
			foreach (Pheremone p in carPheremoneMapsList[index].pheremoneList) {
				if ((p.EndA == a && p.EndB == b) || (p.EndB == a && p.EndA == b)) {
					p.C = color;
					float currentPheremone = GetPheremone (a, b, totalDistance, TotalPheremone, car);
					p.addPheremone (currentPheremone); ////////////<c><c><c>========= updat thsis
					p.UpdateColor ();
				}
			}
		}
	}





	/// <summary>
	/// This is the core function to impliment the depositing of pheremone....
	/// </summary>
	/// <returns>The pheremone.</returns>
	/// <param name="a">First Point.</param>
	/// <param name="b">Second Point.</param>
	/// <param name="totalDistance">Total distance.</param>
	/// <param name="totalPheremone">Total pheremone.</param>
	float GetPheremone (Vector3 a, Vector3 b, float totalDistance, float totalPheremone, GameObject car)
	{
		float finalPheremone = 0f;

		// Road Parameters
		float roadWidth = 0f;
		float roadRisk = 0f;
		float roadLength = 0f;
		float roadTraffic = 0f;
		float roadTrafficLight = 0f;
	

		foreach (Road road in MPACColonyRoadList) {
			if ((road.End_A == a && road.End_B == b) || (road.End_A == b && road.End_B == a)) {
				roadWidth = road.RoadWidth;
				roadRisk = road.RoadRisk;
				roadLength = Vector3.Magnitude (a - b);
				roadTraffic = road.TrafficLoad;
				roadTrafficLight = road.TrafficLights;
			}
		}

		float weightedTotalDistance = totalDistance * W_d + W_d;

		// road Preference....
		float weightedRoadWidth = roadWidth * W_rw + W_rw;
		float weightedRoadRisk = roadRisk * W_rr + W_rr;
		float weightedRoadLength = roadLength * W_l + W_l;
		float weightedRoadTraffic = roadTraffic * W_t + W_t;
		float weightedRoadTrafficLight = roadTrafficLight * W_tl + W_tl;

		// driver preferences...
		finalPheremone = (totalPheremone * weightedRoadWidth) / (weightedTotalDistance * weightedRoadTraffic * weightedRoadTrafficLight * weightedRoadRisk * weightedRoadLength);

		if (UserSelector.AgePref) {
			//check if same age group
			if (UserSelector.AgeGroup == car.GetComponent<carController_new> ().DriverAge) {
				
			} else {
				finalPheremone = finalPheremone * W_uap;
			}
		}

		if (UserSelector.GenderPref) {
			//check if same age group
			if (UserSelector.Gender == car.GetComponent<carController_new> ().Gender) {
				
			} else {
				finalPheremone = finalPheremone * W_ugp;
			}
		}


		return finalPheremone;
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

	public static float getRoadTraffic (Vector3 Point1, Vector3 Point2)
	{
		foreach (Road road in MPACColonyRoadList) {
			if ((Point1 == road.End_A || Point1 == road.End_B) && (Point2 == road.End_A || Point2 == road.End_B)) {
				return road.TrafficLoad;
			}
		}
		return 0.0f;
	}

	public static void clearMapPheremones ()
	{
		if (carPheremoneMapsList != null) {
			carPheremoneMapsList.Clear ();
			carPheremoneMapsList = null;
		}
	}

	void saveUserPrefsLocally ()
	{
		user_ageGroup = UserSelector.AgeGroup;
		user_age_pref = UserSelector.AgePref;
		user_gender_pref = UserSelector.GenderPref;
		user_gender = UserSelector.Gender;
	}

	private static GameObject getBestCar ()
	{

		float MaxSum = 0;
		GameObject bestCar = null;
		foreach (CarPheremonMap cpm in carPheremoneMapsList) {
			float Sum = 0;
			foreach (Pheremone p in cpm.pheremoneList) {
				Sum = Sum + p.pheremone;
			}
			if (Sum > MaxSum) {
				MaxSum = Sum;
				bestCar = cpm.car;
			}
		}
		return bestCar;
	}



	private static List<Road> getRoadParams ()
	{
		float MaxSum = 0;
		CarPheremonMap bestCPM = null;
		foreach (CarPheremonMap cpm in carPheremoneMapsList) {
			float Sum = 0;
			foreach (Pheremone p in cpm.pheremoneList) {
				Sum = Sum + p.pheremone;
			}
			if (Sum > MaxSum) {
				MaxSum = Sum;
				bestCPM = cpm;
			}
		}

		// start from the car and go until the car and find all the parameters.
		List<Road> path = new List<Road>();
		carController_new carCtrl = bestCPM.car.GetComponent<carController_new> ();
		Pheremone startPheremone = bestCPM.pheremoneList.Find (
			                           p => (
				(p.EndA == bestCPM.car.transform.position && p.EndB == carCtrl.targetNode.location) ||
				(p.EndB == bestCPM.car.transform.position && p.EndA == carCtrl.targetNode.location)));
		path.Add (startPheremone.road);

		Vector3 previousLocation = bestCPM.car.transform.position;
		Vector3 currentLocation = ((previousLocation == path [0].End_A) ? path [0].End_B : path [0].End_A);

		// keep on moving until we reach the user.
		int safeexit = 0;
		while (currentLocation != UserSelector.selectedNode.transform.position) {
			safeexit++;
			if (safeexit >= 1000) {
				break;
			}

			// get current node
			Node thisNode = MPACColonyNodeList.Find(node=> node.location == currentLocation);
			Pheremone maxPheremone = null;
			float maxPheremoneFound = 0;
			foreach (Vector3 newLocation in thisNode.ways) {
				// for each path check the pheremone
				if(newLocation != previousLocation){
					Pheremone thisPheremone = bestCPM.pheremoneList.Find (p => 
						(p.EndA == newLocation && p.EndB == currentLocation)||
						(p.EndB == newLocation && p.EndA == currentLocation));
					// check for maximum pheremone
					if(thisPheremone.pheremone>= maxPheremoneFound){
						maxPheremoneFound = thisPheremone.pheremone;
						maxPheremone = thisPheremone;
					}						
				}

			}
			path.Add(maxPheremone.road);
			previousLocation = currentLocation;
			currentLocation = ((previousLocation == maxPheremone.road.End_A) ? maxPheremone.road.End_B : maxPheremone.road.End_A);

		}
		return path;
	}

	class RoadParams
	{
		float totalDistance;
		float totalTraffic;
		float trafficLights;
		float totalRoadRisk;
	}

	public static string getSavingData ()
	{
		GameObject car = getBestCar ();
		List<Road> path = getRoadParams ();

		//debugging
		float totalDistance = 0;
		float totalTraffic = 0;
		float totalTrafficLight = 0;
		float totalRoadRisk = 0;
		foreach (Road r in path) {
			totalDistance += r.Length;
			totalTraffic += r.TrafficLoad;
			totalTrafficLight += r.TrafficLights;
			totalRoadRisk += r.RoadRisk;
		}

		string saveString = "MPAC,";
		saveString = saveString + totalDistance +","; // Distance
		saveString = saveString + totalTraffic*100 + ","; // Total Traffic
		saveString = saveString + totalTrafficLight + ","; // Traffic Lights
		saveString = saveString + totalRoadRisk + ","; // Total Road Risk

		saveString = saveString + car.transform.position.x + ","; // CarLocation_X
		saveString = saveString + car.transform.position.z + ","; // CarLocation_Y
		saveString = saveString + car.GetComponent<carController_new> ().DriverAge + ","; // age
		saveString = saveString + (car.GetComponent<carController_new> ().Gender ? "Female" : "Male") + ","; // Gender
		saveString = saveString + car.GetComponent<carController_new> ().Rating + ","; // Rating
		saveString = saveString + car.GetComponent<carController_new> ().WaitingTime + ","; // Waiting Time
		saveString = saveString + car.GetComponent<carController_new> ().GenderPref + ","; // GenderPref
		saveString = saveString + car.GetComponent<carController_new> ().AgePref + ","; // AgePref

		saveString = saveString + UserSelector.selectedNode.transform.position.x + ","; // UserLocation_X
		saveString = saveString + UserSelector.selectedNode.transform.position.z + ","; // UserLocation_Y
		saveString = saveString + UserSelector.AgeGroup + ","; // Age
		saveString = saveString + (UserSelector.Gender ? "Female" : "Male") + ","; // Gender
		saveString = saveString + UserSelector.GenderPref + ","; // GenderPref
		saveString = saveString + UserSelector.AgePref + ","; // AgePref


		return saveString;
	}


}
