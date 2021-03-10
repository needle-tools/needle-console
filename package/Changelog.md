# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.1.0-pre.8] - 2021-03-10
- fix relative path containing encoded spaces ``%20`` which caused code preview not to work ([see issue](https://github.com/needle-tools/demystify/issues/3))

## [1.1.0-pre.7] - 2021-03-08
- code preview popup follows mouse in y as a workaround for a bug when stacktrace was scrolled that caused a wrong popup position
- slight CodePreview ui fixes/improvements
- add another check: use style name to detect if we should draw CodePreview
- fix getting ConsoleWindow at startup/before first compile

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