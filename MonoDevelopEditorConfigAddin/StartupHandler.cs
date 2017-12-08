using System;
using System.IO;
using System.Linq;
using EditorConfig.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Projects;

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
            Solution solution = e.Document.Project.ParentSolution;
            string path = Path.Combine(e.Document.Project.BaseDirectory, e.Document.PathRelativeToProject);
            var configuration = _parser.Parse(path).First();

            if (configuration == null || configuration.Properties.Count == 0) return;

            Uri uri = new Uri(path);
            var mimeType = DesktopService.GetMimeTypeForRoslynLanguage(
                DesktopService.GetMimeTypeForUri(uri.AbsoluteUri)
            );

            var oldPolicy = solution.Policies.Get<TextStylePolicy>();
            var fileWidth = configuration.MaxLineLength ?? oldPolicy.FileWidth;
            var eolMarkerVal = TranslateEndOfLine(configuration.EndOfLine);
            var eolMarker = eolMarkerVal ?? oldPolicy.EolMarker;
            var indentWidthVal = GetIndentWidth(configuration);
            var indentWidth = indentWidthVal ?? oldPolicy.IndentWidth;
            var tabWidth = configuration.TabWidth ?? oldPolicy.TabWidth;
            var removeTrailingWhitespace = configuration.TrimTrailingWhitespace ?? oldPolicy.RemoveTrailingWhitespace;
            var tabsToSpaces = configuration.IndentStyle.HasValue ? (configuration.IndentStyle == EditorConfig.Core.IndentStyle.Space) : oldPolicy.TabsToSpaces;

            var newPolicy = new TextStylePolicy(fileWidth, tabWidth, indentWidth, tabsToSpaces, oldPolicy.NoTabsAfterNonTabs, removeTrailingWhitespace, eolMarker);
            if (newPolicy != oldPolicy) solution.Policies.Set<TextStylePolicy>(newPolicy, mimeType);
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
    }
}
