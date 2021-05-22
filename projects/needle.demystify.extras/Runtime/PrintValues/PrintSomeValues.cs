using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintSomeValues : MonoBehaviour
{
	public bool LogUngroupedMessage;
	public float TimeInterval;

	private float lastTime = -10000;
	
	private void Update()
	{
		if (TimeInterval > 0 && Time.time - lastTime < TimeInterval) return;
		lastTime = Time.time;
		
		if (LogUngroupedMessage)
			Debug.Log("This is not grouped " + Time.time);
		Debug.Log("<group> Random Value: " + Random.value.ToString("0.00")); // + ", " + Random.insideUnitSphere);
		Debug.Log("<group> Bit more complex: " + Random.value.ToString("0.00") + ", " + Random.insideUnitSphere);
		Debug.Log("<group> Frame: " + Time.frameCount + ", " + Time.time + ", " + Random.rotation);
	}
}