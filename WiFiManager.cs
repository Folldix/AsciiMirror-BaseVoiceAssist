using System;
using System.Diagnostics;
using System.Text;

public static class WiFiManager
{
    public static void MainMenu()
    {
        while (true)
        {
            Console.WriteLine("\n1. Показати доступні Wi-Fi мережі");
            Console.WriteLine("2. Підключитися до Wi-Fi");
            Console.WriteLine("3. Відключитися від Wi-Fi");
            Console.WriteLine("4. Вийти");
            Console.Write("Виберіть опцію: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.WriteLine(ShowAvailableNetworks());
                    break;
                case "2":
                    Console.Write("Введіть SSID мережі: ");
                    string ssid = Console.ReadLine();
                    Console.Write("Введіть пароль: ");
                    string password = Console.ReadLine();
                    ConnectToWiFi(ssid, password);
                    break;
                case "3":
                    DisconnectWiFi();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                    break;
            }
        }
    }

    public static string ShowAvailableNetworks()
    {
        return ExecuteCommand("netsh wlan show networks mode=bssid");
    }

    public static void ConnectToWiFi(string ssid, string password)
    {
        string profile = $"<?xml version=\"1.0\"?>\n" +
                         "<WLANProfile xmlns=\"http://www.microsoft.com/networking/WLAN/profile/v1\">\n" +
                         "<name>" + ssid + "</name>\n" +
                         "<SSIDConfig>\n" +
                         "<SSID>\n" +
                         "<name>" + ssid + "</name>\n" +
                         "</SSID>\n" +
                         "</SSIDConfig>\n" +
                         "<connectionType>ESS</connectionType>\n" +
                         "<connectionMode>auto</connectionMode>\n" +
                         "<MSM>\n" +
                         "<security>\n" +
                         "<authEncryption>\n" +
                         "<authentication>WPA2PSK</authentication>\n" +
                         "<encryption>AES</encryption>\n" +
                         "<useOneX>false</useOneX>\n" +
                         "</authEncryption>\n" +
                         "<sharedKey>\n" +
                         "<keyType>passPhrase</keyType>\n" +
                         "<protected>false</protected>\n" +
                         "<keyMaterial>" + password + "</keyMaterial>\n" +
                         "</sharedKey>\n" +
                         "</security>\n" +
                         "</MSM>\n" +
                         "</WLANProfile>";

        string profilePath = "wifi_profile.xml";
        System.IO.File.WriteAllText(profilePath, profile, Encoding.UTF8);
        ExecuteCommand($"netsh wlan add profile filename=\"{profilePath}\" user=current");
        ExecuteCommand($"netsh wlan connect name=\"{ssid}\"");
    }

    public static void DisconnectWiFi()
    {
        ExecuteCommand("netsh wlan disconnect");
    }

    static string ExecuteCommand(string command)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = command.Replace("netsh ", ""),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    return "Помилка: " + error;
                }
                else
                {
                    return output;
                }
            }
        }
        catch (Exception ex)
        {
            return "Виникла помилка: " + ex.Message;
        }
    }
}