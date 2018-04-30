using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

class AntController : MonoBehaviour
{
	public static float speed = 1.5f;
	Node currentNode = null;
	Node targetNode = null;
	Vector3 previousLocation = Vector3.zero;
	//float speedReductionFactor = 1.0f;
	// 10 = very slow  - 100 means max speed;
	bool reachedUser = false;

	bool isCollisionState = false;
	bool atNest = false;

	List<Node> pathTrace = null;

	void Start ()
	{
		// set the current position as the position of vehicle..
		// and target same as of vehicle target.
		currentNode = transform.parent.GetComponent<AntColony>().carNode;
		targetNode = transform.parent.GetComponent<AntColony>().carTargetNode;

		pathTrace = new List<Node> ();
		Node carlocation = new Node (transform.parent.position);
		pathTrace.Add (currentNode);
		pathTrace.Add (targetNode);

	}

	void Update ()
	{
		//check if ant has reached user loacation		
		float step = speed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, targetNode.location, step);

		Vector3 targetDir = targetNode.location - transform.position;
		Vector3 newDir = Vector3.RotateTowards (transform.forward, targetDir, speed * Time.deltaTime * 2.0f, 0.0F);
		transform.rotation = Quaternion.LookRotation (newDir);

		//check if ther user is found...
		isUserFound ();

		//////// REACHED NEST - LOCATION OF CAR ////////////////
		IsAtNest ();

		//////// REACHED A NODE ////////
		// if reached node choose new path...
		IsAtNode ();

	}

	void IsAtNode ()
	{
		if (Vector3.Magnitude (transform.position - targetNode.location) < SimController.sensitvityRadius) {
			if (!isCollisionState) // avoid duplicate collision instances.
				isCollisionState = true;
			// still searching for the user... so follow the path with highest pheremone.
			if (!reachedUser) {
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


				//currentNode = targetNode;
				//targetNode = pathTrace [pathTrace.Count - 1];
				//pathTrace.RemoveAt (pathTrace.Count - 1);
			}
		} else {
			isCollisionState = false;
		}
	}

	void IsAtNest ()
	{
		//if reached nest ant should clear travel history
		if (Vector3.Magnitude (transform.position - transform.parent.position) < SimController.sensitvityRadius &&
			this.targetNode == transform.parent.GetComponent<AntColony>().carNode) {
			if (!atNest) {
				// check if the ant is return from the user...
				if (reachedUser) {
					Node carlocation = new Node (transform.parent.position);
					reachedUser = false;
					transform.GetComponentInChildren<Renderer> ().material.SetColor ("_Color", transform.parent.GetComponent<AntColony> ().colonyColor);
					pathTrace.RemoveAll (path => true);
					currentNode = transform.parent.GetComponent<AntColony>().carNode;
					targetNode = transform.parent.GetComponent<AntColony> ().carTargetNode;
					pathTrace.Add (currentNode);
					pathTrace.Add (targetNode);
				}
				atNest = true;
			}
		} else {
			atNest = false;
		}
	}

	void setTargetNode (Vector3 previousLocation)
	{
		if (currentNode.ways.Count != 0) {
			
			// have to randomize them later.
			Vector3 targetPosition = currentNode.ways [UnityEngine.Random.Range (0, currentNode.ways.Count)];

			// if there are multiple path connect give less weightage to path with lesser pheremones....
			if (currentNode.ways.Count > 1) {
				List<float> pheremoneList = new List<float> ();
				foreach (Vector3 way in currentNode.ways) {
					pheremoneList.Add (transform.parent.GetComponent<AntColony> ().getPheremoneValue (currentNode.location, way));
				}
				float Sum = 0;
				foreach (float p in pheremoneList)
					Sum = Sum + p;


				///// ***********************************************************//
				//if Sum greater then Zeo.. some paths or all have pheremone set preference accordingly.
				if (Sum > 0) {
					List<float> weightageList = new List<float> ();
					weightageList = GetPheremoneWeightedList (pheremoneList, Sum);
					float spinWheel = UnityEngine.Random.Range (0, 100);
					int wayIndex = GetSelectedWayIndex (spinWheel, weightageList);
					targetPosition = currentNode.ways [wayIndex];
					if (currentNode.location == UserSelector.selectedNode.transform.position) {
						targetPosition = previousLocation;
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
			List<Node> nodeList = transform.parent.GetComponent<AntColony> ().AntColonyNodeList;
			targetNode = nodeList.Find (node => (node.location == targetPosition));
			if (targetNode == null) {
				
			}
		} else {
			targetNode = currentNode;
		}


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

	void getCurrentNode ()
	{
		List<Node> nodeList = transform.parent.GetComponent<AntColony> ().AntColonyNodeList;
		currentNode = nodeList.Find (node => (Vector3.Magnitude (node.location - transform.position) < SimController.sensitvityRadius));
	}

	void isUserFound ()
	{
		if (Vector3.Magnitude (transform.position - UserSelector.selectedNode.transform.position) < SimController.sensitvityRadius) {
			

			if (!transform.parent.GetComponent<AntColony> ().foundUser) {
				GameObject controller = GameObject.FindGameObjectWithTag ("GameController");
				GameObject entry = Instantiate (controller.GetComponent<SimController> ().SelectedCarEntry, controller.GetComponent<SimController> ().SelectedCarContainer.transform);
				entry.GetComponent<Image> ().color = transform.parent.GetComponent<AntColony> ().colonyColor;
				transform.parent.GetComponent<AntColony> ().entry = entry;
				transform.parent.GetComponent<AntColony> ().foundUser = true;
				if (AntColonyController.isFirstCar) {
					AntColonyController.isFirstCar = false;
					AntColonyController.FirstCar = transform.GetComponentInParent<AntColony> ().car;
				}

			}
			if (reachedUser == false) {
				//update pheremone... from the food all the way to the source.....
				transform.parent.GetComponent<AntColony> ().updatePathPheremones (pathTrace);
				transform.GetComponentInChildren<Renderer> ().material.SetColor ("_Color", Color.white);
				ComputeShortestPath ();
				pathTrace.RemoveAt (pathTrace.Count - 1); // pop the location of user himself to initial the looping.
				reachedUser = true;
			}
		}
	}


	void ComputeShortestPath ()
	{
		float distance = 0f;
		for(int i = 1; i<pathTrace.Count;i ++){
			distance = distance + Vector3.Magnitude (pathTrace [i - 1].location - pathTrace [i].location);
		}
		if (distance < transform.parent.GetComponent<AntColony> ().shortestDistance) {
			transform.parent.GetComponent<AntColony> ().shortestDistance = distance;
			transform.parent.GetComponent<AntColony> ().entry.GetComponentInChildren<Text> ().text = "Score = " + ((int)distance).ToString ();
		}
	}
}


