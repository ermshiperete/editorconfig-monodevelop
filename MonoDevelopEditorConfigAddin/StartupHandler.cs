using System;
using System.IO;
using System.Linq;
using System.Text;
using EditorConfig.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Projects;
using MonoDevelop.Core.Logging;

namespace MonoDevelopEditorConfigAddin
{
	public class StartupHandler : CommandHandler
	{
		private EditorConfigParser _parser;

		protected override void Run()
		{
			base.Run();

			_parser = new EditorConfigParser();
			IdeApp.Workbench.DocumentOpened += OnDocumentOpened;
		}

		private void OnDocumentOpened(object sender, DocumentEventArgs e)
		{
			ILogger logger = new ConsoleLogger();

			Solution solution = e.Document.Project.ParentSolution;
			string path = Path.Combine(e.Document.Project.BaseDirectory, e.Document.PathRelativeToProject);
			logger.Log(LogLevel.Info, $"Getting editorconfig data for path: {path}");

			var configuration = _parser.Parse(path).First();

			if (configuration == null || configuration.Properties.Count == 0)
			{
				logger.Log(LogLevel.Warn, "Found no editorconfig data, returning");
				return;
			}

			Uri uri = new Uri(path);
			var mimeType = DesktopService.GetMimeTypeForUri(uri.AbsoluteUri);

			var oldPolicy = solution.Policies.Get<TextStylePolicy>();
			var fileWidth = configuration.MaxLineLength ?? oldPolicy.FileWidth;
			var eolMarkerVal = TranslateEndOfLine(configuration.EndOfLine);
			var eolMarker = eolMarkerVal ?? oldPolicy.EolMarker;
			var indentWidthVal = GetIndentWidth(configuration);
			var indentWidth = indentWidthVal ?? oldPolicy.IndentWidth;
			var tabWidth = configuration.TabWidth ?? oldPolicy.TabWidth;
			var removeTrailingWhitespace = configuration.TrimTrailingWhitespace ?? oldPolicy.RemoveTrailingWhitespace;
			var tabsToSpaces = configuration.IndentStyle.HasValue ? (configuration.IndentStyle == IndentStyle.Space) : oldPolicy.TabsToSpaces;

			var newPolicy = new TextStylePolicy(fileWidth, indentWidth, indentWidth, tabsToSpaces, oldPolicy.NoTabsAfterNonTabs, removeTrailingWhitespace, eolMarker);
			if (newPolicy != oldPolicy)
			{
				logger.Log(LogLevel.Info, $"Setting editorconfig policy: {DescribePolicy(newPolicy)}");
				solution.Policies.Set(newPolicy, mimeType);
			}
		}

		private static EolMarker? TranslateEndOfLine(EndOfLine? eol)
		{
			if (!eol.HasValue) return null;

			switch (eol.Value)
			{
				case EndOfLine.CR: return EolMarker.Mac;
				case EndOfLine.LF: return EolMarker.Unix;
				case EndOfLine.CRLF: return EolMarker.Windows;
				default: return EolMarker.Native;
			}
		}

		private static int? GetIndentWidth(FileConfiguration config)
		{
			if (config.IndentSize == null) return null;

			if (config.IndentSize.UseTabWidth)
				return config.TabWidth;

			return config.IndentSize.NumberOfColumns;
		}

		private static string DescribePolicy(TextStylePolicy policy)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("{ ");

			builder.AppendFormat("tabWidth={0} ", policy.TabWidth);
			builder.AppendFormat("indentWidth={0} ", policy.IndentWidth);
			builder.AppendFormat("tabsToSpaces={0} ", policy.TabsToSpaces);
			builder.AppendFormat("trimWhitespace={0} ", policy.RemoveTrailingWhitespace);

			builder.Append("}");
			return builder.ToString();
		}
	}
}
