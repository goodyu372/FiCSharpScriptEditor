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

            #region Assembly.Load、LoadFrom、LoadFile 区别
            //Assembly.Load()
            //Load()方法接收一个String或AssemblyName类型作为参数，这个参数实际上是需要加载的程序集的强名称（名称，版本，语言，公钥标记）。
            //例如.NET 2.0中的FileIOPermission类，它的强名称是：System.Security.Permissions.FileIOPermission, mscorlib, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = b77a5c561934e089，
            //对于弱命名的程序集，则只会有程序集名称，而不会有版本，语言和公钥标记。
            //CLR内部普遍使用了Load()方法来加载程序集，在Load()方法的内部，CLR首先会应用这个程序集的版本绑定重定向策略，接着在GAC中查找目标程序集。
            //如果GAC中没有找到，则会在应用程序目录和子目录中寻找（应用配置文件的codebase所指定的位置）

            //Assembly.LoadFrom()
            //LoadFrom()方法可以从指定文件中加载程序集，通过查找程序集的AssemblyRef元数据表，得知所有引用和需要的程序集，然后在内部调用Load()方法进行加载。例如：Assembly.LoadFrom(@"C:\ABC\Test.dll");
            //LoadFrom()首先会打开程序集文件，通过GetAssemblyName方法得到程序集名称，然后关闭文件，最后将得到的AssemblyName对象传入Load()方法中。
            //随后，Load()方法会再次打开这个文件进行加载。所以，LoadFrom()加载一个程序集时，会多次打开文件，造成了效率低下的现象（与Load相比）。
            //由于内部调用了Load()，所以LoadFrom()方法还是会应用版本绑定重定向策略，也会在GAC和各个指定位置中进行查找。
            //LoadFrom()会直接返回Load()的结果——一个Assembly对象的引用。
            //如果目标程序集已经加载过，LoadFrom()不会重新进行加载

            //Assembly.LoadFile()
            //LoadFile()从一个指定文件中加载程序集，它和LoadFrom()的不同之处在于LoadFile()不会加载目标程序集所引用和依赖的其他程序集。您需要自己控制并显示加载所有依赖的程序集。
            //LoadFile()不会解析任何依赖。
            //LoadFile()可以多次加载同一程序集。
            //显式加载依赖程序集的方法是，注册AppDomain的AssemblyResolve事件。
            #endregion

            #region 程序集重定向测试
            Assembly asmtest = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll");
            //mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            //最后mscorlib.dll会被重定向至C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

            Assembly asmtest2 = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll");
            //System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            //最后System.dll会被重定向至C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll

            Assembly asmtest3 = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Core.dll");
            //System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            //最后System.Core.dll会被重定向至C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Core\v4.0_4.0.0.0__b77a5c561934e089\System.Core.dll

            Assembly asmtest4 = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Drawing.dll");
            //System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
            //最后System.Drawing.dll会被重定向至C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Drawing\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Drawing.dll

            Assembly asmtest5 = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Windows.Forms.dll");
            //System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            //最后System.Windows.Forms.dll会被重定向至C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Windows.Forms\v4.0_4.0.0.0__b77a5c561934e089\System.Windows.Forms.dll

            referenceTypesIncludingGlobalsType.Add(asmtest);
            var var = referenceTypesIncludingGlobalsType.Distinct();
            #endregion

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
                documentText: String.Empty);

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
