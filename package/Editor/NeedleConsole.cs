using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Needle.Console
{
	public static class NeedleConsole
	{
		/// <summary>
		/// Matches namespaces - "XX.YY.ZZ" is split into the groups "XX.YY" and "ZZ".<br/>
		/// Also handles generics inside "&lt;" and "&gt;".
		/// </summary>
		private static readonly Regex namespaceCompactRegex = new Regex(@"([^<>.\s]+(?:\.[^<>.\s]+)*)(\.)([^<>.(]+)", RegexOptions.Compiled);
		private static readonly Regex paramsRegex = new Regex(@"\((?!at)(.*?)\)", RegexOptions.Compiled);
		private static readonly Regex paramsArgumentRegex = new Regex(@"([ (])([^),]+?) (.+?)([\),])", RegexOptions.Compiled);
		private static readonly Regex paramsRefRegex = new Regex(@"\b ?ref ?\b", RegexOptions.Compiled);
		private static readonly MatchEvaluator namespaceReplacer = NamespaceReplacer;
		private static readonly MatchEvaluator paramReplacer = ParamReplacer;

		private static int namespaceLinkStartIndex;
		private static int namespaceMatchIndex;
		private static string namespaceStored;
		private static int namespaceStartIndex;

		[HyperlinkCallback(Href = "OpenNeedleConsoleSettings")]
		private static void OpenNeedleConsoleUserPreferences()
		{
			SettingsService.OpenUserPreferences("Preferences/Needle/Console");
		}
		
		[InitializeOnLoadMethod]
		private static void Init()
		{
			var projectSettings = NeedleConsoleProjectSettings.instance;
			var settings = NeedleConsoleSettings.instance;
			if (projectSettings.FirstInstall)
			{
				async void InstalledLog()
				{
					await Task.Delay(100);
					Enable();
					projectSettings.FirstInstall = false;
					projectSettings.Save();
					Debug.Log(
						$"Thanks for installing Needle Console. You can find Settings under <a href=\"OpenNeedleConsoleSettings\">Edit/Preferences/Needle/Console</a>\n" +
						$"If you discover issues please report them <a href=\"https://github.com/needle-tools/needle-console/issues\">on github</a>\n" +
						$"Also feel free to join <a href=\"https://discord.gg/CFZDp4b\">our discord</a>");
				}

				InstalledLog();
				InstallDefaultTheme();
				async void InstallDefaultTheme()
				{
					while (true)
					{
						try
						{
							if (settings.SetDefaultTheme()) break;
						}
						catch
						{
							// ignore
						}
						await Task.Delay(1_000);
					}
				}
			}

			if (settings.CurrentTheme != null)
			{
				settings.CurrentTheme.EnsureEntries();
				settings.CurrentTheme.SetActive();
			}
		}

		public static void Enable()
		{
			NeedleConsoleSettings.instance.Enabled = true;
			NeedleConsoleSettings.instance.Save();
			Patcher.ApplyPatches();
		}

		public static void Disable()
		{
			NeedleConsoleSettings.instance.Enabled = false;
			NeedleConsoleSettings.instance.Save();
			Patcher.RemovePatches();
		}

		private static readonly StringBuilder builder = new StringBuilder();
		internal static string DemystifyEndMarker = "�";

		public static void Apply(ref string stacktrace)
		{
			try
			{
				using (new ProfilerMarker("Needle Console.Apply").Auto())
				{
					if(Profiler.enabled) return;
					
					string[] lines = null;
					using (new ProfilerMarker("Split Lines").Auto())
						lines = stacktrace.Split('\n');
					var settings = NeedleConsoleSettings.instance;
					var foundPrefix = false;
					var foundEnd = false;
					foreach (var t in lines)
					{
						var line = t;
						if (line == DemystifyEndMarker)
						{
							foundEnd = true;
							builder.AppendLine();
							continue;
						}

						if (foundEnd)
						{
							builder.AppendLine(line);
							continue;
						}

						using (new ProfilerMarker("Remove Markers").Auto())
						{
							if (StacktraceMarkerUtil.IsPrefix(line))
							{
								StacktraceMarkerUtil.RemoveMarkers(ref line);
								if (!string.IsNullOrEmpty(settings.Separator))
									builder.AppendLine(settings.Separator);
								foundPrefix = true;
							}
						}

						if (foundPrefix)
						{
							if (
								settings.StacktraceNamespaceMode == NeedleConsoleSettings.StacktraceNamespace.Compact ||
								settings.StacktraceNamespaceMode == NeedleConsoleSettings.StacktraceNamespace.CompactNoReturnType)
							{
								namespaceLinkStartIndex = line.IndexOf(" (at ", StringComparison.Ordinal);
								namespaceMatchIndex = 0;
								namespaceStartIndex = -1;
								namespaceStored = "";

								line = namespaceCompactRegex.Replace(line, namespaceReplacer);

								// If the class name was stripped out along with the namespaces, add
								// it back in here.
								int methodIndexEndIndex = line.IndexOf('(');
								if (methodIndexEndIndex != -1 && line.IndexOf('.', 0, methodIndexEndIndex) == -1)
								{
									int classIndex = namespaceStored.LastIndexOf('.');
									if (classIndex != -1)
									{
										namespaceStored = namespaceStored[(classIndex + 1)..];
									}
									line = line.Insert(namespaceStartIndex, $"{namespaceStored}.");
								}

								// Remove the return  type.
								if (settings.StacktraceNamespaceMode == NeedleConsoleSettings.StacktraceNamespace.CompactNoReturnType)
								{
									if (namespaceStartIndex != -1)
									{
										line = line[namespaceStartIndex..];
									}
								}
							}

							if (settings.StacktraceParamsMode != NeedleConsoleSettings.StacktraceParams.Full)
							{
								line = paramsRefRegex.Replace(line, "");
								line = paramsRegex.Replace(line, paramReplacer);
							}
						}

						if (foundPrefix && settings.UseSyntaxHighlighting)
							SyntaxHighlighting.AddSyntaxHighlighting(ref line);

						var l = line.Trim();
#if UNITY_6000_0_OR_NEWER
						// Indent wrapped lines.
						l = "<indent=0.75em><line-indent=-0.75em>" + l + "</line-indent></indent>";
#endif
						if (!string.IsNullOrEmpty(l))
						{
							if (!l.EndsWith("\n"))
								builder.AppendLine(l);
							else
								builder.Append(l);
						}
					}

					var res = builder.ToString();
					if (!string.IsNullOrWhiteSpace(res))
					{
						stacktrace = res;
					}

					builder.Clear();
				}
			}
			catch
				// (Exception e)
			{
				// ignore
			}
		}

		private static string NamespaceReplacer(Match match)
		{
			namespaceMatchIndex++;

			// Prevent replacing stuff in the filename link.
			if (namespaceLinkStartIndex != -1 && match.Index >= namespaceLinkStartIndex)
				return match.Value;

			// At this point there's no wa to differentiate between a generic type parameter and "Class.Method",
			// causing the class name to be stripped which we don't want.
			// Store this so that the class name can be added back at the start after all replacements have happened.
			if (namespaceMatchIndex == 1)
			{
				namespaceStartIndex = match.Index;
				namespaceStored = match.Groups[1].Value;
			}

			return match.Groups[3].Value;
		}

		private static string ParamReplacer(Match match)
		{
			return NeedleConsoleSettings.instance.StacktraceParamsMode switch
			{
				NeedleConsoleSettings.StacktraceParams.TypesOnly => paramsArgumentRegex.Replace(match.Value, "$1$2$4"),
				NeedleConsoleSettings.StacktraceParams.NamesOnly => paramsArgumentRegex.Replace(match.Value, "$1$3$4"),
				NeedleConsoleSettings.StacktraceParams.Compact => "()",
				_ => throw new ArgumentOutOfRangeException(),
			};
		}
	}
}
