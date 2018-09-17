using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace example
{
    class BalanceOfNNC : IExample
    {
        public string Name => "获取 lzc 余额";

        public string ID => "balanceof";

        public async Task Start()
        {
            //string address = "ALjSnMZidJqd18iQaoCgFun6iqWRm2cVtj";
            string address = "AbN2K2trYzgx8WMg2H7U7JHH6RQVzz2fnx";
            byte[] data = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(addr)" + address);//who
                sb.EmitParamJson(array);
                sb.EmitPushString("balanceOf");

                //sb.EmitAppCall(new Hash160("0xbab964febd82c9629cc583596975f51811f25f47"));
                sb.EmitAppCall(new Hash160("0xa0b53d2efa8b1c4a62fcc1fcb54b7641510810c7"));
                data = sb.ToArray();
            }
            string script = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = HttpHelper.MakeRpcUrlPost("https://api.nel.group/api/testnet", "invokescript", out postdata, new MyJson.JsonNode_ValueString(script));
            var result = await HttpHelper.HttpPost(url, postdata);
            Console.WriteLine("得到的结果是：" + result);
        }
    }
}
