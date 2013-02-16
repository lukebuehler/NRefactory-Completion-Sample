using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ICSharpCode.NRefactory.CSharp.Completion;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.NRefactory.Completion;

namespace CompletionSample.Completion
{
    public class CSharpOverloadProvider : PropertyChangedBase, IOverloadProvider,  IParameterDataProvider
    {
        private readonly CSharpCompletionContext context;
        private readonly int startOffset;
        internal readonly IList<CSharpInsightItem> items;
        private int selectedIndex;

        public CSharpOverloadProvider(CSharpCompletionContext context, int startOffset, IEnumerable<CSharpInsightItem> items)
        {
            Debug.Assert(items != null);
            this.context = context;
            this.startOffset = startOffset;
            this.selectedIndex = 0;
            this.items = items.ToList();

            var startsChar = context.Input[startOffset];
            var subtext = context.Input.Substring(0, startOffset);
            Update(context.Input, context.Offset, context.SourceFile);
        }

        public bool RequestClose { get; set; }

        public int Count
        {
            get { return items.Count; }
        }

        public object CurrentContent
        {
            get { return items[selectedIndex].Content; }
        }

        public object CurrentHeader
        {
            get { return items[selectedIndex].Header; }
        }

        public string CurrentIndexText
        {
            get { return (selectedIndex + 1).ToString() + " of " + this.Count.ToString(); }
        }

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                if (selectedIndex >= items.Count)
                    selectedIndex = items.Count - 1;
                if (selectedIndex < 0)
                    selectedIndex = 0;
                NotifyOfPropertyChange(() => SelectedIndex);
                NotifyOfPropertyChange(() => CurrentIndexText);
                NotifyOfPropertyChange(() => CurrentHeader);
                NotifyOfPropertyChange(() => CurrentContent);
            }
        }

        public void Update(string input, int offset, string sourceFile)
        {
            var completionContext = CSharpCompletionContext.Get(input, offset, sourceFile, context.ProjectContent);
            if (completionContext == null)
            {
                RequestClose = true;
                return;
            }

            var completionFactory = new CSharpCompletionDataFactory(completionContext.TypeResolveContextAtCaret, completionContext);
            var pce = new CSharpParameterCompletionEngine(
                completionContext.Document,
                completionContext.CompletionContextProvider,
                completionFactory,
                completionContext.ProjectContent,
                completionContext.TypeResolveContextAtCaret
            );

            var completionChar = completionContext.Input[completionContext.Offset - 1];
            int parameterIndex = pce.GetCurrentParameterIndex(startOffset, completionContext.Offset);
            if (parameterIndex < 0)
            {
                RequestClose = true;
                return;
            }
            else
            {
                if (parameterIndex > items[selectedIndex].Method.Parameters.Count)
                {
                    var newItem = items.FirstOrDefault(i => parameterIndex <= i.Method.Parameters.Count);
                    SelectedIndex = items.IndexOf(newItem);
                }
                if (parameterIndex > 0)
                    parameterIndex--; // NR returns 1-based parameter index
                foreach (var item in items)
                {
                    item.HighlightParameter(parameterIndex);
                }
            }
        }

        #region IParameterDataProvider implementation
        int IParameterDataProvider.StartOffset
        {
            get { return startOffset; }
        }

        string IParameterDataProvider.GetHeading(int overload, string[] parameterDescription, int currentParameter)
        {
            throw new NotImplementedException();
        }

        string IParameterDataProvider.GetDescription(int overload, int currentParameter)
        {
            throw new NotImplementedException();
        }

        string IParameterDataProvider.GetParameterDescription(int overload, int paramIndex)
        {
            throw new NotImplementedException();
        }

        string IParameterDataProvider.GetParameterName(int overload, int currentParameter)
        {
            throw new NotImplementedException();
        }

        int IParameterDataProvider.GetParameterCount(int overload)
        {
            throw new NotImplementedException();
        }

        bool IParameterDataProvider.AllowParameterList(int overload)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
