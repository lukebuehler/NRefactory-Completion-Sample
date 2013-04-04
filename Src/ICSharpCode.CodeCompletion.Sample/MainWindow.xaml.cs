using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.CodeCompletion;
using ICSharpCode.CodeCompletion.Sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CompletionSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string AppTitle = "NRefactory Code Completion";
        private ICSharpCode.CodeCompletion.CSharpCompletion completion;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            completion = new ICSharpCode.CodeCompletion.CSharpCompletion(new ScriptProvider());
            OpenFile(@"..\SampleFiles\Sample1.cs");
            OpenFile(@"..\SampleFiles\SampleScript1.csx");
        }

        private void OnFileOpenClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cs"; // Default file extension 
            dlg.Filter = "C# Files|*.cs?|All Files|*.*"; // Filter files by extension 

            if(dlg.ShowDialog() == true)
            {
                OpenFile(dlg.FileName);
            }
        }

        private void OnSaveFileClick(object sender, RoutedEventArgs e)
        {
            var tabItem = tabs.SelectedItem as TabItem;
            if (tabItem == null) return;
            var editor = tabItem.Content as CodeTextEditor;

            if (editor != null && editor.SaveFile())
            {
                MessageBox.Show("File Saved" + Environment.NewLine + editor.FileName);
            }
        }

        private void OpenFile(string fileName)
        {
            var editor = new CodeTextEditor();
            editor.FontFamily = new FontFamily("Consolas");
            editor.FontSize = 12;
            editor.Completion = completion;
            editor.OpenFile(fileName);
            editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");

            var tabItem = new TabItem();
            tabItem.Content = editor;
            tabItem.Header = System.IO.Path.GetFileName(fileName);
            tabs.Items.Add(tabItem);

        }
    }
}
