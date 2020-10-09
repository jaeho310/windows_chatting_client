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

namespace ChattingClient
{
    /// <summary>
    /// Login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        public string userName
        {
            get
            {
                return NameTextBox.Text;
            }
            private set
            {
                NameTextBox.Text = value;
            }
        }

        public string userIp
        {
            get
            {
                return IpTextBox.Text;
            }
            private set
            {
                IpTextBox.Text = value;
            }
        }

        private void Login_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(NameTextBox.Text) || string.IsNullOrEmpty(IpTextBox.Text))
            {
                MessageBox.Show("이름과 ip를 정확히 입력해주세요", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string nameCheck = string.Format("당신은 {0} 님이 맞습니까?", NameTextBox.Text);
            MessageBoxResult nameMessageBoxResult = MessageBox.Show(nameCheck, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (nameMessageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            string ipCheck = string.Format("서버의 ip는 {0} 이 맞습니까?", IpTextBox.Text);
            MessageBoxResult ipmessageBoxResult = MessageBox.Show(ipCheck, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (ipmessageBoxResult == MessageBoxResult.No)
            {
                return;
            }

            this.DialogResult = true;
        }
    }
}
