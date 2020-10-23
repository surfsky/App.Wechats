using System;
using System.Collections.Generic;
//using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Wechats.Utils
{
    /// <summary>
    /// 类库配置信息
    /// </summary>
    internal class UtilConfig
    {
        //---------------------------------------------
        // 单例
        //---------------------------------------------
        private static UtilConfig _cfg;
        public static UtilConfig Instance
        {
            get
            {
                if (_cfg == null)
                    _cfg = new UtilConfig();
                return _cfg;
            }
        }


        //---------------------------------------------
        // 属性
        //---------------------------------------------
        /// <summary>是否启用国际化支持（使用资源文件获取文本）</summary>
        public bool UseGlobal { get; set; } = false;

        /// <summary>资源类型名称</summary>
        public Type ResType { get; set; }

        /// <summary>机器ID（用于SnowflakerID生成）</summary>
        public int MachineId { get; set; } = 1;


        //---------------------------------------------
        // 事件
        //---------------------------------------------
        /// <summary>日志事件</summary>
        public event Action<string, string, int> OnLog;

        /// <summary>做日志（需配置 OnLog 事件)</summary>
        public static void Log(string type, string info, int level=0)
        {
            if (Instance.OnLog != null)
                Instance.OnLog(type, info, level);
        }
    }
}
