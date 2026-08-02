using System.Windows;
using System.Windows.Controls;

namespace ModbusMonitor
{
    public partial class AddMonitorWindow : Window
    {
        public string SelectedType { get; private set; } = "HOLDING REGISTER (4x)";
        public byte SlaveId { get; private set; } = 1;
        public int StartAddress { get; private set; } = 0;
        public string SelectedDataType { get; private set; } = "UInt16 (16-bit)";
        public string SelectedEndianness { get; private set; } = "ABCD (Big Endian)";
        public int RegisterLength { get; private set; } = 1;
        public int BulkAddCount { get; private set; } = 10;

        public AddMonitorWindow(string defaultRegisterType, int defaultAddress)
        {
            InitializeComponent();
            
            // Set defaults based on passed arguments
            foreach (ComboBoxItem item in cmbRegisterType.Items)
            {
                if (item.Content.ToString() == defaultRegisterType)
                {
                    cmbRegisterType.SelectedItem = item;
                    break;
                }
            }
            if (cmbRegisterType.SelectedItem == null) cmbRegisterType.SelectedIndex = 0;
            
            txtAddress.Text = (defaultAddress + MainWindow.AddressOffset).ToString();
            cmbDataType.SelectedIndex = 0;
            cmbEndianness.SelectedIndex = 0;
        }

        private void CmbRegisterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRegisterType.SelectedItem is ComboBoxItem item && cmbDataType != null)
            {
                string type = item.Content.ToString() ?? "";
                if (type.Contains("COIL") || type.Contains("DISCRETE"))
                {
                    cmbDataType.IsEnabled = false;
                    cmbEndianness.IsEnabled = false;
                    txtLength.Text = "1";
                }
                else
                {
                    cmbDataType.IsEnabled = true;
                    cmbEndianness.IsEnabled = true;
                    UpdateLength();
                }
            }
        }

        private void CmbDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateLength();
        }

        private void UpdateLength()
        {
            if (cmbDataType?.SelectedItem is ComboBoxItem item && txtLength != null)
            {
                string dt = item.Content.ToString() ?? "";
                if (dt.Contains("32-bit"))
                {
                    txtLength.Text = "2";
                }
                else
                {
                    txtLength.Text = "1";
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRegisterType.SelectedItem is ComboBoxItem rtItem) SelectedType = rtItem.Content.ToString() ?? "HOLDING REGISTER (4x)";
            if (cmbDataType.SelectedItem is ComboBoxItem dtItem) SelectedDataType = dtItem.Content.ToString() ?? "UInt16 (16-bit)";
            if (cmbEndianness.SelectedItem is ComboBoxItem edItem) SelectedEndianness = edItem.Content.ToString() ?? "ABCD (Big Endian)";

            if (!byte.TryParse(txtSlaveId.Text, out byte slaveId))
            {
                MessageBox.Show("Invalid Slave ID. Must be a number 1-255.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            SlaveId = slaveId;

            if (!int.TryParse(txtAddress.Text, out int addr) || addr < MainWindow.AddressOffset)
            {
                MessageBox.Show($"Invalid Address. Must be a number greater than or equal to {MainWindow.AddressOffset}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            StartAddress = addr - MainWindow.AddressOffset;

            if (!int.TryParse(txtLength.Text, out int len))
            {
                len = 1;
            }
            RegisterLength = len;

            if (!int.TryParse(txtBulkAddCount.Text, out int count) || count < 1)
            {
                MessageBox.Show("Invalid Bulk Add Count. Must be at least 1.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            BulkAddCount = count;

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
