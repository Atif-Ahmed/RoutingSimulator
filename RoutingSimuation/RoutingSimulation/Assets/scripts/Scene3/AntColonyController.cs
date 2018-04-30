using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntColonyController : MonoBehaviour {
		

	public Material disableCarMat;
	public Material enableCarMat;
	public Material selectedCarMat;

	public GameObject ants;
	public GameObject AntsColony;
	static List<GameObject> antColonyList = new List<GameObject>();
	public static bool isFirstCar = true;
	public static GameObject FirstCar = null;

	List<GameObject> GetCarListInRadius (List<GameObject> cars, float radius, GameObject userLocation)
	{
		List<GameObject> selectedCarList = new List<GameObject> ();
		//compute the distance between each car and user location
		foreach (GameObject car in cars) {
			float distance = Vector3.Distance (userLocation.transform.position, car.transform.position);
			if (distance <= ((float)radius*5)) {				
				selectedCarList.Add (car);
			}
		}
		return selectedCarList;
	}

	public List<GameObject> GetCarsInRadius(List<GameObject> cars, float radius,GameObject userLocation){
		if (userLocation == null) {
			return null;
		}
		//disable all the cars
		foreach (GameObject car in cars) {
			foreach (Transform child in car.transform) {
				if(child.GetComponent<MeshRenderer>() != null)
					child.GetComponent<MeshRenderer> ().material = disableCarMat;
			}
		}
		List<GameObject> selectedCars = GetCarListInRadius (cars, radius, userLocation);

		while (selectedCars.Count <= 0) {
			radius = 2 * radius;
			selectedCars = GetCarListInRadius (cars, radius, userLocation);

			// create radius
			gameObject.GetComponent<UserSelector> ().DrawRadius(radius);
		}

		//enable only the selected cars
		foreach (GameObject car in selectedCars) {
			foreach (Transform child in car.transform) {
				if(child.GetComponent<MeshRenderer>() != null)
					child.GetComponent<MeshRenderer> ().material = enableCarMat;
			}
		}



		return selectedCars;
	}

	public void startAntColony(List<GameObject> cars, GameObject userlocation){
		// setup antColony parameters
		AntColony.antPrefab = ants;
		// define each car as ant nest.
		foreach (GameObject car in cars) {
			//create antcolony
			GameObject  antColony = Instantiate (AntsColony, car.transform.position, Quaternion.identity,car.transform);
			antColony.GetComponent<AntColony> ().startPoint = car.transform.position;
			antColony.GetComponent<AntColony> ().endPoint = UserSelector.selectedNode.transform.position;
			antColony.GetComponent<AntColony> ().car = car;
			antColonyList.Add (antColony);
		}
	}

	public void KillAntColonies(){
		// kill all the ants
		antColonyList.Clear ();
	}



	static List<Road> getPath(GameObject car){
		// get the ant colony associated with car
		AntColony selectedAntColony = null;
		foreach (GameObject goAnt in antColonyList) { 
			AntColony ac = goAnt.GetComponent < AntColony> ();
			if (ac.car == car)
				selectedAntColony = ac;
		}
			
		//start from the car to the user.
		List<Road> path = new List<Road>();
		Pheremone startPheremone = selectedAntColony.pheremoneList.Find ( p => (
			(p.EndA == car.transform.position && p.EndB == selectedAntColony.carTargetNode.location) ||
			(p.EndB == car.transform.position && p.EndA == selectedAntColony.carTargetNode.location)));

		path.Add (startPheremone.road);
		Vector3 previousLocation = car.transform.position;
		Vector3 currentLocation = ((previousLocation == path [0].End_A) ? path [0].End_B : path [0].End_A);


		// keep on moving until we reach the user.
		int safeexit = 0;
		while (currentLocation != UserSelector.selectedNode.transform.position) {
			safeexit++;
			if (safeexit >= 1000) {
				break;
			}

			// get current node
			Node thisNode = selectedAntColony.AntColonyNodeList.Find(node=> node.location == currentLocation);
			Pheremone maxPheremone = null;
			float maxPheremoneFound = 0;
			foreach (Vector3 newLocation in thisNode.ways) {
				// for each path check the pheremone
				if(newLocation != previousLocation){
					Pheremone thisPheremone = selectedAntColony.pheremoneList.Find (p => 
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


	public static string getSavingData(){

		List<Road> path = getPath (FirstCar);

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

		string saveString = "Ant Colony,";
		saveString = saveString + totalDistance +","; // Distance
		saveString = saveString + totalTraffic*100 + ","; // Total Traffic
		saveString = saveString + totalTrafficLight + ","; // Traffic Lights
		saveString = saveString + totalRoadRisk + ","; // Total Road Risk

		saveString = saveString + FirstCar.transform.position.x +","; // CarLocation_X
		saveString = saveString + FirstCar.transform.position.z+","; // CarLocation_Y
		saveString = saveString + FirstCar.GetComponent<carController_new>().DriverAge+","; // age
		saveString = saveString + (FirstCar.GetComponent<carController_new>().Gender? "Female" :"Male")+","; // Gender
		saveString = saveString + FirstCar.GetComponent<carController_new>().Rating+","; // Rating
		saveString = saveString + FirstCar.GetComponent<carController_new>().WaitingTime+","; // Waiting Time
		saveString = saveString + FirstCar.GetComponent<carController_new>().GenderPref+","; // GenderPref
		saveString = saveString + FirstCar.GetComponent<carController_new>().AgePref+","; // AgePref

		saveString = saveString + UserSelector.selectedNode.transform.position.x+","; // UserLocation_X
		saveString = saveString + UserSelector.selectedNode.transform.position.z+","; // UserLocation_Y
		saveString = saveString + UserSelector.AgeGroup+","; // Age
		saveString = saveString + (UserSelector.Gender? "Female" :"Male")+","; // Gender
		saveString = saveString + UserSelector.GenderPref+","; // GenderPref
		saveString = saveString + UserSelector.AgePref+","; // AgePref

		return saveString;

	}
}
