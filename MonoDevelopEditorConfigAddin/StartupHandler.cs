// Copyright (c) 2016 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.IO;
using System.Linq;
using EditorConfig.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Projects;

namespace MonoDevelopEditorConfigAddin
{
	public class StartupHandler: CommandHandler
	{
		private EditorConfigParser _parser;

		protected override void Run()
		{
			base.Run();
			IdeApp.Workspace.SolutionLoaded += OnSolutionLoaded;
		}

		private void OnSolutionLoaded (object sender, SolutionEventArgs e)
		{
			_parser = new EditorConfigParser();
			UpdatePolicy(e.Solution, "text/x-csharp", ".cs");
			UpdatePolicy(e.Solution, "text/plain", "");
		}

		void UpdatePolicy(Solution solution, string mimeType, string extension)
		{
			var configuration = _parser.Parse(
				Path.Combine(solution.BaseDirectory, "FileDoesntHaveToExist" + extension)).First();
			if (configuration == null || configuration.Properties.Count == 0)
				return;

			var mimeTypes = DesktopService.GetMimeTypeInheritanceChain(mimeType);
			var oldPolicy = solution.Policies.Get<TextStylePolicy>(mimeTypes);
			var fileWidth = configuration.MaxLineLength.HasValue ?
				configuration.MaxLineLength.Value : oldPolicy.FileWidth;
			var eolMarkerVal = TranslateEndOfLine(configuration.EndOfLine);
			var eolMarker = eolMarkerVal.HasValue ?
				eolMarkerVal.Value : oldPolicy.EolMarker;
			var indentWidthVal = GetIndentWidth(configuration);
			var indentWidth = indentWidthVal.HasValue ?
				indentWidthVal.Value : oldPolicy.IndentWidth;
			var tabWidth = configuration.TabWidth.HasValue ?
				configuration.TabWidth.Value : oldPolicy.TabWidth;
			var tabsToSpaces = configuration.IndentStyle.HasValue ?
				(configuration.IndentStyle.Value == IndentStyle.Space) : oldPolicy.TabsToSpaces;
			var removeTrailingWhitespace = configuration.TrimTrailingWhitespace.HasValue ?
				configuration.TrimTrailingWhitespace.Value : oldPolicy.RemoveTrailingWhitespace;
			var newPolicy = new TextStylePolicy(fileWidth, tabWidth, indentWidth, tabsToSpaces,
				oldPolicy.NoTabsAfterNonTabs, removeTrailingWhitespace, eolMarker);
			if (newPolicy != oldPolicy)
				solution.Policies.Set<TextStylePolicy>(newPolicy, mimeType);
		}

		private EolMarker? TranslateEndOfLine(EndOfLine? endOfLine)
		{
			if (!endOfLine.HasValue)
				return null;

			switch (endOfLine.Value)
			{
				case EndOfLine.CR:
					return EolMarker.Mac;
				case EndOfLine.CRLF:
					return EolMarker.Windows;
				case EndOfLine.LF:
					return EolMarker.Unix;
			}
			return EolMarker.Native;
		}

		private int? GetIndentWidth(FileConfiguration configuration)
		{
			if (configuration.IndentSize == null)
				return null;

			if (configuration.IndentSize.UseTabWidth)
				return configuration.TabWidth;

			return configuration.IndentSize.NumberOfColumns;
		}

	}
}

