using UnityEngine;

public class PrintSomeValues : MonoBehaviour
{
	public bool EveryFrame;
	public float TimeInterval;
	public float TimeScale = 1;


	[Header("What")] public bool LogSimple;
	public bool LogSimple2;
	public bool LogComplex;
	public bool LogComplex2;
	public bool LogStrings;
	public bool LogVector3;

	private float lastTime = -10000;

	private int index;
	private string[] someStrings = new[] {"one", "two", "three"};

	private void Update()
	{
		var time = Time.time * TimeScale;
		var lt = lastTime * TimeScale;
		if (!EveryFrame)
		{
			if (TimeInterval > 0 && time - lt < TimeInterval * TimeScale)
				return;
		}

		lastTime = Time.time;

		if (LogSimple)
			Debug.Log("Log Simple: " + Mathf.Sin(time));
		if (LogSimple2)
			Debug.Log("Sine Wave: " + Mathf.Sin(time) + "; Cos Wave: " + Mathf.Cos(time));
		if (LogComplex)
			Debug.Log("Bit more complex: " + Random.value.ToString("0.00") + ", " + Random.insideUnitSphere);
		if (LogComplex2)
			Debug.Log("Frame: " + Time.frameCount + ", " + time + ", " + Random.rotation);
		if (LogStrings)
			Debug.Log("Group without value " + someStrings[index % someStrings.Length]);
		if (LogVector3)
			Debug.Log("Vector3 log: " + new Vector3(Mathf.Sin(time), Mathf.Cos(time), Random.value));
		
		index += 1;
	}
}