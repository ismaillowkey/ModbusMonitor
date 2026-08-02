using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using NModbus;
using NModbus.IO;
using NModbus.Serial;
using ModbusMonitor.Models;
using ModbusMonitor.Utils;

namespace ModbusMonitor
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<ModbusRegisterItem> Coils { get; set; } = new();
        public ObservableCollection<ModbusRegisterItem> DiscreteInputs { get; set; } = new();
        public ObservableCollection<ModbusRegisterItem> InputRegisters { get; set; } = new();
        public ObservableCollection<ModbusRegisterItem> HoldingRegisters { get; set; } = new();

        public static int AddressOffset = 0;

        private List<ModbusRegisterItem> _allCoils = new();
        private List<ModbusRegisterItem> _allDiscreteInputs = new();
        private List<ModbusRegisterItem> _allInputRegisters = new();
        private List<ModbusRegisterItem> _allHoldingRegisters = new();

        private int _currentPage = 1;
        private int _pageSize = 10;

        private TcpClient? _tcpClient;
        private SerialPort? _serialPort;
        private IModbusMaster? _modbusMaster;
        private DispatcherTimer _pollTimer;
        private bool _shouldBeConnected = false;
        private bool _isReconnecting = false;
        private ConnectionConfig _currentConfig = new ConnectionConfig();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            // Set dynamic title based on assembly version
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                Title = $"Modbus Monitor v{version.Major}.{version.Minor}.{version.Build}";
            }

            // Populate 10 default rows for all tabs
            for (int i = 0; i < 10; i++)
            {
                _allCoils.Add(new ModbusRegisterItem { Type = "COIL", Address = i });
                _allDiscreteInputs.Add(new ModbusRegisterItem { Type = "DISCRETE INPUT", Address = i });
                _allInputRegisters.Add(new ModbusRegisterItem { Type = "INPUT REGISTER", Address = i });
                _allHoldingRegisters.Add(new ModbusRegisterItem { Type = "HOLDING REGISTER", Address = i });
            }

            UpdatePagination();

            _pollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _pollTimer.Tick += PollTimer_Tick;
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            bool isTcp = _currentConfig.Protocol == ModbusProtocol.Tcp || _currentConfig.Protocol == ModbusProtocol.RtuOverTcp;
            bool isConnected = isTcp ? (_tcpClient != null && _tcpClient.Connected) : (_serialPort != null && _serialPort.IsOpen);

            if (isConnected || _isReconnecting)
            {
                _shouldBeConnected = false;
                Disconnect();
                return;
            }

            var dialog = new ConnectionDialog(_currentConfig) { Owner = this };
            if (dialog.ShowDialog() != true) return;

            _currentConfig = dialog.Config;
            txtConnectionInfo.Text = _currentConfig.ToString();
            txtBadgeInfo.Text = _currentConfig.ToString();
            brdBadgeInfo.Visibility = Visibility.Visible;

            _shouldBeConnected = true;
            btnConnect.IsEnabled = false;

            try
            {
                await ConnectInternalAsync();
                _pollTimer.Start();
            }
            catch (Exception ex)
            {
                _shouldBeConnected = false;
                MessageBox.Show($"Connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Disconnect();
            }
            finally
            {
                btnConnect.IsEnabled = true;
            }
        }

        private async Task ConnectInternalAsync()
        {
            txtStatus.Text = "Connecting...";
            txtStatus.Foreground = new SolidColorBrush(Colors.Orange);

            var factory = new ModbusFactory();

            if (_currentConfig.Protocol == ModbusProtocol.Tcp || _currentConfig.Protocol == ModbusProtocol.RtuOverTcp)
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(_currentConfig.IpAddress, _currentConfig.TcpPort);

                if (_currentConfig.Protocol == ModbusProtocol.Tcp)
                    _modbusMaster = factory.CreateMaster(_tcpClient);
                else
                {
                    var adapter = new TcpClientAdapter(_tcpClient);
                    _modbusMaster = factory.CreateRtuMaster(adapter); // RTU over TCP
                }
            }
            else
            {
                _serialPort = new SerialPort(_currentConfig.ComPort, _currentConfig.BaudRate, _currentConfig.Parity, 8, _currentConfig.StopBits);
                _serialPort.ReadTimeout = _currentConfig.Timeout;
                _serialPort.WriteTimeout = _currentConfig.Timeout;
                _serialPort.Open();

                var adapter = new SerialPortAdapter(_serialPort);
                if (_currentConfig.Protocol == ModbusProtocol.SerialRtu)
                    _modbusMaster = factory.CreateRtuMaster(adapter);
                else
                    _modbusMaster = factory.CreateAsciiMaster(adapter);
            }

            if (_modbusMaster != null)
            {
                _modbusMaster.Transport.ReadTimeout = _currentConfig.Timeout;
                _modbusMaster.Transport.WriteTimeout = _currentConfig.Timeout;
            }

            // Test connection
            try
            {
                await _modbusMaster!.ReadHoldingRegistersAsync(1, 0, 1);
            }
            catch (SlaveException) { }
            catch (Exception ex)
            {
                throw new Exception("Connected, but Modbus check failed: " + ex.Message);
            }
            
            txtStatus.Text = "Connected";
            txtStatus.Foreground = new SolidColorBrush(Colors.Green);
            btnConnect.Content = "Disconnect";
            btnAddMonitor.IsEnabled = true;
            mainTabControl.IsEnabled = true;
        }

        private async Task ReconnectAsync()
        {
            if (_isReconnecting || !_shouldBeConnected) return;
            _isReconnecting = true;

            txtStatus.Text = "Reconnecting...";
            txtStatus.Foreground = new SolidColorBrush(Colors.Orange);

            while (_shouldBeConnected)
            {
                try
                {
                    _tcpClient?.Close();
                    _tcpClient?.Dispose();
                    if (_serialPort != null && _serialPort.IsOpen) _serialPort.Close();
                    _serialPort?.Dispose();
                    _modbusMaster?.Dispose();

                    await ConnectInternalAsync();
                    _isReconnecting = false;
                    return;
                }
                catch
                {
                    await Task.Delay(2000); // Wait 2 seconds before retrying
                }
            }
            _isReconnecting = false;
        }

        private void Disconnect()
        {
            _shouldBeConnected = false;
            _pollTimer.Stop();
            _tcpClient?.Close();
            _tcpClient?.Dispose();
            _tcpClient = null;
            if (_serialPort != null && _serialPort.IsOpen) _serialPort.Close();
            _serialPort?.Dispose();
            _serialPort = null;
            _modbusMaster?.Dispose();
            _modbusMaster = null;

            txtStatus.Text = "Disconnected";
            txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // #DC3545
            btnConnect.Content = "Connect";
            brdBadgeInfo.Visibility = Visibility.Collapsed;
            btnAddMonitor.IsEnabled = false;
            mainTabControl.IsEnabled = false;
        }

        private async void PollTimer_Tick(object? sender, EventArgs e)
        {
            if (_isReconnecting || !_shouldBeConnected) return;

            bool isTcp = _currentConfig.Protocol == ModbusProtocol.Tcp || _currentConfig.Protocol == ModbusProtocol.RtuOverTcp;
            var master = _modbusMaster;
            if (master == null || (isTcp && (_tcpClient == null || !_tcpClient.Connected)) || (!isTcp && (_serialPort == null || !_serialPort.IsOpen)))
            {
                _ = ReconnectAsync();
                return;
            }

            try
            {
                // Poll Coils
                foreach (var item in Coils.ToList())
                {
                    try
                    {
                        if (master == null) break;
                        bool[] coils = await master.ReadCoilsAsync(item.SlaveId, (ushort)item.Address, 1);
                        if (!item.IsEditing) item.Value = coils[0] ? "1" : "0";
                        item.ErrorMessage = string.Empty;
                    }
                    catch (Exception ex) 
                    { 
                        if (ex is System.IO.IOException || ex is InvalidOperationException || ex is NullReferenceException || ex.InnerException is System.Net.Sockets.SocketException) throw;
                        item.ErrorMessage = ex.Message; 
                    }
                }

                // Poll Discrete Inputs
                foreach (var item in DiscreteInputs.ToList())
                {
                    try
                    {
                        if (master == null) break;
                        bool[] inputs = await master.ReadInputsAsync(item.SlaveId, (ushort)item.Address, 1);
                        item.Value = inputs[0] ? "1" : "0";
                        item.ErrorMessage = string.Empty;
                    }
                    catch (Exception ex) 
                    { 
                        if (ex is System.IO.IOException || ex is InvalidOperationException || ex is NullReferenceException || ex.InnerException is System.Net.Sockets.SocketException) throw;
                        item.ErrorMessage = ex.Message; 
                    }
                }

                // Poll Input Registers
                foreach (var item in InputRegisters.ToList())
                {
                    try
                    {
                        if (master == null) break;
                        ushort[] registers = await master.ReadInputRegistersAsync(item.SlaveId, (ushort)item.Address, (ushort)item.Length);
                        item.Value = ModbusDataConverter.ConvertRegistersToString(registers, item.DataType, item.Endian);
                        item.ErrorMessage = string.Empty;
                    }
                    catch (Exception ex) 
                    { 
                        if (ex is System.IO.IOException || ex is InvalidOperationException || ex is NullReferenceException || ex.InnerException is System.Net.Sockets.SocketException) throw;
                        item.ErrorMessage = ex.Message; 
                    }
                }

                // Poll Holding Registers
                foreach (var item in HoldingRegisters.ToList())
                {
                    try
                    {
                        if (master == null) break;
                        ushort[] registers = await master.ReadHoldingRegistersAsync(item.SlaveId, (ushort)item.Address, (ushort)item.Length);
                        if (!item.IsEditing) item.Value = ModbusDataConverter.ConvertRegistersToString(registers, item.DataType, item.Endian);
                        item.ErrorMessage = string.Empty;
                    }
                    catch (Exception ex) 
                    { 
                        if (ex is System.IO.IOException || ex is InvalidOperationException || ex is NullReferenceException || ex.InnerException is System.Net.Sockets.SocketException) throw;
                        item.ErrorMessage = ex.Message; 
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Polling error: {ex.Message}");
                if (ex is System.IO.IOException || ex is InvalidOperationException || ex is NullReferenceException || ex.InnerException is System.Net.Sockets.SocketException)
                {
                    foreach (var item in _allCoils.Concat(_allDiscreteInputs).Concat(_allInputRegisters).Concat(_allHoldingRegisters))
                    {
                        item.ErrorMessage = "Device not connected";
                    }
                    _ = ReconnectAsync();
                }
            }
        }

        private async void TxtValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (_modbusMaster == null)
                {
                    MessageBox.Show("Please connect to the Modbus server first.", "Not Connected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (sender is TextBox textBox && textBox.DataContext is ModbusRegisterItem item)
                {
                    try
                    {
                        if (item.Type == "HOLDING REGISTER")
                        {
                            try
                            {
                                ushort[] registers = ModbusDataConverter.ConvertStringToRegisters(textBox.Text, item.DataType, item.Endian);
                                if (registers.Length == 1)
                                {
                                    await _modbusMaster.WriteSingleRegisterAsync(item.SlaveId, (ushort)item.Address, registers[0]);
                                }
                                else
                                {
                                    await _modbusMaster.WriteMultipleRegistersAsync(item.SlaveId, (ushort)item.Address, registers);
                                }
                                MessageBox.Show("Value written successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                                item.IsEditing = false; // Turn off editing state on enter
                            }
                            catch (ArgumentException)
                            {
                                MessageBox.Show($"Invalid value for {item.DataType}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else if (item.Type == "COIL")
                        {
                            bool val = textBox.Text == "1" || textBox.Text.ToLower() == "true";
                            await _modbusMaster.WriteSingleCoilAsync(item.SlaveId, (ushort)item.Address, val);
                            MessageBox.Show("Coil written successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            item.IsEditing = false; // Turn off editing state on enter
                        }
                        // Remove focus from the textbox to visually indicate save completion
                        Keyboard.ClearFocus();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Write failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private async void Coil_Toggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Primitives.ToggleButton toggleBtn && toggleBtn.DataContext is ModbusRegisterItem item)
            {
                if (_modbusMaster == null || !_shouldBeConnected)
                {
                    MessageBox.Show("Please connect to the Modbus server first.", "Not Connected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    item.BooleanValue = !item.BooleanValue; // Revert
                    return;
                }

                try
                {
                    bool val = item.BooleanValue;
                    await _modbusMaster.WriteSingleCoilAsync(item.SlaveId, (ushort)item.Address, val);
                    item.IsEditing = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Write failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    item.BooleanValue = !item.BooleanValue; // Revert
                }
            }
        }

        private void TxtValue_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ModbusRegisterItem item)
            {
                item.IsEditing = true;
            }
        }

        private void TxtValue_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is ModbusRegisterItem item)
            {
                item.IsEditing = false;
            }
        }

        private void BtnAddMonitor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string registerType = "HOLDING REGISTER (4x)";
                if (dgCoils.IsVisible) registerType = "COIL (0x)";
                else if (dgDiscreteInputs.IsVisible) registerType = "DISCRETE INPUT (1x)";
                else if (dgInputRegisters.IsVisible) registerType = "INPUT REGISTER (3x)";

                int nextAddr = 0;
                if (dgHoldingRegisters.IsVisible) nextAddr = HoldingRegisters.Any() ? HoldingRegisters.Max(x => x.Address) + 1 : 0;
                else if (dgCoils.IsVisible) nextAddr = Coils.Any() ? Coils.Max(x => x.Address) + 1 : 0;
                else if (dgDiscreteInputs.IsVisible) nextAddr = DiscreteInputs.Any() ? DiscreteInputs.Max(x => x.Address) + 1 : 0;
                else if (dgInputRegisters.IsVisible) nextAddr = InputRegisters.Any() ? InputRegisters.Max(x => x.Address) + 1 : 0;

                var dialog = new AddMonitorWindow(registerType, nextAddr) { Owner = this };
                if (dialog.ShowDialog() == true)
                {
                    int count = dialog.BulkAddCount;
                    for (int i = 0; i < count; i++)
                    {
                        var newItem = new ModbusRegisterItem
                        {
                            Type = dialog.SelectedType.Replace(" (4x)", "").Replace(" (0x)", "").Replace(" (1x)", "").Replace(" (3x)", "").ToUpper(),
                            SlaveId = dialog.SlaveId,
                            Address = dialog.StartAddress + i,
                            DataType = dialog.SelectedDataType,
                            Endian = dialog.SelectedEndianness,
                            Length = dialog.RegisterLength
                        };

                        List<ModbusRegisterItem>? targetList = null;

                        if (newItem.Type == "HOLDING REGISTER") targetList = _allHoldingRegisters;
                        else if (newItem.Type == "COIL") targetList = _allCoils;
                        else if (newItem.Type == "DISCRETE INPUT") targetList = _allDiscreteInputs;
                        else if (newItem.Type == "INPUT REGISTER") targetList = _allInputRegisters;

                        if (targetList != null)
                        {
                            if (!targetList.Any(x => x.SlaveId == newItem.SlaveId && x.Address == newItem.Address && x.Type == newItem.Type))
                            {
                                targetList.Add(newItem);
                            }
                        }
                    }

                    SortList(_allHoldingRegisters);
                    SortList(_allCoils);
                    SortList(_allDiscreteInputs);
                    SortList(_allInputRegisters);
                    
                    UpdatePagination();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan saat menambah monitor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortList(List<ModbusRegisterItem> list)
        {
            if (!list.Any()) return;
            var sorted = list.OrderBy(x => x.SlaveId).ThenBy(x => x.Address).ToList();
            list.Clear();
            list.AddRange(sorted);
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ModbusRegisterItem item)
            {
                if (item.Type == "HOLDING REGISTER" || item.Type == "INPUT REGISTER")
                {
                    var dialog = new EditMonitorWindow(item);
                    dialog.Owner = this;
                    dialog.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Editing data type is only available for registers.", "Edit", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ModbusRegisterItem item)
            {
                if (item.Type == "HOLDING REGISTER") _allHoldingRegisters.Remove(item);
                else if (item.Type == "COIL") _allCoils.Remove(item);
                else if (item.Type == "DISCRETE INPUT") _allDiscreteInputs.Remove(item);
                else if (item.Type == "INPUT REGISTER") _allInputRegisters.Remove(item);
                
                UpdatePagination();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                _currentPage = 1;
                UpdatePagination();
            }
        }

        private void TxtScanRate_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateScanRate();
        }

        private void TxtScanRate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateScanRate();
                Keyboard.ClearFocus();
            }
        }

        private void UpdateScanRate()
        {
            if (int.TryParse(txtScanRate.Text, out int ms))
            {
                if (ms < 100) ms = 100;
                if (ms > 100000) ms = 100000;
                txtScanRate.Text = ms.ToString();
                
                if (_pollTimer != null)
                {
                    _pollTimer.Interval = TimeSpan.FromMilliseconds(ms);
                }
            }
            else
            {
                txtScanRate.Text = "1000";
                if (_pollTimer != null)
                {
                    _pollTimer.Interval = TimeSpan.FromMilliseconds(1000);
                }
            }
        }

        private void UpdatePagination()
        {
            if (mainTabControl == null) return;
            
            List<ModbusRegisterItem> currentMasterList = _allCoils;
            ObservableCollection<ModbusRegisterItem> currentUIList = Coils;
            
            if (dgHoldingRegisters != null && dgHoldingRegisters.IsVisible)
            {
                currentMasterList = _allHoldingRegisters;
                currentUIList = HoldingRegisters;
            }
            else if (dgInputRegisters != null && dgInputRegisters.IsVisible)
            {
                currentMasterList = _allInputRegisters;
                currentUIList = InputRegisters;
            }
            else if (dgDiscreteInputs != null && dgDiscreteInputs.IsVisible)
            {
                currentMasterList = _allDiscreteInputs;
                currentUIList = DiscreteInputs;
            }
            
            int totalItems = currentMasterList.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);
            if (totalPages < 1) totalPages = 1;
            
            if (_currentPage > totalPages) _currentPage = totalPages;
            if (_currentPage < 1) _currentPage = 1;
            
            var pagedData = currentMasterList.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
            
            currentUIList.Clear();
            foreach (var item in pagedData)
            {
                currentUIList.Add(item);
            }
            
            if (txtPageInfo != null) txtPageInfo.Text = $"Page {_currentPage} of {totalPages}";
            if (btnPrevPage != null) btnPrevPage.IsEnabled = _currentPage > 1;
            if (btnNextPage != null) btnNextPage.IsEnabled = _currentPage < totalPages;
        }

        private void CmbPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPageSize != null && cmbPageSize.SelectedItem is ComboBoxItem item)
            {
                if (int.TryParse(item.Content.ToString(), out int size))
                {
                    _pageSize = size;
                    _currentPage = 1;
                    UpdatePagination();
                }
            }
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage++;
            UpdatePagination();
        }
        private void CmbAddressBase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAddressBase != null && cmbAddressBase.SelectedIndex >= 0)
            {
                AddressOffset = cmbAddressBase.SelectedIndex; // 0 for "start from 0", 1 for "start from 1"
                
                // Update all items
                foreach (var item in _allCoils) item.UpdateDisplayAddress();
                foreach (var item in _allDiscreteInputs) item.UpdateDisplayAddress();
                foreach (var item in _allInputRegisters) item.UpdateDisplayAddress();
                foreach (var item in _allHoldingRegisters) item.UpdateDisplayAddress();
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuSourceCode_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/ismaillowkey/ModbusMonitor") { UseShellExecute = true });
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Modbus Monitor v0.5.0\n\nA modern WPF Modbus TCP and Serial Client developed by Ismail Lowkey.\n\nThis application allows monitoring and writing to Modbus devices using TCP, RTU over TCP, and Serial (RTU/ASCII) protocols.", "About Modbus Monitor", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}