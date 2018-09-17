using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading;

namespace DownBlock
{
    class Program
    {
        static int blockHeight = 0;
        static bool bExit = false;

        static WebClient wc = new WebClient();
        static void GetBlockHeight() {
            string url = "http://localhost:20332/?jsonrpc=2.0&method=getblockcount&id=1&params=[]";
            string result = wc.DownloadString(url);
            blockHeight = int.Parse(JObject.Parse(result).GetValue("result").ToString());
        }

        static string GetBlock(int i)
        {
            string url = "http://127.0.0.1:20332/?jsonrpc=2.0&method=getblock&id=1&params=[" + i + ",1]";
            string result = wc.DownloadString(url);
            string bb = JObject.Parse(result)["result"].ToString();

            //var json = JObject.Parse(result);
            //var bb = json["result"].ToString();

            return bb;
        }

        //static string GetBlock(int block)
        //{
        //    var getcounturl = "http://127.0.0.1:20332/?jsonrpc=2.0&id=1&method=getblock&params=[" + block + ",1]";
        //    var info = wc.DownloadString(getcounturl);
        //    var json = Newtonsoft.Json.Linq.JObject.Parse(info);
        //    var bb = json["result"].ToString();
        //    return bb;

        //}

        static void DownBlock() {
            
            for (var i = 0; i < blockHeight; i++) {

                var bb = GetBlock(i);
                var path = "dumpdata" + Path.DirectorySeparatorChar + i.ToString("D08") + ".txt";
                File.Delete(path);
                File.WriteAllText(path, bb, System.Text.Encoding.UTF8);
                if (i % 10 == 0)
                {
                    Console.WriteLine("dump =" + i);
                }
            }
            bExit = true;
        }

        static void Main(string[] args)
        {
            GetBlockHeight();
            if (System.IO.Directory.Exists("dumpdata") == false)
            {
                System.IO.Directory.CreateDirectory("dumpdata");
            }
            ThreadPool.QueueUserWorkItem((a) =>
            {              
                DownBlock();
            });
            while (!bExit) {
                var line = Console.ReadLine();
            }
            Console.ReadKey();
        }
    }
}
