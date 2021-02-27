using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DoStuffThatFails : MonoBehaviour
{
	public Object Reference;

	// Start is called before the first frame update
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (Application.isPlaying)
			Debug.Log(Reference.name);
	}
}