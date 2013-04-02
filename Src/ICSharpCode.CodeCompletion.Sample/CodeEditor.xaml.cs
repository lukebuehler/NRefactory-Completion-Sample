using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.CodeCompletion;

namespace CompletionSample
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl
    {
        public CodeEditor()
        {
            InitializeComponent();

        }

        public CodeTextEditor CodeTextEditor { get { return textEditor; } }

    }//end class
}
