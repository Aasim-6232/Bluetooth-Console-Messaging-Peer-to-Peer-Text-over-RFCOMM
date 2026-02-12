using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth;

namespace BtChat.Client.UI
{
    public partial class MainWindow : Window
    {
        private readonly string _username;
        private BluetoothClient _client;
        private BluetoothListener _listener;
        private Stream _stream;
        private bool _isConnected = false;
        private List<BluetoothDeviceInfo> _discoveredDevices = new();
        private string _connectedPeerName = string.Empty;

        public MainWindow(string username)
        {
            InitializeComponent();
            _username = username;
            UsernameTextBlock.Text = _username;

            _client = new BluetoothClient();

            AppendMessage($"Welcome, {_username}!");
            AppendMessage("Listening for incoming connections...");

            Task.Run(() => ListenForIncomingConnections());
        }

        #region Incoming Connections
        private void ListenForIncomingConnections()
        {
            try
            {
                _listener = new BluetoothListener(BluetoothService.SerialPort);
                _listener.Start();

                while (true)
                {
                    var incoming = _listener.AcceptBluetoothClient(); // blocking
                    if (_isConnected)
                    {
                        incoming.Close();
                        continue;
                    }

                    var stream = incoming.GetStream();
                    var reader = new StreamReader(stream);
                    var writer = new StreamWriter(stream) { AutoFlush = true };

                    string peerName = reader.ReadLine();

                    Dispatcher.Invoke(() =>
                    {
                        var result = MessageBox.Show($"{peerName} is trying to connect.", 
                                                     "Incoming Connection", 
                                                     MessageBoxButton.YesNo, 
                                                     MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            _client = incoming;
                            _stream = stream;
                            _isConnected = true;
                            _connectedPeerName = peerName;

                            writer.WriteLine(_username); // send our username
                            AppendMessage($"Connected to {peerName}");
                            Task.Run(() => ReceiveMessages(_stream));
                        }
                        else
                        {
                            incoming.Close();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                AppendMessage($"Listener error: {ex.Message}");
            }
        }
        #endregion

        #region Scan & Manual Connect
        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            AppendMessage("Scanning for devices...");
            _discoveredDevices.Clear();
            DevicesComboBox.Items.Clear();

            await Task.Run(() =>
            {
                _discoveredDevices = _client.DiscoverDevices().ToList();
            });

            if (_discoveredDevices.Count == 0)
            {
                AppendMessage("No devices found.");
                return;
            }

            int index = 0;
            foreach (var device in _discoveredDevices)
            {
                DevicesComboBox.Items.Add($"{index}: {device.DeviceName} [{device.DeviceAddress}]");
                index++;
            }
            AppendMessage("Scan complete.");
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (DevicesComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Select a device first.");
                return;
            }

            if (_isConnected)
            {
                AppendMessage("Already connected to a peer.");
                return;
            }

            var device = _discoveredDevices[DevicesComboBox.SelectedIndex];
            Task.Run(() => ConnectToDevice(device));
        }
        #endregion

        #region Connect by Name
        private void ConnectByNameButton_Click(object sender, RoutedEventArgs e)
        {
            string targetName = ConnectNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(targetName))
            {
                MessageBox.Show("Enter a peer name to connect.");
                return;
            }

            if (_isConnected)
            {
                AppendMessage("Already connected to a peer.");
                return;
            }

            Task.Run(() =>
            {
                var devices = _client.DiscoverDevices().ToList();
                var device = devices.FirstOrDefault(d => d.DeviceName.Equals(targetName, StringComparison.OrdinalIgnoreCase));

                if (device == null)
                {
                    AppendMessage($"No device found with name {targetName}");
                    return;
                }

                ConnectToDevice(device);
            });
        }
        #endregion

        #region Connection Helper
        private void ConnectToDevice(BluetoothDeviceInfo device)
        {
            try
            {
                AppendMessage($"Connecting to {device.DeviceName}...");
                _client.Connect(device.DeviceAddress, BluetoothService.SerialPort);
                _stream = _client.GetStream();
                _isConnected = true;

                var writer = new StreamWriter(_stream) { AutoFlush = true };
                writer.WriteLine(_username);

                var reader = new StreamReader(_stream);
                string peerName = reader.ReadLine();
                _connectedPeerName = peerName;

                AppendMessage($"Connected to {peerName}");
                Task.Run(() => ReceiveMessages(_stream));
            }
            catch (Exception ex)
            {
                AppendMessage($"Connection failed: {ex.Message}");
            }
        }
        #endregion

        #region Messaging
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
                e.Handled = true;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            if (_stream == null || string.IsNullOrWhiteSpace(InputTextBox.Text))
                return;

            string msg = $"{_username}: {InputTextBox.Text.Trim()}";
            byte[] data = Encoding.UTF8.GetBytes(msg + "\n");

            try
            {
                _stream.Write(data, 0, data.Length);
                AppendMessage(msg);
                InputTextBox.Clear();
            }
            catch
            {
                AppendMessage("Send failed. Connection may be lost.");
                CleanupConnection();
            }
        }

        private void ReceiveMessages(Stream stream)
        {
            var reader = new StreamReader(stream);
            try
            {
                while (true)
                {
                    string msg = reader.ReadLine();
                    if (msg == null) break;

                    if (msg == "_DISCONNECT_")
                    {
                        AppendMessage("Peer disconnected.");
                        CleanupConnection();
                        break;
                    }

                    AppendMessage(msg);
                }
            }
            catch
            {
                if (_isConnected)
                {
                    AppendMessage("Connection lost.");
                    CleanupConnection();
                }
            }
        }
        #endregion

        #region Disconnect
        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            DisconnectFromPeer();
        }

        private void DisconnectFromPeer()
        {
            if (!_isConnected)
            {
                AppendMessage("No active connection to disconnect.");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes("_DISCONNECT_\n");
                _stream?.Write(data, 0, data.Length);
            }
            catch { }

            CleanupConnection();
            AppendMessage("Disconnected from peer");
        }

        private void CleanupConnection()
        {
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }

            _stream = null;
            _client = new BluetoothClient();
            _isConnected = false;
            _connectedPeerName = string.Empty;
        }
        #endregion

        private void AppendMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                MessagesTextBox.AppendText($"{message}\n");
                MessagesTextBox.ScrollToEnd();
            });
        }
    }
}
