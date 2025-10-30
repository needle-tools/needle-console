using System;
using UnityEngine;

namespace Editor.DemystifyTestCode
{
	public static class MyConsole
	{
		public static void Log(string message)
		{
			Debug.Log($"MyConsole {DateTime.Now:HH:mm:ss} {message}");
		}
	}
}
