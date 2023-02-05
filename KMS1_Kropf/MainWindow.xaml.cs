using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MessageBox = System.Windows.Forms.MessageBox;
using Path = System.IO.Path;

namespace KMS1_Kropf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Customer> customerList = new List<Customer>();
        public SortedDictionary<string, string> accountSortedDict = new SortedDictionary<string, string>();

        public List<string> accountingList = new List<string>();
        public string[] dataTransferArray;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void readBtn_Click(object sender, RoutedEventArgs e)
        {
            using(FolderBrowserDialog fbDialog = new FolderBrowserDialog())
            {
                if(fbDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) 
                { 
                    
                    string folderPath = fbDialog.SelectedPath;

                    if(Directory.Exists(folderPath) && 
                       File.Exists(Path.Combine(folderPath, "Kunden.csv")) &&
                       File.Exists(Path.Combine(folderPath, "Konten.csv")) &&
                       File.Exists(Path.Combine(folderPath, "Buchungen.csv")))
                    {
                        customerList.Clear();

                        //reading Kunden.csv, filtering data and saving in customerList
                        string[] custArray = File.ReadAllLines(Path.Combine(folderPath, "Kunden.csv")).Skip(1).ToArray();
                        for(int i = 0; i < custArray.Length; i++)
                        {
                            string[] rowData = custArray[i].Split(',');
                            customerList.Add(new Customer() { ID = rowData[0], Name= rowData[1] });
                        }
                        
                        //filling listView with data from Kunden.csv
                        myListView.ItemsSource= customerList;

                        //reading Konten.csv, filtering IDs and AccountNumbers 
                        string[] accountsArray = File.ReadAllLines(Path.Combine(folderPath, "Konten.csv")).Skip(1).ToArray();
                        
                        for(int i = 0; i < accountsArray.Length; i++)
                        {
                            string[] rowData = accountsArray[i].Split(',');

                            //if ID exists -> adds IBAN to existing ID
                            if (accountSortedDict.ContainsKey(rowData[0]))
                            {
                                accountSortedDict[rowData[0]] += "," + rowData[1];
                            }
                            //if not! generates a new Entry with Key(ID) and Value(IBAN)
                            else
                            {
                                accountSortedDict.Add(rowData[0], rowData[1]);
                            } 
                        }

                        //reading Buchungen.csv and saving in dataTransferArray for later usage
                        dataTransferArray = File.ReadAllLines(Path.Combine(folderPath, "Buchungen.csv")).Skip(1).ToArray();
                    }
                    else
                    {
                        MessageBox.Show("Eine oder mehrere der benötigten Dateien, konnten im ausgewählten Ordner nicht gefunden werden! " +
                                        "(Kunden.csv, Konten.csv, Buchungen.csv)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void detailBtn_Click(object sender, RoutedEventArgs e)
        {
            Customer selectedItem = (Customer)myListView.SelectedItem;
            AccountWindow aWindow = new AccountWindow("Konto-ID: " + selectedItem.ID + "\n" + "Name: " + selectedItem.Name,
                                                      this, accountSortedDict, selectedItem.ID, dataTransferArray);
            aWindow.Show();
            this.Hide();
        }

        private void myListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            detailBtn.IsEnabled = true;
        }
    }
}
