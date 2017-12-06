using System;
using System.Xml;
using UnityEngine;

public class OsmNode : BaseOsm
{
	public ulong ID { get; private set; }
	public float latitude;
	public float longitude;


	public static implicit operator Vector3 (OsmNode node){
		return new Vector3(node.longitude, 0, node.latitude);
	}
		
	

	public OsmNode (XmlNode node)
	{
		ID = GetAttribute<ulong> ("id", node.Attributes);
		latitude = GetAttribute<float> ("lat", node.Attributes);
		longitude = GetAttribute<float> ("lon", node.Attributes);
	}


}

