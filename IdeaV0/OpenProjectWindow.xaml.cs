using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IdeaV0
{
    /// <summary>
    /// Interaction logic for OpenProjectWindow.xaml
    /// </summary>
    public partial class OpenProjectWindow : Window
    {
        private MainWindow parentWindow = ((MainWindow)Application.Current.MainWindow);
        private SQLiteConnection dbConnection;

        public OpenProjectWindow()
        {
            dbConnection = parentWindow.dbConnection;
            InitializeComponent();
            LoadProjectList();
            //Automatically focus the list of projects
            if(!ProjectListView.Items.IsEmpty) { ProjectListView.SelectedItem = ProjectListView.Items[0]; }
            FocusManager.SetFocusedElement(this, ProjectListView);
        }

        private void LoadProjectList()
        {
            //list of projects to be displayed
            List<String> projects = new List<string>();

            dbConnection.Open();

            SQLiteCommand command = dbConnection.CreateCommand();
            command.CommandText = "select title from UserProjects order by title asc";
            SQLiteDataReader reader = command.ExecuteReader();
            
            //first call of read goes to the first row so it's not skipped
            while (reader.Read())
            {
                String title = reader.GetString(0);
                projects.Add(title);
            }

            ProjectListView.ItemsSource = projects;
            reader.Close();
            dbConnection.Close();
        }

        private void CancelButton_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CancelButton_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenButton_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(!(ProjectListView.SelectedItem == null)) { e.CanExecute = true;  }
        }

        private void OpenButton_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            parentWindow.NavigateToProject((String)ProjectListView.SelectedItem);
            this.Close();
        }

    }
}
