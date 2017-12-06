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
			antColonyList.Add (antColony);
		}
	}

	public void KillAntColonies(){
		// kill all the ants
		antColonyList.Clear ();
	}

}
