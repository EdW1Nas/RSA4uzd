using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

class ServerApplication
{
    static void Main()
    {

        StartListening();
    }

    static void StartListening()
    {
        int port = 8888;
        try
        {
            
           
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

           
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            listenerSocket.Bind(localEndPoint);

          
            listenerSocket.Listen(10);

            Console.WriteLine("Serveris paleistas ant " + port + " port'o");

            while (true)
            {
                Socket clientSocket = listenerSocket.Accept();
                Console.WriteLine("Uzmegztas rysis su " + clientSocket.RemoteEndPoint);

                
               
                HandleClientRequest(clientSocket);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Klaida su port'u " + port + ": " + ex.Message);
        }
    }

    static void HandleClientRequest(Socket clientSocket)
    {
        try
        {
            
            byte[] publicKeyBytes = ReceiveData(clientSocket);
            byte[] messageBytes = ReceiveData(clientSocket);
            byte[] signature = ReceiveData(clientSocket);

           
            string publicKey = Encoding.UTF8.GetString(publicKeyBytes);
            string message = Encoding.UTF8.GetString(messageBytes);
            Console.WriteLine("Viesasis Raktas: " + publicKey); 
            Console.WriteLine("Pranesimas: " + message);
            Console.WriteLine("Parasas: " + Convert.ToBase64String(signature));

         
            Socket thirdProgramSocket = WaitForConnection();


            SendData(thirdProgramSocket, publicKeyBytes);
            SendData(thirdProgramSocket, messageBytes);
            SendData(thirdProgramSocket, signature);

           
            clientSocket.Close();

            thirdProgramSocket.Close();
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Klaida: " + ex.Message);
        }
    }


    static Socket WaitForConnection()
    {
        try
        {
        
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

           
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9999);
            listenerSocket.Bind(localEndPoint);

         
            listenerSocket.Listen(1);

            Console.WriteLine("Laukiama jungties su trecia programa");

          
            Socket clientSocket = listenerSocket.Accept();
            Console.WriteLine("Uzmegstas rysis su trecia programa");

   
            listenerSocket.Close();

            return clientSocket;
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Klaida laukiant jungties: " + ex.Message);
            return null;
        }
    }

    static void SendData(Socket socket, byte[] data)
    {
    
        byte[] lengthBytes = BitConverter.GetBytes(data.Length);
        socket.Send(lengthBytes);


        socket.Send(data);
    }

    static byte[] ReceiveData(Socket socket)
    {

        byte[] lengthBytes = new byte[4];
        socket.Receive(lengthBytes);
        int length = BitConverter.ToInt32(lengthBytes, 0);


        byte[] data = new byte[length];
        int totalBytesReceived = 0;

        while (totalBytesReceived < length)
        {
            int bytesReceived = socket.Receive(data, totalBytesReceived, length - totalBytesReceived, SocketFlags.None);
            totalBytesReceived += bytesReceived;
        }

        return data;
    }
}