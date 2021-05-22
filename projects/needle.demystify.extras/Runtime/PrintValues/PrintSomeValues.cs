using UnityEngine;

public class PrintSomeValues : MonoBehaviour
{
	public bool LogUngroupedMessage;
	public float TimeInterval;

	private float lastTime = -10000;
	private int index;
	private string[] someStrings = new[] {"one", "two", "three"};
	
	private void Update()
	{
		if (TimeInterval > 0 && Time.time - lastTime < TimeInterval) return;
		lastTime = Time.time;
		
		if (LogUngroupedMessage)
			Debug.Log("This is not grouped " + Time.time);
		Debug.Log("Random Value: " + Random.value.ToString("0.00")); // + ", " + Random.insideUnitSphere);
		Debug.Log("Bit more complex: " + Random.value.ToString("0.00") + ", " + Random.insideUnitSphere);
		Debug.Log("Frame: " + Time.frameCount + ", " + Time.time + ", " + Random.rotation);
		Debug.Log("Group without value " + someStrings[index % someStrings.Length]);
		index += 1;
	}
}