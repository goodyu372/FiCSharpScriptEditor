using System;

namespace CDS.CSharpScripting
{
    /// <summary>
    /// Default settings and data for the project
    /// </summary>
    public static class Defaults
    {
        /// <summary>
        /// The default types used for namespaces and assembly references.
        /// </summary>
        /// <remarks>
        /// For each the namespace will be automatically made available. For example,
        /// this means that Thread can be Rectangle can be declared without requiring
        /// 'using System.Drawing;', and the System.Drawing.dll is automatically
        /// referenced.
        /// </remarks>
        public static readonly Type[] TypesForNamespacesAndAssemblies = new[]
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
            //typeof(System.Drawing.Point), //System.Drawing.dll
            typeof(System.Windows.Forms.Form), //System.Windows.Forms.dll
            typeof(System.Collections.Generic.List<int>),  //mscorlib.dll  (System.Collections.Generic),
            typeof(System.Array), //mscorlib.dll
            typeof(System.Net.Sockets.Socket), //System.dll
            typeof(System.Net.Sockets.TcpClient), //System.dll
        };
    }
}
