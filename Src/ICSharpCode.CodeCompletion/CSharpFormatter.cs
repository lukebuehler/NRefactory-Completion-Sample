#region License
// CShell, A Simple C# Scripting IDE
// Copyright (C) 2013  Arnova Asset Management Ltd., Lukas Buhler
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

// This file is based on code from the SharpDevelop project:
//   Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \Doc\sharpdevelop-copyright.txt)
//   This code is distributed under the GNU LGPL (for details please see \Doc\COPYING.LESSER.txt)
#endregion
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

namespace ICSharpCode.CodeCompletion
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
