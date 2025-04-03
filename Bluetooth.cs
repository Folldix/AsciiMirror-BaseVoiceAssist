using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using InTheHand.Net;
using InTheHand.Net.Sockets;

public class Bluetooth
{
    private BluetoothClient _bluetoothClient;
    private List<BluetoothDeviceInfo> _devices;
    private bool _isConnected = false;
    private bool _readyToSend = false;
    private byte[] _message;
    private readonly object _lock = new object(); // Для синхронізації потоків
    private CancellationTokenSource _cancellationTokenSource; // Для асинхронного завершення
    private Task _connectionTask; // Завдання для обробки з'єднання

    public Bluetooth()
    {
        _bluetoothClient = new BluetoothClient();
        _devices = new List<BluetoothDeviceInfo>();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void MainMenu(Bluetooth bluetooth)
    {
        string choice;
        do
        {
            Console.WriteLine("\n1. Показати доступні Bluetooth-пристрої");
            Console.WriteLine("2. Підключитися до Bluetooth-пристрою");
            Console.WriteLine("3. Відключитися від Bluetooth-пристрою");
            Console.WriteLine("4. Вийти");
            Console.Write("Виберіть опцію: ");

            choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowAvailableBluetoothDevices(bluetooth);
                    break;
                case "2":
                    bluetoothConnect(bluetooth);
                    break;
                case "3":
                    bluetoothDisonnect(bluetooth);
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                    break;
            }
        } while (choice != "4");
    }

    public void ShowAvailableBluetoothDevices(Bluetooth bluetooth)
    {

        // Пошук пристроїв
        bluetooth.DiscoverDevices();
    }

    public void bluetoothConnect(Bluetooth bluetooth)
    {
        var devices = bluetooth.GetDevices();

        // Вибір пристрою
        Console.Write("Введіть номер пристрою для підключення: ");
        if (int.TryParse(Console.ReadLine(), out int deviceIndex))
        {
            if (deviceIndex > 0 && deviceIndex <= devices.Count)
            {
                // Підключення до обраного пристрою
                bluetooth.ConnectToDevice(deviceIndex - 1); // Індекс у списку починається з 0

                //// Відправка тестового повідомлення
                byte[] message = System.Text.Encoding.UTF8.GetBytes("Hello, Bluetooth!");
                bluetooth.SetMessage(message);
            }
            else
            {
                Console.WriteLine("Невірний номер пристрою.");
            }
        }
        else
        {
            Console.WriteLine("Невірне введення. Будь ласка введіть ціле число.");
        }
    }
    public void bluetoothDisonnect(Bluetooth bluetooth)
    {
        Console.WriteLine("Нажміть Enter для відключення...");
        Console.ReadLine();

        // Відключення
        bluetooth.Disconnect();
    }

    // Пошук доступних Bluetooth-пристроїв
    public void DiscoverDevices()
    {
        Console.WriteLine("Сканування bluetooth пристроїв...");
        _devices = _bluetoothClient.DiscoverDevices().ToList();

        if (_devices.Count == 0)
        {
            Console.WriteLine("Пристрої не знайдено.");
            return;
        }

        Console.WriteLine($"Знайдено {_devices.Count} пристроїв:");
        for (int i = 0; i < _devices.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {_devices[i].DeviceName} ({_devices[i].DeviceAddress})");
        }
    }

    // Підключення до пристрою за індексом
    public void ConnectToDevice(int index)
    {
        if (index < 0 || index >= _devices.Count)
        {
            Console.WriteLine("Невірний індекс пристрою.");
            return;
        }

        var device = _devices[index];
        Console.WriteLine($"Підключення до {device.DeviceName}...");

        // Запуск асинхронного методу для підключення та обміну даними
        _connectionTask = Task.Run(() => ClientConnectThread(device, _cancellationTokenSource.Token));
    }

    // Метод для підключення та обміну даними
    private async Task ClientConnectThread(BluetoothDeviceInfo deviceInfo, CancellationToken cancellationToken)
    {
        try
        {
            BluetoothAddress addressSeleccionado = deviceInfo.DeviceAddress;
            Guid mUUID = new Guid("00001101-0000-1000-8000-00805F9B34FB");
            BluetoothEndPoint ep = new BluetoothEndPoint(addressSeleccionado, mUUID);

            _bluetoothClient.Connect(ep);
            _isConnected = true;
            UpdateUI("Успішно підключено!");

            using (var stream = _bluetoothClient.GetStream())
            {
                byte[] received = new byte[1024];

                while (_isConnected && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Чекаємо дані (але реагуємо на скасування)
                        int bytesRead = await stream.ReadAsync(received, 0, received.Length, cancellationToken);

                        if (bytesRead > 0)
                        {
                            UpdateUI("Отримані дані:");
                            for (int i = 0; i < Math.Min(bytesRead, 15); i++)
                            {
                                UpdateUI(received[i].ToString());
                            }
                        }

                        lock (_lock)
                        {
                            if (_readyToSend)
                            {
                                _readyToSend = false;
                                stream.Write(_message, 0, _message.Length);
                                UpdateUI("Повідомлення відправлено.");
                            }
                        }
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException)
                    {
                        UpdateUI("Bluetooth з'єднання втрачено.");
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        UpdateUI("Операцію скасовано.");
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        UpdateUI("Потік було закрито.");
                        break;
                    }
                }
            }
        }
        catch (SocketException ex)
        {
            UpdateUI($"Підключення провалено: {ex.Message}");
        }
        catch (Exception ex)
        {
            UpdateUI($"Помилка спричинена: {ex.Message}");
        }
        finally
        {
            _isConnected = false;
            UpdateUI("З'єднання закрите.");
        }
    }

    // Відключення від пристрою
    public void Disconnect()
    {
        lock (_lock)
        {
            if (!_isConnected)
            {
                UpdateUI("Підключені пристрої відсутні.");
                return;
            }

            try
            {
                // 1. Скасувати всі операції
                _cancellationTokenSource.Cancel();

                // 2. Зачекати завершення потоку (але не блокувати назавжди)
                if (_connectionTask != null && !_connectionTask.IsCompleted)
                {
                    Task.WaitAny(_connectionTask, Task.Delay(1000)); // Чекаємо не більше 1 секунди
                }

                // 3. Закрити клієнт
                if (_bluetoothClient != null)
                {
                    _bluetoothClient.Close();
                    _bluetoothClient.Dispose();
                    _bluetoothClient = new BluetoothClient(); // Новий клієнт для майбутніх з'єднань
                }

                UpdateUI("Відключено від пристрою.");
            }
            catch (Exception ex)
            {
                UpdateUI($"Помилка під час відключення: {ex.Message}");
            }
            finally
            {
                _isConnected = false;

                // Перестворити CancellationTokenSource для майбутніх з'єднань
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }
    }

    // Встановлення повідомлення для відправки
    public void SetMessage(byte[] message)
    {
        lock (_lock) // Синхронізація доступу до _readyToSend та _message
        {
            _message = message;
            _readyToSend = true;
        }
    }

    // Оновлення інтерфейсу користувача (заглушка)
    private void UpdateUI(string message)
    {
        Console.WriteLine(message);
    }

    // Отримання списку пристроїв
    public List<BluetoothDeviceInfo> GetDevices()
    {
        return _devices;
    }
}