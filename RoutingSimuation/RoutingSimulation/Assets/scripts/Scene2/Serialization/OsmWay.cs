using System;
using System.Xml;
using System.Collections.Generic;

public class OsmWay : BaseOsm
{
	public ulong ID {get; private set;}

	public List<ulong> NodeIDs { get; private set;}

	public bool isBoundry { get; private set;}
	public bool isRoad = false;
	public float Height = 0.0f;
	public bool isBuilding = false;
	public int lanes = 1;
	
	public OsmWay (XmlNode node)
	{
		NodeIDs = new List<ulong> ();

		ID = GetAttribute<ulong> ("id", node.Attributes);

		XmlNodeList nds = node.SelectNodes ("nd");

		foreach (XmlNode n in nds) {
			ulong refNo = GetAttribute<ulong> ("ref", n.Attributes);
			NodeIDs.Add (refNo);

			if (NodeIDs.Count > 1) {
				isBoundry = NodeIDs [0] == NodeIDs [NodeIDs.Count - 1];
			}
			XmlNodeList tags = node.SelectNodes ("tag");
			foreach (XmlNode t in tags) {
				string key = GetAttribute<string> ("k", t.Attributes);
				if (key == "highway") {
					isRoad = true;
				}
				if (key == "building") {
					Height = 1.0f;
					isBuilding = true;
				}
				if (key == "lanes") {
					int lane = GetAttribute<int> ("v", t.Attributes);
				}
			}
		}
	}

}


