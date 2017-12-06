using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

class SimController : MonoBehaviour
{
	public float boundry = 50;
	public int nodeGrid = 3;
	public int wayCount = 50;
	public int carCount = 5;

	public static float sensitvityRadius = 0.2f;

	public Material nodeMat = null;
	public Material groundMat = null;
	public GameObject roadPrefab = null;
	public Button resetButton = null;

	public static bool isMapGenerated = false;
	public static List<Node> nodeList = new List<Node> ();
	public static List<Road> roadList = new List<Road> ();


	static int minimumNode = 9;
	static int maxHeight = 0;
	static float gridOffset = 35.0f;
	// Pecentage of grid Values to be considered
	int nodeCount = 0;


	public Button btnEuclidean = null;
	public Button btnAntColony = null;
	public Button btnMPAC = null;
	public Slider sliderAntSpeed = null;
	public Slider sliderPheremoneEvaporate = null;
	public Button btnRefresh = null;
	public Toggle toggleHideAnts = null;

	public GameObject SelectedCarContainer = null;
	public GameObject SelectedCarEntry = null;




	/// <summary>
	///  The three algorithms for testing and verfication of the code..
	/// 1. Euclidean Distance.
	/// 2. Ant Colony
	/// 3. MPAC
	/// </summary>

	UserSelector userSelector = null;
	CarGenerator carGenerator = null;
	Euclidean algoEuclidean = null;
	AntColonyController algoAntColony = null;
	MPAC algoMPAC = null;


	public static bool  isRiding = false;



	//GameObject EuclideanCar = null;
	//GameObject AntCar = null;
	//GameObject MPACCar = null;


	// Use this for initialization
	void Start ()
	{
		nodeCount = nodeGrid * nodeGrid;
		algoEuclidean = gameObject.GetComponent<Euclidean> ();
		algoAntColony = gameObject.GetComponent<AntColonyController> ();
		algoMPAC = gameObject.GetComponent<MPAC> ();

		userSelector = gameObject.GetComponent<UserSelector> ();
		carGenerator = gameObject.GetComponent<CarGenerator> ();
		// create the under laying surface
		GameObject surface;
		surface = GameObject.CreatePrimitive (PrimitiveType.Quad);
		surface.transform.Rotate (new Vector3 (90, 0, 0));
		surface.transform.localScale = new Vector3 (2 * boundry, 2 * boundry, 1);
		surface.GetComponent<MeshRenderer> ().material = groundMat;
		GenerateGraph ();
		isMapGenerated = true;

		resetButton.onClick.AddListener (onClickReset);


		btnEuclidean.onClick.AddListener (onClickEuclidean);
		btnAntColony.onClick.AddListener (onClickAntColony);
		btnMPAC.onClick.AddListener (onClickMPAC);
		sliderAntSpeed.onValueChanged.AddListener (onSliderValueChange);
		sliderPheremoneEvaporate.onValueChanged.AddListener (onPheremoneSliderValueChange);
		btnRefresh.onClick.AddListener (onRefreshButton);
		toggleHideAnts.onValueChanged.AddListener ((value) =>{onToggleHideAnts();});


		carGenerator.Generate (carCount);
	}

	void GenerateGraph ()
	{
		// create a list of random nodes.
		nodeList.Clear ();
		if (nodeCount < minimumNode) {
			nodeCount = minimumNode;
		}

		int row = nodeGrid;
		float gridWidth = (2 * boundry / row); 
// create the node grid on the map
		for (int i = 0; i < row; i++) {
			for (int j = 0; j < row; j++) {
				float x = UnityEngine.Random.Range (-boundry + gridWidth * i + 1 + gridWidth * (gridOffset / 100), -boundry + gridWidth * (i + 1) - gridWidth * (gridOffset / 100));
				float z = UnityEngine.Random.Range (-boundry + gridWidth * j + 1 + gridWidth * (gridOffset / 100), -boundry + gridWidth * (j + 1) - gridWidth * (gridOffset / 100));
				//float x = -boundry + (2*boundry/row)*i + 1 + gridWidth*(gridOffset/100);
				//float z = -boundry + (2 * boundry / row) * j + 1+ gridWidth*(gridOffset/100);;
				x = Mathf.Round (x * 1000.0f) / 1000.0f;
				z = Mathf.Round (z * 1000.0f) / 1000.0f;

				GameObject go_node = GameObject.CreatePrimitive (PrimitiveType.Sphere);
				go_node.transform.position = new Vector3 (x, UnityEngine.Random.Range (1, maxHeight), z); /// nodes to be always on ground level.
				go_node.GetComponent<MeshRenderer> ().material = nodeMat;
				go_node.transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				go_node.tag = "Grid";

				Node node = new Node (go_node.transform.position);
				nodeList.Add (node);
			}
		}

// create the ways
		if (wayCount < nodeCount) {
			wayCount = nodeCount;
		}

		// create random connections.

		for (int i = 1; i <= wayCount; i++) {
		
			int start = 0;
			int end = -1000;
			start = i % (nodeList.Count);
			// find the corresponding border around the selected node assign to end.
			bool isValid = false;
			int counter = 0;
			while (!isValid) {
				isValid = false;
				end = getEndNode (start);
				if (end >= 0 && end < nodeList.Count) {
					isValid = isValidity (start, end);
				}
				counter = counter + 1;
				if (counter >= 1000)
					break;				
			}
			if (counter < 1000) {

				Vector3 point1 = nodeList [start].location;
				Vector3 point2 = nodeList [end].location;
		
				if (point1 == point2) {
						
				} else {
					nodeList [start].ways.Add (point2);
					nodeList [end].ways.Add (point1);

					GameObject path = DrawLine (point1, point2);
					Road road = new Road (point1, point2, path);
					roadList.Add (road);

				}
			} 
		}

	}

	bool isValidity (int start, int end)
	{
		bool valid = true;
		foreach (Vector3 point in nodeList[start].ways) {
			if (point == nodeList [end].location) {
				valid = false;
			}
		}
		return valid;
	}

	int getEndNode (int start)
	{
		int neighbour = UnityEngine.Random.Range (1, 8);

		int end = -100;
		switch (neighbour) {
		case 1:
			end = start - nodeGrid;
			break;
		case 2:
			if (start / nodeGrid == (start - nodeGrid + 1) / nodeGrid)
				end = start - 1;
			else
				end = start - nodeGrid + 1;
			break;
		case 3:
			if (start / nodeGrid == (start + 1) / nodeGrid)
				end = start + 1;
			else
				end = start + nodeGrid;
			break;
		case 4:
			if (start / nodeGrid + 1 == (start + 1 + nodeGrid) / nodeGrid)
				end = start + 1 + nodeGrid;
			else
				end = start + nodeGrid - 1;
			break;
		case 5:
			end = start + nodeGrid;
			break;
		case 6:
			if (start / nodeGrid == (start + nodeGrid - 1) / nodeGrid)
				end = start + 1;
			else
				end = start + nodeGrid - 1;
			break;
		case 7:
			if (start / nodeGrid == (start - 1) / nodeGrid)
				end = start - 1;
			else
				end = start + 1;
			break;
		case 8:
			end = start - nodeGrid - 1;
			break;
		}
		return end;
	}

	GameObject DrawLine (Vector3 start, Vector3 end)
	{
		float dx = start.x - end.x;
		float dy = start.z - end.z;
		float dz = start.y - end.y;
		float len = Mathf.Sqrt (dx * dx + dy * dy);
		float theta = (float)((Mathf.Atan2 (dx, dy) / Math.PI) * 180f); 

		float elevation = (float)((Mathf.Atan2 (end.y - start.y, len) / Math.PI) * 180f); 



		//compute the angle of road
		GameObject patch = Instantiate (roadPrefab, new Vector3 (
			                   (start.x + end.x) / 2, 
			                   (start.y + end.y) / 2,
			                   (start.z + end.z) / 2),
			                   Quaternion.Euler (90 + elevation, theta, 0));
		patch.transform.localScale = new Vector3 (2f, Mathf.Sqrt (dx * dx + dy * dy + dz * dz), 1);
		patch.tag = "Grid";
		return patch;
	}

	void DestroyGraph ()
	{
		foreach (GameObject go in GameObject.FindGameObjectsWithTag ("Grid")) {
			Destroy (go);
		}

		carGenerator.DestroyCars ();
	}

	void onClickReset ()
	{
		Refresh ();
		UserSelector.currentRegionRadius = userSelector.regionRadius;
		isRiding = false;
		UserSelector.selectedNode = null;
		DestroyGraph ();
		nodeList.Clear ();
		GenerateGraph ();
		carGenerator.Generate (carCount);
		algoAntColony.KillAntColonies ();
		DestroyPheremones ();
	}

	void DestroyPheremones ()
	{
		GameObject[] pheremones = GameObject.FindGameObjectsWithTag ("Pheremone");
		foreach (GameObject p in pheremones) {
			GameObject.Destroy (p);
		}
	}

	public static void onClickRide ()
	{
		if (UserSelector.selectedNode != null) {
			// to stop the cars.. set the speed of each car to zero.
			CarGenerator.stopCars ();
			isRiding = true;
		}
	}



	void onClickEuclidean ()
	{
		
		if (isRiding) {	
			Refresh ();
			disableButtons ();
			carGenerator.RefreshCarColor ();
			List<GameObject> selectedCars = algoEuclidean.GetCarsInRadius (carGenerator.getCarList (), userSelector.regionRadius, UserSelector.selectedNode);
			GameObject EuclideanCar = algoEuclidean.SelectNearestCar (selectedCars, UserSelector.selectedNode);
		}
	}

	void onClickAntColony ()
	{

		if (isRiding) {	
			Refresh ();
			disableButtons ();
			carGenerator.RefreshCarColor ();
			List<GameObject> selectedCars = algoAntColony.GetCarsInRadius (carGenerator.getCarList (), userSelector.regionRadius, UserSelector.selectedNode);			
			algoAntColony.startAntColony (selectedCars, UserSelector.selectedNode);

		}
	}

	void onClickMPAC ()
	{
		if (isRiding) {	
			Refresh ();
			disableButtons ();
			carGenerator.RefreshCarColor ();
			algoMPAC.startMPAC ();
		}
	}



	public static float getRoadTraffic(Vector3 Point1, Vector3 Point2){
		foreach( Road road in roadList){
			if ((Point1 == road.End_A || Point1 == road.End_B) && (Point2 == road.End_A || Point2 == road.End_B)) {
				return road.TrafficLoad;
			}
		}
		return 0.0f;
	}

	void onSliderValueChange (float arg0)
	{
		AntController.speed = sliderAntSpeed.value;
		MPACAntController.speed = sliderAntSpeed.value;
	}

	void onPheremoneSliderValueChange(float arg0){
		Pheremone.evaporationRate = (sliderPheremoneEvaporate.maxValue + 1 -  sliderPheremoneEvaporate.value);
	}

	void killAnts(){
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("Ant");

		for(var i = 0 ; i < gameObjects.Length ; i ++)
		{
			Destroy(gameObjects[i]);
		}
	}

	void Refresh(){
		killAnts ();
		clearSelectedCarList ();
		carGenerator.RefreshCarColor ();
		algoAntColony.KillAntColonies ();
		btnAntColony.interactable = true;
		btnEuclidean.interactable = true;
		btnMPAC.interactable = true;
		transform.GetComponent<UserSelector>().resetNode ();
		DestroyPheremones ();
		MPAC.clearMapPheremones ();

	}

	void disableButtons ()
	{
		btnAntColony.interactable = false;
		btnEuclidean.interactable = false;
		btnMPAC.interactable = false;
	}

	void onRefreshButton ()
	{
		Refresh ();
	}

	void clearSelectedCarList(){
		foreach( Transform child in SelectedCarContainer.transform) {
			Destroy (child.gameObject);
		}
	}

	void onToggleHideAnts(){
		GameObject[] ants = GameObject.FindGameObjectsWithTag ("Ant");

		foreach (GameObject ant in ants) {
			if(ant.GetComponent<Renderer>() != null)
				ant.GetComponent<Renderer> ().enabled = !ant.GetComponent<Renderer> ().enabled;			
		}

	}
}
