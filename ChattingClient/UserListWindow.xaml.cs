using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace ChattingClient
{
    /// <summary>
    /// UserListWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserListWindow : Window
    {
        private static ObservableCollection<User> currentUserList = new ObservableCollection<User>();
        private int chattingType = -1;

        private string oneOnOneReceiver { get; set; }
        public string OneOnOneReceiver
        {
            get
            {
                return oneOnOneReceiver;
            }
            private set
            {
                oneOnOneReceiver = value;
            }
        }

        private List<User> groupChattingReceivers { get; set; }
        public List<User> GroupChattingReceivers
        {
            get
            {
                return groupChattingReceivers;
            }
            set
            {
                groupChattingReceivers = value;
            }
        }

        public UserListWindow(int chattingType)
        {
            InitializeComponent();
            UserListView.ItemsSource = currentUserList;

            if (chattingType == StaticDefine.ONE_ON_ONE_CHATTING)
            {
                this.chattingType = chattingType;
                Chatting_btn.Content = "1:1채팅";
                UserListView.SelectionMode = SelectionMode.Single;
            }

            else if (chattingType == StaticDefine.GROUP_CHATTING)
            {
                this.chattingType = chattingType;
                Chatting_btn.Content = "그룹채팅";
                UserListView.SelectionMode = SelectionMode.Extended;
            }
        }

        public static void ChangeUserListView(IEnumerable<User> tempUserList)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                currentUserList.Clear();
                foreach (var item in tempUserList)
                {
                    currentUserList.Add(item);
                }
            }));
        }

        private void Chatting_btn_Click(object sender, RoutedEventArgs e)
        {
            if (UserListView.SelectedItem == null)
            {
                MessageBox.Show("채팅상대를 선택해주세요.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (chattingType == StaticDefine.ONE_ON_ONE_CHATTING)
            {
                List<User> dummyChattingUser = UserListView.SelectedItems.Cast<User>().ToList();

                User selectedUser = (User)UserListView.SelectedItem;
                if (MainWindow.myName == selectedUser.userName)
                {
                    MessageBox.Show("자기 자신과는 채팅할 수 없습니다.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string msg = string.Format("{0}님에게 {1}요청을 하시겠습니까?", selectedUser.userName, Chatting_btn.Content);
                MessageBoxResult messageBoxResult = MessageBox.Show(msg, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    return;
                }
                this.OneOnOneReceiver = selectedUser.userName;

                this.DialogResult = true;

            }
            else if (chattingType == StaticDefine.GROUP_CHATTING)
            {
                List<User> groupChattingUser = UserListView.SelectedItems.Cast<User>().ToList();
                foreach (var item in groupChattingUser)
                {
                    if (item.userName == MainWindow.myName)
                    {
                        MessageBox.Show("자기 자신과는 채팅할 수 없습니다.", "information", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                if (groupChattingUser.Count < 2)
                {
                    MessageBox.Show("그룹채팅의 최소 인원은 2명입니다.", "information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string msg = string.Format("선택유저과 {0}을 하시겠습니까?", Chatting_btn.Content);
                MessageBoxResult messageBoxResult = MessageBox.Show(msg, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    return;
                }

                this.GroupChattingReceivers = groupChattingUser;

                this.DialogResult = true;

            }
        }
    }
}
