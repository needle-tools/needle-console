# Needle Console

*Better Stacktrace readability, syntax highlighting, filters and more*


## **Feature Overview**
- 🔮 Improved stacktrace readability
- 🎨 Syntax highlight stacktraces
- Filter logs: Hide or Solo (e.g. by package, file, line, message)
- Collapse individual logs
- Ping script files from log
- Editor-only logs (extensible)
- Log background colors by type (e.g. warning, error, compiler error)
- Console hyperlinks



## Quick Start ⚡️

<details>
<summary>Add from OpenUPM <em>| via scoped registry, recommended</em></summary>

This package is available on OpenUPM: https://openupm.com/packages/com.needle.demystify

To add it the package to your project:

- open `Edit/Project Settings/Package Manager`
- add a new Scoped Registry:
  ```
  Name: OpenUPM
  URL:  https://package.openupm.com/
  Scope(s): com.needle
  ```
- click <kbd>Save</kbd>
- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `com.needle.demystify`
- click <kbd>Add</kbd>
</details>

<details>
<summary>Add from GitHub | <em>not recommended, no updates through PackMan</em></summary>

You can also add it directly from GitHub on Unity 2019.4+. Note that you won't be able to receive updates through Package Manager this way, you'll have to update manually.

- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/needle-tools/demystify.git?path=/package`
- click <kbd>Add</kbd>
</details>  
<br/>
After installation, by default all logs and exceptions will be demystified in the Console.  
Syntax highlighting will also be applied, and can be configured to your liking.  

Settings can be configured under ``Edit > Preferences > Needle > Demystify``.


## Features in depth 📜

### **Demystified Stacktraces** 🔮 for any log message and exceptions.  
  It *just works*, you don't need to do anything.  
  Logs going into Editor logfiles will also be demystified.  
  Logs in builds will _not_ be demystified (by design).

>.NET stack traces output the compiler transformed methods; rather than the source code methods, which make them slow to mentally parse and match back to the source code.  
Demystify outputs the modern C# 7.0 features in stack traces in an understandable fashion that looks like the source code that generated them.

### **Syntax Highlighting** 🎨

We provide default syntax highlighting for Light and Dark Theme. You can also change colors to your liking.  
  Syntax highlighting comes with a slight performance impact when selecting a log (not when logging it).

- Adjust the highlighting colors to your liking.
- Disable specific highlight types by setting the Alpha value of the respective setting to 0.
- Dark and Light Theme support

![](Documentation~/preferences.png)


### **Log Filtering** 🔍
Log filters allow to hide specific logs in your project or only show the logs that matter for you right now. No need to change the way you write ``Debug.Log`` messages, it is all available at your fingertips via the console context menu.
- Hide logs by message, file, line, package or even time
- Solo logs: show only log entries that matter to you right now

### **Log Colors**
Log colors can be enabled or disabled to show the severity of a log. Compiler errors receive special treatment and are rendered magenta, shader programmers will feel right at home.

![](Documentation~/log-type-colors.png) 


### **Collapse individual repeating logs** 🍔
Multiple logs from the same source can be collapsed to single log and not spam your console bringing you more overview over your project.

### **Console Hyperlinks**

### **Editor Only Logs**

### **Fixes to source code links** 🧁  
  Sometimes, links to code in embedded/local packages don't work, so while we were at it we fixed that as well. (Case 1304954 for Unity people stepping by) [Issue Tracker](https://issuetracker.unity3d.com/issues/script-file-that-references-package-script-is-opened-when-double-clicking-a-console-message-in-editor-that-comes-from-a-package)

## Known Issues

- syntax highlighting has a performance impact, so if you're on a very slow machine you might want to turn it off (demystifying is highly optimized and shouldn't have a noticeable performance impact)

## Support 💚
This project is supported via donations. If you or your team have found it useful, please consider supporting further development through [patreon](https://www.patreon.com/needletools)

## Contact ✒️
<b>[🌵 needle — tools for unity](https://needle.tools)</b> • 
[@NeedleTools](https://twitter.com/NeedleTools) • 
[@marcel_wiessler](https://twitter.com/marcel_wiessler) • 
[@hybridherbst](https://twitter.com/hybridherbst)

[Demystifier by Ben Adams](https://github.com/benaadams/Ben.Demystifier)

