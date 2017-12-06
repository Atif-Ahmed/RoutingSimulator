using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Euclidean : MonoBehaviour {


	public Material disableCarMat;
	public Material enableCarMat;
	public Material selectedCarMat;

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

	public GameObject SelectNearestCar(List<GameObject> cars, GameObject userLocation){
		// computing car with minimum distance
		GameObject finalCar = new GameObject();
		float minDistance = 1000000000.0f;
		foreach (GameObject car in cars) {			
			float distance = Vector3.Distance (userLocation.transform.position, car.transform.position);
			if (distance < minDistance) {
				minDistance = distance;
				finalCar = car;
			}
		}
		//change car material
		foreach (Transform child in finalCar.transform) {
			child.GetComponent<MeshRenderer> ().material = selectedCarMat;
		}

		DrawLine (userLocation.transform.position, finalCar.transform.position, Color.red,userLocation);

		// put the name in the lists
		GameObject entry = Instantiate(transform.GetComponent<SimController>().SelectedCarEntry,transform.GetComponent<SimController>().SelectedCarContainer.transform);
		entry.GetComponent<Image> ().color = Color.red;
		return finalCar;
	}

	GameObject DrawLine(Vector3 start, Vector3 end, Color color, GameObject userLocation)
	{
		GameObject myLine = new GameObject();
		myLine.transform.parent = userLocation.transform;
		myLine.transform.position = new Vector3(start.x, start.y+0.5f,start.z);
		myLine.AddComponent<LineRenderer>();
		LineRenderer lr = myLine.GetComponent<LineRenderer>();
		lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
		lr.startColor = color;
		lr.endColor = color;
		lr.startWidth = 0.5f;
		lr.endWidth = 0.5f;
		lr.SetPosition(0, new Vector3(start.x, start.y+0.5f,start.z));
		lr.SetPosition(1, new Vector3(end.x, end.y+0.5f,end.z));
		return myLine;
	}
}
