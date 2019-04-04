using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ClickNClaim.OpenFireConnector
{
    public class Connector
    {
        public string Domain { get; set; }
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }

        //http://example.org:9090/plugins/restapi/v1/users
        public string CreateUserUrl
        {
            get
            {
                return  "plugins/restapi/v1/users";
            }
        }

        public string DeleteUserUrl
        {
            get
            {
                return   "plugins/restapi/v1/users/{0}";
            }
        }
        public string CreateChatRoomUrl
        {
            get
            {
                return "plugins/restapi/v1/chatrooms";
            }
        }

        public string RoomLink
        {
            get
            {
                return this.Domain.Replace("9090", "7443") + "ofmeet/?r=";
            }
        }


        public Connector()
        {

        }

        public Connector(string domain, string adminUsername, string adminPassword)
        {
            this.Domain = domain;
            this.AdminUsername = adminUsername;
            this.AdminPassword = adminPassword;
        }

        public user CreateUser(user u)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(CreateUserUrl);
                request.Method = "POST";
                byte[] bytes;
                var serializer = new XmlSerializer(typeof(user));
                MemoryStream ms = new MemoryStream();
                serializer.Serialize(ms, u);
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(ms);
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(this.AdminUsername + ":" + this.AdminPassword));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                bytes = System.Text.Encoding.ASCII.GetBytes(sr.ReadToEnd());
                request.ContentLength = bytes.Length;
                request.Method = "POST";
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return u;
                }
                return null;
            }
            catch(Exception e)
            {
                return null;
            }
        }

        public user UserExists(string username)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(CreateUserUrl + "/" + username);
                request.Method = "GET";
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(this.AdminUsername + ":" + this.AdminPassword));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                HttpWebResponse response;
                var serializer = new XmlSerializer(typeof(user));
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    //string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return (user)serializer.Deserialize(responseStream);
                }
                return null;
            }
            catch(Exception e)
            {
                return null;
            }
        }


        public string CreateChatroom(chatRoom c)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(CreateChatRoomUrl);
                request.Method = "POST";
                byte[] bytes;
                var serializer = new XmlSerializer(typeof(chatRoom));
                MemoryStream ms = new MemoryStream();
                serializer.Serialize(ms, c);
                ms.Seek(0, SeekOrigin.Begin);
                StreamReader sr = new StreamReader(ms);
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(this.AdminUsername + ":" + this.AdminPassword));
                request.Headers.Add("Authorization", "Basic " + encoded);
                request.ContentType = "application/xml";
                request.Accept = "application/xml";
                bytes = System.Text.Encoding.ASCII.GetBytes(sr.ReadToEnd());
                request.ContentLength = bytes.Length;
                request.Method = "POST";
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return  this.Domain.Replace("9090", "7443") + "ofmeet/?r=" + c.naturalName;
                }
                return null;
            }
            catch(Exception e)
            {
                return null;
            }
        }
    }
}
