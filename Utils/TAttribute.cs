using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 标题文本相关信息（支持国际化）
    /// </summary>
    internal class TAttribute : Attribute
    {
        /// <summary>标题</summary>
        public string Title { get; set; }

        /// <summary>分组</summary>
        public string Group { get; set; }

        /// <summary>备注</summary>
        public string Remark { get; set; }

        //
        public TAttribute() { }
        public TAttribute(string title) : this("", title) { }
        public TAttribute(string group, string title)
        {
            this.Group = group;
            this.Title = title;
        }
    }

}
