using System.Collections.Generic;
using UnityEngine;

public class Slice2DLayer {
	private Slice2DLayerType layer = Slice2DLayerType.All;
	private bool[] layers = new bool[10];

	static public Slice2DLayer Create()
	{
		return(new Slice2DLayer());
	}

	public void SetLayerType(Slice2DLayerType type)
	{
		layer = type;
	}

	public void SetLayer(int id, bool value)
	{
		layers [id] = value;
	}

	public void DisableLayers()
	{
		layers = new bool[10];
	}

	public Slice2DLayerType GetLayerType()
	{
		return(layer);
	}

	public bool GetLayerState(int id)
	{
		return(layers [id]);
	}

}
