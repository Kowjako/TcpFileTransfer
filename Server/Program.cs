using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server
{
    public class Program
    {
        [Serializable]
        public class FileDetails
        {
            public string FILETYPE = "";
            public string FILESIZE = "";
        }
        private static FileDetails fileDet = new FileDetails();
        private const int remotePort = 8888;
        private static TcpClient client;
        private static TcpListener listener;
        private static NetworkStream NetStream;
        private static FileStream fs;
        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Any, remotePort);
                listener.Start();
                Console.WriteLine("Введите путь к файлу и его имя");
                fs = new FileStream(@Console.ReadLine().ToString(), FileMode.Open, FileAccess.Read);
                while (true)
                {
                    Console.WriteLine("waiting for connections");
                    client = listener.AcceptTcpClient();
                    NetStream = client.GetStream();
                    SendFileInfo();
                    Thread.Sleep(2000);
                    SendFile();
                    Console.ReadLine();
                }
            }
            catch (Exception eR)
            {
                Console.WriteLine($"Main exception {eR.Message}");
                Console.ReadKey();
            }
        }
        public static void SendFileInfo()
        {
            try
            {
                fileDet.FILETYPE = Path.GetExtension(fs.Name);
                fileDet.FILESIZE = Convert.ToString(fs.Length);
                XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
                MemoryStream stream = new MemoryStream();
                fileSerializer.Serialize(stream, fileDet);
                stream.Position = 0;
                Byte[] bytes = new Byte[stream.Length];
                stream.Read(bytes, 0, Convert.ToInt32(stream.Length));
                Console.WriteLine("Отправка деталей файла...");
                NetStream.Write(bytes, 0, bytes.Length);
                stream.Close();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                Console.ReadKey();
            }
        }
        private static void SendFile()
        {
            try
            {
                byte[] bytes = new byte[255];
                int size = 0;
                int i = 0;
                Console.WriteLine("Отправка файла размером " + fs.Length + " байт");
                while ((size = fs.Read(bytes, 0, bytes.Length)) > 0)
                {
                    Console.WriteLine($"{i} size = {size}");
                    i++;
                    NetStream.Write(bytes, 0, size);
                    Console.WriteLine($"Send {i} Part of file");
                    Console.WriteLine($"{i} size = {size}");
                }
            }
            catch
            {
                Console.Write("Send file error");
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine("Файл успешно отправлен.");
                Console.Read();
                fs.Close();
                NetStream.Close();
                client.Close();
            }
        }
    }
}
