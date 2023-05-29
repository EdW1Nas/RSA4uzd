using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

class SendingApplication
{
    static void Main()
    {
        try
        {

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {

                string publicKey = rsa.ToXmlString(false);

                Console.Write("Iveskite pranesima: ");

                string message = Console.ReadLine();

                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                byte[] signature = rsa.SignData(messageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                SendToSecondApp(publicKey, message, signature);

                Console.WriteLine("Pranesimas, viesasis raktas ir skaitmeninis parasas issiusti");
            }
        }
        catch (CryptographicException ex)
        {
            Console.WriteLine("Klaida generuojant skaitmenini parasa: " + ex.Message);
        }
    }

    static void SendToSecondApp(string publicKey, string message, byte[] signature)
    {

        string serverIP = "127.0.0.1";
        int serverPort = 8888;

        try
        {

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress serverIPAddress = IPAddress.Parse(serverIP);
                IPEndPoint serverEndPoint = new IPEndPoint(serverIPAddress, serverPort);

                socket.Connect(serverEndPoint);

                byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
                SendData(socket, publicKeyBytes);

                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                SendData(socket, messageBytes);

                SendData(socket, signature);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Klaida prisijungiant i serveri: " + ex.Message);
        }
    }

    static void SendData(Socket socket, byte[] data)
    {

        byte[] lengthBytes = BitConverter.GetBytes(data.Length);
        socket.Send(lengthBytes);
        socket.Send(data);
    }

}