using System;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace NZazuFiddle
{
    public class SimpleEditor : TextEditor
    {
        public static readonly DependencyProperty GiveMeTheTextProperty =
            DependencyProperty.Register("GiveMeTheText", typeof(string), typeof(SimpleEditor),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        public string GiveMeTheText
        {
            get { return (string)GetValue(GiveMeTheTextProperty); }
            set { SetValue(GiveMeTheTextProperty, value); }
        }

        public SimpleEditor()
        {
            Encoding = System.Text.Encoding.UTF8;
            LostFocus += UpdateText;
        }

        private void UpdateText(object sender, EventArgs eventArgs)
        {
            var textEditor = sender as TextEditor;
            if (textEditor == null) return;
            if (textEditor.Document != null)
                GiveMeTheText = textEditor.Document.Text;
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var editor = (SimpleEditor)dependencyObject;
            if (editor == null) return;
            if (editor.Document == null) return;

            var caretOffset = editor.CaretOffset;

            editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();

            try { editor.CaretOffset = caretOffset; }
            catch (ArgumentOutOfRangeException) { }
        }

        
    }
}