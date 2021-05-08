
using System;
using UnityEngine;

namespace Needle.Demystify
{
	internal readonly struct GUIColorScope : IDisposable
	{
		private readonly Color prev;
		
		public GUIColorScope(Color col)
		{
			prev = GUI.color;
			GUI.color = col;
		}
		
		public void Dispose()
		{
			GUI.color = prev;
		}
	}
	
	public static class ConsoleFilterExtensions
	{
		public static Color DisabledColor =>  new Color(.7f, .7f, .7f, .5f);

		public static Color BeforeOnGUI(this IConsoleFilter fil, bool anySolo)
		{
			var prevColor = GUI.color;
			if (anySolo && !fil.HasAnySolo()) GUI.color = DisabledColor;
			return prevColor;
		}
		
		public static void AfterOnGUI(this IConsoleFilter fil, Color prevColor)
		{
			GUI.color = prevColor;
		}
	}
}