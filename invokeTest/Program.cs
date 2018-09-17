using System;
using System.Linq;
using System.Net;
using System.Text;

namespace invokeTest
{
    class Program
    {
        //合约地址
        static string nnc = "0x906d9a950bfb1b457507cea4bcb326f8b56b3eda";
        ThinNeo.Hash160 nnc_shash = new ThinNeo.Hash160(nnc);

        string name = "name";
        string totalSupply = "totalSupply";
        string symbol = "symbol";
        string decimals = "decimals";
        string balanceOf = "balanceOf";
        string transfer = "transfer";
        string transfer_app = "transfer_app";
        string deploy = "deploy";
        string getInvokescript() {
            var shash = nnc_shash;
            var sb = new ThinNeo.ScriptBuilder();
            sb.EmitParamJson(MyJson.Parse("[]"));
            sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)" + decimals));
            sb.EmitAppCall(shash);


            //sb.EmitParamJson(MyJson.Parse("[]"));
            //sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)symbol"));
            //sb.EmitAppCall(shash);

            //sb.EmitParamJson(MyJson.Parse("[]"));
            //sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)decimals"));
            //sb.EmitAppCall(shash);

            //sb.EmitParamJson(MyJson.Parse("[]"));
            //sb.EmitParamJson(new MyJson.JsonNode_ValueString("(str)totalSupply"));
            //sb.EmitAppCall(shash);

            var data = sb.ToArray();

            //合约
            var script = ThinNeo.Helper.Bytes2HexString(data);

            var str = "{\"jsonrpc\":\"2.0\",\"method\":\"invokescript\",\"id\":1,\"params\":[\"" + script + "\"]}";

            WebClient wc = new WebClient();
            byte[] sendData = Encoding.GetEncoding("UTF-8").GetBytes(MyJson.Parse(str).ToString());
            wc.Headers.Add("Content-Type", "application/json");

            byte[] recData = wc.UploadData("http://localhost:20332/", "POST", sendData);
            var info = Encoding.GetEncoding("UTF-8").GetString(recData);
            var json = MyJson.Parse(info) as MyJson.JsonNode_Object;
            //var stack = json.GetDictItem("result").GetDictItem("stack");
            //string byte1 = stack.GetArrayItem(0).GetDictItem("value").ToString();
            //byte1 = Encoding.UTF8.GetString(ThinNeo.Helper.HexString2Bytes(byte1));
            return json.ToString();
        }

        string getStorage() {
            //合约地址
            var scriptaddress = "0x2e88caf10afe621e90142357236834e010b16df2";
            //公钥
            var key = "9b87a694f0a282b2b5979e4138944b6805350c6fa3380132b21a2f12f9c2f4b6";
            var revkey = ThinNeo.Helper.HexString2Bytes(key);     

            var str = "{\"jsonrpc\":\"2.0\",\"method\":\"getstorage\",\"id\":1,\"params\":[\"" + scriptaddress + "\",\"" + key + "\"]}";

            WebClient wc = new WebClient();
            byte[] sendData = Encoding.GetEncoding("UTF-8").GetBytes(MyJson.Parse(str).ToString());
            wc.Headers.Add("Content-Type", "application/json");

            byte[] recData = wc.UploadData("http://localhost:20332/", "POST", sendData);
            var info = Encoding.GetEncoding("UTF-8").GetString(recData);
            var json = MyJson.Parse(info).ToString();

            return json;
        }



        string makeRpcUrl(string url, string method, string param) {
            string urlout = url + "?jsonrpc=2.0&id=1&method="+ method + "&params=[";
            urlout += param;
            urlout += "]";
            return urlout;
        }

        static void Main(string[] args)
        {

            Console.WriteLine(Encoding.ASCII.GetString(ThinNeo.Helper.HexString2Bytes("21ac")));
            Program p = new Program();

            //string s2 = p.getStorage();
            //Console.WriteLine(s2);

            //string s = p.getInvokescript();
            //Console.WriteLine(s);
            
            Console.ReadKey();
        }
    }
}
