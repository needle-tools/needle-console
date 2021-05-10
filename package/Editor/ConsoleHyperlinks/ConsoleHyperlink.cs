using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Needle
{
	public static class ConsoleHyperlink
	{
		public static string LinkTo(this string text, string link)
		{
			return "<a href=\"" + link + "\">" + text + "</a>";
		}
		
		/// <summary>
		/// register hyperlink clicked handler
		/// </summary>
		/// <param name="receiver">callback invoked when hyperlink clicked</param>
		/// <param name="priority">higher priority is called first</param>
		public static void RegisterClickedCallback(IHyperlinkCallbackReceiver receiver, int priority = 0)
		{
			if (receiver == null) return;
			var existing = registered.FirstOrDefault(e => e.receiver == receiver);
			if (existing.receiver != null)
			{
				existing.receiver = receiver;
				existing.priority = priority;
			}
			else
			{
				registered.Add((priority, receiver));
			}
			dirty = true;
		}

		public static void UnregisterCallback(IHyperlinkCallbackReceiver receiver)
		{
			if (receiver == null) return;
			registered.RemoveAll(e => e.receiver == receiver);
		}

		private static List<(int priority, IHyperlinkCallbackReceiver receiver)> registered = new List<(int priority, IHyperlinkCallbackReceiver receiver)>();
		private static bool dirty;

		private static void EnsureCallbacksOrdered()
		{
			if (!dirty) return;
			registered = registered.OrderBy(p => p.priority).ToList();
			dirty = false;
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			// subscribe to unity event
			var evt = typeof(EditorGUI).GetEvent("hyperLinkClicked", BindingFlags.Static | BindingFlags.NonPublic);
			if (evt != null)
			{
				var method = typeof(ConsoleHyperlink).GetMethod("OnClicked", BindingFlags.Static | BindingFlags.NonPublic);
				if (method != null)
				{
					var handler = Delegate.CreateDelegate(evt.EventHandlerType, method);
					evt.AddMethod.Invoke(null, new object[] {handler});
				}
			}
			
			// register types that implement interface
			var implementations = TypeCache.GetTypesDerivedFrom<IHyperlinkCallbackReceiver>();
			foreach (var t in implementations)
			{
				try
				{
					if (typeof(Object).IsAssignableFrom(t))
					{
						var instances = Object.FindObjectsOfType(t);
						foreach(var inst in instances)
							RegisterClickedCallback(inst as IHyperlinkCallbackReceiver); 
					}
					else
					{
						// skip bridge
						if (typeof(MethodBridge).IsAssignableFrom(t)) continue;
						
						if(t.IsClass && !t.IsAbstract && t.GetConstructors().Any(c => c.GetParameters().Length <= 0))
							RegisterClickedCallback(Activator.CreateInstance(t) as IHyperlinkCallbackReceiver); 
					}
				}
				catch(Exception e)
				{
					Debug.LogException(e);
				}
			}

			var methods = TypeCache.GetMethodsWithAttribute<HyperlinkCallback>();
			foreach (var m in methods)
			{
				if (!m.IsStatic) continue;
				var wrapper = new MethodBridge(m);
				var attribute = m.GetCustomAttribute<HyperlinkCallback>();
				RegisterClickedCallback(wrapper, attribute.Priority);
			}
		}

		private static PropertyInfo property;

		// ReSharper disable once UnusedMember.Local
		private static void OnClicked(object sender, EventArgs args)
		{
			if (property == null)
			{
				property = args.GetType().GetProperty("hyperlinkInfos", BindingFlags.Instance | BindingFlags.Public);
				if (property == null) return;
			}

			if (property.GetValue(args) is Dictionary<string, string> infos)
			{
				if (infos.TryGetValue("href", out var path))
				{
					infos.TryGetValue("line", out var line);
					EnsureCallbacksOrdered();
					for (var index = registered.Count - 1; index >= 0; index--)
					{
						var (_, receiver) = registered[index];
						if (receiver == null || (receiver is Object obj && !obj))
						{
							registered.RemoveAt(index);
							continue;
						}
						var res = receiver?.OnHyperlinkClicked(path, line) ?? false;
						if (res) break;
					}
				}
			}
		}
		

		private class MethodBridge : IHyperlinkCallbackReceiver
		{
			private readonly MethodInfo method;
			private bool pathOnly;

			public MethodBridge(MethodInfo method)
			{
				this.method = method;
				var para = this.method.GetParameters();
				pathOnly = para.Length == 1;
			}
			
			public bool OnHyperlinkClicked(string path, string line)
			{
				var res = pathOnly ?method?.Invoke(null, new object[]{path}) : method?.Invoke(null, new object[]{path, line});
				if (res is bool boolResult) return boolResult;
				return false;
			}
		}
	}
}