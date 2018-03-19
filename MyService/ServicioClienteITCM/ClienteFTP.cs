using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServicioClienteITCM
{
    public class ClienteFTP
    {
        private byte[] file;
        private string path;
        private string fileName;
        private string host;
        private int port;
        private string userName;
        private string password;
        private Uri uri;

        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                path = value;
            }
        }
        
        public string Host
        {
            get
            {
                return host;
            }

            set
            {
                host = value;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                port = value;
            }
        }

        public string UserName
        {
            get
            {
                return userName;
            }

            set
            {
                userName = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value;
            }
        }

        public string FileName
        {
            get
            {
                return fileName;
            }            
        }

        public byte[] File
        {
            get
            {
                return file;
            }            
        }

        public Uri Uri
        {
            get
            {
                return uri;
            }
        }

        public ClienteFTP(string _path)
        {
            if (_path == null || string.IsNullOrWhiteSpace(_path) || string.IsNullOrEmpty(_path))
            {
                throw new ArgumentNullException("_path", "No se acepta valores nulos o cadenas vacias para _path");
            }

            this.path = _path;
            this.fileName = System.IO.Path.GetFileName(this.path);
            this.host = "10.1.2.57";
            this.port = 21;
            this.userName = "optical";
            this.password = "123456";
            this.uri = this.CreateUri(this.fileName);
        }

        public ClienteFTP(string _path, string _host = null, int _port = -1, string _userName = null, string _password = null)
        {
            if (_path == null || string.IsNullOrWhiteSpace(_path) || string.IsNullOrEmpty(_path))
            {
                throw new ArgumentNullException("_path", "No se acepta valores nulos o cadenas vacias para _path");
            }

            if (_host == null || string.IsNullOrWhiteSpace(_host) || string.IsNullOrEmpty(_host))
            {
                _host = "10.1.2.57";
            }
            if (_port == -1)
            {
                _port = 21;
            }
            if (_userName == null || string.IsNullOrWhiteSpace(_userName) || string.IsNullOrEmpty(_userName))
            {
                _userName = "optical";
            }
            if (_password == null || string.IsNullOrWhiteSpace(_password) || string.IsNullOrEmpty(_password))
            {
                _password = "123456";
            }

            this.path = _path;
            this.fileName = System.IO.Path.GetFileName(this.path);
            this.host = _host;
            this.port = _port;
            this.userName = _userName;
            this.password = _password;
            this.uri = this.CreateUri(this.fileName);
        }

        public ClienteFTP(byte[] _file, string _host = null, int _port = -1, string _userName = null, string _password = null)
        {
            if (_file == null)
            {
                throw new ArgumentNullException("_file", "No se acepta valores nulos para _file");
            }

            this.file = _file;
            //this.fileName = System.IO.Path.GetFileName(this.path);

            if (_host == null || string.IsNullOrWhiteSpace(_host) || string.IsNullOrEmpty(_host))
            {
                _host = "10.1.2.57";
            }
            if (_port == -1)
            {
                _port = 21;
            }
            if (_userName == null || string.IsNullOrWhiteSpace(_userName) || string.IsNullOrEmpty(_userName))
            {
                _userName = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }
            if (_password == null || string.IsNullOrWhiteSpace(_password) || string.IsNullOrEmpty(_password))
            {
                _password = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            this.userName = _userName;
            this.password = _password;
            this.uri = this.CreateUri(this.fileName);
        }

        private Uri CreateUri(string path)
        {
            return (new UriBuilder()
            {
                Scheme = "ftp",
                UserName = this.UserName,
                Password = this.Password,
                Host = this.Host,
                Port = this.Port,
                Path = path                
            }).Uri;
        }

        public bool UploadFile()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(this.Uri);
            request.Credentials = new NetworkCredential(this.UserName, this.Password);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.UseBinary = true;
            request.KeepAlive = true;

            using (Stream fileStream = System.IO.File.OpenRead(string.Format(@"{0}", this.Path)))
            using (Stream ftpStream = request.GetRequestStream())
            {
                byte[] buffer = new byte[10240];
                int read;
                int total = 0;
                while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ftpStream.Write(buffer, 0, read);
                    total += read;
                    Console.WriteLine("Uploaded {0} bytes", total);
                }
            }

            FtpWebResponse uploadResponse = (FtpWebResponse)request.GetResponse();
            string value = uploadResponse.StatusDescription;
            uploadResponse.Close();
            return true;
        }
    }
}
