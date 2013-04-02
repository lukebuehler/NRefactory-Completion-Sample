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
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            codeEditor.CodeTextEditor.Completion = new ICSharpCode.CodeCompletion.CSharpCompletion();
            OpenFile(@"..\SampleFiles\Sample1.cs");
        }

        private void OnFileOpenClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".cs"; // Default file extension 
            dlg.Filter = "C# Files (.cs)|*.cs|All Files |*.*"; // Filter files by extension 

            if(dlg.ShowDialog() == true)
            {
                OpenFile(dlg.FileName);
            }
        }

        private void OnSaveFileClick(object sender, RoutedEventArgs e)
        {
            if(codeEditor.CodeTextEditor.SaveFile())
            {
                MessageBox.Show("File Saved.");
            }
        }

        private void OpenFile(string fileName)
        {
            codeEditor.CodeTextEditor.OpenFile(fileName);
            this.Title = AppTitle + " - " + System.IO.Path.GetFileName(fileName);
        }
    }
}
