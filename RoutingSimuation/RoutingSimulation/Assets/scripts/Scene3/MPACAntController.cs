using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MPACAntController : MonoBehaviour
{


	public static float speed = 1.5f;
	public float speedOffset = 0.0f;
	Node currentNode = null;
	Node targetNode = null;
	Vector3 previousLocation = Vector3.zero;
	float speedReductionFactor = 1.0f;

	bool hasVisitedCar = false;
	bool atNest = false;
	bool isCollisionWithNodeStarted = false;

	List<Node> pathTrace = null;

	// Use this for initialization
	void Start ()
	{
		getCurrentNode ();
		setTargetNode (new Vector3 (-100, -100, -100));

		pathTrace = new List<Node> ();
		pathTrace.Add (currentNode);
		pathTrace.Add (targetNode);
		speedOffset = UnityEngine.Random.Range (-0.05f, 0.05f);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
		float step = (speed + speed*speedOffset) * Time.deltaTime * (1 - speedReductionFactor / 1.1f);
		transform.position = Vector3.MoveTowards (transform.position, targetNode.location, step);


		Vector3 targetDir = targetNode.location - transform.position;
		Vector3 newDir = Vector3.RotateTowards (transform.forward, targetDir, speed * Time.deltaTime * 2.0f, 0.0F);
		transform.rotation = Quaternion.LookRotation (newDir);

		// check if a car is found....
		isCarFound ();

		// if ant is at the user location.....
		isAtNest ();

		// check if the car is at node.....
		isAtNode ();




	}

	void setTargetNode (Vector3 previousLocation)
	{ 
		if (!hasVisitedCar) {
			if (currentNode.ways.Count != 0) {
				// have to randomize them later.
				Vector3 targetPosition = currentNode.ways [UnityEngine.Random.Range (0, currentNode.ways.Count)];

				// if there are multiple path connect give less weightage to where car came from..
				if (currentNode.ways.Count > 1) {
					List<float> pheremoneList = new List<float> ();
					foreach (Vector3 way in currentNode.ways) {
						float pathPheremoneSum = 0;
						foreach (GameObject car in CarGenerator.carList) {
							float thisPathPheremone = transform.parent.GetComponent<MPAC> ().getPheremoneValue (currentNode.location, way, car);
							if (thisPathPheremone >= pathPheremoneSum) {
								pathPheremoneSum = thisPathPheremone;
							}
						}
						pheremoneList.Add (pathPheremoneSum);
					}

					float Sum = 0;
					foreach (float p in pheremoneList)
						Sum = Sum + p;

					if (Sum > 0) {
						List<float> weightageList = new List<float> ();
						weightageList = GetPheremoneWeightedList (pheremoneList, Sum);
						float spinWheel = UnityEngine.Random.Range (0, 100);
						int wayIndex = GetSelectedWayIndex (spinWheel, weightageList);
						targetPosition = currentNode.ways [wayIndex];
						if (currentNode.location == UserSelector.selectedNode.transform.position) {
							spinWheel = UnityEngine.Random.Range (0, 100);
							wayIndex = GetSelectedWayIndex (spinWheel, weightageList);
							targetPosition = currentNode.ways [wayIndex];
						} else {						
							while (targetPosition == previousLocation) {
								spinWheel = UnityEngine.Random.Range (0, 100);
								wayIndex = GetSelectedWayIndex (spinWheel, weightageList);
								targetPosition = currentNode.ways [wayIndex];
							}
						}
						///// ***********************************************************//
						/// if there was no pheremone on any possible way simple choose any random node.
					} else {
						// never allow a U-turn for ant except for first node...
						while (targetPosition == previousLocation) {
							targetPosition = currentNode.ways [UnityEngine.Random.Range (0, currentNode.ways.Count)];
						}

					}				
				}
				List<Node> nodeList = MPAC.MPACColonyNodeList;
				targetNode = nodeList.Find (node => (node.location == targetPosition));
				if (targetNode == null) {
				}
			} else {
				targetNode = currentNode;
			}
		} else {
			targetNode = pathTrace [pathTrace.Count - 1];
			pathTrace.RemoveAt (pathTrace.Count - 1);
		}
		speedReductionFactor = MPAC.getRoadTraffic (targetNode.location, currentNode.location);
	}

	void getCurrentNode ()
	{
		List<Node> nodeList = MPAC.MPACColonyNodeList;
		currentNode = nodeList.Find (node => (Vector3.Magnitude (node.location - transform.position) < SimController.sensitvityRadius));
	}

	void isCarFound ()
	{
		// if an ant aready found a car no need to check any thing
		if (transform.GetChild (0).GetComponent<Renderer> ().material.GetColor ("_Color") == Color.white) {
			foreach (GameObject car in MPAC.carlist) {
				// check if at any car.....
				if (Vector3.Magnitude (car.transform.position - transform.position) <= SimController.sensitvityRadius &&
				    //this.targetNode.location == car.transform.GetComponent<carController_new> ().targetNode.location &&
					//this.currentNode.location == car.transform.position) {
					this.targetNode.location == car.transform.position) {
					Color color = new Color ();
					if (MPAC.selectedCarlist.Contains (car)) {
						//if car is already added just color the ant with same color as car.
						color = car.transform.GetChild (0).GetComponent<Renderer> ().material.GetColor ("_Color");
					} else {
						// if car is not already added in the list then add this car
						color = Random.ColorHSV (0, 1, 1, 1, 1, 1);
						MPAC.selectedCarlist.Add (car);

						GameObject controller = GameObject.FindGameObjectWithTag ("GameController");
						GameObject entry = Instantiate (controller.GetComponent<SimController> ().SelectedCarEntry, controller.GetComponent<SimController> ().SelectedCarContainer.transform);
						entry.GetComponent<Image> ().color = color;
					
						//set color of ant to some random color and same to 
						foreach (Transform child in car.transform) {
							if (child.GetComponent<Renderer> () != null)
								child.GetComponent<Renderer> ().material.SetColor ("_Color", color);
						}
					}
					if (hasVisitedCar == false) {

						transform.GetChild (0).GetComponent<Renderer> ().material.SetColor ("_Color", color);
						transform.parent.GetComponent<MPAC> ().updatePathPheremones (pathTrace, color,car);
						pathTrace.RemoveAt (pathTrace.Count - 1);
						//update the pheremones...on the road.

					}

					hasVisitedCar = true;	
				}
			}
		}
	}

	void isAtNest ()
	{
		//if reached nest ant should clear travel history
		if (Vector3.Magnitude (transform.position - UserSelector.selectedNode.transform.position) < SimController.sensitvityRadius) {
			if (!atNest) {
				// check if the ant is return from the user...
				if (hasVisitedCar) {
					transform.GetChild (0).GetComponent<Renderer> ().material.SetColor ("_Color", Color.white);

				}
				atNest = true;
				pathTrace.RemoveAll (path => true);
				pathTrace.Add (currentNode);
				pathTrace.Add (targetNode);
				hasVisitedCar = false;
			}
		} else {
			atNest = false;
		}
	}

	void isAtNode ()
	{
		
		if (Vector3.Magnitude (transform.position - targetNode.location) < SimController.sensitvityRadius) {
			if (!isCollisionWithNodeStarted) { // avoid duplicate collision instances.
				isCollisionWithNodeStarted = true;
				// still searching for the user... so follow the path with highest pheremone.
				if (!hasVisitedCar) {
					previousLocation = currentNode.location;
					Node temp = currentNode;
					getCurrentNode ();
					setTargetNode (previousLocation);
					pathTrace.Add (targetNode);
				}
				//going back to the nest... go back throught the same path taken.
				else {
					previousLocation = currentNode.location;
					Node temp = currentNode;
					getCurrentNode ();
					setTargetNode (previousLocation);
				}
			}
		} else {
			isCollisionWithNodeStarted = false;
		}
	}

	List<float> GetPheremoneWeightedList (List<float> pheremoneList, float Sum)
	{
		float defaultWeigthFactor = 2;
		List<float> weightageList = new List<float> ();	
		float defaultWeight = defaultWeigthFactor * (currentNode.ways.Count);
		// rest is based on their pheremone level.
		int counter = 0;
		foreach (Vector3 way in currentNode.ways) {
			weightageList.Add (pheremoneList [counter] / Sum * (100 - defaultWeight) + defaultWeigthFactor);
			counter++;
		}
		//weightage segmntation, 
		for (int i = 1; i < weightageList.Count; i++)
			weightageList [i] += weightageList [i - 1];

		return weightageList;
	}

	int GetSelectedWayIndex (float spinWheel, List<float> weightageList)
	{
		float previous = 0;
		for (int i = 0; i < weightageList.Count; i++) {
			if (spinWheel >= previous && spinWheel < weightageList [i])
				return i;
		}
		return -1;
	}
}
