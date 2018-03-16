using System;
using System.Collections.Generic;
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
    /// Interaction logic for TTAssociationWindow.xaml
    /// </summary>
    public partial class TTAssociationWindow : Window
    {
        private Boolean adding;
        private String tag;
        private Project project;

        public TTAssociationWindow(String tag, Project project, Boolean addOrRemove)
        {
            this.tag = tag;
            this.project = project;
            this.adding = addOrRemove;
            InitializeComponent();
            if(adding)
            {
                this.Title = "Add Topics to " + tag;
                this.InstructionBlock.Text = "Add to " + tag + ":";
                foreach(String topic in project.GetTopicList())
                {
                    if (!project.GetAssociatedTopics(tag).Contains(topic))
                    {
                        TopicsListView.Items.Add(topic);
                    }
                }
                ConfirmButton.Content = "Add";
            }
            else //removing
            {
                this.Title = "Remove Topics from " + tag;
                this.InstructionBlock.Text = "Remove from " + tag + ":";
                foreach (String topic in project.GetTopicList())
                {
                    if (project.GetAssociatedTopics(tag).Contains(topic))
                    {
                        TopicsListView.Items.Add(topic);
                    }
                }
                ConfirmButton.Content = "Remove";
            }
        }

        private void ConfirmButton_Clicked(Object sender, RoutedEventArgs e)
        {
            List<String> tagAsList = new List<string>();
            tagAsList.Add(this.tag);
            if (adding)
            {
                foreach (String topic in TopicsListView.SelectedItems)
                {
                    this.project.UpdateTopicTags(topic, tagAsList, null);
                }
            }
            else
            {
                foreach (String topic in TopicsListView.SelectedItems)
                {
                    this.project.UpdateTopicTags(topic, null, tagAsList);
                }
            }
            this.Close();
        }

        private void CancelButton_Clicked(Object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
