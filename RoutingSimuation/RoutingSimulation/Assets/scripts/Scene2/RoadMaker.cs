using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class RoadMaker : MonoBehaviour
{
	mapReader map;
	public Material roadMat;
	public GameObject road_prefab;
	public GameObject roadCirclePrefab;

	IEnumerator Start ()
	{
		float Scale = mapReader.Scale;
		map = GetComponent<mapReader> ();
		while (!map.isReady) {
			yield return null;
		}
		//TODO: call roads functions here

		foreach (OsmWay way in map.ways.FindAll((w)=>{return w.isRoad;})) {
			GameObject road = new GameObject ();
			for (int i = 1; i < way.NodeIDs.Count; i++) {
				OsmNode p1 = map.nodes [way.NodeIDs [i - 1]];
				OsmNode p2 = map.nodes [way.NodeIDs [i]];

				Vector3 v1 = (p1 - map.bounds.Center) * Scale;
				Vector3 v2 = (p2 - map.bounds.Center) * Scale;

				float dx = v1.x - v2.x;
				float dy = v1.z - v2.z;
				float len = Mathf.Sqrt(dx * dx + dy * dy);
				float theta = (float)((Mathf.Atan2 (dx, dy) / Math.PI) * 180f); 

				//compute the angle of road


				GameObject patch = Instantiate (road_prefab,new Vector3((v1.x + v2.x)/2,0,(v1.z+v2.z)/2),Quaternion.Euler(90,theta,0),road.transform);
				patch.transform.localScale = new Vector3 (0.1f*way.lanes, len, 1);

				GameObject circle =  Instantiate (roadCirclePrefab, v2, Quaternion.Euler(90,0,0), road.transform);
				circle.transform.localScale = new Vector3 (0.1f*way.lanes, 0.1f*way.lanes, 1);
			}
			}
			yield return null;
		}
}


