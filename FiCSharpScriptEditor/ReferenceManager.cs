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
        /// <summary>
        /// 保存自定义引用dll全名称的列表
        /// </summary>
        public static List<ReferenceItem> ListReferences = new List<ReferenceItem>();

        public static void AddReference(string ReferenceDLLFileFullName, ReferenceType type) 
        {
            if (!ListReferences.Any(p=>p.Location.Equals(ReferenceDLLFileFullName)))
            {
                ReferenceItem item = new ReferenceItem() { Type = type, Name = Path.GetFileNameWithoutExtension(ReferenceDLLFileFullName), Location = ReferenceDLLFileFullName };
                ListReferences.Add(item);
            }
        }

        public static void RemoveReference(int index)
        {
            ListReferences.RemoveAt(index);
        }

        public static void CheckValid() 
        {
            foreach (var item in ListReferences)
            {
                item.Exist = File.Exists(item.Location);
            }
        }

        public static List<Assembly> GetAssemblies() 
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (var item in ListReferences)
            {
                if (File.Exists(item.Location))
                {
                    Assembly asm = Assembly.Load(item.Location);
                    assemblies.Add(asm);
                }
            }
            return assemblies;
        }

        public static List<Assembly> GetValidAssemblies() 
        {
            return GetAssemblies().Distinct().ToList();  //对Assembly去重 因为有可能出现重定向的情况
        }

        private const string filename = "ListReferences.xml";

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
                    CheckValid();
                }
            }
            catch (Exception ex)
            {
                //throw ex;
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
