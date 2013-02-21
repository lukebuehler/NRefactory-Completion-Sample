// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

namespace CompletionSample.Completion
{
	public class CSharpFormatter
	{
		/// <summary>
		/// Formats the specified part of the document.
		/// </summary>
		public static void Format(IDocument document, int offset, int length, CSharpFormattingOptions options)
		{
            var syntaxTree = new CSharpParser().Parse(document);
            var fv = new AstFormattingVisitor(options, document);
            fv.FormattingRegion = new DomRegion(document.GetLocation(offset), document.GetLocation(offset + length));
			syntaxTree.AcceptVisitor(fv);
			fv.ApplyChanges(offset, length);
		}
	}
}
