using OpenCvSharp;
using System.Text;
using System;
using System.Diagnostics;
using System.IO;

public class AsciiArtConverter
{
    private int _width; // Ширина ASCII-арту
    private double _aspectRatio; // Співвідношення сторін
    private string _gradient; // Градієнт для ASCII-арту

    public AsciiArtConverter(int width = 200, double aspectRatio = 0.5625)
    {
        _width = width;
        _aspectRatio = aspectRatio;
        _gradient = " .:-=+*#%@4H9W8$@"; // Більш різноманітний градієнт
    }

    // Завантаження зображення з файлу або URL
    public Mat LoadImage(string imagePath)
    {
        Mat image;
        if (imagePath.StartsWith("http://") || imagePath.StartsWith("https://"))
        {
            // Завантаження зображення з URL
            using (var webClient = new System.Net.WebClient())
            {
                byte[] imageBytes = webClient.DownloadData(imagePath);
                image = Cv2.ImDecode(imageBytes, ImreadModes.Color);
            }
        }
        else
        {
            // Завантаження зображення з файлу
            image = Cv2.ImRead(imagePath, ImreadModes.Color);
        }

        if (image.Empty())
        {
            throw new Exception("Не вдалося завантажити зображення.");
        }

        return image;
    }

    // Зміна розміру зображення для ASCII-арту
    public Mat ResizeImage(Mat image)
    {
        int height = (int)(_width * _aspectRatio * (image.Height / (double)image.Width));
        Mat resizedImage = new Mat();
        Cv2.Resize(image, resizedImage, new Size(_width, height), interpolation: InterpolationFlags.Nearest);
        return resizedImage;
    }

    // Перетворення зображення в ASCII-арт
    public string ConvertToAscii(Mat image)
    {
        StringBuilder asciiArt = new StringBuilder();

        // Зменшуємо розмір зображення
        Mat resizedImage = ResizeImage(image);

        // Перетворюємо пікселі в ASCII-символи
        for (int y = 0; y < resizedImage.Height; y++)
        {
            for (int x = 0; x < resizedImage.Width; x++)
            {
                Vec3b pixel = resizedImage.At<Vec3b>(y, x);
                int brightness = (int)(0.249 * pixel[2] + 0.587 * pixel[1] + 0.214 * pixel[0]); // Правильна формула для яскравості
                int gradientIndex = brightness * (_gradient.Length - 1) / 255;
                asciiArt.Append(_gradient[gradientIndex]);
            }
            asciiArt.AppendLine(); // Перехід на новий рядок
        }

        // Звільняємо ресурси
        resizedImage.Dispose();

        return asciiArt.ToString();
    }

    // Виведення ASCII-арту в консоль
    public void DisplayAsciiArt(string asciiArt)
    {
        Console.Clear();
        Console.WriteLine(asciiArt);
    }

    // Основна функція для завантаження, обробки та виведення ASCII-арту
    public void ConvertAndDisplay(string imagePath)
    {
        try
        {
            Mat image = LoadImage(imagePath);
            string asciiArt = ConvertToAscii(image);
            DisplayAsciiArt(asciiArt);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка: {ex.Message}");
        }
    }
}