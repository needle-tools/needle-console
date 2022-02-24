# Needle Console
 
## License
Needle Console is [available on the Asset Store](https://assetstore.unity.com/packages/tools/utilities/needle-console-194002) for commercial use.  
Other versions are only allowed to be used non-commercially and only if you're entitled to use Unity Personal (the same restrictions apply).
 
## **Feature Overview**
- Improved stacktrace readability
- Syntax highlight stacktraces
- Log background colors by type (e.g. warning, error, compiler error)
- Console log prefixes (only visually in editor)
- Filter logs: Hide or Solo (e.g. by package, file, line, message)
- Collapse individual logs
- Ping script files from log
- Console hyperlinks
- Editor-only logs (extensible)
- Fixes to source code links

## How to Install

<details>
<summary>Add from OpenUPM <em>| via scoped registry, recommended</em></summary>

This package is available on OpenUPM: https://openupm.com/packages/com.needle.console

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
- paste `com.needle.console`
- click <kbd>Add</kbd>
</details>

<details>
<summary>Add from GitHub | <em>not recommended, no updates through PackMan</em></summary>

You can also add it directly from GitHub on Unity 2019.4+. Note that you won't be able to receive updates through Package Manager this way, you'll have to update manually.

- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/needle-tools/console.git?path=/package`
- click <kbd>Add</kbd>
</details>

After installation, by default all logs and exceptions will be demystified in the Console.<br>
Syntax highlighting will also be applied, and can be configured to your liking.<br/>
Settings can be configured under ``Edit > Preferences > Needle > Console``.


## How To Use 💡
Please open the <a href="https://github.com/needle-tools/demystify/blob/main/package/Readme.md">Package Readme</a> for more information.

## Supported Versions
Unity 2019.4 until 2021.1

## Contact ✒️
<b>[🌵 needle — tools for unity](https://needle.tools)</b> • 
[@NeedleTools](https://twitter.com/NeedleTools) • 
[@marcel_wiessler](https://twitter.com/marcel_wiessler) • 
[@hybridherbst](https://twitter.com/hybridherbst)

[![discord online](https://img.shields.io/discord/717429793926283276?label=Needle&logo=discord&style=social)](https://discord.gg/CFZDp4b)


