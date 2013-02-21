// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Refactoring;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.CSharp.TypeSystem;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.NRefactory.TypeSystem;

namespace CompletionSample.Completion
{
    class CompletionData : ICompletionData, ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData
	{
		protected CompletionData()
        { }

        public CompletionData(string text)
        {
            DisplayText = CompletionText = Description = text;
        }

        public string TriggerWord { get; set; }
        public int TriggerWordLength { get; set; }

        #region NRefactory ICompletionData implementation
        public CompletionCategory CompletionCategory { get; set; }
        public string DisplayText { get; set; }
        public virtual string Description { get; set; }
        public string CompletionText { get; set; }
        public DisplayFlags DisplayFlags { get; set; }

        public bool HasOverloads
        {
            get { return overloadedData.Count > 0; }
        }

        readonly List<ICompletionData> overloadedData = new List<ICompletionData>();
        public IEnumerable<ICompletionData> OverloadedData
        {
            get { return overloadedData; }
        }

        public void AddOverload(ICompletionData data)
        {
            if (overloadedData.Count == 0)
                overloadedData.Add(this);
            overloadedData.Add(data);
        }
        #endregion

        #region AvalonEdit ICompletionData implementation

        public System.Windows.Media.ImageSource Image { get; set; }

        public virtual void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var replaceSegment = new ICSharpCode.AvalonEdit.Document.TextSegment();
            replaceSegment.StartOffset = completionSegment.Offset;
            replaceSegment.Length = completionSegment.Length;
            replaceSegment.EndOffset = completionSegment.EndOffset;

            //if text was entered before the code complete was called there might be some text before this segment that needs replacing,
            // e.g., I type "DateT" and then press ctrl+space, then the insert needs to also replace the "DateT" part of the text.
            replaceSegment.StartOffset -= TriggerWordLength;
            replaceSegment.Length += TriggerWordLength;

            textArea.Document.Replace(replaceSegment, this.CompletionText);
        }

        public object Content
        {
            get { return DisplayText; }
        }

        object ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Description
        {
            get { return this.Description; }
        }

        public virtual double Priority
        {
            get { return 1.0; }
        }

        public string Text
        {
            get { return this.CompletionText; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            var other = obj as CompletionData;
            return other != null && DisplayText == other.DisplayText;
        }

        public override int GetHashCode()
        {
            return DisplayText.GetHashCode();
        }
    }

    class EntityCompletionData : CompletionData, IEntityCompletionData
    {
        readonly IEntity entity;
        static readonly CSharpAmbience csharpAmbience = new CSharpAmbience();

        public IEntity Entity
        {
            get { return entity; }
        }

        public EntityCompletionData(IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            this.entity = entity;
            IAmbience ambience = new CSharpAmbience();
            ambience.ConversionFlags = entity is ITypeDefinition ? ConversionFlags.ShowTypeParameterList : ConversionFlags.None;
            DisplayText = entity.Name;
            this.CompletionText = ambience.ConvertEntity(entity);
            ambience.ConversionFlags = ConversionFlags.StandardConversionFlags;
            if (entity is ITypeDefinition)
            {
                // Show fully qualified Type name
                ambience.ConversionFlags |= ConversionFlags.UseFullyQualifiedTypeNames;
            }
            this.Image = ClassBrowserIconService.GetIcon(entity).ImageSource;
        }

        #region Description & Documentation
        private string description;
        public override string Description
        {
            get
            {
                if (description == null)
                {
                    description = GetText(Entity);
                    if (HasOverloads)
                    {
                        description += " (+" + OverloadedData.Count() + " overloads)";
                    }
                    description += Environment.NewLine + XmlDocumentationToText(Entity.Documentation);
                }
                return description;
            }
            set
            {
                description = value;
            }
        }

        /// <summary>
        /// Converts a member to text.
        /// Returns the declaration of the member as C# or VB code, e.g.
        /// "public void MemberName(string parameter)"
        /// </summary>
        static string GetText(IEntity entity)
        {
            IAmbience ambience = csharpAmbience;
            ambience.ConversionFlags = ConversionFlags.StandardConversionFlags;
            if (entity is ITypeDefinition)
            {
                // Show fully qualified Type name
                ambience.ConversionFlags |= ConversionFlags.UseFullyQualifiedTypeNames;
            }
            return ambience.ConvertEntity(entity);
        }

        public static string XmlDocumentationToText(string xmlDoc)
        {
            System.Diagnostics.Debug.WriteLine(xmlDoc);
            StringBuilder b = new StringBuilder();
            try
            {
                using (XmlTextReader reader = new XmlTextReader(new StringReader("<root>" + xmlDoc + "</root>")))
                {
                    reader.XmlResolver = null;
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Text:
                                b.Append(reader.Value);
                                break;
                            case XmlNodeType.Element:
                                switch (reader.Name)
                                {
                                    case "filterpriority":
                                        reader.Skip();
                                        break;
                                    case "returns":
                                        b.AppendLine();
                                        b.Append("Returns: ");
                                        break;
                                    case "param":
                                        b.AppendLine();
                                        b.Append(reader.GetAttribute("name") + ": ");
                                        break;
                                    case "remarks":
                                        b.AppendLine();
                                        b.Append("Remarks: ");
                                        break;
                                    case "see":
                                        if (reader.IsEmptyElement)
                                        {
                                            b.Append(reader.GetAttribute("cref"));
                                        }
                                        else
                                        {
                                            reader.MoveToContent();
                                            if (reader.HasValue)
                                            {
                                                b.Append(reader.Value);
                                            }
                                            else
                                            {
                                                b.Append(reader.GetAttribute("cref"));
                                            }
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }
                return b.ToString();
            }
            catch (XmlException)
            {
                return xmlDoc;
            }
        }

        #endregion
    }

    /// <summary>
    /// Completion item that introduces a using declaration.
    /// </summary>
    class ImportCompletionData : EntityCompletionData
    {
        string insertUsing;
        string insertionText;

        public ImportCompletionData(ITypeDefinition typeDef, CSharpTypeResolveContext contextAtCaret, bool useFullName)
            : base(typeDef)
        {
            this.Description = "using " + typeDef.Namespace + ";";
            if (useFullName)
            {
                var astBuilder = new TypeSystemAstBuilder(new CSharpResolver(contextAtCaret));
                insertionText = astBuilder.ConvertType(typeDef).GetText();
            }
            else
            {
                insertionText = typeDef.Name;
                insertUsing = typeDef.Namespace;
            }
        }
    }

    internal class VariableCompletionData : CompletionData, IVariableCompletionData
    {
        public VariableCompletionData(IVariable variable)
        {
            if (variable == null) throw new ArgumentNullException("variable");
            Variable = variable;

            IAmbience ambience = new CSharpAmbience();
            DisplayText = variable.Name;
            Description = ambience.ConvertVariable(variable);
            CompletionText = Variable.Name;
            this.Image = ClassBrowserIconService.GetIcon(variable).ImageSource;
        }

        public IVariable Variable { get; private set; }
    }


    /// <summary>
    /// Item for 'override' completion.
    /// </summary>
    internal class OverrideCompletionData : EntityCompletionData
    {
        readonly int declarationBegin;
        readonly CSharpTypeResolveContext contextAtCaret;

        public OverrideCompletionData(int declarationBegin, IMember m, CSharpTypeResolveContext contextAtCaret)
            : base(m)
        {
            this.declarationBegin = declarationBegin;
            this.contextAtCaret = contextAtCaret;
            var ambience = new CSharpAmbience();
            ambience.ConversionFlags = ConversionFlags.ShowTypeParameterList | ConversionFlags.ShowParameterList | ConversionFlags.ShowParameterNames;
            this.CompletionText = ambience.ConvertEntity(m);
        }

        public override void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            if (declarationBegin > completionSegment.Offset)
            {
                base.Complete(textArea, completionSegment, insertionRequestEventArgs);
                return;
            }
            TypeSystemAstBuilder b = new TypeSystemAstBuilder(new CSharpResolver(contextAtCaret));
            b.ShowTypeParameterConstraints = false;
            b.GenerateBody = true;

            var entityDeclaration = b.ConvertEntity(this.Entity);
            entityDeclaration.Modifiers &= ~(Modifiers.Virtual | Modifiers.Abstract);
            entityDeclaration.Modifiers |= Modifiers.Override;

            if (!this.Entity.IsAbstract)
            {
                // modify body to call the base method
                if (this.Entity.EntityType == EntityType.Method)
                {
                    var baseCall = new BaseReferenceExpression().Invoke(this.Entity.Name, ParametersToExpressions(this.Entity));
                    var body = entityDeclaration.GetChildByRole(Roles.Body);
                    body.Statements.Clear();
                    if (((IMethod)this.Entity).ReturnType.IsKnownType(KnownTypeCode.Void))
                        body.Statements.Add(new ExpressionStatement(baseCall));
                    else
                        body.Statements.Add(new ReturnStatement(baseCall));
                }
                else if (this.Entity.EntityType == EntityType.Indexer || this.Entity.EntityType == EntityType.Property)
                {
                    Expression baseCall;
                    if (this.Entity.EntityType == EntityType.Indexer)
                        baseCall = new BaseReferenceExpression().Indexer(ParametersToExpressions(this.Entity));
                    else
                        baseCall = new BaseReferenceExpression().Member(this.Entity.Name);
                    var getterBody = entityDeclaration.GetChildByRole(PropertyDeclaration.GetterRole).Body;
                    if (!getterBody.IsNull)
                    {
                        getterBody.Statements.Clear();
                        getterBody.Add(new ReturnStatement(baseCall.Clone()));
                    }
                    var setterBody = entityDeclaration.GetChildByRole(PropertyDeclaration.SetterRole).Body;
                    if (!setterBody.IsNull)
                    {
                        setterBody.Statements.Clear();
                        setterBody.Add(new AssignmentExpression(baseCall.Clone(), new IdentifierExpression("value")));
                    }
                }
            }

            var document = textArea.Document;
            StringWriter w = new StringWriter();
            var formattingOptions = FormattingOptionsFactory.CreateSharpDevelop();
            var segmentDict = SegmentTrackingOutputFormatter.WriteNode(w, entityDeclaration, formattingOptions, textArea.Options);

            string newText = w.ToString().TrimEnd();
            document.Replace(declarationBegin, completionSegment.EndOffset - declarationBegin, newText);
            var throwStatement = entityDeclaration.Descendants.FirstOrDefault(n => n is ThrowStatement);
            if (throwStatement != null)
            {
                var segment = segmentDict[throwStatement];
                textArea.Selection = new RectangleSelection(textArea, new TextViewPosition(textArea.Document.GetLocation(declarationBegin + segment.Offset)), new TextViewPosition(textArea.Document.GetLocation(declarationBegin + segment.Offset + segment.Length)));
            }
            CSharpFormatter.Format(textArea.Document, declarationBegin, newText.Length, formattingOptions);
        }

        IEnumerable<Expression> ParametersToExpressions(IEntity entity)
        {
            foreach (var p in ((IParameterizedMember)entity).Parameters)
            {
                if (p.IsRef || p.IsOut)
                    yield return new DirectionExpression(p.IsOut ? FieldDirection.Out : FieldDirection.Ref, new IdentifierExpression(p.Name));
                else
                    yield return new IdentifierExpression(p.Name);
            }
        }
    }

}//end namespace
