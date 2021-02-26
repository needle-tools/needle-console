using UnityEditor;
using UnityEngine;

namespace Demystify._Tests
{
	public class Logging
	{
		
		private static bool once = false;

		[MenuItem("Test/Log")]
		private static void Test()
		{
			once = false;
			Debug.Log("hello"); 
		}
		
	}
}