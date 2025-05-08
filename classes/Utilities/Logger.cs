public static class Logger
{
    public static void LogToFile(string? filePath, string content)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found!");
        }
        File.WriteAllText(filePath, content);
    }
}