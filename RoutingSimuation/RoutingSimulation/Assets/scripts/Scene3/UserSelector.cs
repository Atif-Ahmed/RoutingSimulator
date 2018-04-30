using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class UserSelector : MonoBehaviour {

	public Material selectedMat = null;
	public Material unselectedMat = null;
	public GameObject selectionRegion = null;
	public int regionRadius = 2;
	public static float currentRegionRadius =2.0f;

	public static GameObject selectedNode = null;

	[HideInInspector]
	public static bool Gender = false; // Men = false;  Female = true;
	[HideInInspector]
	public static int AgeGroup = 2; //1: less than 30
	[HideInInspector]
	public static bool GenderPref = false;
	[HideInInspector]
	public static bool AgePref = false;


	void Update()
	{
		if (Input.GetMouseButtonUp (0)) { // << use GetMouseButton instead of GetMouseButtonDown
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, 1000.0f)) {
				
				if (hit.transform.name == "Sphere") {
					if (selectedNode != null) {
						selectedNode.GetComponent<MeshRenderer> ().material = unselectedMat;
						selectedNode.transform.localScale = Vector3.one;
						foreach (Transform child in selectedNode.transform) {
							GameObject.Destroy(child.gameObject);
						}
						currentRegionRadius = 2;
					}
					selectedNode = hit.transform.gameObject;
					selectedNode.GetComponent<MeshRenderer> ().material = selectedMat;
					selectedNode.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
					GameObject region = Instantiate (selectionRegion, selectedNode.transform);
					float scaleFactor = selectedNode.transform.localScale.x;
					region.transform.localScale = new Vector3 (regionRadius/scaleFactor,1, regionRadius/scaleFactor);
					region.transform.position = new Vector3 (region.transform.position.x, region.transform.position.y+0.1f,region.transform.position.z);
					SimController.onClickRide ();
				}
			}
		}
	}

	public int DrawRadius(float radius){
		if (selectedNode == null) {
			return 0;
		}
		if (currentRegionRadius < radius) {
			GameObject region = Instantiate (selectionRegion, selectedNode.transform);
			float scaleFactor = selectedNode.transform.localScale.x;
			region.transform.localScale = new Vector3 (radius / scaleFactor, 1, radius / scaleFactor);
			region.transform.position = new Vector3 (region.transform.position.x, region.transform.position.y + 0.1f, region.transform.position.z);
			currentRegionRadius = radius;
		}
		return 1;
	}


	public void resetNode(){
		if (selectedNode != null) {
			foreach (Transform child in selectedNode.transform) {
				GameObject.Destroy(child.gameObject);
			}
			currentRegionRadius = 2;
			GameObject region = Instantiate (selectionRegion, selectedNode.transform);
			float scaleFactor = selectedNode.transform.localScale.x;
			region.transform.localScale = new Vector3 (regionRadius/scaleFactor,1, regionRadius/scaleFactor);
			region.transform.position = new Vector3 (region.transform.position.x, region.transform.position.y+0.1f,region.transform.position.z);
		}
	}
}
