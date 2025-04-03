using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using InTheHand.Net.Sockets;

public class Program
{

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.Unicode;

        menu();
    }

    public static void menu()
    {
        string choice;
        do
        {
            Console.WriteLine("Виберіть опцію:");
            Console.WriteLine("0. Вихід");
            Console.WriteLine("1. Стрімінг відео з вебкамери");
            Console.WriteLine("2. Зробити фото з вебкамери");
            Console.WriteLine("3. Перетворити зображення в ASCII-арт");
            Console.WriteLine("4. Перетворити відео в ASCII-картину");
            Console.WriteLine("5. Визначити настрій з фото");
            Console.WriteLine("6. Визначити настрій з камери");
            Console.WriteLine("7. Голосовий асистент");
            Console.WriteLine("8. Робота з WiFi");
            Console.WriteLine("9. Робота з Bluetooth");
            Console.WriteLine("10. Відкрити посилання");
            Console.WriteLine("11. увімкнути музику");
            Console.WriteLine("12. відсоток жиру");
            Console.WriteLine("13. індекс маси тіла");

            choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    AsciiVideoStreamer.StartVideoStreamer();
                    break;
                case "2":
                    CaptureAndDisplayPhoto();
                    break;
                case "3":
                    AsciiArtConvert();
                    break;
                case "4":
                    AsciiVideoStreamer.ConvertVideoToAscii();
                    break;
                case "5":

                    Console.Write("Будь ласка, введіть шлях до фото:");
                    string imagePath = Console.ReadLine();

                    Console.WriteLine(EmotionRecognition.RecognizeFromImage(imagePath)); 
                    break;
                case "6":
                    EmotionRecognition.RecognizeFromWebcam(); 
                    break;
                case "7":
                    VoiceAssist();
                    break;
                case "8":
                    WiFiManager.MainMenu();
                    break;
                case "9":
                    Bluetooth bluetooth = new Bluetooth();
                    bluetooth.MainMenu(bluetooth);
                    break;
                case "10":
                    Console.Write("Будь ласка, введіть посилання:");
                    string link = Console.ReadLine();

                    Process.Start(link); 
                    break;
                case "11":
                    YouTubeApiPlayer.PlayRandomMusic();
                    break;
                case "12":
                    BodyFat();
                    break;
                case "13":
                    BodyMassIndex();
                    break;

                default:
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз."); //               

                    break;
            }
        } while (choice != "0" || choice != "7");
    }

    static void BodyFat()
    {
        Console.Write("Введіть стать (Чоловік(1)/Жінка(0)): ");
        string genderInput = Console.ReadLine().Trim().ToLower();
        bool isMale = genderInput == "1";

        Console.Write("Введіть вік: ");
        int age = int.Parse(Console.ReadLine());

        Console.Write(isMale ? "Введіть товщину шкірної складки на грудях (мм): " : "Введіть товщину шкірної складки на трицепсі (мм): ");
        double fold1 = double.Parse(Console.ReadLine());

        Console.Write(isMale ? "Введіть товщину шкірної складки на животі (мм): " : "Введіть товщину шкірної складки на боці живота (мм): ");
        double fold2 = double.Parse(Console.ReadLine());

        Console.Write("Введіть товщину шкірної складки на стегні (мм): ");
        double fold3 = double.Parse(Console.ReadLine());

        BodyState fatPercentage = new BodyState(isMale, age, fold1, fold2, fold3);
        
        Console.WriteLine($"Відсоток жиру в тілі: {fatPercentage.CalculateBodyFatPercentage()}%");
    }

    static void BodyMassIndex()
    {
        Console.Write( "Введіть вагу: ");
        float weight = float.Parse(Console.ReadLine());

        Console.Write("Введіть зріст (см): ");
        float height = float.Parse(Console.ReadLine()); 

        BodyState fatPercentage = new BodyState(weight, height);

        Console.WriteLine($"Відсоток жиру в тілі: {fatPercentage.CalculateIMB()}%");
    }


    static void VoiceAssist()
    {
        VoiceAssistant assistant = new VoiceAssistant();
        assistant.Voice();
    }

    static void AsciiArtConvert()
    {
        int width = 720;
        AsciiArtConverter converter = new AsciiArtConverter(width: width);

        Console.Write("Введіть шлях до зображення або URL: ");
        string imagePath = Console.ReadLine();

        converter.ConvertAndDisplay(imagePath);
    }
    static void CaptureAndDisplayPhoto()
    {
        using (var capture = new VideoCapture(0)) // Відкриваємо камеру
        {
            if (!capture.IsOpened())
            {
                Console.WriteLine("Не вдалося підключитися до веб-камери.");
                return;
            }

            Mat frame = new Mat();
            capture.Read(frame); // Отримуємо кадр

            if (!frame.Empty())
            {
                // Створюємо конвертер зі зменшеним розміром ASCII-арту
                AsciiArtConverter converter = new AsciiArtConverter(width: 480); // Зменшуємо ширину для кращого відображення
                string asciiArt = converter.ConvertToAscii(frame);
                converter.DisplayAsciiArt(asciiArt);
            }
            else
            {
                Console.WriteLine("Не вдалося отримати кадр з камери.");
            }

            frame.Dispose();
        }
    }


}