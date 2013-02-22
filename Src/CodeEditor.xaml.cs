using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using CompletionSample.Completion;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Highlighting;

namespace CompletionSample
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl
    {
        CompletionWindow completionWindow;
        OverloadInsightWindow insightWindow;

        public CodeEditor()
        {
            InitializeComponent();

            textEditor.TextArea.TextEntering += TextAreaOnTextEntering;
            textEditor.TextArea.TextEntered += TextAreaOnTextEntered;
            textEditor.ShowLineNumbers = true;


            var ctrlSpace = new RoutedCommand();
            ctrlSpace.InputGestures.Add(new KeyGesture(Key.Space, ModifierKeys.Control));
            var cb = new CommandBinding(ctrlSpace, OnCtrlSpaceCommand);
            this.CommandBindings.Add(cb);
        }

        #region Open & Save File
        public string FileName { get; private set; }

        public void OpenFile(string fileName)
        {
            if(!System.IO.File.Exists(fileName))
                throw new FileNotFoundException(fileName);

            if (completionWindow != null)
                completionWindow.Close();
            if (insightWindow != null)
                insightWindow.Close();

            FileName = fileName;
            textEditor.Load(fileName);
            textEditor.Document.FileName = FileName;

            textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinitionByExtension(Path.GetExtension(fileName));
        }

        public bool SaveFile()
        {
            if (String.IsNullOrEmpty(FileName))
                return false;

            textEditor.Save(FileName);
            return true;
        }
        #endregion


        #region Code Completion
        private void TextAreaOnTextEntered(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            ShowCompletion(textCompositionEventArgs.Text, false);
        }

        private void OnCtrlSpaceCommand(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
        {
            ShowCompletion(null, true);
        }

        private void ShowCompletion(string enteredText, bool controlSpace)
        {
            if (!controlSpace)
                Debug.WriteLine("Code Completion: TextEntered: " + enteredText);
            else
                Debug.WriteLine("Code Completion: Ctrl+Space");

            //only process csharp files
            if (String.IsNullOrEmpty(textEditor.Document.FileName))
                return;
            var fileExtension = Path.GetExtension(textEditor.Document.FileName);
            fileExtension = fileExtension != null ? fileExtension.ToLower() : null;
            //check file extension to be a c# file (.cs, .csx, etc.)
            if (fileExtension == null || (!fileExtension.StartsWith(".cs")))
                return;

            if (completionWindow == null)
            {
                CodeCompletionResult results = null;
                try
                {
                    results = CSharpCompletion.GetCompletions(textEditor.Document, textEditor.CaretOffset, controlSpace);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("Error in getting completion: " + exception);
                }
                if (results == null)
                    return;

                if (insightWindow == null && results.OverloadProvider != null)
                {
                    insightWindow = new OverloadInsightWindow(textEditor.TextArea);
                    insightWindow.Provider = results.OverloadProvider;
                    insightWindow.Show();
                    insightWindow.Closed += (o, args) => insightWindow = null;
                    return;
                }

                if (completionWindow == null && results != null && results.CompletionData.Any())
                {
                    // Open code completion after the user has pressed dot:
                    completionWindow = new CompletionWindow(textEditor.TextArea);
                    completionWindow.CloseWhenCaretAtBeginning = controlSpace;
                    completionWindow.StartOffset -= results.TriggerWordLength;
                    //completionWindow.EndOffset -= results.TriggerWordLength;

                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    foreach (var completion in results.CompletionData.OrderBy(item => item.Text))
                    {
                        data.Add(completion);
                    }
                    if (results.TriggerWordLength > 0)
                    {
                        //completionWindow.CompletionList.IsFiltering = false;
                        completionWindow.CompletionList.SelectItem(results.TriggerWord);
                    }
                    completionWindow.Show();
                    completionWindow.Closed += (o, args) => completionWindow = null;
                }
            }//end if


            //update the insight window
            if (!string.IsNullOrEmpty(enteredText) && insightWindow != null)
            {
                //whenver text is entered update the provider
                var provider = insightWindow.Provider as CSharpOverloadProvider;
                if (provider != null)
                {
                    //since the text has not been added yet we need to tread it as if the char has already been inserted
                    provider.Update(textEditor.Document, textEditor.CaretOffset);
                    //if the windows is requested to be closed we do it here
                    if (provider.RequestClose)
                    {
                        insightWindow.Close();
                        insightWindow = null;
                    }
                }
            }
        }//end method

        private void TextAreaOnTextEntering(object sender, TextCompositionEventArgs textCompositionEventArgs)
        {
            Debug.WriteLine("TextEntering: " + textCompositionEventArgs.Text);
            if (textCompositionEventArgs.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(textCompositionEventArgs.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(textCompositionEventArgs);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }
        #endregion

    }//end class
}
