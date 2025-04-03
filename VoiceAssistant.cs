using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows;

public class VoiceAssistant
{
    private static SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();
    private static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
    private static bool isWaitingForInput;
    private static bool exit;
    private static Bluetooth bluetoothConnector = new Bluetooth();

    public VoiceAssistant()
    {
        isWaitingForInput = false;

    }

    public bool Exit { get => exit; }

    public void Voice()
    {
        synthesizer.Speak("Hello! I am your voice assistant.");
        Console.WriteLine("Voice assistant is active. Speak a command...");

        // Встановлення англійської мови для розпізнавання
        recognizer.SetInputToDefaultAudioDevice();
        recognizer.SpeechRecognized += Recognizer_SpeechRecognized;

        // Додавання команд
        Choices commands = new Choices();
        commands.Add(new string[]
        {
            "hello",
            "how are you",
            "what time is it",
            "stop",
            "scan wifi",
            "connect to wifi",
            "disconnect wifi",
            "detect mood from image",
            "detect mood from webcam",
            "convert image to ascii",
            "convert video to ascii",
            "start video stream",
            "take photo",
            "open menu",
            "connect bluetooth",
            "scan bluetooth",
            "disconnect bluetooth",
            "play music",
            "open link",
        }); 

        GrammarBuilder grammarBuilder = new GrammarBuilder();
        grammarBuilder.Culture = new System.Globalization.CultureInfo("en-US"); // Встановлення культури на англійську
        grammarBuilder.Append(commands);

        Grammar grammar = new Grammar(grammarBuilder);
        recognizer.LoadGrammar(grammar);

        recognizer.RecognizeAsync(RecognizeMode.Multiple);

        while (!exit) // Головний цикл роботи помічника
        {
            if (isWaitingForInput)
            {
                Console.Write("Enter input: ");
                string input = "";
                input = Console.ReadLine();
                HandleManualInput(input);
            }
        }
    }
    private void HandleManualInput(string input)
    {
        isWaitingForInput = false;
        Console.WriteLine($"Processing input: {input}");
    }

    private static void Recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        if (isWaitingForInput) return;

        string command = e.Result.Text;
        Console.WriteLine($"You said: {command}");

        switch (command.ToLower())
        {
            case "open menu":
                OpenMenu();
                break;
            case "hello":
                Respond("Hello! How can I help you?");
                break;
            case "how are you":
                Respond("I'm tired boss...");
                break;
            case "what time is it":
                Respond($"The current time is {DateTime.Now:HH:mm}");
                break;
            case "stop":
                Respond("Goodbye! Shutting down.");
                recognizer.RecognizeAsyncStop();
                exit = true;
                //Environment.Exit(0);
                break;
            case "scan wifi":
                ScanWifi();
                break;
            case "connect to wifi":
                ConnectToWifi();
                break;
            case "disconnect wifi":
                DisconnectWifi();
                break;
            case "detect mood from image":
                DetectMoodFromImage();
                break;
            case "detect mood from webcam":
                DetectMoodFromWebcam();
                break;
            case "convert image to ascii":
                ConvertImageToAscii();
                break;
            case "convert video to ascii":
                ConvertVideoToAscii();
                break;
            case "start video stream":
                StartVideoStream();
                break;
            case "take photo":
                Respond("Taking photo. Say Floccinaucinihilipilification."); //Floccinaucinihilipilification is the act of regarding something as unimportant or as worthless. It is commonly used in a humorous way.
                takePhoto();
                break;
            case "connect bluetooth":
                ConnectBluetooth();
                break;
            case "scan bluetooth":
                ScanBluetooth();
                break;
            case "disconnect bluetooth":
                DisconnectBluetooth();
                break;
            case "play music":
                YouTubeApiPlayer.PlayRandomMusic();
                break;
            case "open link":
                Respond("Enter link, Boss:");
                string link = Console.ReadLine();

                Process.Start(link);
                break;
            default:
                Respond("I don't understand this command.");
                break;
        }
    }

    static void takePhoto()
    {

        using (var capture = new VideoCapture(0)) // Відкриваємо камеру
        {
            if (!capture.IsOpened())
            {
                Respond("Failed to connect to webcam.");
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
                Respond("Failed to ge frame from camera.");
            }

            frame.Dispose();
        }
    }

    private static void Respond(string message)
    {
        Console.WriteLine($"Assistant: {message}");
        synthesizer.Speak(message);
    }

    private static void ScanWifi()
    {
        Respond(WiFiManager.ShowAvailableNetworks());
    }

    private static void ConnectToWifi()
    {
        isWaitingForInput = true;
        recognizer.RecognizeAsyncCancel(); // Зупиняємо розпізнавання голосу

        Console.Write("Enter Wi-Fi SSID: ");
        string ssid = Console.ReadLine();
        Console.Write("Enter Wi-Fi Password: ");
        string password = Console.ReadLine();

        WiFiManager.ConnectToWiFi(ssid, password);
        Respond($"Trying to connect to {ssid}.");

        isWaitingForInput = false;
        recognizer.RecognizeAsync(RecognizeMode.Multiple); // Відновлюємо розпізнавання
    }

    private static void DisconnectWifi()
    {
        WiFiManager.DisconnectWiFi();
        Respond("Disconnected from Wi-Fi.");
    }

    private static void DetectMoodFromImage()
    {
        isWaitingForInput = true;
        recognizer.RecognizeAsyncCancel(); // Зупиняємо розпізнавання

        Respond("Please enter the path to the image:");
        string imagePath = Console.ReadLine();

        string emotion = EmotionRecognition.RecognizeFromImage(imagePath);
        Respond("Emotion on image - " + emotion);

        isWaitingForInput = false;
        recognizer.RecognizeAsync(RecognizeMode.Multiple); // Відновлюємо розпізнавання
    }

    private static void DetectMoodFromWebcam()
    {
        Respond("Starting mood detection from webcam...");
        EmotionRecognition.RecognizeFromWebcam();
    }

    private static void ConvertImageToAscii()
    {
        isWaitingForInput = true;
        recognizer.RecognizeAsyncCancel(); // Зупиняємо розпізнавання

        Respond("Please enter the path to the image:");
        string imagePath = Console.ReadLine();

        AsciiArtConverter converter = new AsciiArtConverter(width: 720);
        converter.ConvertAndDisplay(imagePath);
        Respond("Image converted to ASCII.");

        isWaitingForInput = false;
        recognizer.RecognizeAsync(RecognizeMode.Multiple); // Відновлюємо розпізнавання
    }

    private static void ConvertVideoToAscii()
    {
        Respond("Starting video to ASCII conversion...");
        AsciiVideoStreamer.StartVideoConverter();
    }

    private static void StartVideoStream()
    {
        Respond("Starting video stream...");
        AsciiVideoStreamer.StartVideoStreamer();
    }

    private static void ConnectBluetooth()
    {
        Respond("Connecting to Bluetooth...");
        bluetoothConnector.bluetoothConnect(bluetoothConnector);
    }
    private static void ScanBluetooth()
    {
        Respond("Scanning Bluetooth...");
        bluetoothConnector.ShowAvailableBluetoothDevices(bluetoothConnector);
    }
    private static void DisconnectBluetooth()
    {
        Respond("Disconnecting Bluetooth...");
        bluetoothConnector.bluetoothDisonnect(bluetoothConnector);
    }

    private static void OpenMenu()
    {
        Respond("Opening menu...");
        recognizer.RecognizeAsyncCancel(); // Зупиняємо розпізнавання
        Program.menu();
    }
}