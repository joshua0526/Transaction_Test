using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace QBTest
{
    class Program
    {
        static string MyAddress = "AeHmmkX8MZWDyM4LnXiHjN6ghPmxvSYBPZ";
        static string NextAddress = "AGtt1RZ7JV1WTWvKL55RtmzE3F8tmjqkvM";
        static string pubkey = "038c80e2b4b9b76212e6741f28d0bcb5ef719978b16df94d64fea55c644e33d4d8";

        static string NEO = "c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
        static string GAS = "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        static System.Net.WebClient wc = new System.Net.WebClient();
        //static string sendToAddress(string asset, string address) {
        public static string Post(string url, string data, Encoding encoding, int type = 3)
        {
            HttpWebRequest req = null;
            HttpWebResponse rsp = null;
            Stream reqStream = null;
            //Stream resStream = null;

            try
            {
                req = WebRequest.CreateHttp(new Uri(url));
                if (type == 1)
                {
                    req.ContentType = "application/json;charset=utf-8";
                }
                else if (type == 2)
                {
                    req.ContentType = "application/xml;charset=utf-8";
                }
                else
                {
                    req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                }

                req.Method = "POST";
                //req.Accept = "text/xml,text/javascript";
                req.ContinueTimeout = 60000;

                byte[] postData = encoding.GetBytes(data);
                reqStream = req.GetRequestStreamAsync().Result;
                reqStream.Write(postData, 0, postData.Length);
                //reqStream.Dispose();

                rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                string result = GetResponseAsString(rsp, encoding);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                // 释放资源
                if (reqStream != null)
                {
                    reqStream.Close();
                    reqStream = null;
                }
                if (rsp != null)
                {
                    rsp.Close();
                    rsp = null;
                }
                if (req != null)
                {
                    req.Abort();

                    req = null;
                }
            }
        }

        private static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);

                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null)
                    reader.Close();
                if (stream != null)
                    stream.Close();

                reader = null;
                stream = null;

            }
        }

        public static string PostData(string param) {
            JObject jObject = JObject.Parse("{\"jsonrpc\":\"2.0\",\"method\":\"sendrawtransaction\",\"id\":1,\"params\":[\"" + param + "\"]}");
            byte[] sendData = Encoding.GetEncoding("UTF-8").GetBytes(jObject.ToString());
            wc.Headers.Add("Content-Type", "application/json");
            
            byte[] recData = wc.UploadData("http://localhost:20332/", "POST", sendData);
            var info = Encoding.GetEncoding("UTF-8").GetString(recData);
            var json = JObject.Parse(info).ToString();
            return json;
        }

        static void Main(string[] args)
        {                       
            Console.WriteLine("开始线程");
            ThreadPool.QueueUserWorkItem((a) => {
                Program.getJson();
            });
            while (bExit == false)
            {
                var line = Console.ReadLine();
            }
            foreach (KeyValuePair<string, string> kvp in Program.icon)
            {
                Console.WriteLine("资产+地址：{0}, 数量：{1}", kvp.Key, kvp.Value);
            }
            Console.ReadKey();
        }

        static string url = "http://localhost:20332/";

        static Dictionary<string, JObject> txList = new Dictionary<string, JObject>();
        static Dictionary<string, string> icon = new Dictionary<string, string>();

        static string GetBlock(int block)
        {
            var getcounturl = "http://127.0.0.1:20332/?jsonrpc=2.0&id=1&method=getblock&params=[" + block + ",1]";
            var info = wc.DownloadString(getcounturl);
            var json = Newtonsoft.Json.Linq.JObject.Parse(info);
            var bb = json["result"].ToString();
            return bb;
        }

        static bool bExit = false;
        static void getJson()
        {
            for (int i = 0; i < 20000; i++)
            {
                Console.Clear();
                Console.WriteLine("正在执行第{0}个", i);

                string result = GetBlock(i);

                JObject a = JObject.Parse(result);
                if (i == 19999) {
                    Console.Clear();
                    Console.WriteLine(a.ToString());
                    break;
                }               
                foreach (JObject tx in a["tx"])
                {
                    foreach (JObject vin in tx["vin"])
                    {
                        foreach (JObject vout in txList[vin["txid"].ToString()]["vout"])
                        {
                            if (vin["vout"].ToString() == vout["n"].ToString())
                            {
                                if (!icon.ContainsKey(vout["asset"] + "_" + vout["address"]))
                                {
                                    icon.Add(vout["asset"] + "_" + vout["address"], vout["value"].ToString());
                                }
                                else
                                {
                                    string s1 = (double.Parse(icon[vout["asset"] + "_" + vout["address"]].ToString()) - double.Parse(vout["value"].ToString())).ToString();
                                    icon[vout["asset"] + "_" + vout["address"]] = s1;
                                }
                            }
                        }
                    }
                    bool b = true;
                    foreach (JObject vout in tx["vout"])
                    {
                        if (b)
                        {
                            txList.Add(tx["txid"].ToString(), tx);
                            b = false;
                        }
                        if (!icon.ContainsKey(vout["asset"] + "_" + vout["address"]))
                        {
                            icon.Add(vout["asset"] + "_" + vout["address"], vout["value"].ToString());
                        }
                        else
                        {
                            string s = (double.Parse(icon[vout["asset"] + "_" + vout["address"]].ToString()) + double.Parse(vout["value"].ToString())).ToString();
                            icon[vout["asset"] + "_" + vout["address"]] = s;
                        }
                    }
                }
            }
            bExit = true;
        }
    }
}
