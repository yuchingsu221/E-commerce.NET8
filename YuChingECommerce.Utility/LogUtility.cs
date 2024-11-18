using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System.Diagnostics;

namespace YuChingECommerce.Utility
{
    public class LogUtility
    {
        private static readonly Logger m_Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 記錄資訊
        /// </summary>
        /// <param name="logMsg"></param>
        /// <param name="logTag"></param>
        public static void LogInfo(string logMsg, string logTag = null)
        {
            if (string.IsNullOrWhiteSpace(logTag))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                var className = method.ReflectedType.Name;
                logTag = $"{className}.{method.Name}";
            }
            var msg = $"[{logTag}] || {logMsg}";

            m_Logger.Info(logMsgToJson(msg));
        }

        /// <summary>
        /// Trace Debug使用
        /// </summary>
        /// <param name="logMsg"></param>
        /// <param name="logTag"></param>

        public static void LogDebug(string logMsg, string logTag = null)
        {
            if (string.IsNullOrWhiteSpace(logTag))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                var className = method.ReflectedType.Name;
                logTag = $"{className}.{method.Name}";
            }
            var msg = $"[{logTag}] || {logMsg}";

            m_Logger.Debug(logMsgToJson(msg));
        }

        /// <summary>
        /// 記錄程式錯誤
        /// </summary>
        /// <param name="logMsg"></param>
        /// <param name="exception"></param>
        /// <param name="logTag"></param>

        public static void LogError(string logMsg, Exception exception = null, string logTag = null)
        {
            if (string.IsNullOrWhiteSpace(logTag))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                var className = method.ReflectedType.Name;
                logTag = $"{className}.{method.Name}";
            }
            var msg = $"[{logTag}] || {logMsg} || {JsonConvert.SerializeObject(exception)}";

            m_Logger.Error(logMsgToJson(msg));
        }

        /// <summary>
        /// 紀錄資料庫錯誤資訊
        /// </summary>
        /// <param name="sqlCommand"></param>
        /// <param name="sqlParameter"></param>
        /// <param name="exception"></param>
        /// <param name="logTag"></param>
        public static void LogDBError(string sqlCommand, string sqlParameter, Exception exception, string logTag = null)
        {
            if (string.IsNullOrWhiteSpace(logTag))
            {
                var method = new StackTrace().GetFrame(1).GetMethod();
                var className = method.ReflectedType.Name;
                logTag = $"{className}.{method.Name}";
            }

            var msg = $"[{logTag}] || SQL Excepiton!";
                //$"  || 錯誤原因:{exception.Message}";
                //$" || Command:\r\n{sqlCommand}";
                //$" || Parameter:\r\n{sqlParameter}";

            m_Logger.Error(logMsgToJson(msg));
        }

        /**
	     * Log Forging 漏洞校驗
	     * @param logs
	     * @return
	     */
        public static string vaildLog(String logs)
        {
            return logs.Replace("\r", "_").Replace("\n", "_");
        }

        public static string logMsgToJson(string log)
        {
            dynamic obj = new JObject();
            obj.LOGMSG = vaildLog(log);
            
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return jsonString;
        }
    }
}
