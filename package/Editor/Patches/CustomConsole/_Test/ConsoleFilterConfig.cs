using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Needle.Demystify
{
	[CreateAssetMenu(fileName = nameof(ConsoleFilterConfig), menuName = "Demystify/Console Filter")]
	public class ConsoleFilterConfig : ScriptableObject
	{
		public MessageFilter messageFilter = new MessageFilter();
		public LineFilter lineFilter = new LineFilter();
		public FileFilter fileFilter = new FileFilter();
		public PackageFilter packageFilter = new PackageFilter();

		public IEnumerable<IConsoleFilter> EnumerateFilter()
		{
			yield return messageFilter;
			yield return lineFilter;
			yield return fileFilter;
			yield return packageFilter;
		}

		private void OnEnable()
		{
			Activate();
		}

		[ContextMenu(nameof(Activate))]
		public void Activate()
		{
			ConsoleFilter.RemoveAllFilter();
			foreach (var f in EnumerateFilter())
				ConsoleFilter.AddFilter(f);
		}
		

		[ContextMenu(nameof(Deactivate))]
		public void Deactivate()
		{
			ConsoleFilter.RemoveAllFilter();
		}

		[CustomEditor(typeof(ConsoleFilterConfig))]
		private class ConsoleFilterEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				var t = (ConsoleFilterConfig) target;

				if (GUILayout.Button("Activate"))
				{
					t.Activate();
				}
				if (GUILayout.Button("Deactivate"))
				{
					t.Deactivate();
				}
				
				foreach (var f in t.EnumerateFilter())
				{
					f.OnGUI();
				}
			}
		}
	}
}