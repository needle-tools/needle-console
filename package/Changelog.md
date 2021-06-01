# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

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