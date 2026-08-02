using System.Windows;
using System.Windows.Controls;
using ModbusMonitor.Models;

namespace ModbusMonitor
{
    public partial class EditMonitorWindow : Window
    {
        public ModbusRegisterItem RegisterItem { get; private set; }

        public EditMonitorWindow(ModbusRegisterItem item)
        {
            InitializeComponent();
            RegisterItem = item;

            // Load values
            foreach (ComboBoxItem cbItem in cmbDataType.Items)
            {
                if (cbItem.Content.ToString() == item.DataType)
                {
                    cmbDataType.SelectedItem = cbItem;
                    break;
                }
            }

            foreach (ComboBoxItem cbItem in cmbEndianness.Items)
            {
                if (cbItem.Content.ToString() == item.Endian)
                {
                    cmbEndianness.SelectedItem = cbItem;
                    break;
                }
            }

            txtLength.Text = item.Length.ToString();
        }

        private void CmbDataType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDataType.SelectedItem is ComboBoxItem item && txtLength != null)
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
            if (cmbDataType.SelectedItem is ComboBoxItem dtItem)
            {
                RegisterItem.DataType = dtItem.Content.ToString() ?? "UInt16 (16-bit)";
            }

            if (cmbEndianness.SelectedItem is ComboBoxItem edItem)
            {
                RegisterItem.Endian = edItem.Content.ToString() ?? "ABCD (Big Endian)";
            }
            
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
