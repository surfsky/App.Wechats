using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 列类型
    /// </summary>
    internal enum ColumnType
    {
        [UI("不显示")]     None,
        [UI("自动")]       Auto,
        [UI("文本")]       Text,
        [UI("枚举")]       Enum,
        [UI("布尔")]       Bool,
        [UI("链接 ")]      Link,
        [UI("图片")]       Image,
        [UI("日期")]       Date,
        [UI("时间日期")]   DateTime,
        [UI("时间")]       Time,
        [UI("图标")]       Icon,
        [UI("文件")]       File,
        [UI("弹窗")]       Win,
        [UI("弹窗网格")]   WinGrid,
        [UI("弹窗表单")]   WinForm,
    }

    /// <summary>
    /// 编辑器类型
    /// </summary>
    internal enum EditorType
    {
        [UI("不显示")]       None,
        [UI("自动选择")]     Auto,

        //
        [UI("标签")]         Label,
        [UI("文本框 ")]      Text,
        [UI("多行文本框")]   TextArea,
        [UI("HTML编辑框")]   Html,
        [UI("MD编辑框")]     Markdown,
        [UI("数字框")]       Number,
        [UI("GPS位置")]      GPS,

        //
        [UI("日期选择")]     Date,
        [UI("时间选择")]     Time,
        [UI("日期时间选择")] DateTime,

        //
        [UI("图片选择")]     Image,
        [UI("文件选择")]     File,
        [UI("图片列表")]     Images,
        [UI("文件列表")]     Files,

        //
        [UI("枚举下拉框")]   Enum,
        [UI("枚举组合框")]   EnumGroup,

        //
        [UI("布尔下拉框")]   Bool,
        [UI("布尔选择器")]   BoolGroup,

        // 
        [UI("内嵌面板")]     Panel,
        [UI("内嵌表格")]     Grid,

        // 弹窗类（具体是DropDownList、ActionSheet、弹窗、切屏，由客户端自己去决定，此处统一命名为 WinXXX）
        [UI("弹窗选择")]     Win,       //  指定url
        [UI("弹出网格")]     WinGrid,   //  自动网格
        [UI("弹出列表")]     WinList,
        [UI("弹出树")]       WinTree,
    }


    /// <summary>
    /// UI 外观信息
    /// </summary>
    internal class UIAttribute : ParamAttribute
    {
        //
        // 表单模式属性
        //
        /// <summary>表单模式下的编辑控件</summary>
        public EditorType Editor { get; set; } = EditorType.Auto;

        //
        // 列模式属性
        //
        /// <summary>列模式下的展示方式</summary>
        public ColumnType Column { get; set; } = ColumnType.Auto;

        /// <summary>列模式下的列宽</summary>
        public int ColumnWidth { get; set; } = 0;

        /// <summary>排序方向（true 正序 | false 逆序）</summary>
        public bool? Sort { get; set; }

        /// <summary>对应的字段信息</summary>
        [JsonIgnore]
        public PropertyInfo Field { get; set; }

        /// <summary>标题全称</summary>
        public string FullTitle
        {
            get
            {
                if (string.IsNullOrEmpty(Group)) return Title;
                else return string.Format("{0}-{1}", Group, Title);
            }
        }


        //
        // 构造函数
        //
        public UIAttribute() { }
        public UIAttribute(string title) : this("", title, "") { }
        public UIAttribute(string group, string title, string formatString="")
        {
            this.Group = group;
            this.Title = title;
            this.Format = formatString;
        }
        public UIAttribute(string title, ExportMode export) : this("", title, export) { }
        public UIAttribute(string group, string title, ExportMode export)
        {
            this.Group = group;
            this.Title = title;
            this.Export = export;
        }
        public UIAttribute(string title, Type valueType) : this("", title, valueType) { }
        public UIAttribute(string group, string title, Type valueType)
        {
            this.Group = group;
            this.Title = title;
            this.ValueType = valueType;
        }


        //
        // 方法
        //
        /// <summary>格式化为文本</summary>
        public override string ToString()
        {
            return this.FullTitle;
        }

        // 链式表达式
        public UIAttribute SetEditor(EditorType editor, object tag=null)
        {
            this.Editor = editor;
            this.Tag = tag.ToJson();
            return this;
        }
        public UIAttribute SetColumn(ColumnType column, int? width = null, string title = "", bool? sort = null, object tag = null)
        {
            if (width != null)
                this.ColumnWidth = width.Value;
            if (title.IsNotEmpty())
                this.Title = title;
            this.Column = column;
            this.Sort = sort;
            this.Tag = tag.ToJson();
            return this;
        }
        public UIAttribute SetValues(Type valueType)
        {
            this.ValueType = ValueType;
            return this;
        }
        public UIAttribute SetValues(Dictionary<string, object> dict)
        {
            this.Values = dict;
            return this;
        }
        public UIAttribute SetMode(PageMode mode)
        {
            this.Mode = mode;
            return this;
        }
        public UIAttribute SetTitle(string title)
        {
            this.Title = title;
            return this;
        }
    }
}