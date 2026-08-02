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
        public int BulkAddCount { get; private set; } = 1;

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
                    txtLength.IsReadOnly = true;
                    txtLength.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#E9ECEF");
                    
                    if (pnlDataType != null) pnlDataType.Visibility = System.Windows.Visibility.Collapsed;
                    if (gridOptions != null) gridOptions.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    cmbDataType.IsEnabled = true;
                    cmbEndianness.IsEnabled = true;
                    
                    if (pnlDataType != null) pnlDataType.Visibility = System.Windows.Visibility.Visible;
                    if (gridOptions != null) gridOptions.Visibility = System.Windows.Visibility.Visible;
                    
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
            if (cmbDataType?.SelectedItem is ComboBoxItem item && txtLength != null && cmbEndianness != null && lblLength != null)
            {
                string dt = item.Content.ToString() ?? "";
                
                if (dt.Contains("String"))
                {
                    lblLength.Text = "Length of String";
                    txtLength.IsReadOnly = false;
                    txtLength.Background = System.Windows.Media.Brushes.White;
                    bool isTwoChars = dt.Contains("2 chars");
                    int expectedCount = isTwoChars ? 2 : 1;

                    if (cmbEndianness.Items.Count != expectedCount || !((ComboBoxItem)cmbEndianness.Items[0]).Content.ToString().Contains("String"))
                    {
                        cmbEndianness.Items.Clear();
                        cmbEndianness.Items.Add(new ComboBoxItem { Content = "String Normal" });
                        if (isTwoChars)
                        {
                            cmbEndianness.Items.Add(new ComboBoxItem { Content = "String Reverse" });
                        }
                        cmbEndianness.SelectedIndex = 0;
                    }
                }
                else
                {
                    lblLength.Text = "Length (Words)";
                    txtLength.IsReadOnly = true;
                    txtLength.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#E9ECEF");
                    if (cmbEndianness.Items.Count < 4 || !((ComboBoxItem)cmbEndianness.Items[0]).Content.ToString().Contains("ABCD"))
                    {
                        cmbEndianness.Items.Clear();
                        cmbEndianness.Items.Add(new ComboBoxItem { Content = "ABCD (Big Endian)" });
                        cmbEndianness.Items.Add(new ComboBoxItem { Content = "DCBA (Little Endian)" });
                        cmbEndianness.Items.Add(new ComboBoxItem { Content = "BADC (Byte Swap)" });
                        cmbEndianness.Items.Add(new ComboBoxItem { Content = "CDAB (Word Swap)" });
                        cmbEndianness.SelectedIndex = 0;
                    }

                    if (dt.Contains("64-bit")) txtLength.Text = "4";
                    else if (dt.Contains("32-bit")) txtLength.Text = "2";
                    else txtLength.Text = "1";
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
