using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DoStuffThatFails : MonoBehaviour
{
	[HideInInspector] public Object Reference;

	// Start is called before the first frame update
	void Start()
	{
	}

	[ContextMenu(nameof(DeletePlayPref))]
	public void DeletePlayPref()
	{
		Debug.Log("done");
	}

	[ContextMenu(nameof(RunInvalidOperation))]
	void RunInvalidOperation()
	{
#if UNITY_EDITOR
#endif
		Reference = null;
		// ReSharper disable once PossibleNullReferenceException
		Debug.Log(Reference.name);
	}

	void Update()
	{
		if (Application.isPlaying)
			RunInvalidOperation();
	}
}