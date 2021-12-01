using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;


namespace CDS.CSharpScripting
{
    /// <summary>
    /// Editor control for C#, pre-configured with keywords, dark theme, etc.
    /// </summary>
    public partial class CodeEditor : UserControl
    {
        public List<Type> namespaceTypes = new List<Type>();

        public List<Type> referenceTypes = new List<Type>();

        FoldingManager foldingManager;  //21-11-16折叠
        BraceFoldingStrategy foldingStrategy = new BraceFoldingStrategy();  //21-11-16折叠 允许基于大括号从文档中生成折叠.

        /// <summary>
        /// Delete an unmanaged object
        /// </summary>
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);


        private const string CDSPropertyCategory = "CDS";
        private RoslynCodeEditor editor;
        private bool isInitialised;
        bool supressTextChangedEvent;


        /// <summary>
        /// The C# script.
        /// </summary>
        /// <remarks>
        /// Note: we use the label to store the script if the RoslynPad editor hasn't been created yet.
        /// </remarks>
        [Description("The C# script")]
        [Category(CDSPropertyCategory)]
        [Editor(typeof(System.ComponentModel.Design.MultilineStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [SettingsBindable(true)]
        public string CDSScript
        {
            get => isInitialised ? editor.Document.Text : labelTempScript.Text;

            set
            {
                if (isInitialised)
                {
                    editor.Document.Text = value;
                }
                else
                {
                    labelTempScript.Text = value;
                }
            }
        }


        /// <summary>
        /// Fired when the text changes
        /// </summary>
        [Description("Fired when the script changes")]
        [Category(CDSPropertyCategory)]
        public event EventHandler CDSScriptChanged;


        /// <summary>
        /// Basic initialisation
        /// </summary>
        public CodeEditor()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Initialise: configures and shows a Roslyn code editor. 
        /// A default set of namespaces and assemblies are automatically used.
        /// </summary>
        public void CDSInitialize()
        {
            CDSInitialize(globalsType: null);
        }


        /// <summary>
        /// Initialise: configures and shows a Roslyn code editor. 
        /// A default set of namespaces and assemblies are automatically used.
        /// </summary>
        /// <param name="globalsType">
        /// Optional (can be null): the type of a global variable made available to the
        /// script without any other namespace resolution. 
        /// </param>
        public void CDSInitialize(Type globalsType)
        {
            CDSInitialize(
                namespaceTypes: Defaults.TypesForNamespacesAndAssemblies,
                referenceTypes: Defaults.TypesForNamespacesAndAssemblies,
                globalsType: globalsType);
        }


        /// <summary>
        /// Initialise: configures and shows a Roslyn code editor.
        /// </summary>
        /// <param name="referenceTypes">
        /// The assembly for each type in this list is referenced by the editor session.
        /// Added, for example, typeof(int), will result in the core framework library 
        /// (mscorlib) being loaded.
        /// </param>
        /// <param name="namespaceTypes">
        /// The namespace for each type is made automatically available for each type in
        /// this list. E.g. sending typeof(int) will make the System namespace available; 
        /// it's the equivalet of adding 'using System' to the top of the script.
        /// </param>
        /// <param name="globalsType">
        /// Optional (can be null): the type of a global variable made available to the
        /// script without any other namespace resolution. 
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either of <paramref name="referenceTypes"/> or <paramref name="namespaceTypes"/>
        /// is null.
        /// </exception>
        public void CDSInitialize(
            IEnumerable<Type> namespaceTypes,
            IEnumerable<Type> referenceTypes,
            Type globalsType)
        {
            CDSUninitialise();

            if (namespaceTypes == null) { throw new ArgumentNullException(nameof(namespaceTypes)); }
            if (referenceTypes == null) { throw new ArgumentNullException(nameof(referenceTypes)); }


            var referenceTypesIncludingGlobalsType = GenerateAllReferenceTypes(referenceTypes, globalsType);

            Assembly asmtest = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll");
            referenceTypesIncludingGlobalsType.Add(asmtest);
            var var = referenceTypesIncludingGlobalsType.Distinct();

            this.referenceTypes = referenceTypes.ToList(); //.
            if (globalsType != null)
            {
                this.referenceTypes.Add(globalsType);
            }

            var namespaceTypesIncludingGlobalsType = GenerateAllNamespaceTypes(namespaceTypes, globalsType);

            this.namespaceTypes = namespaceTypesIncludingGlobalsType; //.

            var roslynHost = CreateRosylnHost(
                referenceTypesIncludingGlobalsType,
                null);

            CreateNewEditor(roslynHost);

            //var roslynHost = CreateRosylnHost(referenceTypesIncludingGlobalsType, namespaceTypesIncludingGlobalsType);

            //CreateNewEditor(roslynHost);

            TransferScriptFromTempStoreToEditor();

            this.foldingManager = FoldingManager.Install(editor.TextArea);  //21-11-16折叠

            isInitialised = true;
        }


        private void TransferScriptFromTempStoreToEditor()
        {
            supressTextChangedEvent = true;
            editor.Text = labelTempScript.Text;
            supressTextChangedEvent = false;
        }


        private void CreateNewEditor(CustomRoslynHost roslynHost)
        {
            editor = new RoslynCodeEditor();
            var workingDirectory = Directory.GetCurrentDirectory();

            editor.Initialize(
                roslynHost: roslynHost,
                highlightColors: new ClassificationHighlightColors(),
                workingDirectory: workingDirectory,
                documentText: "test");

            editor.IsBraceCompletionEnabled = true;

            // This causes the light bulb to appear but the context menu doesn't
            // seem to work properly...

            //var handle = Properties.Resource.IntellisenseLightBulb_16x.GetHbitmap();

            //editor.ContextActionsIcon = Imaging.CreateBitmapSourceFromHBitmap(
            //    handle,
            //    IntPtr.Zero,
            //    Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());

            //DeleteObject(handle);

            editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            editor.FontFamily = new System.Windows.Media.FontFamily(this.Font.FontFamily.Name);
            editor.FontSize = this.Font.Size * 96 / 72;
            editor.TextChanged += Editor_TextChanged;
            editor.Options.HighlightCurrentLine = true;
            wpfEditorHost.Dock = DockStyle.Fill;
            wpfEditorHost.Visible = true;
            labelTempScript.Visible = false;
            wpfEditorHost.Child = editor;
        }

        private void CreateNewEditor(RoslynHost roslynHost)
        {
            editor = new RoslynCodeEditor();
            var workingDirectory = Directory.GetCurrentDirectory();

            editor.Initialize(
                roslynHost: roslynHost,
                highlightColors: new ClassificationHighlightColors(),
                workingDirectory: workingDirectory,
                documentText: "test");

            editor.IsBraceCompletionEnabled = true;

            // This causes the light bulb to appear but the context menu doesn't
            // seem to work properly...

            //var handle = Properties.Resource.IntellisenseLightBulb_16x.GetHbitmap();

            //editor.ContextActionsIcon = Imaging.CreateBitmapSourceFromHBitmap(
            //    handle,
            //    IntPtr.Zero,
            //    Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());

            //DeleteObject(handle);

            editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            editor.FontFamily = new System.Windows.Media.FontFamily(this.Font.FontFamily.Name);
            editor.FontSize = this.Font.Size * 96 / 72;
            editor.TextChanged += Editor_TextChanged;
            editor.Options.HighlightCurrentLine = true;
            wpfEditorHost.Dock = DockStyle.Fill;
            wpfEditorHost.Visible = true;
            labelTempScript.Visible = false;
            wpfEditorHost.Child = editor;
        }

        private static RoslynHost CreateRosylnHost(IEnumerable<Assembly> assemblyReferences, IEnumerable<Type> namespaceTypesIncludingGlobalsType)
        {
            var roslynHostReferences =
                RoslynHostReferences
                .Empty
                .With(assemblyReferences: assemblyReferences,typeNamespaceImports: namespaceTypesIncludingGlobalsType);

            var roslynHost = new RoslynHost(references: roslynHostReferences);
            return roslynHost;
        }

        private static CustomRoslynHost CreateRosylnHost(List<Assembly> assemblyReferences, List<Type> namespaceTypesIncludingGlobalsType)
        {
            var namespaceImports =
                RoslynHostReferences
                .Empty
                .With(assemblyReferences: assemblyReferences,typeNamespaceImports: namespaceTypesIncludingGlobalsType);


            var roslynHost = new CustomRoslynHost(additionalAssemblies: new[]
                {
                    Assembly.Load("RoslynPad.Roslyn.Windows"),
                    Assembly.Load("RoslynPad.Editor.Windows"),  //这两句必加
                }, references: namespaceImports);
            return roslynHost;
        }

        private static CustomRoslynHost CreateRosylnHost(Type globalsType, List<Assembly> referenceTypesIncludingGlobalsType, List<Type> namespaceTypesIncludingGlobalsType)
        {
            var namespaceImports =
                RoslynHostReferences
                .Empty
                .With(assemblyReferences: referenceTypesIncludingGlobalsType)
                .With(typeNamespaceImports: namespaceTypesIncludingGlobalsType);


            var roslynHost = new CustomRoslynHost(
                globalsType: globalsType,
                additionalAssemblies: new[]
                {
                    Assembly.Load("RoslynPad.Roslyn.Windows"),
                    Assembly.Load("RoslynPad.Editor.Windows"),
                },
                references: namespaceImports);
            return roslynHost;
        }


        private static List<Type> GenerateAllNamespaceTypes(IEnumerable<Type> namespaceTypes, Type globalsType)
        {
            List<Type> namespaceTypesIncludingGlobalsType = new List<Type>(namespaceTypes);

            if (globalsType != null)
            {
                namespaceTypesIncludingGlobalsType.Add(globalsType);
            }

            return namespaceTypesIncludingGlobalsType;
        }


        private static List<Assembly> GenerateAllReferenceTypes(IEnumerable<Type> referenceTypes, Type globalsType)
        {
            List<Assembly> referenceTypesIncludingGlobalsType = referenceTypes.Select(rt => rt.Assembly).ToList();

            if (globalsType != null)
            {
                referenceTypesIncludingGlobalsType.Add(globalsType.Assembly);
            }

            return referenceTypesIncludingGlobalsType;
        }


        /// <summary>
        /// Uninitialise: closes the editor; the script remains visible on a read-only display.
        /// </summary>
        /// <remarks>
        /// Does nothing if not already initialised.
        /// </remarks>
        public void CDSUninitialise()
        {
            if(!isInitialised) { return; }

            TransferScriptFromEditorToTempStore();
            labelTempScript.Visible = true;
            wpfEditorHost.Visible = false;

            editor.TextChanged -= Editor_TextChanged;
            wpfEditorHost.Child = null;
            editor = null;

            isInitialised = false;
        }


        private void TransferScriptFromEditorToTempStore()
        {
            labelTempScript.Text = editor.Text;
        }


        /// <summary>
        /// The text has changed - fire up to the owner
        /// </summary>
        private void Editor_TextChanged(object sender, EventArgs e)
        {
            if (!supressTextChangedEvent)
            {
                CDSScriptChanged?.Invoke(sender, e);
            }

            if (foldingManager != null && this.isInitialised)
            {
                this.foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
            }
        }
    }
}
