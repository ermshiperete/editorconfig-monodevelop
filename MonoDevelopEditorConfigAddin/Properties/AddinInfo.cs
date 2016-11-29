using System;
using Mono.Addins;
using Mono.Addins.Description;

[assembly:Addin(
	"MonoDevelopEditorConfigAddin",
	Namespace = "MonoDevelopEditorConfigAddin",
	Version = "0.1"
)]

[assembly:AddinName("EditorConfigAddin")]
[assembly:AddinCategory("IDE extensions")]
[assembly:AddinDescription(@"EditorConfig Addin for MonoDevelop

Maps the settings from .editorconfig files to the settings of MonoDevelop.")]
[assembly:AddinAuthor("Eberhard Beilharz")]
[assembly:AddinUrl("editorconfig.org")]

[assembly:AddinDependency ("::MonoDevelop.Core", MonoDevelop.BuildInfo.Version)]
[assembly:AddinDependency ("::MonoDevelop.Ide", MonoDevelop.BuildInfo.Version)]
