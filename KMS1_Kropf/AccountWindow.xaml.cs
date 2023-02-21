using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;

namespace KMS1_Kropf
{
    /// <summary>
    /// Interaction logic for AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        MainWindow main;
        
        public List<Helper> moneySendList = new List<Helper>();
        public List<Helper> moneyReceiveList = new List<Helper>();

        public string[] moneyTransfer;
        public decimal accountBalance;
        
        public AccountWindow(string strHeader, MainWindow mainWindow, 
                             SortedDictionary<string, string> accountSortedDict, 
                             string accountID, string[] cash)
        {
            InitializeComponent();

            headerLabel.Content = strHeader;
            moneyTransfer = cash;

            string[] data = accountSortedDict[accountID].Split(',');
            for (int i = 0; i < data.Length; i++)
            {
                listViewIBAN.Items.Add(data[i]);
            }

            main = mainWindow;
        }
        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        { 
            main.Show();
        }
        private void listViewIBAN_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            saveBtn.IsEnabled= true;
            AccountingLogicWithBalanceChange();
        }

        /// <summary>
        /// This Method clears AND fills the listView.ItemsSources with Lists, which get sorted by "spending" and "receiving" Money!
        /// It also calculates the balance of each account!
        /// </summary>
        private void AccountingLogicWithBalanceChange()
        {
            moneyReceiveList.Clear();
            moneySendList.Clear();
            listViewSending.ItemsSource = null;
            listViewRecieving.ItemsSource = null;
            accountBalance = 0;

            for (int i = 0; i < moneyTransfer.Length; i++)
            {
                //splitting moneyTransfer into every Row
                string[] rowData = moneyTransfer[i].Split(',');

                //logic for sorting transactions, every Row gets filtered
                if (rowData[1].Equals(listViewIBAN.SelectedItem))
                {
                    moneySendList.Add(new Helper() { accNumber = rowData[0], purpose = rowData[2], amount = rowData[3], date = rowData[4] });
                    accountBalance += Math.Round(Convert.ToDecimal(rowData[3].Replace('.', ',')), 2);
                }
                else if (rowData[0].Equals(listViewIBAN.SelectedItem))
                {
                    moneyReceiveList.Add(new Helper() { accNumber = rowData[1], purpose = rowData[2], amount = rowData[3], date = rowData[4] });
                    accountBalance -= Math.Round(Convert.ToDecimal(rowData[3].Replace('.', ',')), 2);
                }
            }

            //filling ListViews, Kontoeingang AND Kontoausgang
            listViewSending.ItemsSource = moneySendList;
            listViewRecieving.ItemsSource = moneyReceiveList;

            //setting Label Content = Kontostand
            balanceLabel.Content = accountBalance + "€";
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog fbDialog = new FolderBrowserDialog())
            {
                if(fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
                { 
                    string folderPath = fbDialog.SelectedPath;
                    string senderFilePath = Path.Combine(folderPath, "KontoAuszug_Gesendet.csv");
                    string receiverFilePath = Path.Combine(folderPath, "KontoAuszug_Empfangen.csv");

                    SaveMoneyTransactions(senderFilePath, moneySendList);
                    SaveMoneyTransactions(receiverFilePath, moneyReceiveList);

                    MessageBox.Show($"Dateien wurden sicher exportiert: \n{senderFilePath}\n{receiverFilePath}.", "Erfolgreich Expoertiert", (MessageBoxButtons)MessageBoxButton.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Uses filePath and transactionList to save it as an account statement!
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="transactionList"></param>
        private void SaveMoneyTransactions(string filePath, List<Helper> transactionList)
        {
            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                streamWriter.WriteLine($"Kontostand: {accountBalance},-\n\nKontonummer, Verwendungszweck, Betrag, Buchungsdatum");
                foreach(Helper entry in transactionList)
                {
                    streamWriter.WriteLine(string.Join(",", entry.accNumber, entry.purpose, entry.amount, entry.date));
                }
                streamWriter.Close();
            }
        }
    }
}
