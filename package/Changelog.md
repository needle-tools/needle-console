# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).


## [1.2.0-exp.1] - 2021-03-08
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