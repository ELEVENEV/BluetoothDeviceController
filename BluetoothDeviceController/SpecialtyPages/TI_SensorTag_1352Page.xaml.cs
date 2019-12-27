﻿using BluetoothDeviceController.Charts;
using BluetoothDeviceController.Names;
using BluetoothProtocols;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Utilities;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BluetoothDeviceController.SpecialtyPages
{
    /// <summary>
    /// Specialty page for the [[NAME]] device
    /// </summary>
    public sealed partial class TI_SensorTag_1352Page : Page, HasId, ISetHandleStatus
    {
        public TI_SensorTag_1352Page()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }
        private string DeviceName = "TI_SensorTag_1352";
        private string DeviceNameUser = "Multi-Sensor";

        int ncommand = 0;
        TI_SensorTag_1352 bleDevice = new TI_SensorTag_1352();
        protected async override void OnNavigatedTo(NavigationEventArgs args)
        {
            SetStatusActive(true);
            var di = args.Parameter as DeviceInformation;
            var ble = await BluetoothLEDevice.FromIdAsync(di.Id);
            SetStatusActive(false);

            bleDevice.ble = ble;
            bleDevice.Status.OnBluetoothStatus += bleDevice_OnBluetoothStatus;
            DoReadDevice_Name();
        }

        public string GetId()
        {
            return bleDevice?.ble?.DeviceId ?? "";
        }

        public string GetPicturePath()
        {
            return $"/Assets/DevicePictures/{DeviceName}-175.PNG";
        }

        public string GetDeviceNameUser()
        {
            return $"{DeviceNameUser}";
        }

        private IHandleStatus ParentStatusHandler = null;

        public void SetHandleStatus(IHandleStatus handleStatus)
        {
            ParentStatusHandler = handleStatus;
        }

        private void SetStatus(string status)
        {
            uiStatus.Text = status;
            ParentStatusHandler?.SetStatusText(status);
        }
        private void SetStatusActive(bool isActive)
        {
            uiProgress.IsActive = isActive;
            ParentStatusHandler?.SetStatusActive(isActive);
        }

        private async void bleDevice_OnBluetoothStatus(object source, BluetoothCommunicationStatus status)
        {
            var now = DateTime.Now;
            var nowstr = $"{now.Hour:D2}:{now.Minute:D2}:{now.Second:D2}.{now.Millisecond:D03}";
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => {
                SetStatus(nowstr + ": " + status.AsStatusString);
                SetStatusActive(false);
            });
        }

        // Functions for Common Configuration


        public class Device_NameRecord : INotifyPropertyChanged
        {
            public Device_NameRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _Device_Name;
            public string Device_Name { get { return _Device_Name; } set { if (value == _Device_Name) return; _Device_Name = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Device_NameRecord> Device_NameRecordData { get; } = new DataCollection<Device_NameRecord>();
        private void OnDevice_Name_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Device_NameRecordData.Count == 0)
                {
                    Device_NameRecordData.AddRecord(new Device_NameRecord());
                }
                Device_NameRecordData[Device_NameRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountDevice_Name(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Device_NameRecordData.MaxLength = value;


        }

        private void OnAlgorithmDevice_Name(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Device_NameRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyDevice_Name(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Device_Name,Notes\n");
            foreach (var row in Device_NameRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Device_Name},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        public void DoReadDevice_Name()
        {
            OnReadDevice_Name(null, null);
        }

        private async void OnReadDevice_Name(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadDevice_Name();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Device_Name");
                    return;
                }

                var record = new Device_NameRecord();

                var Device_Name = valueList.GetValue("Device_Name");
                if (Device_Name.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Device_Name.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Device_Name = (string)Device_Name.AsString;
                    Device_Name_Device_Name.Text = record.Device_Name.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Device_NameRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class AppearanceRecord : INotifyPropertyChanged
        {
            public AppearanceRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Appearance;
            public double Appearance { get { return _Appearance; } set { if (value == _Appearance) return; _Appearance = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<AppearanceRecord> AppearanceRecordData { get; } = new DataCollection<AppearanceRecord>();
        private void OnAppearance_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (AppearanceRecordData.Count == 0)
                {
                    AppearanceRecordData.AddRecord(new AppearanceRecord());
                }
                AppearanceRecordData[AppearanceRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountAppearance(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            AppearanceRecordData.MaxLength = value;


        }

        private void OnAlgorithmAppearance(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            AppearanceRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyAppearance(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Appearance,Notes\n");
            foreach (var row in AppearanceRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Appearance},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadAppearance(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadAppearance();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Appearance");
                    return;
                }

                var record = new AppearanceRecord();

                var Appearance = valueList.GetValue("Appearance");
                if (Appearance.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Appearance.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Appearance = (double)Appearance.AsDouble;
                    Appearance_Appearance.Text = record.Appearance.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                AppearanceRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Connection_ParameterRecord : INotifyPropertyChanged
        {
            public Connection_ParameterRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _ConnectionParameter;
            public string ConnectionParameter { get { return _ConnectionParameter; } set { if (value == _ConnectionParameter) return; _ConnectionParameter = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Connection_ParameterRecord> Connection_ParameterRecordData { get; } = new DataCollection<Connection_ParameterRecord>();
        private void OnConnection_Parameter_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Connection_ParameterRecordData.Count == 0)
                {
                    Connection_ParameterRecordData.AddRecord(new Connection_ParameterRecord());
                }
                Connection_ParameterRecordData[Connection_ParameterRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountConnection_Parameter(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Connection_ParameterRecordData.MaxLength = value;


        }

        private void OnAlgorithmConnection_Parameter(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Connection_ParameterRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyConnection_Parameter(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,ConnectionParameter,Notes\n");
            foreach (var row in Connection_ParameterRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.ConnectionParameter},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadConnection_Parameter(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadConnection_Parameter();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Connection_Parameter");
                    return;
                }

                var record = new Connection_ParameterRecord();

                var ConnectionParameter = valueList.GetValue("ConnectionParameter");
                if (ConnectionParameter.CurrentType == BCBasic.BCValue.ValueType.IsDouble || ConnectionParameter.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.ConnectionParameter = (string)ConnectionParameter.AsString;
                    Connection_Parameter_ConnectionParameter.Text = record.ConnectionParameter.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Connection_ParameterRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Central_Address_ResolutionRecord : INotifyPropertyChanged
        {
            public Central_Address_ResolutionRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _AddressResolutionSupported;
            public double AddressResolutionSupported { get { return _AddressResolutionSupported; } set { if (value == _AddressResolutionSupported) return; _AddressResolutionSupported = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Central_Address_ResolutionRecord> Central_Address_ResolutionRecordData { get; } = new DataCollection<Central_Address_ResolutionRecord>();
        private void OnCentral_Address_Resolution_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Central_Address_ResolutionRecordData.Count == 0)
                {
                    Central_Address_ResolutionRecordData.AddRecord(new Central_Address_ResolutionRecord());
                }
                Central_Address_ResolutionRecordData[Central_Address_ResolutionRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountCentral_Address_Resolution(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Central_Address_ResolutionRecordData.MaxLength = value;


        }

        private void OnAlgorithmCentral_Address_Resolution(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Central_Address_ResolutionRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyCentral_Address_Resolution(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,AddressResolutionSupported,Notes\n");
            foreach (var row in Central_Address_ResolutionRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.AddressResolutionSupported},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadCentral_Address_Resolution(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadCentral_Address_Resolution();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Central_Address_Resolution");
                    return;
                }

                var record = new Central_Address_ResolutionRecord();

                var AddressResolutionSupported = valueList.GetValue("AddressResolutionSupported");
                if (AddressResolutionSupported.CurrentType == BCBasic.BCValue.ValueType.IsDouble || AddressResolutionSupported.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.AddressResolutionSupported = (double)AddressResolutionSupported.AsDouble;
                    Central_Address_Resolution_AddressResolutionSupported.Text = record.AddressResolutionSupported.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Central_Address_ResolutionRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Resolvable_Private_Address_OnlyRecord : INotifyPropertyChanged
        {
            public Resolvable_Private_Address_OnlyRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _ResolvablePrivateAddressFlag;
            public double ResolvablePrivateAddressFlag { get { return _ResolvablePrivateAddressFlag; } set { if (value == _ResolvablePrivateAddressFlag) return; _ResolvablePrivateAddressFlag = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Resolvable_Private_Address_OnlyRecord> Resolvable_Private_Address_OnlyRecordData { get; } = new DataCollection<Resolvable_Private_Address_OnlyRecord>();
        private void OnResolvable_Private_Address_Only_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Resolvable_Private_Address_OnlyRecordData.Count == 0)
                {
                    Resolvable_Private_Address_OnlyRecordData.AddRecord(new Resolvable_Private_Address_OnlyRecord());
                }
                Resolvable_Private_Address_OnlyRecordData[Resolvable_Private_Address_OnlyRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountResolvable_Private_Address_Only(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Resolvable_Private_Address_OnlyRecordData.MaxLength = value;


        }

        private void OnAlgorithmResolvable_Private_Address_Only(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Resolvable_Private_Address_OnlyRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyResolvable_Private_Address_Only(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,ResolvablePrivateAddressFlag,Notes\n");
            foreach (var row in Resolvable_Private_Address_OnlyRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.ResolvablePrivateAddressFlag},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadResolvable_Private_Address_Only(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadResolvable_Private_Address_Only();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Resolvable_Private_Address_Only");
                    return;
                }

                var record = new Resolvable_Private_Address_OnlyRecord();

                var ResolvablePrivateAddressFlag = valueList.GetValue("ResolvablePrivateAddressFlag");
                if (ResolvablePrivateAddressFlag.CurrentType == BCBasic.BCValue.ValueType.IsDouble || ResolvablePrivateAddressFlag.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.ResolvablePrivateAddressFlag = (double)ResolvablePrivateAddressFlag.AsDouble;
                    Resolvable_Private_Address_Only_ResolvablePrivateAddressFlag.Text = record.ResolvablePrivateAddressFlag.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Resolvable_Private_Address_OnlyRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }


        // Functions for Generic Service



        // Functions for Device Info


        public class System_IDRecord : INotifyPropertyChanged
        {
            public System_IDRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _SystemId;
            public string SystemId { get { return _SystemId; } set { if (value == _SystemId) return; _SystemId = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<System_IDRecord> System_IDRecordData { get; } = new DataCollection<System_IDRecord>();
        private void OnSystem_ID_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (System_IDRecordData.Count == 0)
                {
                    System_IDRecordData.AddRecord(new System_IDRecord());
                }
                System_IDRecordData[System_IDRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountSystem_ID(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            System_IDRecordData.MaxLength = value;


        }

        private void OnAlgorithmSystem_ID(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            System_IDRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopySystem_ID(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,SystemId,Notes\n");
            foreach (var row in System_IDRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.SystemId},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadSystem_ID(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadSystem_ID();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read System_ID");
                    return;
                }

                var record = new System_IDRecord();

                var SystemId = valueList.GetValue("SystemId");
                if (SystemId.CurrentType == BCBasic.BCValue.ValueType.IsDouble || SystemId.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.SystemId = (string)SystemId.AsString;
                    System_ID_SystemId.Text = record.SystemId.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                System_IDRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Model_NumberRecord : INotifyPropertyChanged
        {
            public Model_NumberRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _ModelNumber;
            public string ModelNumber { get { return _ModelNumber; } set { if (value == _ModelNumber) return; _ModelNumber = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Model_NumberRecord> Model_NumberRecordData { get; } = new DataCollection<Model_NumberRecord>();
        private void OnModel_Number_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Model_NumberRecordData.Count == 0)
                {
                    Model_NumberRecordData.AddRecord(new Model_NumberRecord());
                }
                Model_NumberRecordData[Model_NumberRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountModel_Number(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Model_NumberRecordData.MaxLength = value;


        }

        private void OnAlgorithmModel_Number(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Model_NumberRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyModel_Number(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,ModelNumber,Notes\n");
            foreach (var row in Model_NumberRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.ModelNumber},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadModel_Number(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadModel_Number();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Model_Number");
                    return;
                }

                var record = new Model_NumberRecord();

                var ModelNumber = valueList.GetValue("ModelNumber");
                if (ModelNumber.CurrentType == BCBasic.BCValue.ValueType.IsDouble || ModelNumber.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.ModelNumber = (string)ModelNumber.AsString;
                    Model_Number_ModelNumber.Text = record.ModelNumber.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Model_NumberRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Serial_NumberRecord : INotifyPropertyChanged
        {
            public Serial_NumberRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _SerialNumber;
            public string SerialNumber { get { return _SerialNumber; } set { if (value == _SerialNumber) return; _SerialNumber = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Serial_NumberRecord> Serial_NumberRecordData { get; } = new DataCollection<Serial_NumberRecord>();
        private void OnSerial_Number_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Serial_NumberRecordData.Count == 0)
                {
                    Serial_NumberRecordData.AddRecord(new Serial_NumberRecord());
                }
                Serial_NumberRecordData[Serial_NumberRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountSerial_Number(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Serial_NumberRecordData.MaxLength = value;


        }

        private void OnAlgorithmSerial_Number(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Serial_NumberRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopySerial_Number(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,SerialNumber,Notes\n");
            foreach (var row in Serial_NumberRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.SerialNumber},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadSerial_Number(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadSerial_Number();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Serial_Number");
                    return;
                }

                var record = new Serial_NumberRecord();

                var SerialNumber = valueList.GetValue("SerialNumber");
                if (SerialNumber.CurrentType == BCBasic.BCValue.ValueType.IsDouble || SerialNumber.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.SerialNumber = (string)SerialNumber.AsString;
                    Serial_Number_SerialNumber.Text = record.SerialNumber.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Serial_NumberRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Firmware_RevisionRecord : INotifyPropertyChanged
        {
            public Firmware_RevisionRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _FirmwareRevision;
            public string FirmwareRevision { get { return _FirmwareRevision; } set { if (value == _FirmwareRevision) return; _FirmwareRevision = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Firmware_RevisionRecord> Firmware_RevisionRecordData { get; } = new DataCollection<Firmware_RevisionRecord>();
        private void OnFirmware_Revision_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Firmware_RevisionRecordData.Count == 0)
                {
                    Firmware_RevisionRecordData.AddRecord(new Firmware_RevisionRecord());
                }
                Firmware_RevisionRecordData[Firmware_RevisionRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountFirmware_Revision(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Firmware_RevisionRecordData.MaxLength = value;


        }

        private void OnAlgorithmFirmware_Revision(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Firmware_RevisionRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyFirmware_Revision(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,FirmwareRevision,Notes\n");
            foreach (var row in Firmware_RevisionRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.FirmwareRevision},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadFirmware_Revision(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadFirmware_Revision();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Firmware_Revision");
                    return;
                }

                var record = new Firmware_RevisionRecord();

                var FirmwareRevision = valueList.GetValue("FirmwareRevision");
                if (FirmwareRevision.CurrentType == BCBasic.BCValue.ValueType.IsDouble || FirmwareRevision.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.FirmwareRevision = (string)FirmwareRevision.AsString;
                    Firmware_Revision_FirmwareRevision.Text = record.FirmwareRevision.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Firmware_RevisionRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Hardware_RevisionRecord : INotifyPropertyChanged
        {
            public Hardware_RevisionRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _HardwareRevision;
            public string HardwareRevision { get { return _HardwareRevision; } set { if (value == _HardwareRevision) return; _HardwareRevision = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Hardware_RevisionRecord> Hardware_RevisionRecordData { get; } = new DataCollection<Hardware_RevisionRecord>();
        private void OnHardware_Revision_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Hardware_RevisionRecordData.Count == 0)
                {
                    Hardware_RevisionRecordData.AddRecord(new Hardware_RevisionRecord());
                }
                Hardware_RevisionRecordData[Hardware_RevisionRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountHardware_Revision(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Hardware_RevisionRecordData.MaxLength = value;


        }

        private void OnAlgorithmHardware_Revision(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Hardware_RevisionRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyHardware_Revision(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,HardwareRevision,Notes\n");
            foreach (var row in Hardware_RevisionRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.HardwareRevision},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadHardware_Revision(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadHardware_Revision();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Hardware_Revision");
                    return;
                }

                var record = new Hardware_RevisionRecord();

                var HardwareRevision = valueList.GetValue("HardwareRevision");
                if (HardwareRevision.CurrentType == BCBasic.BCValue.ValueType.IsDouble || HardwareRevision.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.HardwareRevision = (string)HardwareRevision.AsString;
                    Hardware_Revision_HardwareRevision.Text = record.HardwareRevision.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Hardware_RevisionRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Software_RevisionRecord : INotifyPropertyChanged
        {
            public Software_RevisionRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _SoftwareRevision;
            public string SoftwareRevision { get { return _SoftwareRevision; } set { if (value == _SoftwareRevision) return; _SoftwareRevision = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Software_RevisionRecord> Software_RevisionRecordData { get; } = new DataCollection<Software_RevisionRecord>();
        private void OnSoftware_Revision_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Software_RevisionRecordData.Count == 0)
                {
                    Software_RevisionRecordData.AddRecord(new Software_RevisionRecord());
                }
                Software_RevisionRecordData[Software_RevisionRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountSoftware_Revision(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Software_RevisionRecordData.MaxLength = value;


        }

        private void OnAlgorithmSoftware_Revision(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Software_RevisionRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopySoftware_Revision(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,SoftwareRevision,Notes\n");
            foreach (var row in Software_RevisionRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.SoftwareRevision},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadSoftware_Revision(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadSoftware_Revision();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Software_Revision");
                    return;
                }

                var record = new Software_RevisionRecord();

                var SoftwareRevision = valueList.GetValue("SoftwareRevision");
                if (SoftwareRevision.CurrentType == BCBasic.BCValue.ValueType.IsDouble || SoftwareRevision.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.SoftwareRevision = (string)SoftwareRevision.AsString;
                    Software_Revision_SoftwareRevision.Text = record.SoftwareRevision.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Software_RevisionRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Manuafacturer_NameRecord : INotifyPropertyChanged
        {
            public Manuafacturer_NameRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _ManufacturerName;
            public string ManufacturerName { get { return _ManufacturerName; } set { if (value == _ManufacturerName) return; _ManufacturerName = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Manuafacturer_NameRecord> Manuafacturer_NameRecordData { get; } = new DataCollection<Manuafacturer_NameRecord>();
        private void OnManuafacturer_Name_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Manuafacturer_NameRecordData.Count == 0)
                {
                    Manuafacturer_NameRecordData.AddRecord(new Manuafacturer_NameRecord());
                }
                Manuafacturer_NameRecordData[Manuafacturer_NameRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountManuafacturer_Name(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Manuafacturer_NameRecordData.MaxLength = value;


        }

        private void OnAlgorithmManuafacturer_Name(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Manuafacturer_NameRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyManuafacturer_Name(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,ManufacturerName,Notes\n");
            foreach (var row in Manuafacturer_NameRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.ManufacturerName},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadManuafacturer_Name(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadManuafacturer_Name();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Manuafacturer_Name");
                    return;
                }

                var record = new Manuafacturer_NameRecord();

                var ManufacturerName = valueList.GetValue("ManufacturerName");
                if (ManufacturerName.CurrentType == BCBasic.BCValue.ValueType.IsDouble || ManufacturerName.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.ManufacturerName = (string)ManufacturerName.AsString;
                    Manuafacturer_Name_ManufacturerName.Text = record.ManufacturerName.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Manuafacturer_NameRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Regulatory_ListRecord : INotifyPropertyChanged
        {
            public Regulatory_ListRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _BodyType;
            public double BodyType { get { return _BodyType; } set { if (value == _BodyType) return; _BodyType = value; OnPropertyChanged(); } }

            private double _BodyStructure;
            public double BodyStructure { get { return _BodyStructure; } set { if (value == _BodyStructure) return; _BodyStructure = value; OnPropertyChanged(); } }

            private string _Data;
            public string Data { get { return _Data; } set { if (value == _Data) return; _Data = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Regulatory_ListRecord> Regulatory_ListRecordData { get; } = new DataCollection<Regulatory_ListRecord>();
        private void OnRegulatory_List_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Regulatory_ListRecordData.Count == 0)
                {
                    Regulatory_ListRecordData.AddRecord(new Regulatory_ListRecord());
                }
                Regulatory_ListRecordData[Regulatory_ListRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountRegulatory_List(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Regulatory_ListRecordData.MaxLength = value;


        }

        private void OnAlgorithmRegulatory_List(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Regulatory_ListRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyRegulatory_List(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,BodyType,BodyStructure,Data,Notes\n");
            foreach (var row in Regulatory_ListRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.BodyType},{row.BodyStructure},{row.Data},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadRegulatory_List(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadRegulatory_List();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Regulatory_List");
                    return;
                }

                var record = new Regulatory_ListRecord();

                var BodyType = valueList.GetValue("BodyType");
                if (BodyType.CurrentType == BCBasic.BCValue.ValueType.IsDouble || BodyType.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.BodyType = (double)BodyType.AsDouble;
                    Regulatory_List_BodyType.Text = record.BodyType.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }

                var BodyStructure = valueList.GetValue("BodyStructure");
                if (BodyStructure.CurrentType == BCBasic.BCValue.ValueType.IsDouble || BodyStructure.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.BodyStructure = (double)BodyStructure.AsDouble;
                    Regulatory_List_BodyStructure.Text = record.BodyStructure.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }

                var Data = valueList.GetValue("Data");
                if (Data.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Data.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Data = (string)Data.AsString;
                    Regulatory_List_Data.Text = record.Data.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Regulatory_ListRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class PnP_IDRecord : INotifyPropertyChanged
        {
            public PnP_IDRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _PnPID;
            public string PnPID { get { return _PnPID; } set { if (value == _PnPID) return; _PnPID = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<PnP_IDRecord> PnP_IDRecordData { get; } = new DataCollection<PnP_IDRecord>();
        private void OnPnP_ID_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (PnP_IDRecordData.Count == 0)
                {
                    PnP_IDRecordData.AddRecord(new PnP_IDRecord());
                }
                PnP_IDRecordData[PnP_IDRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountPnP_ID(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            PnP_IDRecordData.MaxLength = value;


        }

        private void OnAlgorithmPnP_ID(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            PnP_IDRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyPnP_ID(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,PnPID,Notes\n");
            foreach (var row in PnP_IDRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.PnPID},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadPnP_ID(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadPnP_ID();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read PnP_ID");
                    return;
                }

                var record = new PnP_IDRecord();

                var PnPID = valueList.GetValue("PnPID");
                if (PnPID.CurrentType == BCBasic.BCValue.ValueType.IsDouble || PnPID.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.PnPID = (string)PnPID.AsString;
                    PnP_ID_PnPID.Text = record.PnPID.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                PnP_IDRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }


        // Functions for Imaging


        private async void OnWriteImg_Identify(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Bytes Img_Identify;
                var parsedImg_Identify = Utilities.Parsers.TryParseBytes(Img_Identify_Img_Identify.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Img_Identify);
                if (!parsedImg_Identify)
                {
                    parseError = "Img Identify";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteImg_Identify(Img_Identify);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Img_IdentifyRecord : INotifyPropertyChanged
        {
            public Img_IdentifyRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _Img_Identify;
            public string Img_Identify { get { return _Img_Identify; } set { if (value == _Img_Identify) return; _Img_Identify = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Img_IdentifyRecord> Img_IdentifyRecordData { get; } = new DataCollection<Img_IdentifyRecord>();
        private void OnImg_Identify_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Img_IdentifyRecordData.Count == 0)
                {
                    Img_IdentifyRecordData.AddRecord(new Img_IdentifyRecord());
                }
                Img_IdentifyRecordData[Img_IdentifyRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountImg_Identify(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Img_IdentifyRecordData.MaxLength = value;


        }

        private void OnAlgorithmImg_Identify(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Img_IdentifyRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyImg_Identify(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Img_Identify,Notes\n");
            foreach (var row in Img_IdentifyRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Img_Identify},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyImg_IdentifySettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int Img_IdentifyNotifyIndex = 0;
        bool Img_IdentifyNotifySetup = false;
        private async void OnNotifyImg_Identify(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!Img_IdentifyNotifySetup)
                {
                    Img_IdentifyNotifySetup = true;
                    bleDevice.Img_IdentifyEvent += BleDevice_Img_IdentifyEvent;
                }
                var notifyType = NotifyImg_IdentifySettings[Img_IdentifyNotifyIndex];
                Img_IdentifyNotifyIndex = (Img_IdentifyNotifyIndex + 1) % NotifyImg_IdentifySettings.Length;
                var result = await bleDevice.NotifyImg_IdentifyAsync(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_Img_IdentifyEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new Img_IdentifyRecord();

                    var Img_Identify = valueList.GetValue("Img_Identify");
                    if (Img_Identify.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Img_Identify.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.Img_Identify = (string)Img_Identify.AsString;
                        Img_Identify_Img_Identify.Text = record.Img_Identify.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = Img_IdentifyRecordData.AddRecord(record);

                });
            }
        }
        private async void OnWriteImg_Block(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Bytes Img_Block;
                var parsedImg_Block = Utilities.Parsers.TryParseBytes(Img_Block_Img_Block.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Img_Block);
                if (!parsedImg_Block)
                {
                    parseError = "Img Block";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteImg_Block(Img_Block);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Img_BlockRecord : INotifyPropertyChanged
        {
            public Img_BlockRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _Img_Block;
            public string Img_Block { get { return _Img_Block; } set { if (value == _Img_Block) return; _Img_Block = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Img_BlockRecord> Img_BlockRecordData { get; } = new DataCollection<Img_BlockRecord>();
        private void OnImg_Block_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Img_BlockRecordData.Count == 0)
                {
                    Img_BlockRecordData.AddRecord(new Img_BlockRecord());
                }
                Img_BlockRecordData[Img_BlockRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountImg_Block(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Img_BlockRecordData.MaxLength = value;


        }

        private void OnAlgorithmImg_Block(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Img_BlockRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyImg_Block(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Img_Block,Notes\n");
            foreach (var row in Img_BlockRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Img_Block},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyImg_BlockSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int Img_BlockNotifyIndex = 0;
        bool Img_BlockNotifySetup = false;
        private async void OnNotifyImg_Block(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!Img_BlockNotifySetup)
                {
                    Img_BlockNotifySetup = true;
                    bleDevice.Img_BlockEvent += BleDevice_Img_BlockEvent;
                }
                var notifyType = NotifyImg_BlockSettings[Img_BlockNotifyIndex];
                Img_BlockNotifyIndex = (Img_BlockNotifyIndex + 1) % NotifyImg_BlockSettings.Length;
                var result = await bleDevice.NotifyImg_BlockAsync(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_Img_BlockEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new Img_BlockRecord();

                    var Img_Block = valueList.GetValue("Img_Block");
                    if (Img_Block.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Img_Block.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.Img_Block = (string)Img_Block.AsString;
                        Img_Block_Img_Block.Text = record.Img_Block.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = Img_BlockRecordData.AddRecord(record);

                });
            }
        }
        private async void OnWriteOAD_Extended_Control(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Bytes OAD_Extended_Control;
                var parsedOAD_Extended_Control = Utilities.Parsers.TryParseBytes(OAD_Extended_Control_OAD_Extended_Control.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out OAD_Extended_Control);
                if (!parsedOAD_Extended_Control)
                {
                    parseError = "OAD Extended Control";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteOAD_Extended_Control(OAD_Extended_Control);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class OAD_Extended_ControlRecord : INotifyPropertyChanged
        {
            public OAD_Extended_ControlRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _OAD_Extended_Control;
            public string OAD_Extended_Control { get { return _OAD_Extended_Control; } set { if (value == _OAD_Extended_Control) return; _OAD_Extended_Control = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<OAD_Extended_ControlRecord> OAD_Extended_ControlRecordData { get; } = new DataCollection<OAD_Extended_ControlRecord>();
        private void OnOAD_Extended_Control_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (OAD_Extended_ControlRecordData.Count == 0)
                {
                    OAD_Extended_ControlRecordData.AddRecord(new OAD_Extended_ControlRecord());
                }
                OAD_Extended_ControlRecordData[OAD_Extended_ControlRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountOAD_Extended_Control(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            OAD_Extended_ControlRecordData.MaxLength = value;


        }

        private void OnAlgorithmOAD_Extended_Control(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            OAD_Extended_ControlRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyOAD_Extended_Control(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,OAD_Extended_Control,Notes\n");
            foreach (var row in OAD_Extended_ControlRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.OAD_Extended_Control},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyOAD_Extended_ControlSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int OAD_Extended_ControlNotifyIndex = 0;
        bool OAD_Extended_ControlNotifySetup = false;
        private async void OnNotifyOAD_Extended_Control(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!OAD_Extended_ControlNotifySetup)
                {
                    OAD_Extended_ControlNotifySetup = true;
                    bleDevice.OAD_Extended_ControlEvent += BleDevice_OAD_Extended_ControlEvent;
                }
                var notifyType = NotifyOAD_Extended_ControlSettings[OAD_Extended_ControlNotifyIndex];
                OAD_Extended_ControlNotifyIndex = (OAD_Extended_ControlNotifyIndex + 1) % NotifyOAD_Extended_ControlSettings.Length;
                var result = await bleDevice.NotifyOAD_Extended_ControlAsync(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_OAD_Extended_ControlEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new OAD_Extended_ControlRecord();

                    var OAD_Extended_Control = valueList.GetValue("OAD_Extended_Control");
                    if (OAD_Extended_Control.CurrentType == BCBasic.BCValue.ValueType.IsDouble || OAD_Extended_Control.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.OAD_Extended_Control = (string)OAD_Extended_Control.AsString;
                        OAD_Extended_Control_OAD_Extended_Control.Text = record.OAD_Extended_Control.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = OAD_Extended_ControlRecordData.AddRecord(record);

                });
            }
        }

        // Functions for Temperature


        public class Temperature_DataRecord : INotifyPropertyChanged
        {
            public Temperature_DataRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Temperature;
            public double Temperature { get { return _Temperature; } set { if (value == _Temperature) return; _Temperature = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Temperature_DataRecord> Temperature_DataRecordData { get; } = new DataCollection<Temperature_DataRecord>();
        private void OnTemperature_Data_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Temperature_DataRecordData.Count == 0)
                {
                    Temperature_DataRecordData.AddRecord(new Temperature_DataRecord());
                }
                Temperature_DataRecordData[Temperature_DataRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountTemperature_Data(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Temperature_DataRecordData.MaxLength = value;

            Temperature_DataChart.RedrawYTime<Temperature_DataRecord>(Temperature_DataRecordData);
        }

        private void OnAlgorithmTemperature_Data(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Temperature_DataRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyTemperature_Data(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Temperature,Notes\n");
            foreach (var row in Temperature_DataRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Temperature},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadTemperature_Data(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadTemperature_Data();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Temperature_Data");
                    return;
                }

                var record = new Temperature_DataRecord();

                var Temperature = valueList.GetValue("Temperature");
                if (Temperature.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Temperature.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Temperature = (double)Temperature.AsDouble;
                    Temperature_Data_Temperature.Text = record.Temperature.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Temperature_DataRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyTemperature_DataSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int Temperature_DataNotifyIndex = 0;
        bool Temperature_DataNotifySetup = false;
        private async void OnNotifyTemperature_Data(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!Temperature_DataNotifySetup)
                {
                    Temperature_DataNotifySetup = true;
                    bleDevice.Temperature_DataEvent += BleDevice_Temperature_DataEvent;
                }
                var notifyType = NotifyTemperature_DataSettings[Temperature_DataNotifyIndex];
                Temperature_DataNotifyIndex = (Temperature_DataNotifyIndex + 1) % NotifyTemperature_DataSettings.Length;
                var result = await bleDevice.NotifyTemperature_DataAsync(notifyType);



                var EventTimeProperty = typeof(Temperature_DataRecord).GetProperty("EventTime");
                var properties = new System.Collections.Generic.List<System.Reflection.PropertyInfo>()
                {
typeof(Temperature_DataRecord).GetProperty("Temperature"),
                };
                var names = new List<string>()
                {
"Temperature",
                };
                Temperature_DataChart.SetDataProperties(properties, EventTimeProperty, names);
                Temperature_DataChart.SetTitle("Temperature Data Chart");
                Temperature_DataChart.UISpec = new BluetoothDeviceController.Names.UISpecifications()
                {
                    tableType = "standard",
                    chartType = "standard",
                    chartCommand = "AddYTime<Temperature_PeriodRecord>(addResult, Temperature_PeriodRecordData)",
                    chartDefaultMaxY = 25,
                    chartDefaultMinY = 20,
                }
;

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_Temperature_DataEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new Temperature_DataRecord();

                    var Temperature = valueList.GetValue("Temperature");
                    if (Temperature.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Temperature.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.Temperature = (double)Temperature.AsDouble;
                        Temperature_Data_Temperature.Text = record.Temperature.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = Temperature_DataRecordData.AddRecord(record);
                    Temperature_DataChart.AddYTime<Temperature_DataRecord>(addResult, Temperature_DataRecordData);
                });
            }
        }
        private async void OnWriteTemperature_Conf(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Byte Enable;
                var parsedEnable = Utilities.Parsers.TryParseByte(Temperature_Conf_Enable.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Enable);
                if (!parsedEnable)
                {
                    parseError = "Enable";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteTemperature_Conf(Enable);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Temperature_ConfRecord : INotifyPropertyChanged
        {
            public Temperature_ConfRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Enable;
            public double Enable { get { return _Enable; } set { if (value == _Enable) return; _Enable = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Temperature_ConfRecord> Temperature_ConfRecordData { get; } = new DataCollection<Temperature_ConfRecord>();
        private void OnTemperature_Conf_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Temperature_ConfRecordData.Count == 0)
                {
                    Temperature_ConfRecordData.AddRecord(new Temperature_ConfRecord());
                }
                Temperature_ConfRecordData[Temperature_ConfRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountTemperature_Conf(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Temperature_ConfRecordData.MaxLength = value;


        }

        private void OnAlgorithmTemperature_Conf(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Temperature_ConfRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyTemperature_Conf(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Enable,Notes\n");
            foreach (var row in Temperature_ConfRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Enable},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadTemperature_Conf(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadTemperature_Conf();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Temperature_Conf");
                    return;
                }

                var record = new Temperature_ConfRecord();

                var Enable = valueList.GetValue("Enable");
                if (Enable.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Enable.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Enable = (double)Enable.AsDouble;
                    Temperature_Conf_Enable.Text = record.Enable.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Temperature_ConfRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void OnWriteTemperature_Period(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Byte Period;
                var parsedPeriod = Utilities.Parsers.TryParseByte(Temperature_Period_Period.Text, System.Globalization.NumberStyles.None, null, out Period);
                if (!parsedPeriod)
                {
                    parseError = "Period";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteTemperature_Period(Period);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Temperature_PeriodRecord : INotifyPropertyChanged
        {
            public Temperature_PeriodRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Period;
            public double Period { get { return _Period; } set { if (value == _Period) return; _Period = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Temperature_PeriodRecord> Temperature_PeriodRecordData { get; } = new DataCollection<Temperature_PeriodRecord>();
        private void OnTemperature_Period_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Temperature_PeriodRecordData.Count == 0)
                {
                    Temperature_PeriodRecordData.AddRecord(new Temperature_PeriodRecord());
                }
                Temperature_PeriodRecordData[Temperature_PeriodRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountTemperature_Period(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Temperature_PeriodRecordData.MaxLength = value;


        }

        private void OnAlgorithmTemperature_Period(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Temperature_PeriodRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyTemperature_Period(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Period,Notes\n");
            foreach (var row in Temperature_PeriodRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Period},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadTemperature_Period(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadTemperature_Period();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Temperature_Period");
                    return;
                }

                var record = new Temperature_PeriodRecord();

                var Period = valueList.GetValue("Period");
                if (Period.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Period.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Period = (double)Period.AsDouble;
                    Temperature_Period_Period.Text = record.Period.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Temperature_PeriodRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }


        // Functions for Humidity


        public class Humidity_DataRecord : INotifyPropertyChanged
        {
            public Humidity_DataRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Humidty;
            public double Humidty { get { return _Humidty; } set { if (value == _Humidty) return; _Humidty = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Humidity_DataRecord> Humidity_DataRecordData { get; } = new DataCollection<Humidity_DataRecord>();
        private void OnHumidity_Data_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Humidity_DataRecordData.Count == 0)
                {
                    Humidity_DataRecordData.AddRecord(new Humidity_DataRecord());
                }
                Humidity_DataRecordData[Humidity_DataRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountHumidity_Data(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Humidity_DataRecordData.MaxLength = value;

            Humidity_DataChart.RedrawYTime<Humidity_DataRecord>(Humidity_DataRecordData);
        }

        private void OnAlgorithmHumidity_Data(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Humidity_DataRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyHumidity_Data(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Humidty,Notes\n");
            foreach (var row in Humidity_DataRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Humidty},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadHumidity_Data(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadHumidity_Data();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Humidity_Data");
                    return;
                }

                var record = new Humidity_DataRecord();

                var Humidty = valueList.GetValue("Humidty");
                if (Humidty.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Humidty.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Humidty = (double)Humidty.AsDouble;
                    Humidity_Data_Humidty.Text = record.Humidty.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Humidity_DataRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyHumidity_DataSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int Humidity_DataNotifyIndex = 0;
        bool Humidity_DataNotifySetup = false;
        private async void OnNotifyHumidity_Data(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!Humidity_DataNotifySetup)
                {
                    Humidity_DataNotifySetup = true;
                    bleDevice.Humidity_DataEvent += BleDevice_Humidity_DataEvent;
                }
                var notifyType = NotifyHumidity_DataSettings[Humidity_DataNotifyIndex];
                Humidity_DataNotifyIndex = (Humidity_DataNotifyIndex + 1) % NotifyHumidity_DataSettings.Length;
                var result = await bleDevice.NotifyHumidity_DataAsync(notifyType);



                var EventTimeProperty = typeof(Humidity_DataRecord).GetProperty("EventTime");
                var properties = new System.Collections.Generic.List<System.Reflection.PropertyInfo>()
                {
typeof(Humidity_DataRecord).GetProperty("Humidty"),
                };
                var names = new List<string>()
                {
"Humidty",
                };
                Humidity_DataChart.SetDataProperties(properties, EventTimeProperty, names);
                Humidity_DataChart.SetTitle("Humidity Data Chart");
                Humidity_DataChart.UISpec = new BluetoothDeviceController.Names.UISpecifications()
                {
                    tableType = "standard",
                    chartType = "standard",
                    chartCommand = "AddYTime<Humidity_PeriodRecord>(addResult, Humidity_PeriodRecordData)",
                    chartDefaultMaxY = 100,
                    chartDefaultMinY = 0,
                }
;

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_Humidity_DataEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new Humidity_DataRecord();

                    var Humidty = valueList.GetValue("Humidty");
                    if (Humidty.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Humidty.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.Humidty = (double)Humidty.AsDouble;
                        Humidity_Data_Humidty.Text = record.Humidty.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = Humidity_DataRecordData.AddRecord(record);
                    Humidity_DataChart.AddYTime<Humidity_DataRecord>(addResult, Humidity_DataRecordData);
                });
            }
        }
        private async void OnWriteHumidity_Conf(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Byte Enable;
                var parsedEnable = Utilities.Parsers.TryParseByte(Humidity_Conf_Enable.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Enable);
                if (!parsedEnable)
                {
                    parseError = "Enable";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteHumidity_Conf(Enable);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Humidity_ConfRecord : INotifyPropertyChanged
        {
            public Humidity_ConfRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Enable;
            public double Enable { get { return _Enable; } set { if (value == _Enable) return; _Enable = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Humidity_ConfRecord> Humidity_ConfRecordData { get; } = new DataCollection<Humidity_ConfRecord>();
        private void OnHumidity_Conf_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Humidity_ConfRecordData.Count == 0)
                {
                    Humidity_ConfRecordData.AddRecord(new Humidity_ConfRecord());
                }
                Humidity_ConfRecordData[Humidity_ConfRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountHumidity_Conf(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Humidity_ConfRecordData.MaxLength = value;


        }

        private void OnAlgorithmHumidity_Conf(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Humidity_ConfRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyHumidity_Conf(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Enable,Notes\n");
            foreach (var row in Humidity_ConfRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Enable},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadHumidity_Conf(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadHumidity_Conf();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Humidity_Conf");
                    return;
                }

                var record = new Humidity_ConfRecord();

                var Enable = valueList.GetValue("Enable");
                if (Enable.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Enable.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Enable = (double)Enable.AsDouble;
                    Humidity_Conf_Enable.Text = record.Enable.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Humidity_ConfRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void OnWriteHumidity_Period(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Byte Period;
                var parsedPeriod = Utilities.Parsers.TryParseByte(Humidity_Period_Period.Text, System.Globalization.NumberStyles.None, null, out Period);
                if (!parsedPeriod)
                {
                    parseError = "Period";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteHumidity_Period(Period);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Humidity_PeriodRecord : INotifyPropertyChanged
        {
            public Humidity_PeriodRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Period;
            public double Period { get { return _Period; } set { if (value == _Period) return; _Period = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Humidity_PeriodRecord> Humidity_PeriodRecordData { get; } = new DataCollection<Humidity_PeriodRecord>();
        private void OnHumidity_Period_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Humidity_PeriodRecordData.Count == 0)
                {
                    Humidity_PeriodRecordData.AddRecord(new Humidity_PeriodRecord());
                }
                Humidity_PeriodRecordData[Humidity_PeriodRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountHumidity_Period(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Humidity_PeriodRecordData.MaxLength = value;


        }

        private void OnAlgorithmHumidity_Period(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Humidity_PeriodRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyHumidity_Period(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Period,Notes\n");
            foreach (var row in Humidity_PeriodRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Period},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadHumidity_Period(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadHumidity_Period();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Humidity_Period");
                    return;
                }

                var record = new Humidity_PeriodRecord();

                var Period = valueList.GetValue("Period");
                if (Period.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Period.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Period = (double)Period.AsDouble;
                    Humidity_Period_Period.Text = record.Period.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Humidity_PeriodRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }


        // Functions for Optical Service


        public class Light_DataRecord : INotifyPropertyChanged
        {
            public Light_DataRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Lux;
            public double Lux { get { return _Lux; } set { if (value == _Lux) return; _Lux = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Light_DataRecord> Light_DataRecordData { get; } = new DataCollection<Light_DataRecord>();
        private void OnLight_Data_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Light_DataRecordData.Count == 0)
                {
                    Light_DataRecordData.AddRecord(new Light_DataRecord());
                }
                Light_DataRecordData[Light_DataRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountLight_Data(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Light_DataRecordData.MaxLength = value;


        }

        private void OnAlgorithmLight_Data(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Light_DataRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyLight_Data(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Lux,Notes\n");
            foreach (var row in Light_DataRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Lux},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadLight_Data(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadLight_Data();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Light_Data");
                    return;
                }

                var record = new Light_DataRecord();

                var Lux = valueList.GetValue("Lux");
                if (Lux.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Lux.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Lux = (double)Lux.AsDouble;
                    Light_Data_Lux.Text = record.Lux.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Light_DataRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyLight_DataSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int Light_DataNotifyIndex = 0;
        bool Light_DataNotifySetup = false;
        private async void OnNotifyLight_Data(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!Light_DataNotifySetup)
                {
                    Light_DataNotifySetup = true;
                    bleDevice.Light_DataEvent += BleDevice_Light_DataEvent;
                }
                var notifyType = NotifyLight_DataSettings[Light_DataNotifyIndex];
                Light_DataNotifyIndex = (Light_DataNotifyIndex + 1) % NotifyLight_DataSettings.Length;
                var result = await bleDevice.NotifyLight_DataAsync(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_Light_DataEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new Light_DataRecord();

                    var Lux = valueList.GetValue("Lux");
                    if (Lux.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Lux.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.Lux = (double)Lux.AsDouble;
                        Light_Data_Lux.Text = record.Lux.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = Light_DataRecordData.AddRecord(record);

                });
            }
        }
        private async void OnWriteLight_Conf(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Bytes Enable;
                var parsedEnable = Utilities.Parsers.TryParseBytes(Light_Conf_Enable.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Enable);
                if (!parsedEnable)
                {
                    parseError = "Enable";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteLight_Conf(Enable);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Light_ConfRecord : INotifyPropertyChanged
        {
            public Light_ConfRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _Enable;
            public string Enable { get { return _Enable; } set { if (value == _Enable) return; _Enable = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Light_ConfRecord> Light_ConfRecordData { get; } = new DataCollection<Light_ConfRecord>();
        private void OnLight_Conf_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Light_ConfRecordData.Count == 0)
                {
                    Light_ConfRecordData.AddRecord(new Light_ConfRecord());
                }
                Light_ConfRecordData[Light_ConfRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountLight_Conf(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Light_ConfRecordData.MaxLength = value;


        }

        private void OnAlgorithmLight_Conf(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Light_ConfRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyLight_Conf(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Enable,Notes\n");
            foreach (var row in Light_ConfRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Enable},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadLight_Conf(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadLight_Conf();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Light_Conf");
                    return;
                }

                var record = new Light_ConfRecord();

                var Enable = valueList.GetValue("Enable");
                if (Enable.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Enable.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Enable = (string)Enable.AsString;
                    Light_Conf_Enable.Text = record.Enable.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Light_ConfRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void OnWriteLight_Period(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Bytes Light_Period;
                var parsedLight_Period = Utilities.Parsers.TryParseBytes(Light_Period_Light_Period.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Light_Period);
                if (!parsedLight_Period)
                {
                    parseError = "Light Period";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteLight_Period(Light_Period);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Light_PeriodRecord : INotifyPropertyChanged
        {
            public Light_PeriodRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _Light_Period;
            public string Light_Period { get { return _Light_Period; } set { if (value == _Light_Period) return; _Light_Period = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Light_PeriodRecord> Light_PeriodRecordData { get; } = new DataCollection<Light_PeriodRecord>();
        private void OnLight_Period_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Light_PeriodRecordData.Count == 0)
                {
                    Light_PeriodRecordData.AddRecord(new Light_PeriodRecord());
                }
                Light_PeriodRecordData[Light_PeriodRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountLight_Period(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Light_PeriodRecordData.MaxLength = value;


        }

        private void OnAlgorithmLight_Period(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Light_PeriodRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyLight_Period(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Light_Period,Notes\n");
            foreach (var row in Light_PeriodRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Light_Period},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadLight_Period(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadLight_Period();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Light_Period");
                    return;
                }

                var record = new Light_PeriodRecord();

                var Light_Period = valueList.GetValue("Light_Period");
                if (Light_Period.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Light_Period.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Light_Period = (string)Light_Period.AsString;
                    Light_Period_Light_Period.Text = record.Light_Period.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Light_PeriodRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }


        // Functions for LED


        private async void OnWriteRed(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Byte Red;
                var parsedRed = Utilities.Parsers.TryParseByte(Red_Red.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Red);
                if (!parsedRed)
                {
                    parseError = "Red";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteRed(Red);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class RedRecord : INotifyPropertyChanged
        {
            public RedRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Red;
            public double Red { get { return _Red; } set { if (value == _Red) return; _Red = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<RedRecord> RedRecordData { get; } = new DataCollection<RedRecord>();
        private void OnRed_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (RedRecordData.Count == 0)
                {
                    RedRecordData.AddRecord(new RedRecord());
                }
                RedRecordData[RedRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountRed(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            RedRecordData.MaxLength = value;


        }

        private void OnAlgorithmRed(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            RedRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyRed(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Red,Notes\n");
            foreach (var row in RedRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Red},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadRed(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadRed();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Red");
                    return;
                }

                var record = new RedRecord();

                var Red = valueList.GetValue("Red");
                if (Red.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Red.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Red = (double)Red.AsDouble;
                    Red_Red.Text = record.Red.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                RedRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void OnWriteGreen(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Byte Green;
                var parsedGreen = Utilities.Parsers.TryParseByte(Green_Green.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Green);
                if (!parsedGreen)
                {
                    parseError = "Green";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteGreen(Green);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class GreenRecord : INotifyPropertyChanged
        {
            public GreenRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Green;
            public double Green { get { return _Green; } set { if (value == _Green) return; _Green = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<GreenRecord> GreenRecordData { get; } = new DataCollection<GreenRecord>();
        private void OnGreen_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (GreenRecordData.Count == 0)
                {
                    GreenRecordData.AddRecord(new GreenRecord());
                }
                GreenRecordData[GreenRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountGreen(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            GreenRecordData.MaxLength = value;


        }

        private void OnAlgorithmGreen(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            GreenRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyGreen(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Green,Notes\n");
            foreach (var row in GreenRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Green},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadGreen(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadGreen();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Green");
                    return;
                }

                var record = new GreenRecord();

                var Green = valueList.GetValue("Green");
                if (Green.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Green.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Green = (double)Green.AsDouble;
                    Green_Green.Text = record.Green.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                GreenRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void OnWriteBlue(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Byte Blue;
                var parsedBlue = Utilities.Parsers.TryParseByte(Blue_Blue.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Blue);
                if (!parsedBlue)
                {
                    parseError = "Blue";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteBlue(Blue);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class BlueRecord : INotifyPropertyChanged
        {
            public BlueRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Blue;
            public double Blue { get { return _Blue; } set { if (value == _Blue) return; _Blue = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<BlueRecord> BlueRecordData { get; } = new DataCollection<BlueRecord>();
        private void OnBlue_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (BlueRecordData.Count == 0)
                {
                    BlueRecordData.AddRecord(new BlueRecord());
                }
                BlueRecordData[BlueRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountBlue(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            BlueRecordData.MaxLength = value;


        }

        private void OnAlgorithmBlue(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            BlueRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyBlue(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Blue,Notes\n");
            foreach (var row in BlueRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Blue},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadBlue(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadBlue();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Blue");
                    return;
                }

                var record = new BlueRecord();

                var Blue = valueList.GetValue("Blue");
                if (Blue.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Blue.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Blue = (double)Blue.AsDouble;
                    Blue_Blue.Text = record.Blue.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                BlueRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }


        // Functions for Button


        public class Button_0Record : INotifyPropertyChanged
        {
            public Button_0Record()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Button0;
            public double Button0 { get { return _Button0; } set { if (value == _Button0) return; _Button0 = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Button_0Record> Button_0RecordData { get; } = new DataCollection<Button_0Record>();
        private void OnButton_0_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Button_0RecordData.Count == 0)
                {
                    Button_0RecordData.AddRecord(new Button_0Record());
                }
                Button_0RecordData[Button_0RecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountButton_0(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Button_0RecordData.MaxLength = value;


        }

        private void OnAlgorithmButton_0(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Button_0RecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyButton_0(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Button0,Notes\n");
            foreach (var row in Button_0RecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Button0},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadButton_0(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadButton_0();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Button_0");
                    return;
                }

                var record = new Button_0Record();

                var Button0 = valueList.GetValue("Button0");
                if (Button0.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Button0.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Button0 = (double)Button0.AsDouble;
                    Button_0_Button0.Text = record.Button0.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Button_0RecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyButton_0Settings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int Button_0NotifyIndex = 0;
        bool Button_0NotifySetup = false;
        private async void OnNotifyButton_0(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!Button_0NotifySetup)
                {
                    Button_0NotifySetup = true;
                    bleDevice.Button_0Event += BleDevice_Button_0Event;
                }
                var notifyType = NotifyButton_0Settings[Button_0NotifyIndex];
                Button_0NotifyIndex = (Button_0NotifyIndex + 1) % NotifyButton_0Settings.Length;
                var result = await bleDevice.NotifyButton_0Async(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_Button_0Event(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new Button_0Record();

                    var Button0 = valueList.GetValue("Button0");
                    if (Button0.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Button0.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.Button0 = (double)Button0.AsDouble;
                        Button_0_Button0.Text = record.Button0.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = Button_0RecordData.AddRecord(record);

                });
            }
        }
        public class Button_1Record : INotifyPropertyChanged
        {
            public Button_1Record()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Button1;
            public double Button1 { get { return _Button1; } set { if (value == _Button1) return; _Button1 = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Button_1Record> Button_1RecordData { get; } = new DataCollection<Button_1Record>();
        private void OnButton_1_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Button_1RecordData.Count == 0)
                {
                    Button_1RecordData.AddRecord(new Button_1Record());
                }
                Button_1RecordData[Button_1RecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountButton_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Button_1RecordData.MaxLength = value;


        }

        private void OnAlgorithmButton_1(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Button_1RecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyButton_1(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Button1,Notes\n");
            foreach (var row in Button_1RecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Button1},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadButton_1(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadButton_1();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Button_1");
                    return;
                }

                var record = new Button_1Record();

                var Button1 = valueList.GetValue("Button1");
                if (Button1.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Button1.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Button1 = (double)Button1.AsDouble;
                    Button_1_Button1.Text = record.Button1.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Button_1RecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyButton_1Settings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int Button_1NotifyIndex = 0;
        bool Button_1NotifySetup = false;
        private async void OnNotifyButton_1(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!Button_1NotifySetup)
                {
                    Button_1NotifySetup = true;
                    bleDevice.Button_1Event += BleDevice_Button_1Event;
                }
                var notifyType = NotifyButton_1Settings[Button_1NotifyIndex];
                Button_1NotifyIndex = (Button_1NotifyIndex + 1) % NotifyButton_1Settings.Length;
                var result = await bleDevice.NotifyButton_1Async(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_Button_1Event(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new Button_1Record();

                    var Button1 = valueList.GetValue("Button1");
                    if (Button1.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Button1.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.Button1 = (double)Button1.AsDouble;
                        Button_1_Button1.Text = record.Button1.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = Button_1RecordData.AddRecord(record);

                });
            }
        }

        // Functions for Battery


        public class Battery_DataRecord : INotifyPropertyChanged
        {
            public Battery_DataRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _BatteryLevel;
            public double BatteryLevel { get { return _BatteryLevel; } set { if (value == _BatteryLevel) return; _BatteryLevel = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Battery_DataRecord> Battery_DataRecordData { get; } = new DataCollection<Battery_DataRecord>();
        private void OnBattery_Data_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Battery_DataRecordData.Count == 0)
                {
                    Battery_DataRecordData.AddRecord(new Battery_DataRecord());
                }
                Battery_DataRecordData[Battery_DataRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountBattery_Data(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Battery_DataRecordData.MaxLength = value;


        }

        private void OnAlgorithmBattery_Data(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Battery_DataRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyBattery_Data(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,BatteryLevel,Notes\n");
            foreach (var row in Battery_DataRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.BatteryLevel},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadBattery_Data(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadBattery_Data();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Battery_Data");
                    return;
                }

                var record = new Battery_DataRecord();

                var BatteryLevel = valueList.GetValue("BatteryLevel");
                if (BatteryLevel.CurrentType == BCBasic.BCValue.ValueType.IsDouble || BatteryLevel.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.BatteryLevel = (double)BatteryLevel.AsDouble;
                    Battery_Data_BatteryLevel.Text = record.BatteryLevel.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Battery_DataRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyBattery_DataSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int Battery_DataNotifyIndex = 0;
        bool Battery_DataNotifySetup = false;
        private async void OnNotifyBattery_Data(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!Battery_DataNotifySetup)
                {
                    Battery_DataNotifySetup = true;
                    bleDevice.Battery_DataEvent += BleDevice_Battery_DataEvent;
                }
                var notifyType = NotifyBattery_DataSettings[Battery_DataNotifyIndex];
                Battery_DataNotifyIndex = (Battery_DataNotifyIndex + 1) % NotifyBattery_DataSettings.Length;
                var result = await bleDevice.NotifyBattery_DataAsync(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_Battery_DataEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new Battery_DataRecord();

                    var BatteryLevel = valueList.GetValue("BatteryLevel");
                    if (BatteryLevel.CurrentType == BCBasic.BCValue.ValueType.IsDouble || BatteryLevel.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.BatteryLevel = (double)BatteryLevel.AsDouble;
                        Battery_Data_BatteryLevel.Text = record.BatteryLevel.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = Battery_DataRecordData.AddRecord(record);

                });
            }
        }

        // Functions for Accelerometer


        private async void OnWriteAccel_Enable(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Note: This template isn't smart enough to piece together
                // multi-field characteristics. It can support simple characterisitics
                // where there's only one data item.
                string parseError = null;

                Bytes Enable;
                var parsedEnable = Utilities.Parsers.TryParseBytes(Accel_Enable_Enable.Text, System.Globalization.NumberStyles.AllowHexSpecifier, null, out Enable);
                if (!parsedEnable)
                {
                    parseError = "Enable";
                }

                if (parseError == null)
                {
                    await bleDevice.WriteAccel_Enable(Enable);
                }
                else
                { //NOTE: pop up a dialog?
                    SetStatus($"Error: could not parse {parseError}");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Accel_EnableRecord : INotifyPropertyChanged
        {
            public Accel_EnableRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private string _Enable;
            public string Enable { get { return _Enable; } set { if (value == _Enable) return; _Enable = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Accel_EnableRecord> Accel_EnableRecordData { get; } = new DataCollection<Accel_EnableRecord>();
        private void OnAccel_Enable_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Accel_EnableRecordData.Count == 0)
                {
                    Accel_EnableRecordData.AddRecord(new Accel_EnableRecord());
                }
                Accel_EnableRecordData[Accel_EnableRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountAccel_Enable(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Accel_EnableRecordData.MaxLength = value;


        }

        private void OnAlgorithmAccel_Enable(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Accel_EnableRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyAccel_Enable(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Enable,Notes\n");
            foreach (var row in Accel_EnableRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Enable},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadAccel_Enable(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadAccel_Enable();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Accel_Enable");
                    return;
                }

                var record = new Accel_EnableRecord();

                var Enable = valueList.GetValue("Enable");
                if (Enable.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Enable.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Enable = (string)Enable.AsString;
                    Accel_Enable_Enable.Text = record.Enable.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Accel_EnableRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class Accel_RangeRecord : INotifyPropertyChanged
        {
            public Accel_RangeRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _Accel_Range;
            public double Accel_Range { get { return _Accel_Range; } set { if (value == _Accel_Range) return; _Accel_Range = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<Accel_RangeRecord> Accel_RangeRecordData { get; } = new DataCollection<Accel_RangeRecord>();
        private void OnAccel_Range_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (Accel_RangeRecordData.Count == 0)
                {
                    Accel_RangeRecordData.AddRecord(new Accel_RangeRecord());
                }
                Accel_RangeRecordData[Accel_RangeRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountAccel_Range(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Accel_RangeRecordData.MaxLength = value;


        }

        private void OnAlgorithmAccel_Range(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            Accel_RangeRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyAccel_Range(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,Accel_Range,Notes\n");
            foreach (var row in Accel_RangeRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.Accel_Range},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        private async void OnReadAccel_Range(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true); // the false happens in the bluetooth status handler.
            ncommand++;
            try
            {
                var valueList = await bleDevice.ReadAccel_Range();
                if (valueList == null)
                {
                    SetStatus($"Error: unable to read Accel_Range");
                    return;
                }

                var record = new Accel_RangeRecord();

                var Accel_Range = valueList.GetValue("Accel_Range");
                if (Accel_Range.CurrentType == BCBasic.BCValue.ValueType.IsDouble || Accel_Range.CurrentType == BCBasic.BCValue.ValueType.IsString)
                {
                    record.Accel_Range = (double)Accel_Range.AsDouble;
                    Accel_Range_Accel_Range.Text = record.Accel_Range.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                }


                Accel_RangeRecordData.Add(record);

            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        public class XRecord : INotifyPropertyChanged
        {
            public XRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _AccelX;
            public double AccelX { get { return _AccelX; } set { if (value == _AccelX) return; _AccelX = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<XRecord> XRecordData { get; } = new DataCollection<XRecord>();
        private void OnX_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (XRecordData.Count == 0)
                {
                    XRecordData.AddRecord(new XRecord());
                }
                XRecordData[XRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountX(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            XRecordData.MaxLength = value;


        }

        private void OnAlgorithmX(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            XRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyX(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,AccelX,Notes\n");
            foreach (var row in XRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.AccelX},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyXSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int XNotifyIndex = 0;
        bool XNotifySetup = false;
        private async void OnNotifyX(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!XNotifySetup)
                {
                    XNotifySetup = true;
                    bleDevice.XEvent += BleDevice_XEvent;
                }
                var notifyType = NotifyXSettings[XNotifyIndex];
                XNotifyIndex = (XNotifyIndex + 1) % NotifyXSettings.Length;
                var result = await bleDevice.NotifyXAsync(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_XEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new XRecord();

                    var AccelX = valueList.GetValue("AccelX");
                    if (AccelX.CurrentType == BCBasic.BCValue.ValueType.IsDouble || AccelX.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.AccelX = (double)AccelX.AsDouble;
                        X_AccelX.Text = record.AccelX.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = XRecordData.AddRecord(record);

                });
            }
        }
        public class YRecord : INotifyPropertyChanged
        {
            public YRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _AccelY;
            public double AccelY { get { return _AccelY; } set { if (value == _AccelY) return; _AccelY = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<YRecord> YRecordData { get; } = new DataCollection<YRecord>();
        private void OnY_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (YRecordData.Count == 0)
                {
                    YRecordData.AddRecord(new YRecord());
                }
                YRecordData[YRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountY(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            YRecordData.MaxLength = value;


        }

        private void OnAlgorithmY(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            YRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyY(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,AccelY,Notes\n");
            foreach (var row in YRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.AccelY},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyYSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int YNotifyIndex = 0;
        bool YNotifySetup = false;
        private async void OnNotifyY(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!YNotifySetup)
                {
                    YNotifySetup = true;
                    bleDevice.YEvent += BleDevice_YEvent;
                }
                var notifyType = NotifyYSettings[YNotifyIndex];
                YNotifyIndex = (YNotifyIndex + 1) % NotifyYSettings.Length;
                var result = await bleDevice.NotifyYAsync(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_YEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new YRecord();

                    var AccelY = valueList.GetValue("AccelY");
                    if (AccelY.CurrentType == BCBasic.BCValue.ValueType.IsDouble || AccelY.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.AccelY = (double)AccelY.AsDouble;
                        Y_AccelY.Text = record.AccelY.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = YRecordData.AddRecord(record);

                });
            }
        }
        public class ZRecord : INotifyPropertyChanged
        {
            public ZRecord()
            {
                this.EventTime = DateTime.Now;
            }
            // For the INPC INotifyPropertyChanged values
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            private DateTime _EventTime;
            public DateTime EventTime { get { return _EventTime; } set { if (value == _EventTime) return; _EventTime = value; OnPropertyChanged(); } }

            private double _AccelZ;
            public double AccelZ { get { return _AccelZ; } set { if (value == _AccelZ) return; _AccelZ = value; OnPropertyChanged(); } }

            private String _Note;
            public String Note { get { return _Note; } set { if (value == _Note) return; _Note = value; OnPropertyChanged(); } }
        }

        public DataCollection<ZRecord> ZRecordData { get; } = new DataCollection<ZRecord>();
        private void OnZ_NoteKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                var text = (sender as TextBox).Text.Trim();
                (sender as TextBox).Text = "";
                // Add the text to the notes section
                if (ZRecordData.Count == 0)
                {
                    ZRecordData.AddRecord(new ZRecord());
                }
                ZRecordData[ZRecordData.Count - 1].Note = text;
                e.Handled = true;
            }
        }

        // Functions called from the expander
        private void OnKeepCountZ(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            ZRecordData.MaxLength = value;


        }

        private void OnAlgorithmZ(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 1) return;
            int value;
            var ok = Int32.TryParse((e.AddedItems[0] as FrameworkElement).Tag as string, out value);
            if (!ok) return;
            ZRecordData.RemoveAlgorithm = (RemoveRecordAlgorithm)value;
        }
        private void OnCopyZ(object sender, RoutedEventArgs e)
        {
            // Copy the contents over...
            var sb = new System.Text.StringBuilder();
            sb.Append("EventDate,EventTime,AccelZ,Notes\n");
            foreach (var row in ZRecordData)
            {
                var time24 = row.EventTime.ToString("HH:mm:ss.f");
                sb.Append($"{row.EventTime.ToShortDateString()},{time24},{row.AccelZ},{AdvancedCalculator.BCBasic.RunTimeLibrary.RTLCsvRfc4180.Encode(row.Note)}\n");
            }
            var str = sb.ToString();
            var datapackage = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
            datapackage.SetText(str);
            Clipboard.SetContent(datapackage);
        }

        GattClientCharacteristicConfigurationDescriptorValue[] NotifyZSettings = {
            GattClientCharacteristicConfigurationDescriptorValue.Notify,

            GattClientCharacteristicConfigurationDescriptorValue.None,
        };
        int ZNotifyIndex = 0;
        bool ZNotifySetup = false;
        private async void OnNotifyZ(object sender, RoutedEventArgs e)
        {
            SetStatusActive(true);
            ncommand++;
            try
            {
                // Only set up the event callback once.
                if (!ZNotifySetup)
                {
                    ZNotifySetup = true;
                    bleDevice.ZEvent += BleDevice_ZEvent;
                }
                var notifyType = NotifyZSettings[ZNotifyIndex];
                ZNotifyIndex = (ZNotifyIndex + 1) % NotifyZSettings.Length;
                var result = await bleDevice.NotifyZAsync(notifyType);



            }
            catch (Exception ex)
            {
                SetStatus($"Error: exception: {ex.Message}");
            }
        }

        private async void BleDevice_ZEvent(BleEditor.ValueParserResult data)
        {
            if (data.Result == BleEditor.ValueParserResult.ResultValues.Ok)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    var valueList = data.ValueList;

                    var record = new ZRecord();

                    var AccelZ = valueList.GetValue("AccelZ");
                    if (AccelZ.CurrentType == BCBasic.BCValue.ValueType.IsDouble || AccelZ.CurrentType == BCBasic.BCValue.ValueType.IsString)
                    {
                        record.AccelZ = (double)AccelZ.AsDouble;
                        Z_AccelZ.Text = record.AccelZ.ToString(); // "N0"); // either N or F3 based on DEC HEX FIXED. hex needs conversion to int first?
                    }

                    var addResult = ZRecordData.AddRecord(record);

                });
            }
        }

        private async void OnRereadDevice(object sender, RoutedEventArgs e)
        {
            SetStatus("Reading device");
            SetStatusActive(true);
            await bleDevice.EnsureCharacteristicAsync(true);
            SetStatusActive(false);
        }
    }
}