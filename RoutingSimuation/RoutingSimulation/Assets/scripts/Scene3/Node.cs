using System;
using System.Collections.Generic;
using UnityEngine;


public class Node
{

	public Vector3 location;
	public List<Vector3> ways;

	public Node (Vector3 loc)
	{
		location = loc;
		ways = new List<Vector3> ();
	}

}


