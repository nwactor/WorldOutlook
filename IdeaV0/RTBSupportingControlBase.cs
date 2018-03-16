using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IdeaV0
{
    public class RTBSupportingControlBase : UserControl
    {
        //Switching text property boolean
        internal Boolean fontPropertySwitching = false;
        internal BetterRichTextBox rtb;
        internal ToggleButton BoldButton;
        internal ToggleButton ItalicButton;
        internal ToggleButton UnderlineButton;
        internal ComboBox FontTypeBox;
        internal ComboBox FontSizeBox;

        public RTBSupportingControlBase()
        { this.PreviewKeyDown += HandleTab; }

        //Connects the properties of this class to the controls defined in the XAML of the inheritor
        internal void ConnectUIElements(ToggleButton BoldButton, ToggleButton ItalicButton,
            ToggleButton UnderlineButton, ComboBox FontTypeBox,
            ComboBox FontSizeBox, BetterRichTextBox rtb)
        {
            this.rtb = rtb;
            this.rtb.parentControl = this;
            rtb.DataContext = this; //allows the BetterRichTextBox to use the information from the UserControl
            this.BoldButton = BoldButton;
            this.ItalicButton = ItalicButton;
            this.UnderlineButton = UnderlineButton;
            this.FontTypeBox = FontTypeBox;
            this.FontSizeBox = FontSizeBox;
            FontTypeBox.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
            FontTypeBox.SelectedItem = FontTypeBox;
            FontSizeBox.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
            this.rtb.Selection.Select(rtb.Document.ContentStart, rtb.Document.ContentStart);

        }

        private void HandleTab(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                if(rtb.IsFocused)
                {
                    rtb.HandleTab();
                }
                e.Handled = true;
            }
        }


        /** Input handling **/

        public void BoldToggle_Clicked(Object sender, RoutedEventArgs e)
        {
            ToggleBold();
        }
        
        //called from keyboard shortcut ctrl+b
        public void ToggleBold_Keyboard(Object sender, ExecutedRoutedEventArgs e)
        {
            BoldButton.IsChecked = !BoldButton.IsChecked;
            ToggleBold();
        }

        public void ItalicToggle_Clicked(Object sender, RoutedEventArgs e)
        {
            ToggleItalics();
        }
        
        //called from keyboard shortcut ctrl+i
        public void ToggleItalics_Keyboard(Object sender, ExecutedRoutedEventArgs e)
        {
            ItalicButton.IsChecked = !ItalicButton.IsChecked;
            ToggleItalics();
        }

        public void UnderlineToggle_Clicked(Object sender, RoutedEventArgs e)
        {
            ToggleUnderline();
        }

        //called from keyboard shortcut ctrl+u
        public void ToggleUnderline_Keyboard(Object sender, ExecutedRoutedEventArgs e)
        {
            UnderlineButton.IsChecked = !UnderlineButton.IsChecked;
            ToggleUnderline();
        }


        /** Text Editing Functionality **/

        //applies new font to selected text in the RichTextBox when the value in the font box changes
        public void FontTypeChanged(Object sender, SelectionChangedEventArgs e)
        {
            if (FontTypeBox.SelectedItem != null)
            {
                if (!rtb.Selection.Text.Equals(""))
                {
                    rtb.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, FontTypeBox.SelectedItem);
                }
                else
                {
                    fontPropertySwitching = true;
                }
            }
            rtb.Focus();
        }

        //applies new size to selected text in the RichTextBox when the value in the size box changes
        public void FontSizeChanged(Object sender, TextChangedEventArgs e)
        {
            if (!rtb.Selection.Text.Equals("") && !FontSizeBox.Text.ToString().Equals(""))
            {
                rtb.Selection.ApplyPropertyValue(Inline.FontSizeProperty, FontSizeBox.Text);
            }
            else
            {
               fontPropertySwitching = true;
            }
            rtb.Focus();
        }
        
        //bolds/unbolds the selected text in TopicRichTextBox
        public void ToggleBold()
        {
            if (!rtb.Selection.Text.Equals("")) //if text is highlighted
            {
                if ((bool)BoldButton.IsChecked)
                {
                    rtb.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
                }
                else
                {
                    rtb.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Normal);
                }
            }
            else
            {
                fontPropertySwitching = true;
            }
        }
        
        //italicizes/unitalicizes the selected text in TopicRichTextBox
        public void ToggleItalics()
        {
            if (!rtb.Selection.Text.Equals("")) //if text is highlighted
            {
                if ((bool)ItalicButton.IsChecked)
                {
                    rtb.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Italic);
                }
                else
                {
                    rtb.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Normal);
                }
            }
            else
            {
                fontPropertySwitching = true;
            }
        }

        //underlines/un-underlines the selected text in TopicRichTextBox
        public void ToggleUnderline()
        {
            if (!rtb.Selection.Text.Equals("")) //if text is highlighted
            {
                if ((bool)UnderlineButton.IsChecked)
                {
                    rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                }
                else
                {
                    rtb.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, new TextDecorationCollection());
                }
            }
            else
            {
                fontPropertySwitching = true;
            }
        }

        //updates the state of the text editing controls whenever new text is selected
        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Object temp;
            
            //update bold button
            temp = rtb.Selection.GetPropertyValue(Inline.FontWeightProperty);
            BoldButton.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontWeights.Bold));
            //update italic button
            temp = rtb.Selection.GetPropertyValue(Inline.FontStyleProperty);
            ItalicButton.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(FontStyles.Italic));
            //update underline button
            temp = rtb.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            UnderlineButton.IsChecked = (temp != DependencyProperty.UnsetValue) && (temp.Equals(TextDecorations.Underline));

            //update font box
            temp = rtb.Selection.GetPropertyValue(Inline.FontFamilyProperty);
            FontTypeBox.SelectedItem = temp;

            //update size box
            temp = rtb.Selection.GetPropertyValue(Inline.FontSizeProperty);
            double size;
            if (double.TryParse(temp.ToString(), out size))
            {
                FontSizeBox.Text = temp.ToString();
            }
            else
            {
                FontSizeBox.Text = "";
            }
        }
    }

    //Makes RichTextBox behave more like a MS Word document
    public class BetterRichTextBox : RichTextBox
    {
        public RTBSupportingControlBase parentControl { get; set; }

        public BetterRichTextBox() : base()
        {
            this.PreviewKeyDown += HandleSpace;
        }

        private void HandleSpace(Object sender, KeyEventArgs e)
        {
            if (parentControl != null && parentControl.fontPropertySwitching)
            {
                if (e.Key == Key.Space)
                {
                    UpdateTextFormatting(" ");
                }
            }
        }

        public void HandleTab()
        {
            UpdateTextFormatting("\t");
        }

        //Override instantly changes the properties of the first character inputted when
        //switching booleans are set to true in the parent control
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (parentControl != null && parentControl.fontPropertySwitching)
            {
                UpdateTextFormatting(e.Text);
            }
            else
            {
                base.OnTextInput(e);
            }
        }

        private void UpdateTextFormatting(String text)
        {
            //get the inputted text and its position
            TextPointer pointer = this.CaretPosition.GetInsertionPosition(LogicalDirection.Forward);
            Run run = new Run(text, pointer);
            //set text appearance to what is specified by the user
            run.FontFamily = (FontFamily)parentControl.FontTypeBox.SelectedItem;
            //check for null because text pasted from outside might be a weird size with a repeating decimal that gets converted to null
            //error only happens when changing from lowercase to uppercase...
            //temporary solution, probably want to override the pasting event to check for dumb values like this
            if (parentControl.FontSizeBox.SelectedItem != null) { run.FontSize = (double)parentControl.FontSizeBox.SelectedItem; }
            Boolean isBold = (bool)parentControl.BoldButton.IsChecked;
            Boolean isItalic = (bool)parentControl.ItalicButton.IsChecked;
            Boolean isUnderline = (bool)parentControl.UnderlineButton.IsChecked;
            if (isBold) { run.FontWeight = FontWeights.Bold; }
            else { run.FontWeight = FontWeights.Normal; }
            if (isItalic) { run.FontStyle = FontStyles.Italic; }
            else { run.FontStyle = FontStyles.Normal; }
            if (isUnderline) { run.TextDecorations = TextDecorations.Underline; }
            else { run.TextDecorations.Remove(TextDecorations.Underline[0]); }
            //move caret forward
            this.CaretPosition = run.ElementEnd;
            parentControl.fontPropertySwitching = false;
            //workaround for fixing the case where font properties switch away
            //from the new the proprties back to old properties after deletion 
            this.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, run.FontFamily);
            this.Selection.ApplyPropertyValue(Inline.FontSizeProperty, run.FontSize);
            this.Selection.ApplyPropertyValue(Inline.FontWeightProperty, run.FontWeight);
            this.Selection.ApplyPropertyValue(Inline.FontStyleProperty, run.FontStyle);
            this.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, run.TextDecorations);
        }
    }
}
