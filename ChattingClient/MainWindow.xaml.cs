using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChattingClient
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string myName = null;
        TcpClient client = null;
        Thread ReceiveThread = null;
        ChattingWindow chattingWindow = null;
        Dictionary<string, ChattingThreadData> chattingThreadDic = new Dictionary<string, ChattingThreadData>();
        Dictionary<int, ChattingThreadData> groupChattingThreadDic = new Dictionary<int, ChattingThreadData>();

        public MainWindow()
        {
            InitializeComponent();
            //if (System.Diagnostics.Process.GetProcessesByName("ChattingServiceClient").Length > 1)
            //{
            //    MessageBox.Show("채팅프로그램이 실행중입니다.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            //    Environment.Exit(1);
            //}
        }

        private void RecieveMessage()
        {
            string receiveMessage = "";
            List<string> receiveMessageList = new List<string>();
            while (true)
            {
                try
                {
                    byte[] receiveByte = new byte[1024];
                    client.GetStream().Read(receiveByte, 0, receiveByte.Length);

                    receiveMessage = Encoding.Default.GetString(receiveByte);

                    string[] receiveMessageArray = receiveMessage.Split('>');
                    foreach (var item in receiveMessageArray)
                    {
                        if (!item.Contains('<'))
                            continue;
                        if (item.Contains("관리자<TEST"))
                            continue;

                        receiveMessageList.Add(item);
                    }

                    ParsingReceiveMessage(receiveMessageList);
                }
                catch (Exception e)
                {
                    MessageBox.Show("서버와의 연결이 끊어졌습니다.", "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageBox.Show(e.Message);
                    MessageBox.Show(e.StackTrace);
                    Environment.Exit(1);
                }
                Thread.Sleep(500);
            }
        }

        private void ParsingReceiveMessage(List<string> messageList)
        {
            foreach (var item in messageList)
            {
                string chattingPartner = "";
                string message = "";

                if (item.Contains('<'))
                {
                    string[] splitedMsg = item.Split('<');

                    chattingPartner = splitedMsg[0];
                    message = splitedMsg[1];

                    // 하트비트 
                    if (chattingPartner == "관리자")
                    {
                        ObservableCollection<User> tempUserList = new ObservableCollection<User>();
                        string[] splitedUser = message.Split('$');
                        foreach (var el in splitedUser)
                        {
                            if (string.IsNullOrEmpty(el))
                                continue;
                            tempUserList.Add(new User(el));
                        }
                        UserListWindow.ChangeUserListView(tempUserList);
                        messageList.Clear();
                        return;
                    }

                    // 그룹채팅
                    else if (chattingPartner.Contains("#"))
                    {
                        string[] splitedChattingPartner = chattingPartner.Split('#');
                        List<string> chattingPartners = new List<string>();

                        foreach (var el in splitedChattingPartner)
                        {
                            if (string.IsNullOrEmpty(el))
                                continue;
                            chattingPartners.Add(el);
                        }

                        string sender = chattingPartners[0];

                        int chattingRoomNum = GetChattingRoomNum(chattingPartners);

                        if (chattingRoomNum < 0)
                        {
                            Thread groupChattingThread = new Thread(() => ThreadStartingPoint(chattingPartners));
                            groupChattingThread.SetApartmentState(ApartmentState.STA);
                            groupChattingThread.IsBackground = true;
                            groupChattingThread.Start();
                        }
                        else
                        {
                            if (groupChattingThreadDic[chattingRoomNum].chattingThread.IsAlive)
                            {
                                groupChattingThreadDic[chattingRoomNum].chattingWindow.ReceiveMessage(sender, message);
                            }
                        }
                        messageList.Clear();
                        return;
                    }

                    // 1:1채팅
                    else
                    {
                        if (!chattingThreadDic.ContainsKey(chattingPartner))
                        {
                            if(message == "ChattingStart")
                            {
                                Thread chattingThread = new Thread(() => ThreadStartingPoint(chattingPartner));
                                chattingThread.SetApartmentState(ApartmentState.STA);
                                chattingThread.IsBackground = true;
                                chattingThread.Start();
                            }
                        }
                        else
                        {
                            if (chattingThreadDic[chattingPartner].chattingThread.IsAlive)
                            {
                                chattingThreadDic[chattingPartner].chattingWindow.ReceiveMessage(chattingPartner, message);
                            }
                        }
                        messageList.Clear();
                        return;
                    }
                }
            }
            messageList.Clear();
        }

        private int GetChattingRoomNum(List<string> chattingPartners)
        {
            chattingPartners.Sort();
            string reqMember = "";
            foreach (var item in chattingPartners)
            {
                reqMember += item;
            }

            string originMember = "";
            foreach (var item in groupChattingThreadDic)
            {
                foreach (var el in item.Value.chattingWindow.chattingPartners)
                {
                    originMember += el;
                }
                if (originMember == reqMember)
                    return item.Value.chattingRoomNum;
                originMember = "";
            }
            return -1;
        }

        private void ThreadStartingPoint(string chattingPartner)
        {
            chattingWindow = new ChattingWindow(client, chattingPartner);
            chattingThreadDic.Add(chattingPartner, new ChattingThreadData(Thread.CurrentThread, chattingWindow));

            if (chattingWindow.ShowDialog() == true)
            {
                MessageBox.Show("채팅이 종료되었습니다.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                chattingThreadDic.Remove(chattingPartner);
            }
        }

        private void ThreadStartingPoint(List<string> chattingPartners)
        {
            chattingPartners.Sort();
            chattingWindow = new ChattingWindow(client, chattingPartners);
            ChattingThreadData tempThreadData = new ChattingThreadData(Thread.CurrentThread, chattingWindow);
            groupChattingThreadDic.Add(tempThreadData.chattingRoomNum, tempThreadData);

            if (chattingWindow.ShowDialog() == true)
            {
                MessageBox.Show("채팅이 종료되었습니다.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                groupChattingThreadDic.Remove(tempThreadData.chattingRoomNum);
            }
        }




        private void Login_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (client != null)
            {
                MessageBox.Show("이미 로그인되었습니다.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Login login = new Login();
            if (login.ShowDialog() == true)
            {
                try
                {
                    string ip = login.IpTextBox.Text;
                    string parsedName = "%^&";
                    parsedName += login.NameTextBox.Text;

                    client = new TcpClient();
                    client.Connect(ip, 9999);

                    byte[] byteData = new byte[parsedName.Length];
                    byteData = Encoding.Default.GetBytes(parsedName);
                    client.GetStream().Write(byteData, 0, byteData.Length);

                    Info.Text = string.Format("{0} 님 반갑습니다 ", login.NameTextBox.Text);
                    myName = login.NameTextBox.Text;

                    ReceiveThread = new Thread(RecieveMessage);
                    ReceiveThread.Start();
                }

                catch
                {
                    MessageBox.Show("서버연결에 실패하였습니다.", "Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    client = null;
                }
            }
        }

        private void OneOnOneChatting_Btn_Click(object sender, RoutedEventArgs e)
        {

            if (client == null)
            {
                MessageBox.Show("먼저 로그인해주세요.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }


            string getUserProtocol = myName + "<GiveMeUserList>";
            byte[] byteData = new byte[getUserProtocol.Length];
            byteData = Encoding.Default.GetBytes(getUserProtocol);

            client.GetStream().Write(byteData, 0, byteData.Length);

            UserListWindow userListWindow = new UserListWindow(StaticDefine.ONE_ON_ONE_CHATTING);

            if (userListWindow.ShowDialog() == true)
            {
                if (chattingThreadDic.ContainsKey(userListWindow.OneOnOneReceiver))
                {
                    MessageBox.Show("해당유저와는 이미 채팅중입니다.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                string chattingStartMessage = string.Format("{0}<ChattingStart>", userListWindow.OneOnOneReceiver);
                byte[] chattingStartByte = Encoding.Default.GetBytes(chattingStartMessage);

                client.GetStream().Write(chattingStartByte, 0, chattingStartByte.Length);
            }
        }

        private void GroupChatting_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (client == null)
            {
                MessageBox.Show("먼저 로그인해주세요.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string getUserProtocol = myName + "<GiveMeUserList>";
            byte[] byteData = new byte[getUserProtocol.Length];
            byteData = Encoding.Default.GetBytes(getUserProtocol);

            client.GetStream().Write(byteData, 0, byteData.Length);

            UserListWindow userListWindow = new UserListWindow(StaticDefine.GROUP_CHATTING);

            if (userListWindow.ShowDialog() == true)
            {
                string groupChattingUserStrData = MainWindow.myName;
                foreach (var item in userListWindow.GroupChattingReceivers)
                {
                    groupChattingUserStrData += "#";
                    groupChattingUserStrData += item.userName;
                }


                string chattingStartMessage = string.Format("{0}<GroupChattingStart>", groupChattingUserStrData);
                byte[] chattingStartByte = Encoding.Default.GetBytes(chattingStartMessage);

                client.GetStream().Write(chattingStartByte, 0, chattingStartByte.Length);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("채팅프로그램을 종료하시겠습니까?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }

            Environment.Exit(1);
        }
    }
}
