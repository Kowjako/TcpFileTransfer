using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TcpFileTransfer
{
    public class Program
    {
        [Serializable]
        public class FileDetails
        {
            public string FILETYPE = "";
            public string FILESIZE = "";
        }

        private static FileDetails fileDet;

        private static int port = 8888;
        private static TcpClient receivingTcpClient = new TcpClient();
        private static string ip = "127.0.0.1";
        private static NetworkStream stream;
        private static FileStream fs;
        private static Byte[] receiveBytes = new Byte[255];
        [STAThread]
        static void Main(string[] args)
        {
            receivingTcpClient.Connect(ip, port);
            stream = receivingTcpClient.GetStream();
            GetFileDetails();
            ReceiveFile();
        }
        private static void GetFileDetails()
        {
            try
            {
                StringBuilder resp = new StringBuilder();
                Console.WriteLine("Ожидание информации о файле");
                do
                {
                    int bytes = stream.Read(receiveBytes, 0, receiveBytes.Length);
                    resp.Append(Encoding.UTF8.GetString(receiveBytes, 0, bytes));
                }
                while (stream.DataAvailable);
                Console.WriteLine("Информация о файле получена!");
                XmlSerializer fileSerializer = new XmlSerializer(typeof(FileDetails));
                MemoryStream stream1 = new MemoryStream();
                stream1.Write(receiveBytes, 0, receiveBytes.Length);
                stream1.Position = 0;
                fileDet = (FileDetails)fileSerializer.Deserialize(stream1);
                Console.WriteLine("Получен файл типа " + fileDet.FILETYPE +
                    " имеющий размер " + fileDet.FILESIZE.ToString() + " байт");
            }
            catch (Exception eR)
            {
                Console.WriteLine(eR.ToString());
            }
        }
        public static void ReceiveFile()
        {
            try
            {
                int i = 0;
                fs = new FileStream("temp." + fileDet.FILETYPE, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                Console.WriteLine("-----------*******Ожидайте получение файла*******-----------");
                while(fs.Length!=Convert.ToInt64(fileDet.FILESIZE))
                {
                    i++;
                    int bytes = stream.Read(receiveBytes, 0, receiveBytes.Length);
                    fs.Write(receiveBytes, 0, bytes);
                    Console.WriteLine($"Получена {i} часть файла");
                }
            }
            catch (Exception eR)
            {
                Console.WriteLine(eR.ToString());
            }
            finally
            {
                Console.WriteLine("End of data :(");
                fs.Close();
                receivingTcpClient.Close();
                stream.Close();
                Console.Read();
            }
        }
    }
}
