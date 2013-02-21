// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.CSharp;
using CSharpParser = ICSharpCode.NRefactory.CSharp.CSharpParser;

namespace CompletionSample.Completion
{
	public sealed class CSharpCompletionContext
	{
        public readonly IDocument OriginalDocument;
	    public readonly int OriginalOffset;

	    public readonly int Offset;
        public readonly IDocument Document;
        public readonly ICompilation Compilation;
		public readonly IProjectContent ProjectContent;
	    public readonly CSharpResolver Resolver;
		public readonly CSharpTypeResolveContext TypeResolveContextAtCaret;
		public readonly ICompletionContextProvider CompletionContextProvider;

        public CSharpCompletionContext(IDocument document, int offset, IProjectContent projectContent)
        {
            OriginalDocument = document;
            Document = document;

            OriginalOffset = offset;
            Offset = offset;

            var syntaxTree = new CSharpParser().Parse(document, document.FileName);
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();

            ProjectContent = projectContent.AddOrUpdateFiles(unresolvedFile);
            //note: it's important that the project content is used that is returned after adding the unresolved file
            Compilation = ProjectContent.CreateCompilation();

            var location = Document.GetLocation(offset);
            Resolver = unresolvedFile.GetResolver(Compilation, location);
            TypeResolveContextAtCaret = unresolvedFile.GetTypeResolveContext(Compilation, location);
            CompletionContextProvider = new DefaultCompletionContextProvider(document, unresolvedFile);
		}
	}
}
