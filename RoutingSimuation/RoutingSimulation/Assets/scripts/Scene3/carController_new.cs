using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carController_new : MonoBehaviour {


	public float speed = 50.0f;
	public float speedSwing = 2.0f;
	[HideInInspector]
	public Node currentNode = null;
	[HideInInspector]
	public Node targetNode = null;
	Vector3 previousLocation = Vector3.zero;
	float speedReductionFactor = 1.0f; // 10 = very slow  - 100 means max speed;


	[HideInInspector]
	public int Age = 0;
	[HideInInspector]
	public bool Gender = false; // Men = false;  Female = true;
	[HideInInspector]
	public float WaitingTime = 10000;
	[HideInInspector]
	public int Rating = 5;
	[HideInInspector]
	public float Distance = 10e10f;
	[HideInInspector]
	public bool  GenderPref = true;
	[HideInInspector]
	public bool AgePref = true;


	// Use this for initialization
	void Start () {
		// set the target position for vehicle
		getCurrentNode();
		setTargetNode();
	}

	// Update is called once per frame
	void Update () {
		if (true) {
			float step = speed * Time.deltaTime * (1 - speedReductionFactor/1.3f);
			transform.position = Vector3.MoveTowards (transform.position, targetNode.location, step);


			Vector3 targetDir = targetNode.location - transform.position;
			Vector3 newDir = Vector3.RotateTowards (transform.forward, targetDir, speed * Time.deltaTime * 2.0f, 0.0F);
			transform.rotation = Quaternion.LookRotation (newDir);


			if (Vector3.Magnitude(transform.position  - targetNode.location)< SimController.sensitvityRadius) {
				previousLocation = currentNode.location;
				getCurrentNode ();
				setTargetNode ();
			}
		}			
	}

	void setTargetNode(){
		if (currentNode.ways.Count != 0) {
			// have to randomize them later.
			Vector3 targetPosition = currentNode.ways[Random.Range(0, currentNode.ways.Count)];

			// if there are multiple path connect give less weightage to where car came from..
			if (currentNode.ways.Count > 1) {
				bool accept = false;
				while (!accept) {
					if (targetPosition == previousLocation) {
						int chance = Random.Range (0, 5);
						if (chance == 3) {
							accept = true;
						} else {
							targetPosition = currentNode.ways[Random.Range(0, currentNode.ways.Count)];
						}
					} else {
						accept = true;
					}
				}
			}
			List<Node> nodeList = SimController.nodeList;
			targetNode = nodeList.Find (node => (node.location == targetPosition));
		} else {
			targetNode = currentNode;
		}

		speedReductionFactor = SimController.getRoadTraffic (targetNode.location, currentNode.location);
	}

	void getCurrentNode(){
		List<Node> nodeList = SimController.nodeList;
		currentNode = nodeList.Find (node => (Vector3.Magnitude(node.location - transform.position)<SimController.sensitvityRadius));
	}
}
