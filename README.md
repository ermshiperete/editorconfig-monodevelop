# EditorConfig - MonoDevelop addin

> [EditorConfig](http://editorconfig.org) helps developers define and maintain consistent coding styles between different editors and IDEs. The EditorConfig project consists of a file format for defining coding styles and a collection of text editor plugins that enable editors to read the file format and adhere to defined styles. EditorConfig files are easily readibly and they work nicely with version control systems.

This addin maps settings from the EditorConfig file to the solution settings of MonoDevelop.

## Installation

Install with MonoDevelop's Addin Manager (category "IDE extensions").

## What it does

When opening a solution the addin reads the `.editconfig` files starting in the solution
folder and maps the settings to the solution's MonoDevelop settings. It maps settings for C#
source code (*.cs) and Text files.

### Limitations

- Since the addin works at the solution level it won't change any project level settings. I.e.
if a project (*.csproj file) defines it's own settings they won't be changed. Also, if there
are additional `.editconfig` files in subdirectories they will be ignored.

- While the `.editconfig` file allows to define filepath globs for many different files, only
the settings for `*.cs` (or rather: `FileDoesntHaveToExist.cs`) and text files
(`FileDoesntHaveToExist`) will be mapped.

## Supported Properties

The following properties are supported. Leave the property unset to use the default MonoDevelop
setting.

- **`indent_style`**: set to "tab" or "space" to use hard tabs or soft tabs respectively. Maps
to *"Convert tabs to spaces"* in MonoDevelop.
- **`indent_size`**: a whole number defining the number of columns used for each indentation
level and the width of soft tabs (when supported). When set to "tab", the value of **tab_width**
(if specified) will be used. Maps to *"Indent Width"* in MonoDevelop.
- **`tab_width`**: a whole number defining the number of columns used to represent a tab
character. This defaults to the value of indent_size and doesn't usually need to be specified.
Maps to *"Tab Width"* in MonoDevelop.
- **`end_of_line`**: set to "lf", "cr", or "crlf" to control how line breaks are represented.
Leave unset to use MonoDevelop default settings. Maps to *"Line Ending"* in MonoDevelop.
- **`trim_trailing_whitespace`**: set to "true" to remove any whitespace characters preceding
newline characters and "false" to ensure it doesn't. Maps to *"Remove trailing whitespace"* in
MonoDevelop.
- **`max_line_length`**: a whole number defining the maximum line length. MonoDevelop will
display a ruler line in that column. Leave unset to use MonoDevelop default settings. Maps to
*"Desired file width"* in MonoDevelop.
