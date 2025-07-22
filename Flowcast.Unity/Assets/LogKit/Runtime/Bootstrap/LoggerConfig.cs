using UnityEngine;

namespace LogKit.Bootstrap
{
    [CreateAssetMenu(fileName = "LoggerConfig", menuName = "Logkit/Create Logger Config")]
    public class LoggerConfig : ScriptableObject
    {
        public LoggerOptions Options;
    }
}
