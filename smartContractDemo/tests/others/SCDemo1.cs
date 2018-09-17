﻿using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace smartContractDemo
{
    public class SCDemo1:ITest
    {
        string api = "https://api.nel.group/api/testnet";

        public string Name => "智能合约3连 1/3";

        public string ID => "SC1/3";

        public async Task Demo()
        {
            string scriptaddress = "0x2e88caf10afe621e90142357236834e010b16df2";
            string key = "9b87a694f0a282b2b5979e4138944b6805350c6fa3380132b21a2f12f9c2f4b6";
            var rev = ThinNeo.Helper.HexString2Bytes(key).Reverse().ToArray();
            var revkey = ThinNeo.Helper.Bytes2HexString(rev);

            var url = Helper.MakeRpcUrl(api, "getstorage", new MyJson.JsonNode_ValueString(scriptaddress), new MyJson.JsonNode_ValueString(key));

            string result = await Helper.HttpGet(url);
            Console.WriteLine("得到的结果是：" + result);
        }
    }
}
