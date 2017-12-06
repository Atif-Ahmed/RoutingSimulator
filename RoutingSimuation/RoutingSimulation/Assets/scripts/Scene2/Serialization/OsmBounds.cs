using System;
using System.Xml;
using UnityEngine;

public class OsmBounds :BaseOsm
{
	public float MinLat{ get; private set;}
	public float MaxLat{ get; private set; }
	public float MinLon{ get; private set; }
	public float MaxLon{ get; private set; }

	public Vector3 Center{ get; private set; }

	public OsmBounds (XmlNode node)
	{
		MinLat = GetAttribute<float> ("minlat", node.Attributes);
		MaxLat = GetAttribute<float> ("maxlat", node.Attributes);
		MinLon = GetAttribute<float> ("minlon", node.Attributes);
		MaxLon = GetAttribute<float> ("maxlon", node.Attributes);

		float x = (float)(MaxLon + MinLon) / 2.0f;
		float y = (float)(MaxLat + MinLat) / 2.0f;
		Center = new Vector3(x, 0, y);
	}
}

