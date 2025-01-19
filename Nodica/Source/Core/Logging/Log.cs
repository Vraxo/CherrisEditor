namespace Cherris;

public class Log
{
    private readonly static ConsoleColor infoColor = ConsoleColor.DarkGray;
    private readonly static ConsoleColor warningColor = ConsoleColor.Yellow;
    private readonly static ConsoleColor errorColor = ConsoleColor.Red;

    public static void Info(string message, string condition = "ClickManager/ViableClickable")
    {
        if (LogSettings.Instance.GetLogCondition("Info") && LogSettings.Instance.GetLogCondition(condition))
        {
            Console.ForegroundColor = infoColor;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [INFO] {message}");
            Console.ResetColor();
        }
    }

    public static void Warning(string message, string condition = "ClickManager/ViableClickable")
    {
        if (LogSettings.Instance.GetLogCondition("Warning") && LogSettings.Instance.GetLogCondition(condition))
        {
            Console.ForegroundColor = warningColor;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [WARNING] {message}");
            Console.ResetColor();
        }
    }

    public static void Error(string message, string condition = "ClickManager/ViableClickable")
    {
        if (LogSettings.Instance.GetLogCondition("Error") && LogSettings.Instance.GetLogCondition(condition))
        {
            Console.ForegroundColor = errorColor;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [ERROR] {message}");
            Console.ResetColor();
        }
    }
}