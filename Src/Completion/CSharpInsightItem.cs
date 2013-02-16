// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.NRefactory.TypeSystem;

namespace CompletionSample.Completion
{
	public sealed class CSharpInsightItem
	{
		public readonly IParameterizedMember Method;
		
		public CSharpInsightItem(IParameterizedMember method)
		{
			this.Method = method;
		}
		
		TextBlock header;
		
		public object Header {
			get {
				if (header == null) {
					header = new TextBlock();
					GenerateHeader();
				}
				return header;
			}
		}
		
		int highlightedParameterIndex = -1;
		
		public void HighlightParameter(int parameterIndex)
		{
            if (IsExtensionMethod)
                parameterIndex++;

			if (highlightedParameterIndex == parameterIndex)
				return;
			this.highlightedParameterIndex = parameterIndex;
			if (header != null)
				GenerateHeader();
		}
		
		void GenerateHeader()
		{
			CSharpAmbience ambience = new CSharpAmbience();
			ambience.ConversionFlags = ConversionFlags.StandardConversionFlags;
			var stringBuilder = new StringBuilder();
			var formatter = new ParameterHighlightingOutputFormatter(stringBuilder, highlightedParameterIndex);
			ambience.ConvertEntity(Method, formatter, FormattingOptionsFactory.CreateSharpDevelop());
			var inlineBuilder = new HighlightedInlineBuilder(stringBuilder.ToString());
			inlineBuilder.SetFontWeight(formatter.parameterStartOffset, formatter.parameterLength, FontWeights.Bold);
			header.Inlines.Clear();
			header.Inlines.AddRange(inlineBuilder.CreateRuns());
		}
		
		public object Content {
			get { return Documentation; }
		}

	    public bool IsExtensionMethod
	    {
            get
            {
                var im = Method as IMethod;
                return im != null && im.IsExtensionMethod;
            }

	    }

	    private string documentation;
        public string Documentation
        {
            get
            {
                if(documentation == null)
                {
                    if (Method.Documentation == null)
                        documentation = "";
                    else
                        documentation = EntityCompletionData.XmlDocumentationToText(Method.Documentation);
                }
                return documentation;
            }
        }
		
		sealed class ParameterHighlightingOutputFormatter : TextWriterOutputFormatter
		{
			StringBuilder b;
			int highlightedParameterIndex;
			int parameterIndex;
			internal int parameterStartOffset;
			internal int parameterLength;
			
			public ParameterHighlightingOutputFormatter(StringBuilder b, int highlightedParameterIndex)
				: base(new StringWriter(b))
			{
				this.b = b;
				this.highlightedParameterIndex = highlightedParameterIndex;
			}
			
			public override void StartNode(AstNode node)
			{
				if (parameterIndex == highlightedParameterIndex && node is ParameterDeclaration) {
					parameterStartOffset = b.Length;
				}
				base.StartNode(node);
			}
			
			public override void EndNode(AstNode node)
			{
				base.EndNode(node);
				if (node is ParameterDeclaration) {
					if (parameterIndex == highlightedParameterIndex)
						parameterLength = b.Length - parameterStartOffset;
					parameterIndex++;
				}
			}
		}
	}
}
