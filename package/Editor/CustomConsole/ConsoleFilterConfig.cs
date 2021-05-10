﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Hardware;
using UnityEngine;

namespace Needle.Demystify
{
	[CreateAssetMenu(fileName = nameof(ConsoleFilterConfig), menuName = "Demystify/Console Filter")]
	public class ConsoleFilterConfig : ScriptableObject
	{
		private static readonly List<ConsoleFilterConfig> _allConfigs = new List<ConsoleFilterConfig>();

		public static IReadOnlyList<ConsoleFilterConfig> AllConfigs
		{
			get
			{
				for (var index = _allConfigs.Count - 1; index >= 0; index--)
				{
					var c = _allConfigs[index];
					if (!c) _allConfigs.RemoveAt(index);
				}

				return _allConfigs;
			}
		}

		private static string LastSelectedPath
		{
			get => EditorPrefs.GetString("ConsoleFilterConfigLastPath", Application.dataPath);
			set => EditorPrefs.SetString("ConsoleFilterConfigLastPath", value);
		}

		public static ConsoleFilterConfig CreateAsset()
		{
			var dir = LastSelectedPath;
			if (!Directory.Exists(dir) || !dir.StartsWith(Application.dataPath.Replace("\\", "/"))) dir = Application.dataPath;
			var path = EditorUtility.SaveFilePanel("Create Console Filter", dir, "Console Filter Config", "asset");
			path = path.Replace("\\", "/");
			var validPath = Path.GetFullPath(Application.dataPath + "/../").Replace("\\", "/");
			if (!path.StartsWith(validPath))
			{
				Debug.Log("Please select a path in your project " + validPath);
				return null;
			}

			LastSelectedPath = Path.GetDirectoryName(path);
			path = path.Substring(validPath.Length);
			var inst = CreateInstance<ConsoleFilterConfig>();
			AssetDatabase.CreateAsset(inst, path);
			return inst;
		}

		public bool IsActive => DemystifySettings.instance.ActiveConsoleFilterConfig == this;

		[SerializeField]
		private List<FilterBase<string>.FilterEntry> messages, files, packages;
		[SerializeField]
		private List<FilterBase<int>.FilterEntry> ids;
		[SerializeField]
		private List<FilterBase<FileLine>.FilterEntry> lines;
		
		private MessageFilter messageFilter;
		private LineFilter lineFilter;
		private FileFilter fileFilter;
		private ObjectIdFilter idFilter;
		private PackageFilter packageFilter;

		public IEnumerable<IConsoleFilter> EnumerateFilter()
		{
			yield return messageFilter;
			yield return lineFilter;
			yield return fileFilter;
			yield return idFilter;
			yield return packageFilter;
		}

		private void OnEnable()
		{
			messageFilter = new MessageFilter(messages);
			lineFilter = new LineFilter(lines);
			fileFilter = new FileFilter(files);
			idFilter = new ObjectIdFilter(ids);
			packageFilter = new PackageFilter(packages);
			
			if (!_allConfigs.Contains(this))
				_allConfigs.Add(this);

			foreach (var f in EnumerateFilter())
			{
				f.WillChange += OnFilterWillChange;
				f.HasChanged += OnFilterChanged;
			}

			if (DemystifySettings.instance.ActiveConsoleFilterConfig == this)
				Activate();
		}

		private void OnDisable()
		{
			foreach (var f in EnumerateFilter())
			{
				f.WillChange -= OnFilterWillChange;
				f.HasChanged -= OnFilterChanged;
			}
		}

		private void OnDestroy()
		{
			_allConfigs.Remove(this);
		}
		
		private void OnFilterWillChange(IConsoleFilter filter)
		{
			ConsoleFilter.RegisterUndo(this, filter.GetType().Name + " changed");
		}

		private void OnFilterChanged(IConsoleFilter filter)
		{
			EditorUtility.SetDirty(this);
			AssetDatabase.SaveAssets();
		}

		[ContextMenu(nameof(Activate))]
		public void Activate()
		{
			DemystifySettings.instance.ActiveConsoleFilterConfig = this;
			ConsoleFilter.RemoveAllFilter();
			foreach (var f in EnumerateFilter())
				ConsoleFilter.AddFilter(f);
		}


		[ContextMenu(nameof(Deactivate))]
		public void Deactivate()
		{
			if(DemystifySettings.instance.ActiveConsoleFilterConfig == this)
				DemystifySettings.instance.ActiveConsoleFilterConfig = null;
			ConsoleFilter.RemoveAllFilter();
		}

		[CustomEditor(typeof(ConsoleFilterConfig))]
		private class ConsoleFilterEditor : Editor
		{
			public override void OnInspectorGUI()
			{
				var t = (ConsoleFilterConfig) target;

				if (t.IsActive)
				{
					if (GUILayout.Button("Deactivate", GUILayout.Height(30)))
					{
						t.Deactivate();
					}
				}
				else
				{
					if (GUILayout.Button("Activate", GUILayout.Height(30)))
					{
						t.Activate();
					}
				}

				GUILayout.Space(10);
				var list = t.EnumerateFilter().ToList();
				Draw.FilterList(list);
			}
		}

		internal static void DrawHowToFilterHelpBox()
		{
			EditorGUILayout.HelpBox("You haven't selected any logs for filtering yet. Try right clicking a log in the console window. Select an option in the menu to start using console filters", MessageType.Info);
		}
	}
}