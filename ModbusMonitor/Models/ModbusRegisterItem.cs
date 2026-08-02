using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModbusMonitor.Models
{
    public class ModbusRegisterItem : INotifyPropertyChanged
    {
        private string _type = string.Empty;
        private byte _slaveId = 1;
        private int _address;
        private string _dataType = "UInt16 (16-bit)";
        private string _endian = "ABCD (Big Endian)";
        private int _length = 1;
        private string _value = "0";
        private string _errorMessage = string.Empty;

        public bool BooleanValue
        {
            get => _value == "1" || _value.ToLower() == "true";
            set
            {
                Value = value ? "1" : "0";
                OnPropertyChanged();
            }
        }

        public bool IsEditing { get; set; } = false;

        public string ErrorMessage
        {
            get => _errorMessage;
            set 
            { 
                _errorMessage = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(HasError)); 
            }
        }

        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public byte SlaveId
        {
            get => _slaveId;
            set { _slaveId = value; OnPropertyChanged(); }
        }

        public int Address
        {
            get => _address;
            set 
            { 
                _address = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayAddress));
            }
        }

        public int DisplayAddress
        {
            get => _address + MainWindow.AddressOffset;
        }

        public void UpdateDisplayAddress()
        {
            OnPropertyChanged(nameof(DisplayAddress));
        }

        public string DataType
        {
            get => _dataType;
            set 
            { 
                _dataType = value; 
                if (value.Contains("64-bit")) Length = 4;
                else if (value.Contains("32-bit")) Length = 2;
                else if (value.Contains("16-bit")) Length = 1;
                OnPropertyChanged(); 
            }
        }

        public string Endian
        {
            get => _endian;
            set { _endian = value; OnPropertyChanged(); }
        }

        public int Length
        {
            get => _length;
            set { _length = value; OnPropertyChanged(); }
        }

        public string Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); OnPropertyChanged(nameof(BooleanValue)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
