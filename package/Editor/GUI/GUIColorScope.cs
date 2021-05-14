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
}