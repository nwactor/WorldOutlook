using System;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace IdeaV0
{
    class HelpFunctions
    {

        public static Boolean StringIsBlank(String s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (!(s[i] == ' ')) { return false; }
            }
            return true;
        }


        /* Visual Functions */

        public static TextBlock CreateTagBlock(String tagName)
        {
            TextBlock tag = new TextBlock();
            tag.Text = tagName;
            tag.Margin = new Thickness(5, 0, 0, 0);
            return tag;
        }

        public static ToggleButton CreateTagToggle(String tagName)
        {
            ToggleButton tag = new ToggleButton();
            tag.Content = tagName;
            tag.Margin = new Thickness(5, 0, 0, 0);
            return tag;
        }

        public static TextBlock CreateSideBarLink(String linkName)
        {
            TextBlock link = new TextBlock();
            link.Text = linkName;
            link.Margin = new Thickness(0, 3, 0, 0);
            link.FontSize = 15;
            link.TextWrapping = TextWrapping.Wrap;
            return link;
        }

        /* Image encoding/decoding */

        public static byte[] ImageToBytes(BitmapImage image)
        {
            if(image == null) { return null; }

            //Chose Jpeg b/c no need to preserve transparency
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

        public static BitmapImage LoadImageFromBytes(byte[] bytes)
        {
            BitmapImage image = new BitmapImage();
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; //saves the image even after the stream is closed
                image.StreamSource = ms;
                image.EndInit();
            }
            return image;
        }
    }
}
