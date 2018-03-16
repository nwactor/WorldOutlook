using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;

namespace IdeaV0
{
    //class to represent a topic in memory
    public class Topic
    {
        public String title { get; set; }
        internal String encoded_text { get; set; }
        internal BitmapImage main_image { get; set; }
        private List<BitmapImage> gallery { get; set; }
        //private List<Tag> tags { get; set; } = new List<Tag>();
        public String parent_project { get; set; }

        //empty constructor called when creating an unsaved topic
        public Topic() { }

        public Topic(String title, String encoded_text, BitmapImage topic_image, String tags)
        {
            this.title = title;
            this.encoded_text = encoded_text;
            this.main_image = topic_image;
        }

        public String GetTagsAsString()
        {
            List<String> tagsList = ((MainWindow)Application.Current.MainWindow).currentPage.project.GetAssociatedTags(this.title);
            String tagsString = "";
            for (int i = 0; i < tagsList.Count; i++)
            {
                tagsString += tagsList[i];
                if (!(i == tagsList.Count - 1))
                {
                    tagsString += ',';
                }
            }
            return tagsString;
        }
    }
}
