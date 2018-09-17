using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SnapshotAndAirdrop.Helper;

namespace UTXOTest
{
    class Program
    {
        static AutoResetEvent myEvent = new AutoResetEvent(false);
        static string url = "http://localhost:20332/";

        static Dictionary<string, JObject> txList = new Dictionary<string, JObject>();
        static Dictionary<string, string> icon = new Dictionary<string, string>();

        static System.Net.WebClient wc = new System.Net.WebClient();
        static string GetBlock(int block)
        {
            var getcounturl = "http://127.0.0.1:10332/?jsonrpc=2.0&id=1&method=getblock&params=[" + block + ",1]";
            var info = wc.DownloadString(getcounturl);
            var json = Newtonsoft.Json.Linq.JObject.Parse(info);
            var bb = json["result"].ToString();
            return bb;
        }

        static void getJson() {
            int num = 0;
            int num1 = 0;
            for (int i = 0; i < 10000; i++) {
                Console.Clear();
                Console.WriteLine("正在执行第{0}个", i);

                string result = GetBlock(1845633);
                Console.WriteLine(result.ToString());
                
                JObject a = JObject.Parse(result);             

                foreach (JObject tx in a["tx"])
                {
                    if (tx["net_fee"].ToString() == "0") {
                        num++;
                    }
                    if (tx["sys_fee"].ToString() == "0")
                    {
                        num1++;
                    }

                    //foreach (JObject vin in tx["vin"])
                    //{
                    //    foreach (JObject vout in txList[vin["txid"].ToString()]["vout"])
                    //    {
                    //        if (vin["vout"].ToString() == vout["n"].ToString())
                    //        {
                    //            if (!icon.ContainsKey(vout["asset"] + "_" + vout["address"]))
                    //            {
                    //                icon.Add(vout["asset"] + "_" + vout["address"], vout["value"].ToString());
                    //            }
                    //            else
                    //            {
                    //                string s1 = (double.Parse(icon[vout["asset"] + "_" + vout["address"]].ToString()) - double.Parse(vout["value"].ToString())).ToString();
                    //                icon[vout["asset"] + "_" + vout["address"]] = s1;
                    //            }
                    //        }
                    //    }
                    //}
                    //bool b = true;
                    //foreach (JObject vout in tx["vout"])
                    //{
                    //    if (b) {
                    //        txList.Add(tx["txid"].ToString(), tx);
                    //        b = false;
                    //    }
                    //    if (!icon.ContainsKey(vout["asset"] + "_" + vout["address"]))
                    //    {
                    //        icon.Add(vout["asset"] + "_" + vout["address"], vout["value"].ToString());
                    //    }
                    //    else
                    //    {
                    //        string s = (double.Parse(icon[vout["asset"] + "_" + vout["address"]].ToString()) + double.Parse(vout["value"].ToString())).ToString();
                    //        icon[vout["asset"] + "_" + vout["address"]] = s;
                    //    }                           
                    //}
                }
                break;
            }
            bExit = true;
            Console.WriteLine("num = " + num);
            Console.WriteLine("num1 = " + num1);
        }

        static bool bExit = false;
        static void Main(string[] args)
        {
            //var paths = Directory.EnumerateFiles(".", "chain.*.acc", SearchOption.TopDirectoryOnly).Select(p => new
            //{
            //    FileName = Path.GetFileName(p),
            //    Start = uint.Parse(Regex.Match(p, @"\d+").Value),
            //    IsCompressed = p.EndsWith(".zip")
            //}).OrderBy((p) => p.Start);
            //foreach (var p in paths) {
            //    Console.WriteLine(p);
            //}
            //Task.Run(()=> {
            //    Console.WriteLine(122);
            //    Thread.Sleep(2000);
            //    Console.WriteLine(122222);
            //});
            //var path = Directory.EnumerateFiles(@"c:\", "*.txt", SearchOption.AllDirectories);

            Console.WriteLine("开始线程");
            ThreadPool.QueueUserWorkItem((a) =>
            {
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
    }
}
