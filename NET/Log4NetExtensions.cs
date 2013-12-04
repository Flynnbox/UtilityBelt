using System;
using log4net;

namespace UtilityBelt
{
  public static class Log4NetExtensions
  {
    public static void Debug(this ILog log, Func<string> formattingCallback)
    {
      if (log.IsDebugEnabled)
      {
        log.Debug(formattingCallback());
      }
    }
    public static void Info(this ILog log, Func<string> formattingCallback)
    {
      if (log.IsInfoEnabled)
      {
        log.Info(formattingCallback());
      }
    }
    public static void Warn(this ILog log, Func<string> formattingCallback)
    {
      if (log.IsWarnEnabled)
      {
        log.Warn(formattingCallback());
      }
    }
    public static void Error(this ILog log, Func<string> formattingCallback)
    {
      if (log.IsErrorEnabled)
      {
        log.Error(formattingCallback());
      }
    }
    public static void Fatal(this ILog log, Func<string> formattingCallback)
    {
      if (log.IsFatalEnabled)
      {
        log.Fatal(formattingCallback());
      }
    }
  }
}
