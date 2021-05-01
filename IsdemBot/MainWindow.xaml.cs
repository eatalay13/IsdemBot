using IsdemBot.Managers;
using IsdemBot.Models;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace IsdemBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BotManager _botManager;
        private readonly OpenFileDialog _openFileDialog;

        private ThreadStart _botThreadStart;
        private Thread _botThread;

        private string _excelFilePath;

        private readonly ManualResetEvent _manualResetEvent = new(true);

        public MainWindow(BotManager botManager, OpenFileDialog openFileDialog)
        {
            InitializeComponent();
            _botManager = botManager;
            _openFileDialog = openFileDialog;
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if ((string)btnStart.Content != "Başlat") return;

            if (datePicker.SelectedDate == null)
            {
                MessageBox.Show("Lütfen Tarih Seçiniz", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            if (string.IsNullOrWhiteSpace(_excelFilePath))
            {
                MessageBox.Show("Lütfen Excel Dosyasını Seçiniz", "Dikkat", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            _botThreadStart = StartSendData;
            _botThread = new Thread(_botThreadStart);

            _botThread.Start();

            btnStart.Content = "Başladı...";
            btnStart.IsEnabled = false;
            btnExcelFile.IsEnabled = false;
            btnIndexReset.IsEnabled = false;
            btnPause.IsEnabled = true;
        }

        private void StartSendData()
        {
            DateTime selectedDate = DateTime.Now;

            datePicker.Dispatcher.Invoke(() =>
            {
                if (datePicker.SelectedDate != null)
                    selectedDate = datePicker.SelectedDate.Value;
            });

            if (!_botManager.Connect())
                ShowError("Bağlantı Sorunu");

            string strUserName = string.Empty;
            string strPassword = string.Empty;
            string strAdres = string.Empty;

            ExcelColumn excelColumn = ExcelColumn.A;

            Application.Current.Dispatcher.Invoke(() =>
            {
                strUserName = tbxUserName.Text;
                strPassword = tbxPassword.Password;
                strAdres = tbxAdres.Text;
                excelColumn = Enum.Parse<ExcelColumn>(cbxExcelColumn.SelectedValue.ToString());
            });

            Log("Sisteme Giriş Yapılıyor...");

            _botManager.Login(new LoginUser
            {
                UserName = strUserName,
                Password = strPassword
            });

            Log("Excelden Veriler Okunuyor");
            var list = DataManager.GetExcelList(_excelFilePath, excelColumn);

            Log($"Excelden {list.Count} adet Kimlik numarası okundu.");

            int lastIndex = 0;
            Application.Current.Dispatcher.Invoke(() =>
            {
                lastIndex = int.Parse(tbxIndexNo.Text) - 1;
            });

            Log($"{lastIndex + 1}. sıradan başlayarak devam ediyor.");

            for (int index = lastIndex; index < list.Count; index++)
            {
                try
                {
                    _manualResetEvent.WaitOne();
                    _botManager.SendData(list[index], strAdres, selectedDate);
                    Log($"{list[index]} Kimlik numarası başarıyla kayıt edildi.");
                    Log($"İşlenen: {index + 1}/{list.Count} | kalan:{list.Count - (index + 1)} ");

                    consoleScroll.Dispatcher.Invoke(() =>
                    {
                        consoleScroll.ScrollToEnd();
                    });
                }
                catch (Exception e)
                {
                    Log(e.Message);
                    _botManager.PageReload();
                }
                finally
                {
                    DataManager.SetLastIndex(index);
                    lastIndex = index;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        tbxIndexNo.Text = (lastIndex + 1).ToString();
                    });
                }
            }

            if (lastIndex != list.Count - 1) return;

            ShowMessage("Tüm Kimlik Numaraları Başarıyla Kayıt Edildi.");
            Log("İşlem Sona Erdi... 😊😊");

            DataManager.SetLastIndex(0);
        }

        private void ShowError(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        private void ShowMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void Log(string log)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                textBlock.Text += $"\n{log}\n";
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbxUserName.Text = LoginUser.DefaultUser.UserName;
            tbxPassword.Password = LoginUser.DefaultUser.Password;

            datePicker.DisplayDateEnd = DateTime.Now;
            tbxIndexNo.Text = (DataManager.ReadLastIndex() + 1).ToString();

            var enumList = Enum.GetNames<ExcelColumn>();

            foreach (var col in enumList)
            {
                cbxExcelColumn.Items.Add(col);
            }

            cbxExcelColumn.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _botManager.Quit();
            Environment.Exit(0);
        }

        private void label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "https://www.instagram.com/emrhatalay",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if ((string)btnPause.Content == "Duraklat")
            {
                btnPause.Content = "Devam Et";
                _manualResetEvent.Reset();
            }
            else
            {
                btnPause.Content = "Duraklat";
                _manualResetEvent.Set();
            }
        }

        private void label_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void label_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Hand;
        }

        private void btnExcelFile_Click(object sender, RoutedEventArgs e)
        {
            _openFileDialog.Title = "Excel Dosyasını seçiniz.";
            _openFileDialog.Filter = "Office Files|*.xlsx;*.xls";

            if (_openFileDialog.ShowDialog() != true) return;

            _excelFilePath = _openFileDialog.FileName;
            Log($"{_excelFilePath} Excel Dosyası Seçildi.");
        }

        private void btnIndexReset_Click(object sender, RoutedEventArgs e)
        {
            DataManager.SetLastIndex(0);
            tbxIndexNo.Text = "1";
            MessageBox.Show("Index Sıfırlandı", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
