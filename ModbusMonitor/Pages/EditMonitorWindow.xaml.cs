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
            
            txtAddress.Text = item.Address.ToString();

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
            if (cmbDataType.SelectedItem is ComboBoxItem item && txtLength != null && cmbEndianness != null && lblLength != null)
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
            if (cmbDataType.SelectedItem is ComboBoxItem dtItem)
            {
                RegisterItem.DataType = dtItem.Content.ToString() ?? "UInt16 (16-bit)";
            }

            if (cmbEndianness.SelectedItem is ComboBoxItem edItem)
            {
                RegisterItem.Endian = edItem.Content.ToString() ?? "ABCD (Big Endian)";
            }

            if (int.TryParse(txtLength.Text, out int len))
            {
                RegisterItem.Length = len;
            }

            if (int.TryParse(txtAddress.Text, out int addr))
            {
                RegisterItem.Address = addr;
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
