using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

class VerificationApplication
{
    static void Main()
    {
      
        string serverIP = "127.0.0.1";
        int serverPort = 9999;

        try
        {
           
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
              
                IPAddress serverIPAddress = IPAddress.Parse(serverIP);
                IPEndPoint serverEndPoint = new IPEndPoint(serverIPAddress, serverPort);

                socket.Connect(serverEndPoint);

                
                byte[] publicKeyBytes = ReceiveData(socket);
                byte[] messageBytes = ReceiveData(socket);
                byte[] signature = ReceiveData(socket);
                
                
              
                string publicKey = Encoding.UTF8.GetString(publicKeyBytes);
                string message = Encoding.UTF8.GetString(messageBytes);
                Console.WriteLine("Viesasis raktas: " + publicKey);
                Console.WriteLine("Pranesimas: " + message);
                Console.WriteLine("Parasas: " + Convert.ToBase64String(signature));

              
                bool isSignatureValid = VerifyDigitalSignature(publicKey, message, signature);

               
                Console.WriteLine("Skaitmeninio paraso tikrinimo rezultatas: " + isSignatureValid);
            }
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Klaida prisijungiant i serveri: " + ex.Message);
        }
    }

    static bool VerifyDigitalSignature(string publicKey, string message, byte[] signature)
    {
        try
        {
            
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(publicKey);

               
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                SHA256Managed sha256 = new SHA256Managed();
                byte[] hash = sha256.ComputeHash(messageBytes);

             
                return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), signature);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Klaida patvirtinant parasa: " + ex.Message);
            return false;
        }
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