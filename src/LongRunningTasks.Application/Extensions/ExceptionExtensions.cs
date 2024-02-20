namespace LongRunningTasks.Application.ExtensionsN
{
    public static class ExceptionExtensions
    {
        public static string GetFullMessage(this Exception ex) =>
            ex.Message + Environment.NewLine +
            (ex.InnerException != null ? ex.InnerException.Message + Environment.NewLine : string.Empty) +
            ex.StackTrace;
    }
}