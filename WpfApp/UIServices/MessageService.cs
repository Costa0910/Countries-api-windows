using System.Windows;

namespace WpfApp.UIServices
{
    public static class DialogService
    {
        public static void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }
    }
}
