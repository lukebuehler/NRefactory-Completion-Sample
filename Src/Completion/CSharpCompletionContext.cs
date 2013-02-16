// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Completion;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

namespace CompletionSample.Completion
{
	public sealed class CSharpCompletionContext
	{
	    public readonly string Input;
	    public readonly int Offset;
	    public readonly string SourceFile;

        public readonly IDocument Document;
        public readonly ICompilation Compilation;
		public readonly IProjectContent ProjectContent;
	    public readonly CSharpResolver Resolver;
		public readonly CSharpTypeResolveContext TypeResolveContextAtCaret;
		public readonly ICompletionContextProvider CompletionContextProvider;

        public static CSharpCompletionContext Get(string input, int offset, string sourceFile, IProjectContent projectContent)
        {
            var syntaxTree = new CSharpParser().Parse(input, sourceFile);
            syntaxTree.Freeze();
            var unresolvedFile = syntaxTree.ToTypeSystem();

            var doc = new ReadOnlyDocument(input);

            return new CSharpCompletionContext(input, offset, sourceFile, doc, unresolvedFile, projectContent);
		}

        private CSharpCompletionContext(string input, int offset, string sourceFile,
            IDocument document, CSharpUnresolvedFile unresolvedFile, IProjectContent projectContent)
        {
            Input = input;
            Offset = offset;
            SourceFile = sourceFile;
            Document = document;

            ProjectContent = projectContent.AddOrUpdateFiles(unresolvedFile);
            Compilation = projectContent.CreateCompilation();

            var location = Document.GetLocation(offset);
            Resolver = unresolvedFile.GetResolver(Compilation, location);
            TypeResolveContextAtCaret = unresolvedFile.GetTypeResolveContext(Compilation, location);
            CompletionContextProvider = new DefaultCompletionContextProvider(document, unresolvedFile);
		}
	}
}
