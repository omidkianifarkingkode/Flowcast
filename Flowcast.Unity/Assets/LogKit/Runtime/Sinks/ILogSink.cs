namespace LogKit.Sinks
{
    public interface ILogSink
    {
        void Emit(LogEvent logEvent);
    }
}
