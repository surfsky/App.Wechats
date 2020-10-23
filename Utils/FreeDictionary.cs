using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 可安全访问的字典。对于dict["key"], 如果键不存在则返回null，而不报异常
    /// </summary>
    internal class FreeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        /// <summary>获取或设置查询字符串成员</summary>
        public new TValue this[TKey key]
        {
            get
            {
                if (this.Keys.Contains(key))
                    return base[key];
                return default(TValue);
            }
            set { base[key] = value; }
        }

        /// <summary>转化为查询字符串</summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var key in this.Keys)
                sb.AppendFormat("{0}={1}&", key, this[key]);
            return sb.ToString().TrimEnd('&');
        }
    }

}
