using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using Debug = UnityEngine.Debug;

namespace Needle.Console
{
	[Serializable]
	public class NamespaceFilter : FilterBase<string>
	{
		private static Dictionary<string, int> fileNamespaceDict = new Dictionary<string, int>();

		public NamespaceFilter(ref List<FilterEntry> namespaces) : base(ref namespaces)
		{
		}

		protected override void OnChanged()
		{
			base.OnChanged();
			fileNamespaceDict.Clear();
		}

		public override string GetLabel(int index)
		{
			return this[index];
		}

		protected override (FilterResult result, int index) OnFilter(string message, int mask, int row, LogEntryInfo info)
		{
			if (Count <= 0) return (FilterResult.Keep, -1);
			
			var index = -1;
			if (Count > 0 && !fileNamespaceDict.TryGetValue(info.file, out index))
			{
				index = -1;
				if (TryGetNamespace(info.file, out string @namespace))
				{
					for (var i = 0; i < Count; i++)
					{
						var filtered = this[i];
						if (filtered == @namespace)
						{
							index = i;
							break;
						}
					}
				}

				fileNamespaceDict.Add(info.file, index);
			}

			if (index == -1) return (FilterResult.Keep, -1);
			if (IsSoloAtIndex(index)) return (FilterResult.Solo, index);
			if (IsActiveAtIndex(index)) return (FilterResult.Exclude, index);
			return (FilterResult.Keep, -1);
		}

		protected override bool MatchFilter(string entry, int index, string message, int mask, int row, LogEntryInfo info)
		{
			// custom implementation of Filter, should never be called
			return false;
		}

		internal static bool TryGetNamespace(string path, out string @namespace)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				@namespace = null;
				return false;
			}

			try
			{
				path = Path.GetFullPath(path);
			}
			catch (ArgumentException)
			{
				@namespace = null;
				return false;
			}
			
			string fileName = Path.GetFileName(path);
			
			// Uncomment to get more verbose info;
			// Debug.Log("finding namespace in " + path);
			// Get the "namespace" line and extract full namespace
			// string[] lines = File.ReadAllLines(path);
			// string namespaceLine = lines.Select(line => Regex.Match(line, "^\\s*namespace ([^;{}]*)"))
			// 	.Where(match => match.Success)
			// 	.Select(match => match.Groups[1].Value).FirstOrDefault();

			string[] scriptNameSpaces;
			
			try
			{
				scriptNameSpaces = File.ReadAllLines(path)
					.Select(line => Regex.Match(line, "^\\s*namespace ([^;{}]*)"))
					.Where(match => match.Success)
					.Select(match => match.Groups[1].Value).ToArray();
			}
			catch (Exception)
			{
				@namespace = "Failed to load file " + fileName;
				return false;
			}
				
				
			//Make sure only one namespace is found;
			if (scriptNameSpaces.Length != 1)
			{
				if (scriptNameSpaces.Length > 1)
				{
					@namespace = "Too many namespaces in; " + fileName;
					return false;
				}
				
				@namespace = "No namespaces in; " + fileName;
				return false;
			}

			// Debug.Log($"Namespace: {scriptNameSpaces[0]}");
			
			@namespace = scriptNameSpaces[0];
			return true;
		}
		
		static string GetTopLevelNamespace(Type t)
		{
			string ns = t.Namespace ?? "";
			int firstDot = ns.IndexOf('.');
			return firstDot == -1 ? ns : ns.Substring(0, firstDot);
		}


		public override void AddLogEntryContextMenuItems(GenericMenu menu, LogEntryInfo clickedLog, string preview)
		{
			if (TryGetNamespace(clickedLog.file, out var @namespace))
			{
				var str = @namespace;
				var text = "Namespace " + str;
				AddContextMenuItem_Hide(menu, HideMenuItemPrefix + text, str);
				AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + text, str);
			}
			else
			{
				AddContextMenuItem_Hide(menu, HideMenuItemPrefix + @namespace, null, false);
				AddContextMenuItem_Solo(menu, SoloMenuItemPrefix + @namespace, null, false);
			}
		}
	}
}