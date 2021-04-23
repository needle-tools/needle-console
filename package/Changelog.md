# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).


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