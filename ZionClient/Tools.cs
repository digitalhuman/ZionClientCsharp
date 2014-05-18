using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace ZionClient
{
    class Tools
    {
        public static Message FromJson(string input)
        {
            return JsonConvert.DeserializeObject<Message>(input);
        }

        public static string ToJson(Message input)
        {
            return JsonConvert.SerializeObject(input);
        }

        public static string urlToLinks(string message)
        {
            Regex linkParser = new Regex(@"\b(?:http|https://|www|[A-Za-z]{3,9}\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            MatchCollection matched = linkParser.Matches(message);
            foreach (Match m in matched)
            {
                if (m.Value.StartsWith("http://") || m.Value.StartsWith("https://"))
                {
                    message = message.Replace(m.Value, "<a href='" + m.Value + "'>" + m.Value + "</a>");
                }
                else
                {
                    message = message.Replace(m.Value, "<a href='http://" + m.Value + "'>" + m.Value + "</a>");
                }
            }
            return message;
        }

        public static ListViewItem getUserFromListByClientID(string clientID, ListView lst)
        {
            ListViewItem found = null;
            foreach (ListViewItem i in lst.Items)
            {
                foreach (ListViewItem.ListViewSubItem sub in i.SubItems)
                {
                    if (sub.Text == clientID)
                    {
                        found = i;
                        break;
                    }
                }
                if (found != null)
                {
                    break;
                }
            }
            if (found != null)
            {
                return found;
            }
            else
            {
                return new ListViewItem();
            }
        }

        public static byte[] ReadResource(string resourceName)
        {
            try
            {
                using (Stream inFile = Assembly.GetExecutingAssembly().GetManifestResourceStream("ZionClient." + resourceName))
                {
                    BinaryReader r = new BinaryReader(inFile);
                    byte[] buffer = new byte[inFile.Length];
                    int read = r.Read(buffer, 0, buffer.Length);
                    r.Close();
                    return buffer;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return new byte[] { 0 };
            }
        }

        public static string GUID()
        {
            try
            {
                //Guid gid = Guid.NewGuid();
                //byte[] buffer = gid.ToByteArray();                
                //string part1 = getWMI("Win32_Processor", "ProcessorId");
                string part2 = getWMI("Win32_NetworkAdapter", "MACAddress");
                byte[] binbytes = ASCIIEncoding.UTF8.GetBytes(part2.Replace(":",""));
                string UID = BitConverter.ToUInt32(binbytes, 0).ToString();
                return UID;
                //return "1127231536";
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return "";
            }
        }

        private static string getWMI(string table, string field)
        {
            string id = "";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM " + table);
                foreach (ManagementObject share in searcher.Get())
                {
                    foreach (PropertyData data in share.Properties)
                    {
                        if (data.Name == field && Convert.ToString(data.Value) != "")
                        {
                            id += Convert.ToString(data.Value);
                        }
                    }
                    if(id != ""){
                        break;
                    }
                }
                return id;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.ToString());
                return "";
            }
        }
    }
}
