using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Net;

namespace 팀프로젝트_서버
{
    class Program
    {
        static TcpListener server = new TcpListener(7777);
        static string dataFromClient = null;// 스트링형 변수를 선언.
        static NetworkStream networkStream; // 네트워크 스트림 선언
        static byte[] bytesFrom; // 클라이언트로 부터 온 정보를 저장하는 바이트형 변수 선언. 
        static TcpClient client; // client 선언 
        public static Hashtable clientList = new Hashtable();
        static void Main(string[] args)
        {

            server.Start();//tcplisten을 통해 7777포트를 통해 서버를 만들고 서버를 시작.            
            while (true) // 클라이언트의 접속을 무한으로 대기함으로서 1:n 통신을 하게 만듬.
            {
                Console.WriteLine("서버 대기중");
                client = server.AcceptTcpClient();// 클라이언트의 접속을 대기함. accept되면 tcpclient를 반환함

                bytesFrom = new byte[client.ReceiveBufferSize];//바이트형 변수를 선언 tcp클라이언트로 오는 데이터를 저장함.


                networkStream = client.GetStream();// 스트림을 통해서 데이터를 주고 받는데 클라이언트의 스트림을 받는 네트위크스트림형 변수를 클라이언트의 스트림을 저장.
                networkStream.Read(bytesFrom, 0, client.ReceiveBufferSize);//네트워크스트림의 데이터를 읽음 클라이언트가 보낸 퍼버사이즈만큼 읽어서 바이트형byteFrom에 저장.
                dataFromClient = Encoding.Unicode.GetString(bytesFrom);//아스키코드로 dataFromClient를 변환함.
                dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));//$까지만 읽는거임
                clientList.Add(dataFromClient, client);// 클라이언트의 정보를 리스트에 저장 
                Console.WriteLine(dataFromClient + "입장");// $까지 읽은 정보를 출력함.

                handleClient clientinfo = new handleClient();
                clientinfo.startClient(client, dataFromClient, clientList);

            }
        }
       //static void sendinfo()
       // {
       //     networkStream.Write(bytesFrom, 0, client.ReceiveBufferSize);
       //     networkStream.Flush();
       // }

        private static void broadcast(string msg, string uName)
        {
            TcpClient broadcastSocket; // 각각
            NetworkStream broadcastStream;
            byte[] broadcastBytes = null;
            //string broadcastSting = null;
            foreach (DictionaryEntry item in clientList)
            {
                broadcastSocket = (TcpClient)item.Value;
                broadcastStream = broadcastSocket.GetStream();
                broadcastBytes = Encoding.Unicode.GetBytes((uName + ":" + msg + "$"));
                //dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                broadcastStream.Write(broadcastBytes, 0, broadcastBytes.Length);
                broadcastStream.Flush();
             }
            
        }

        private class handleClient
        {
            TcpClient client;
            string clNo;
            Hashtable clientList;

            public void startClient(TcpClient clientSocket, string dataFromClient, Hashtable clientList)
            {
                this.client = clientSocket;
                this.clNo = dataFromClient;
                this.clientList = clientList;

                Thread clTherad = new Thread(doChat);
                clTherad.Start();
            }

            private void doChat()
            {

                byte[] byteFrom = new byte[client.ReceiveBufferSize];
                NetworkStream networkStream;
                string dataFromClient = null;

                while (true)
                {
                    try
                    {
                        networkStream = client.GetStream();
                        networkStream.Read(byteFrom, 0, client.ReceiveBufferSize);
                        dataFromClient = Encoding.Unicode.GetString(byteFrom);
                        dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));

                        broadcast(dataFromClient, clNo);

                        Console.WriteLine("From client -" + clNo + ":" + dataFromClient);


                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
        }
    }
}
