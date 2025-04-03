using System;
using System.Diagnostics;
using System.Text;

public static class EmotionRecognition
{
    public static string RecognizeFromImage(string imagePath)
    {
        string pythonScriptPath = @"EmotionRecognitionImage.py"; // Вкажи повний шлях, якщо потрібно
        string pythonExePath = @"python"; // C:\Users\YourUser\AppData\Local\Programs\Python\Python39\python.exe

        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = pythonExePath,
            Arguments = $"-u \"{pythonScriptPath}\" --image \"{imagePath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using (Process process = Process.Start(start))
        {
            using (System.IO.StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit(); // Дочекатися завершення процесу

                return result.Trim();
            }
        }
    }

    public static void RecognizeFromWebcam()
    {
        string pythonScriptPath = @"EmotionRecognition.py";  // Шлях до Python-скрипта
        string pythonExePath = "python";  // Або "python3" на Linux/Mac

        ProcessStartInfo start = new ProcessStartInfo
        {
            FileName = pythonExePath,
            Arguments = pythonScriptPath,
            UseShellExecute = false,
            RedirectStandardOutput = false,  // Відео не можна перенаправити в консоль
            CreateNoWindow = false          // Показувати вікно (для OpenCV)
        };

        using (Process process = Process.Start(start))
        {
            process.WaitForExit();  // Чекаємо, поки користувач закриє вікно
        }
    }
}