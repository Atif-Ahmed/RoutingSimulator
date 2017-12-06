using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class mapReader : MonoBehaviour
{
	[HideInInspector]
	public Dictionary<ulong, OsmNode> nodes;

	[HideInInspector]
	public List<OsmWay> ways;

	[HideInInspector]
	public OsmBounds bounds;

	public bool isReady= false;

	public static float Scale = 1111.0f;

	[Tooltip ("The resource file that contains the OSM map data")]
	public string resourceFile;

	// Use this for initialization
	void Start ()
	{

		nodes = new Dictionary<ulong, OsmNode> ();
		ways = new List<OsmWay> ();
		var txtAsset = Resources.Load<TextAsset> (resourceFile);
		XmlDocument doc = new XmlDocument ();
		doc.LoadXml (txtAsset.text);
		SetBounds (doc.SelectSingleNode ("/osm/bounds"));
		GetNodes (doc.SelectNodes ("/osm/node"));
		GetWays (doc.SelectNodes ("/osm/way"));
		isReady = true;

	}

	void Update ()
	{
		foreach (OsmWay w in ways) {
			
			Color c = Color.red;
			if (w.isRoad) {
				c = Color.red;
				for (int i = 1; i < w.NodeIDs.Count; i++) {
					OsmNode p1 = nodes [w.NodeIDs [i - 1]];
					OsmNode p2 = nodes [w.NodeIDs [i]];

					Vector3 v1 = (p1 - bounds.Center) * Scale;
					Vector3 v2 = (p2 - bounds.Center) * Scale;
					Debug.DrawLine (v2, v1, c);

				}
			}
//			} else if (w.isBuilding) {
//				c = Color.red;
//				for (int i = 1; i < w.NodeIDs.Count; i++) {
//					OsmNode p1 = nodes [w.NodeIDs [i - 1]];
//					OsmNode p2 = nodes [w.NodeIDs [i]];
//
//					Vector3 v1 = (p1 - bounds.Center) * Scale;
//					Vector3 v2 = (p2 - bounds.Center) * Scale;
//					Debug.DrawLine (v2, v1, c);
//
//				}
//			} else {
//				c = Color.blue;
//				for (int i = 1; i < w.NodeIDs.Count; i++) {
//					OsmNode p1 = nodes [w.NodeIDs [i - 1]];
//					OsmNode p2 = nodes [w.NodeIDs [i]];
//
//					Vector3 v1 = (p1 - bounds.Center) * Scale;
//					Vector3 v2 = (p2 - bounds.Center) * Scale;
//					Debug.DrawLine (v2, v1, c);
//
//				}
//				
//			}
		}
	}

	void GetNodes (XmlNodeList xmlNodeList)
	{
		foreach (XmlNode n in xmlNodeList) {
			OsmNode node = new OsmNode (n);
			nodes [node.ID] = node;
		}
	}

	void SetBounds (XmlNode xmlNode)
	{
		bounds = new OsmBounds (xmlNode);
	}

	void GetWays (XmlNodeList xmlNodeList)
	{
		foreach (XmlNode node in xmlNodeList) {
			OsmWay way = new OsmWay (node);
			ways.Add (way);
		}
	}




}
