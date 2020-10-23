using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Wechats.Utils
{
    /// <summary>导出模式</summary>
    [Flags, UI("数据导出方式")]
    internal enum ExportMode : int
    {
        [UI("不导出")] None = 0,
        [UI("简单")] Simple = 1,
        [UI("普通")] Normal = 2,
        [UI("详细")] Detail = 4,
        [UI("全部")] All = Simple | Normal | Detail,
        //[UI("模型")] Schema
    }

    /// <summary>页面访问模式</summary>
    [Flags, UI("页面访问模式")]
    internal enum PageMode : int
    {
        [UI("查看")] View = 1,
        [UI("新建")] New = 2,
        [UI("编辑")] Edit = 4,
        [UI("选择")] Select = 8,

        [UI("无")]   None = 0,
        [UI("全部")] All = View | New | Edit | Select,
    }

    /// <summary>视图类别</summary>
    [Flags, UI("视图类别")]
    internal enum ViewType
    {
        [UI("网格")] Grid,
        [UI("表单")] Form,
    }




    /// <summary>
    /// 数据模型描述
    /// </summary>
    internal interface IParam
    {
        /// <summary>名称</summary>
        string Name { get; set; }

        /// <summary>数据类型</summary>
        Type Type { get; set; }

        /// <summary>格式化字符串</summary>
        string Format { get; set; }

        /// <summary>是否只读</summary>
        bool ReadOnly { get; set; }

        /// <summary>是否必填</summary>
        bool Required { get; set; }

        /// <summary>长度</summary>
        int Length { get; set; }

        /// <summary>精度（小数类型）</summary>
        int Precision { get; set; }

        /// <summary>正则表达式</summary>
        string Regex { get; set; }

        /// <summary>默认值</summary>
        object Default { get; set; }

        /// <summary>允许的值</summary>
        Dictionary<string, object> Values { get; set; }
    }



    /// <summary>
    /// 参数信息
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    internal class ParamAttribute : TAttribute, IParam
    {
        //------------------------------------------------
        // IParam
        //------------------------------------------------
        /// <summary>名称</summary>
        public string Name { get; set; }

        /// <summary>参数类型</summary>
        public Type Type { get; set; }

        /// <summary>格式化字符串</summary>
        public string Format { get; set; }

        /// <summary>是否只读</summary>
        public bool ReadOnly { get; set; } = false;

        /// <summary>是否必填</summary>
        public bool Required { get; set; } = false;

        /// <summary>长度</summary>
        public int Length { get; set; } = -1;

        /// <summary>精度（小数类型）</summary>
        public int Precision { get; set; } = 2;

        /// <summary>正则表达式</summary>
        public string Regex { get; set; }

        /// <summary>在何种页面模式下显示该控件</summary>
        public PageMode Mode { get; set; } = PageMode.All;

        /// <summary>默认值</summary>
        public object Default { get; set; }

        /// <summary>宽度</summary>
        public int Width { get; set; } = -1;

        /// <summary>高度</summary>
        public int Height { get; set; } = -1;

        /// <summary>查询参数模板</summary>
        public string QueryString{ get; set; }

        //------------------------------------------------
        // 扩展信息
        //------------------------------------------------
        /// <summary>文本</summary>
        public string Text { get; set; }

        /// <summary>对应的文本字段</summary>
        public string TextField { get; set; } = "Name";

        /// <summary>对应的值字段</summary>
        public string ValueField { get; set; } = "ID";

        /// <summary>表现为树</summary>
        public bool Tree { get; set; } = false;

        /// <summary>数据导出时机</summary>
        public ExportMode Export { get; set; } = ExportMode.All;

        /// <summary>URL模板（弹窗或调整页面时有用）</summary>
        public string UrlTemplate { get; set; }

        /// <summary>URL地址模式</summary>
        public PageMode? UrlMode { get; set; }

        /// <summary>附属参数</summary>
        public string Tag { get; set; }

        /// <summary>弹窗大小</summary>
        public Size? WinSize { get; set; } = new Size(1000, 800);

        //------------------------------------------------
        // 参数值控制
        // 方式一：直接设置值字典   Values
        // 方式二：设置值对应的类型 ValueType，如有需要还要设置对应的 TextField 等
        //------------------------------------------------
        /// <summary>值类型（如long ProductID 的值类型等于 Product； 还需设置TextField=Name）</summary>
        public Type ValueType { get; set; }

        /// <summary>值类型对应的 UI 设置ID </summary>
        public long? ValueUIID { get; set; }

        /// <summary>允许的值</summary>
        public Dictionary<string, object> Values { get; set; }

        /// <summary>可选值说明</summary>
        public string ValueInfo 
        {
            get
            {
                if (Values != null)
                    return Values.ToJson();
                return this.Type?.GetEnumString();
            }
        }


        //------------------------------------------------
        // 只读属性
        //------------------------------------------------
        /// <summary>参数类型名</summary>
        public string TypeName => this.Type?.GetTypeString();


        //
        // 构造函数
        //
        public ParamAttribute() { }
        public ParamAttribute(string name, string title, bool required=false)
        {
            this.Name = name;
            this.Title = title;
            this.Required = required;
        }
        public ParamAttribute(string name, string title, Type type, bool required=false)
        {
            this.Name = name;
            this.Title = title;
            this.Type = type;
            this.Required = required;
        }

        //
        // 方法
        //
        /// <summary>格式化为文本</summary>
        public override string ToString()
        {
            return $"{Name} {TypeName} {Remark}";
        }
    }
}
