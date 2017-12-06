using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGenerator : MonoBehaviour {

	public GameObject carPrefab;
	public static List<GameObject> carList = new List<GameObject>();
	public Material matBody = null;
	public Material matTyre = null;

	public void Generate(int count){
		List<Node> nodeList = SimController.nodeList;
		//on car generation randomly put cars on different nodes
		for (int i = 0; i < count; i++) {
			// randomly select a node
			int index = Random.Range (0, nodeList.Count);
			Vector3 position = nodeList [index].location;

			if (count < nodeList.Count) {
				while (notValidPosition (position)) {
					index = Random.Range (0, nodeList.Count);
					position = nodeList [index].location;
				}
			}
				GameObject car = Instantiate (carPrefab, position, Quaternion.Euler (0, 0, 0));
			float carSpeed = car.GetComponent<carController_new> ().speed;
			float speedSwing = car.GetComponent<carController_new> ().speedSwing;
			car.GetComponent<carController_new> ().speed = UnityEngine.Random.Range (carSpeed - speedSwing, carSpeed + speedSwing);
				carList.Add (car);
			}

		
	}

	public void DestroyCars(){
		foreach (GameObject car in carList) {
			Destroy (car);
		}
		carList.Clear ();
	}

	bool notValidPosition(Vector3 pos){
		foreach (GameObject car in carList) {
			if (pos == car.transform.position)
				return true;
		}
		return false;
	}

	public static void stopCars ()
	{
		//set speed of all the car to zero
		foreach (GameObject car in carList) {
			car.GetComponent<carController_new> ().speed = 0;
		}
	}

	public void startCars (float carspeed)
	{
		foreach (GameObject car in carList) {
			car.GetComponent<carController_new> ().speed = carspeed;
		}

	}

	public List<GameObject> getCarList(){
		return carList;
	}

	public void RefreshCarColor(){
		foreach (GameObject car in carList) {
			foreach (Transform child in car.transform) {
				if (child.name == "Body")
					child.GetComponent<MeshRenderer> ().material = matBody;
				else
					if (child.name == "Tyre1" || child.name == "Tyre")
					child.GetComponent<MeshRenderer> ().material = matTyre;
			}
			
		}
	}
}
