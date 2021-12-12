using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using RoslynPad.Editor;
using RoslynPad.Roslyn;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;


namespace FiCSharpScriptEditor
{
    /// <summary>
    /// C#代码编辑器
    /// </summary>
    public partial class FiCodeEditor : UserControl
    {
        private FoldingManager foldingManager;
        private BraceFoldingStrategy foldingStrategy;  //允许基于大括号从文档中生成折叠.

        private RoslynCodeEditor editor;
        private RoslynHost roslynHost;
        private DocumentId documentId;

        private bool isInitialized;

        public FiCodeEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initialise: configures and shows a Roslyn code editor. 
        /// A default set of namespaces and assemblies are automatically used.
        /// </summary>
        public async Task Initialize()
        {
            if (!ReferenceManager.bInitialized)
            {
                ReferenceManager.LoadData();
            }
            await Initialize(assemblyReferences: ReferenceManager.ListAssemblyReferences, null).ConfigureAwait(false);
        }

        /// <summary>
        /// 初始化:配置并显示一个Roslyn代码编辑器
        /// </summary>
        /// <param name="assemblyReferences">此列表中每种类型的程序集由编辑器会话引用。例如，mscorlib.dll将导致核心框架库被加载</param>
        /// <param name="typeNamespaceImports">此列表中的每种类型的名称空间将自动为每种类型提供。例如，发送typeof(int)将使System命名空间可用，这相当于在脚本顶部添加“using System”</param>
        /// <exception cref="ArgumentNullException">assemblyReferences不可以为null，typeNamespaceImports允许为null（用户必须在脚本顶部添加using命名空间）</exception>
        public async Task Initialize(IEnumerable<Assembly> assemblyReferences, IEnumerable<Type> typeNamespaceImports)
        {
            if (assemblyReferences == null)
            {
                throw new ArgumentNullException(nameof(assemblyReferences));
            }

            Uninitialise();

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
            //Assembly asmtest = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll");
            ////mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            ////最后mscorlib.dll会被重定向至C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

            //Assembly asmtest2 = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll");
            ////System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            ////最后System.dll会被重定向至C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System\v4.0_4.0.0.0__b77a5c561934e089\System.dll

            //Assembly asmtest3 = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Core.dll");
            ////System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            ////最后System.Core.dll会被重定向至C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Core\v4.0_4.0.0.0__b77a5c561934e089\System.Core.dll

            //Assembly asmtest4 = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Drawing.dll");
            ////System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
            ////最后System.Drawing.dll会被重定向至C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Drawing\v4.0_4.0.0.0__b03f5f7f11d50a3a\System.Drawing.dll

            //Assembly asmtest5 = Assembly.LoadFrom(@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Windows.Forms.dll");
            ////System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            ////最后System.Windows.Forms.dll会被重定向至C:\WINDOWS\Microsoft.Net\assembly\GAC_MSIL\System.Windows.Forms\v4.0_4.0.0.0__b77a5c561934e089\System.Windows.Forms.dll

            //referenceTypesIncludingGlobalsType.Add(asmtest);
            //var var = referenceTypesIncludingGlobalsType.Distinct();
            #endregion

            if (typeNamespaceImports != null)
            {
                IEnumerable<Assembly> assemblies2 = typeNamespaceImports.Select(p => p.Assembly).Distinct();
                assemblyReferences = assemblyReferences.Union(assemblies2).Distinct();
            }

            //耗时创建RosylnHost
            this.roslynHost = await Task.Run(() => CreateRosylnHost(assemblyReferences, typeNamespaceImports)).ConfigureAwait(true);

            //耗时UI初始化
            CreateNewEditor(this.roslynHost);

            this.foldingManager = FoldingManager.Install(this.editor.TextArea);
            this.foldingStrategy = new BraceFoldingStrategy();

            this.isInitialized = true;
        }

        private static CustomRoslynHost CreateCustomRosylnHost(IEnumerable<Assembly> assemblyReferences, IEnumerable<Type> typeNamespaceImports)
        {
            //var Imports = typeNamespaceImports.Select(p=>p.Namespace).Distinct();
            //var FinalImports = RoslynHostReferences.NamespaceDefault.Imports.Union(Imports).Distinct();

            var roslynHostReferences = RoslynHostReferences.NamespaceDefault.With(assemblyReferences: assemblyReferences, typeNamespaceImports: typeNamespaceImports);

            var roslynHost = new CustomRoslynHost(additionalAssemblies: new[]
                {
                    Assembly.Load("RoslynPad.Roslyn.Windows"),
                    Assembly.Load("RoslynPad.Editor.Windows"),  //这两句必加
                }, references: roslynHostReferences);

            return roslynHost;
        }

        private static RoslynHost CreateRosylnHost(IEnumerable<Assembly> assemblyReferences, IEnumerable<Type> typeNamespaceImports)
        {
            //var Imports = typeNamespaceImports.Select(p => p.Namespace).Distinct();
            //var FinalImports = RoslynHostReferences.NamespaceDefault.Imports.Union(Imports).Distinct();

            var roslynHostReferences = RoslynHostReferences.NamespaceDefault.With(assemblyReferences: assemblyReferences, typeNamespaceImports: typeNamespaceImports);

            var roslynHost = new RoslynHost(additionalAssemblies: new[]
                {
                    Assembly.Load("RoslynPad.Roslyn.Windows"),
                    Assembly.Load("RoslynPad.Editor.Windows"),  //这两句必加
                }, references: roslynHostReferences);

            return roslynHost;
        }

        private void CreateNewEditor(RoslynHost roslynHost)
        {
            var workingDirectory = Directory.GetCurrentDirectory();

            this.editor = new RoslynCodeEditor();
            this.documentId = this.editor.Initialize(roslynHost: roslynHost, highlightColors: new ClassificationHighlightColors(), workingDirectory: workingDirectory, documentText: String.Empty);

            this.editor.IsBraceCompletionEnabled = true;
            this.editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            //this.editor.FontFamily = new System.Windows.Media.FontFamily(this.Font.FontFamily.Name);
            //this.editor.FontSize = this.Font.Size * 96 / 72;
            this.editor.Options.HighlightCurrentLine = true;
            this.editor.Encoding = System.Text.Encoding.UTF8;
            this.editor.TextChanged += Editor_TextChanged;

            this.wpfEditorHost.Visible = true;
            this.wpfEditorHost.Child = this.editor;
        }

        /// <summary>
        /// 关闭编辑器
        /// </summary>
        public void Uninitialise()
        {
            if(!this.isInitialized) { return; }

            this.editor.TextChanged -= Editor_TextChanged;
            this.editor = null;
            this.roslynHost = null;
            this.documentId = null;

            this.wpfEditorHost.Visible = false;
            this.wpfEditorHost.Child = null;

            this.isInitialized = false;
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            if (this.isInitialized && this.foldingManager != null) //触发一次代码折叠
            {
                this.foldingStrategy.UpdateFoldings(foldingManager, editor.Document);
            }
        }

        const string singleLineCommentString = "//";

        /// <summary>
        /// 注释或取消注释选中内容
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task CommentUncommentSelectionAsync(CommentAction action) 
        {
            var document = this.roslynHost?.GetDocument(this.documentId);
            if (document == null)
            {
                return;
            }

            TextSpan selection = new TextSpan(this.editor.SelectionStart, this.editor.SelectionLength); //获取选中内容

            var documentText = await document.GetTextAsync().ConfigureAwait(false);
            var changes = new List<TextChange>();
            var lines = documentText.Lines.SkipWhile(x => !x.Span.IntersectsWith(selection))
               .TakeWhile(x => x.Span.IntersectsWith(selection)).ToArray();

            if (action == CommentAction.Comment)
            {
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(documentText.GetSubText(line.Span).ToString()))
                    {
                        changes.Add(new TextChange(new TextSpan(line.Start, 0), singleLineCommentString));
                    }
                }
            }
            else if (action == CommentAction.Uncomment)
            {
                foreach (var line in lines)
                {
                    var text = documentText.GetSubText(line.Span).ToString();
                    if (text.TrimStart().StartsWith(singleLineCommentString, StringComparison.Ordinal))
                    {
                        changes.Add(new TextChange(new TextSpan(
                            line.Start + text.IndexOf(singleLineCommentString, StringComparison.Ordinal),
                            singleLineCommentString.Length), string.Empty));
                    }
                }
            }

            if (changes.Count == 0) return;

            this.roslynHost.UpdateDocument(document.WithText(documentText.WithChanges(changes)));
            if (action == CommentAction.Uncomment)
            {
                await FormatDocumentAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 格式化文档
        /// </summary>
        /// <returns></returns>
        public async Task FormatDocumentAsync()
        {
            var document = this.roslynHost?.GetDocument(this.documentId);
            if (document == null)
            {
                return;
            }

            var formattedDocument = await Formatter.FormatAsync(document).ConfigureAwait(false);
            this.roslynHost.UpdateDocument(formattedDocument);
        }

        public void LoadFile(string csFileFullName) 
        {
            this.editor.OpenFile(csFileFullName);
        }

        public bool SaveFile()
        {
           return this.editor.SaveFile();
        }

        public string Text 
        {
            get => this.editor.Text;
            set => this.editor.Text = value;
        }

        public string FileName => this.editor.Document.FileName;

        public void Redo() => this.editor.Redo();

        public void Undo() => this.editor.Undo();
    }

    public enum CommentAction
    {
        Comment,
        Uncomment
    }
}
