# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [2.4.1] - 2023-08-22
- fix: catch patching exceptions
- fix: disable patching on Apple Silicon (unsupported, see https://github.com/pardeike/Harmony/issues/424)
- fix: compiler warnings on 2023.1

## [2.4.0] - 2023-06-24
- add: namespace filter (thanks [TheXRMonk](https://github.com/needle-tools/needle-console/pull/23))

## [2.3.16] - 2022-05-04
- fix: compiler error in Unity 2021.3.24 (thanks [clandais](https://github.com/needle-tools/needle-console/pull/22))

## [2.3.15] - 2022-03-11
- change: remove AssetProcessor [issue 19](https://github.com/needle-tools/needle-console/issues/19)

## [2.3.14] - 2022-12-16
- fix: stacktrace drawing in 2020.3.38 and later

## [2.3.13] - 2022-12-15
- change: disable stacktrace highlighting while profiling
- change: enable custom console for >= 2020.3.36
- fix: stacktrace highlighting for 2022

## [2.3.12] - 2022-09-19
- fix for Unity 2022.2+ and 2023.1
- workaround for custom drawing being broken on `[2020.3.34f1,2021.1)`
- fix issues URL

## [2.3.11] - 2022-06-11
- update minimum supported version to 2020.3
- fix for Unity 2022.1.2

## [2.3.10] - 2022-03-04
- fix log double click to open file

## [2.3.9] - 2022-02-19
- remove nullable for 2019.4 compatibility
- fix some allocations in hyperlinks patch and general performance improvement
- fix prefix for classes without namespace
- fix highlighting for types without namespace 

## [2.3.9-pre.3] - 2022-02-17
- fix compilation error on Unity 2022.1

## [2.3.9-pre.2] - 2022-01-31
- fix prefix color marker when filename is not provided by Unity

## [2.3.9-pre.1] - 2022-01-04
- added prefix fix for local methods
- added prefix fallback for cache key when Unity doesn't give us file or line info

## [2.3.9-pre] - 2022-01-04
- slightly reduced log highlight brightness
- moved Demystify into runtime folder and enabled runtime usage

## [2.3.8] - 2021-11-11
- Remove duplicate info (file paths) in compiler error- and warning-logs when prefix is enabled. This moves actual information to the front and file information is already shown in prefix (and stacktrace still)

## [2.3.7] - 2021-11-10
- Recreate custom font if material is missing
- Log prefixes use global line identifier in 2021 now if file path is missing
- Log prefixes show line number now
- Adjusted selected logs colors
- Fix Ctrl+A in stacktrace to select all

## [2.3.5-pre] - 2021-10-21
- Add compiler warning filter
- Improved log prefixes for 2021.2

## [2.3.4] - 2021-10-15
- Ping file for compiler error
- Updated github issue installation link
- Demystifier plugin is now using MIT license

## [2.3.3] - 2021-07-28
- Performance improvement with many logs

## [2.3.2] - 2021-07-14
- Improve auto scrolling
- Fix shortcut ``B`` to scroll to bottom

## [2.3.2-exp] - 2021-07-12
- Changed console row colors slightly
- Capture stacktrace error in Unity code

## [2.3.1-exp] - 2021-07-01
- Add copy log functionality again
- Change filter icon to have min width (3 digits)
- Display assertion logs as error (red background)

## [2.2.1-exp] - 2021-07-01
- Syntax highlighting uses alpha
- Fix stacktrace selection being stuck sometimes after recompile
- Draw collapsed logs in filter foldout

## [2.2.0-exp] - 2021-06-28
- Add option to preserve context when collapsing logs
- Fix logs updating when user changes log line count

## [2.1.3-exp.3] - 2021-06-25
- Fix prefix using false cached value for same message
- Prefix slightly darker

## [2.1.3-exp.1] - 2021-06-19
- Fix scrolling to selected log with filters 
- Disable CodePreview by default, moved to experimental
- Settings cleanup pass
- Fix issue with Console enabled state not being serialized
- Log prefix text and color marker can now be enabled/disabled individually 

## [2.1.3-exp] - 2021-06-18
- Add ``Ping script`` context menu option to logs

## [2.1.2-exp.2] - 2021-06-15
- Fix scaled display settings breaking stacktrace drawing

## [2.1.2-exp.1] - 2021-06-11
- ``DebugEditor`` now uses ``Conditional`` attribute to eliminate impact in builds and method signature  is now just like in ``Debug``
- Add ``Href`` to ``HyperlinkCallback`` attribute to be called automatically for a specific link e.g. a method with ``[HyperlinkCallback("Test")]`` is called for ``<a href="Test">Call Test</a>``
- Add support for font asset and replace textfield with dropdown of system fonts

## [2.1.1-exp] - 2021-06-10
- Add support to specify custom log font
- Small readability tweak in log prefix

## [2.1.0-exp.3] - 2021-06-10
- Add editor only logs (won't show up in standalone):``DebugEditor.Log``, ``DebugEditor.Warning``, ``DebugEditor.LogError``.  Hyperlink path is skipped for methods that contains ``DebugEditor.Log`` so double click opens original file. These class and methods can be defined in user code as well.
- Make compiler errors background magenta similar to shader compiler errors.
- Fix auto scroll

## [2.0.0-exp.1] - 2021-06-06
- Update package url
- Update some readme
- Fix setting default theme on first installation
- Fix Console-Hyperlink in latest 2021.2 alpha
- Dont filter compiler errors

## [2.0.0-exp] - 2021-06-02
- Rename to Needle Console
- Fix IL error in 2019.4

## [1.6.0-exp.2] - 2021-06-01
- fixed compilation errors on 2020.3.10f1 and 2021.1+

## [1.6.0-exp] - 2021-05-25
- Initial experimental support for individual logs collapsing (context menu)
- Work towards custom log entry row drawing and potentially visualizing log data
- Hyperlink supports reveal in finder when link points to a directory

## [1.5.1-exp] - 2021-05-18
- Cleanup context menu
- Fix issue with presets not being found on startup and applied presets were not saved to UserSettings
- Improve regex performance of log prefixes
- Better first install log with clickable preferences link
- Added generic hyperlink type

## [1.5.0-exp] - 2021-05-14
- Refactor to save filters in UserSettings by default. Created filter group objects behave like presets and can only be applied or saved to.
- Improve console list scrolling (auto-scrolling, selection and focus of previously selected row if filters change)
- Selected row can be focused with ``F`` now, ``B`` toggles auto scrolling, ``Esc`` to clear selected row
- Name prefix improved for local function names  
- Added Time Filter
- Various minor UI fixes (fixed stacktrace separator line, moved Filter foldout button to left to be also visible in very small console layouts)
- Initial support for 2021.1

## [1.4.0-exp.2] - 2021-05-11
- Disabled automatic referencing in asmdefs and dlls
- Various UX improvements

## [1.4.0-exp.1] - 2021-05-10
- Initial support for console filtering
- Removed dependency to EditorPatching
- Add support for console hyperlinks

## [1.3.0-exp.4] - 2021-05-05
- Added color bar, currently procedurally colored using the filename
- Remove square brackets for filename and timestamp  
- Show Filename: will not skip ``MoveNext`` calls when searching filename

## [1.3.0-exp.3] - 2021-05-01
- Show Filename: Catch Exception when file name contained invalid characters
- Show Filename: Performance improvement
- Add light gray line above console stacktrace to visually better separate listview from content

## [1.3.0-exp.2] - 2021-04-30
- Show Filename: Always prefix file path from engine (when FilePath prefix is enabled) even when file does not exist. That way it will show native file paths as well if Unity logs obscure messages/internal logs
- Show Filename: Try parse method name from stacktrace
- Added some profiler markers and started optimization to reduce allocations

## [1.3.0-exp.1] - 2021-04-27
- Moved assemblies into Editor folder to avoid compilation warnings regarding renamed assemblies
- Fix issue with stacktrace utility patch skipping too many frames

## [1.3.0-exp] - 2021-04-23
- Add prefix to console entries with file name (only visual in console)
- Add Demystify settings menu to console window
- Add experimental support for automatic filtering of console entries based on selection in project or hierarchy view. Is disabled by default.

## [1.2.0-exp.7] - 2021-04-22
- First implementation of logging full Method name when Debug.Log is empty or null

## [1.2.0-exp.6] - 2021-04-20
- Rename dlls to avoid collisions
- Change Unity path syntax from ``(at`` to ``in`` 
- Add option to shorten PackageCache file paths (default is enabled, can be disabled in ``Preferences/Demystify``)

## [1.2.0-exp.5] - 2021-04-15
- Fix issue with first installation
- Fix "Enable patches" button not working when patches have been persistently disabled
- Upgrade EditorPatching reference
- Fix warning "Assembly can not compile"

## [1.2.0-exp.4] - 2021-03-11
- Fix CodePreview being shown when mouse is outside of stack-view ([Pull Request](https://github.com/needle-tools/demystify/pull/4) thanks for neon-age) 

## [1.2.0-exp.3] - 2021-03-10
- fix relative path containing encoded spaces ``%20`` which caused code preview not to work ([see issue](https://github.com/needle-tools/demystify/issues/3))

## [1.2.0-exp.2] - 2021-03-08
- first version containing theme scriptable objects
- added CodePreview patch check: check by style name if we're in the right place to draw the preview

## [1.1.0-pre.5] - 2021-03-08
- code preview popup follows mouse in y as a workaround for a bug when stacktrace was scrolled that caused a wrong popup position
- slight CodePreview ui fixes/improvements

## [1.1.0-pre.3] - 2021-03-05
- added experimental code preview feature
- added basic syntax highlighting to code preview

## [1.0.1-pre.7] - 2021-03-03
- moved demystify dll into runtime assembly
- exposed separator string to add between normal log message and stacktrace

## [1.0.1-pre.6] - 2021-03-03
- fix/improve complex syntax highlighting for generic return types
- prevent syntax highlighting being applied to message
- fixed empty lines in Editor.log stacktraces

## [1.0.1-pre.4] - 2021-01-03
- improved theme color editing
- updated EditorPatching dependency to 1.0.1
- moved first install flag to project settings
- try make debug log absolute paths relative
- add live update in console when editing theme colors
- fix Readme.md README.md.meta mismatch

## [1.0.0-preview] - 2021-28-02
- initial public release