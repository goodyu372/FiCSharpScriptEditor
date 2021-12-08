using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FiCSharpScriptEditor
{
    public static class ReferenceManager
    {
        private const string filename = "ListReferences.xml"; //序列化文件保存位置

        public const string System_GAC_Reference_Directory = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\";

        public const string Local_Reference_Directory = @"D:\Fi\";

        public static bool bInitialized = false;

        /// <summary>
        /// 保存自定义引用dll全名称的列表
        /// </summary>
        public static List<ReferenceItem> ListReferences = new List<ReferenceItem>();

        public static List<Assembly> ListAssemblyReferences { get; private set; } = new List<Assembly>();

        public static List<string> ListAssemblyNameReferences { get; private set; } = new List<string>();

        public static List<string> ListAssemblyLocationReferences { get; private set; } = new List<string>();

        public static readonly Type[] DefaultTypesForNamespaces = new[]
        {
            typeof(int), //mscorlib.dll
            typeof(System.Threading.Thread), //mscorlib.dll
            typeof(System.Threading.Tasks.Task), //mscorlib.dll
            typeof(System.Threading.CancellationTokenSource), //mscorlib.dll
            typeof(System.Threading.ManualResetEvent), //mscorlib.dll
            typeof(System.Collections.ArrayList), //mscorlib.dll
            typeof(System.Collections.Generic.Comparer<object>), //mscorlib.dll
            typeof(System.Text.ASCIIEncoding), //mscorlib.dll
            typeof(System.Text.RegularExpressions.Capture), //System.dll
            typeof(System.Linq.Enumerable), //System.Core.dll
            typeof(System.IO.Directory), //mscorlib.dll
            typeof(System.IO.BinaryReader), //mscorlib.dll
            typeof(System.Reflection.Assembly), //mscorlib.dll
            typeof(System.Drawing.Point), //System.Drawing.dll
            typeof(System.Drawing.Color), //System.Drawing.dll
            typeof(System.Windows.Forms.Form), //System.Windows.Forms.dll
            typeof(System.Collections.Generic.List<int>),  //mscorlib.dll  (System.Collections.Generic),
            typeof(System.Array), //mscorlib.dll
            typeof(System.Net.Sockets.Socket), //System.dll
            typeof(System.Net.Sockets.TcpClient), //System.dll
        };

        public static void AddReference(string ReferenceDLLFileFullName, ReferenceType type) 
        {
            if (!ListReferences.Any(p => p.Location.Equals(ReferenceDLLFileFullName)))
            {
                ReferenceItem item = new ReferenceItem() { Type = type, Name = Path.GetFileNameWithoutExtension(ReferenceDLLFileFullName), Location = ReferenceDLLFileFullName };
                ListReferences.Add(item);
                UpdateReferencesList();
            }
        }

        public static void RemoveReference(int index)
        {
            ListReferences.RemoveAt(index);
            UpdateReferencesList();
        }

        public static void CheckExistReferences() 
        {
            foreach (var item in ListReferences)
            {
                item.Exist = File.Exists(item.Location);
            }
        }

        public static void UpdateReferencesList() 
        {
            ListAssemblyReferences = GetAssemblies();
            ListAssemblyNameReferences = GetAssemblyNames();
            ListAssemblyLocationReferences = GetAssemblyLocations();
        }

        private static List<Assembly> GetAssemblies() 
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var item in ListReferences)
            {
                if (File.Exists(item.Location))
                {
                    Assembly asm = Assembly.LoadFrom(item.Location); //注意这里不能用Load 可能会抛找不到依赖项异常
                    assemblies.Add(asm);
                }
            }
            assemblies = assemblies.Distinct().ToList();  //对Assembly去重 因为有可能出现重定向的情况
            return assemblies;
        }

        private static List<string> GetAssemblyNames()
        {
            return GetAssemblies().Select(p => p.FullName).Distinct().ToList();  //对AssemblyName去重
        }

        private static List<string> GetAssemblyLocations()
        {
            List<string> assemblies = new List<string>();
            foreach (var item in ListReferences)
            {
                if (File.Exists(item.Location))
                {
                    assemblies.Add(item.Location);
                }
            }
            assemblies = assemblies.Distinct().ToList();  //对路径去重
            return assemblies;
        }

        public static void AddDefaultDllReferences() 
        {
            AddReference($"{System_GAC_Reference_Directory}mscorlib.dll",ReferenceType.System_GAC);
            AddReference($"{System_GAC_Reference_Directory}System.dll", ReferenceType.System_GAC);
            AddReference($"{System_GAC_Reference_Directory}System.Core.dll", ReferenceType.System_GAC);
            AddReference($"{System_GAC_Reference_Directory}System.Drawing.dll", ReferenceType.System_GAC);
            AddReference($"{System_GAC_Reference_Directory}System.Windows.Forms.dll", ReferenceType.System_GAC);
            AddReference($"{Local_Reference_Directory}Fi.Core.dll", ReferenceType.Local);
            AddReference($@"{Local_Reference_Directory}App\Common\MotionServer.dll", ReferenceType.Local);
            AddReference($@"{Local_Reference_Directory}App\Common\FiInterface.dll", ReferenceType.Local);
        }

        public static void LoadData()
        {
            try
            {
                if (!File.Exists(filename))
                {
                    return;
                }
                using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    Type type = typeof(List<ReferenceItem>);
                    XmlSerializer serializer = new XmlSerializer(type);
                    ListReferences = serializer.Deserialize(fs) as List<ReferenceItem>;
                    CheckExistReferences();
                }
                bInitialized = true;
            }
            catch (Exception ex)
            {
                //throw ex;
            }
            finally 
            {
                if (ListReferences.Count == 0)
                {
                    AddDefaultDllReferences();
                    SaveData();
                }
                UpdateReferencesList();
            }
        }

        public static void SaveData()
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite))
                {
                    Type type = typeof(List<ReferenceItem>);
                    XmlSerializer serializer = new XmlSerializer(type);
                    serializer.Serialize(fs, ListReferences);
                }
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }
    }

    /// <summary>
    /// 引用项
    /// </summary>
    public class ReferenceItem 
    {
        /// <summary>
        /// 引用位置类型
        /// </summary>
        public ReferenceType Type { get; set; } = ReferenceType.Local;

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool Exist { get; set; } = true;

        /// <summary>
        /// 路径
        /// </summary>
        public string Location { get; set; } = string.Empty;
    }

    public enum ReferenceType 
    {
        Local = 0,
        System_GAC
    }
}
