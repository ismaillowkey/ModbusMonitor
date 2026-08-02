using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using ModbusMonitor.Models;

namespace ModbusMonitor
{
    public partial class ConnectionDialog : Window
    {
        public ConnectionConfig Config { get; private set; } = new ConnectionConfig();

        public ConnectionDialog(ConnectionConfig? existingConfig = null)
        {
            InitializeComponent();
            
            LoadPorts();

            if (existingConfig != null)
            {
                Config = existingConfig;
                LoadConfigIntoUI();
            }
        }

        private void LoadPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            cmbComPort.ItemsSource = ports;
            if (ports.Length > 0 && cmbComPort.SelectedItem == null)
            {
                cmbComPort.SelectedIndex = 0;
            }
        }

        private void LoadConfigIntoUI()
        {
            cmbProtocol.SelectedIndex = (int)Config.Protocol;
            txtIpAddress.Text = Config.IpAddress;
            txtTcpPort.Text = Config.TcpPort.ToString();
            if (txtTimeout != null) txtTimeout.Text = Config.Timeout.ToString();

            if (cmbComPort.Items.Contains(Config.ComPort))
                cmbComPort.SelectedItem = Config.ComPort;
            
            SelectComboBoxItem(cmbBaudRate, Config.BaudRate.ToString());
            SelectComboBoxItem(cmbDataBits, Config.DataBits.ToString());
            SelectComboBoxItem(cmbParity, Config.Parity.ToString());
            
            string stopBitsStr = Config.StopBits == StopBits.OnePointFive ? "1.5" : Config.StopBits == StopBits.One ? "1" : Config.StopBits.ToString();
            SelectComboBoxItem(cmbStopBits, stopBitsStr);
        }

        private void SelectComboBoxItem(ComboBox comboBox, string contentToMatch)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content.ToString() == contentToMatch)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void CmbProtocol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pnlTcpSettings == null || pnlSerialSettings == null) return;

            if (cmbProtocol.SelectedIndex == 0 || cmbProtocol.SelectedIndex == 1) // TCP or RTU over TCP
            {
                pnlTcpSettings.Visibility = Visibility.Visible;
                pnlSerialSettings.Visibility = Visibility.Collapsed;
            }
            else // Serial
            {
                pnlTcpSettings.Visibility = Visibility.Collapsed;
                pnlSerialSettings.Visibility = Visibility.Visible;
            }
        }

        private void BtnRefreshPorts_Click(object sender, RoutedEventArgs e)
        {
            LoadPorts();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            Config.Protocol = (ModbusProtocol)cmbProtocol.SelectedIndex;
            
            if (!int.TryParse(txtTimeout.Text, out int timeout))
            {
                MessageBox.Show("Invalid Timeout. Must be a number in milliseconds.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Config.Timeout = timeout;

            if (Config.Protocol == ModbusProtocol.Tcp || Config.Protocol == ModbusProtocol.RtuOverTcp)
            {
                if (string.IsNullOrWhiteSpace(txtIpAddress.Text))
                {
                    MessageBox.Show("IP Address cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                if (!int.TryParse(txtTcpPort.Text, out int port))
                {
                    MessageBox.Show("Invalid TCP Port.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                Config.IpAddress = txtIpAddress.Text;
                Config.TcpPort = port;
            }
            else
            {
                if (cmbComPort.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM Port.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                Config.ComPort = cmbComPort.SelectedItem.ToString() ?? "COM1";
                Config.BaudRate = int.Parse(((ComboBoxItem)cmbBaudRate.SelectedItem).Content.ToString() ?? "9600");
                Config.DataBits = int.Parse(((ComboBoxItem)cmbDataBits.SelectedItem).Content.ToString() ?? "8");
                Config.Parity = (Parity)Enum.Parse(typeof(Parity), ((ComboBoxItem)cmbParity.SelectedItem).Content.ToString() ?? "None");
                
                string stopBitsStr = ((ComboBoxItem)cmbStopBits.SelectedItem).Content.ToString() ?? "1";
                Config.StopBits = stopBitsStr == "1.5" ? StopBits.OnePointFive : stopBitsStr == "1" ? StopBits.One : (StopBits)Enum.Parse(typeof(StopBits), stopBitsStr);
            }

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
