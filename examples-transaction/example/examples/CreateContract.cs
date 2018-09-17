using Neo.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace example
{
    class CreateContract : IExample
    {
        public string Name => "发布合约";

        public string ID => "CreateContract";

        //拼交易体
        Transaction makeTran(Dictionary<string, List<UTXO>> dir_utxos, string fromAddress, string targetAddress, ThinNeo.Hash256 assetid, decimal sendcount)
        {
            if (!dir_utxos.ContainsKey(assetid.ToString()))
                throw new Exception("no enough money.");

            List<UTXO> utxos = dir_utxos[assetid.ToString()];

            Transaction tran = new Transaction();
            utxos.Sort((a, b) =>
            {
                if (a.value > b.value)
                    return 1;
                else if (a.value < b.value)
                    return -1;
                else
                    return 0;
            });

            decimal count = decimal.Zero;
            List<TransactionInput> list_inputs = new List<TransactionInput>();
            for (var i = 0; i < utxos.Count; i++)
            {
                TransactionInput input = new TransactionInput();
                input.hash = utxos[i].txid;
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].value;
                if (count >= (sendcount))
                {
                    break;
                }
            }

            tran.inputs = list_inputs.ToArray();

            if (count >= sendcount)//输入大于等于输出
            {
                List<TransactionOutput> list_outputs = new List<TransactionOutput>();
                //输出
                if (sendcount > decimal.Zero)
                {
                    TransactionOutput output = new TransactionOutput();
                    output.assetId = assetid;
                    output.value = sendcount;
                    output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(targetAddress);
                    list_outputs.Add(output);
                }

                //找零
                var change = count - sendcount;
                if (change > decimal.Zero)
                {
                    TransactionOutput outputchange = new TransactionOutput();
                    outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(fromAddress);
                    outputchange.value = change;
                    outputchange.assetId = assetid;
                    list_outputs.Add(outputchange);
                }
                tran.outputs = list_outputs.ToArray();
            }
            else
            {
                throw new Exception("no enough money.");
            }
            return tran;
        }

        public async Task Start()
        {
            /**私钥*/
            string privateKey = "7c5a23ddb24c5eacb200874a32a64585bea7f1dd8efa23745e2b2a7753004c81";
            /**gas资产编号*/
            string gasAsset = "0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b";
            /**合约脚本散列*/
            string scriptHash = "0x98f56b5dc40650da5c1ead5894979e0c2fe4bcd6";
            /**合约代码*/
            string scriptCode = "00c56b616168164e656f2e53746f726167652e476574436f6e746578740548656c6c6f05576f726c64615272680f4e656f2e53746f726167652e50757461616c7566";
            /**名称*/
            string name = "helloworld";
            /**版本*/
            string version = "1.0";
            /**作者*/
            string author = "zhan.li";
            /**邮箱*/
            string email = "joshua0526@163.com";
            /**说明*/
            string description = "test Create Contract";
            /**参数列表*/
            string paramList = "hello";
            /**返回值*/
            string returnValue = "world";

            byte[] priKey = Encoding.Default.GetBytes(privateKey);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(priKey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            byte[] script = null;
            using (ScriptBuilder sb = new ScriptBuilder())
            {
                sb.EmitSysCall(HexToBytes(scriptCode), HexToBytes(paramList), returnValue, "0", name, version, author, email, description);
                sb.EmitSysCall("Neo.Contract.Create");
                script = sb.ToArray();
            }

            Dictionary<string, List<UTXO>> dic_UTXO = await GetUTXOByAddress("http://localhost:20332/", address);
            Transaction tran = makeTran(dic_UTXO, address, targetAddress, new Hash256("0xc56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b"), new decimal(90));
            tran.type = ThinNeo.TransactionType.InvocationTransaction;
            tran.version = 0;
            tran.attributes = new ThinNeo.Attribute[0];
            var idata = new ThinNeo.InvokeTransData();
            tran.extdata = idata;
            idata.script = script;
            idata.gas = 0;

            byte[] msg = tran.GetMessage();
            string msgstr = ThinNeo.Helper.Bytes2HexString(msg);
            byte[] signdata = ThinNeo.Helper.Sign(msg, priKey);
            tran.AddWitness(signdata, pubkey, address);
            string txid = tran.GetHash().ToString();
            byte[] data = tran.GetRawData();
            string rawdata = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = HttpHelper.MakeRpcUrlPost("https://api.nel.group/api/testnet", "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
            var result = await HttpHelper.HttpPost(url, postdata);
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
        }

        public static byte[] HexToBytes(this string value)
        {
            if (value == null || value.Length == 0)
                return new byte[0];
            if (value.Length % 2 == 1)
                throw new FormatException();
            byte[] result = new byte[value.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            return result;
        }

        public static async Task<Dictionary<string, List<UTXO>>> GetUTXOByAddress(string api, string _addr)
        {
            MyJson.JsonNode_Object response = (MyJson.JsonNode_Object)MyJson.Parse(await HttpHelper.HttpGet(api + "?method=getutxo&id=1&params=['" + _addr + "']"));
            MyJson.JsonNode_Array resJA = (MyJson.JsonNode_Array)response["result"];
            Dictionary<string, List<UTXO>> _dir = new Dictionary<string, List<UTXO>>();
            foreach (MyJson.JsonNode_Object j in resJA)
            {
                UTXO utxo = new UTXO(j["addr"].ToString(), new ThinNeo.Hash256(j["txid"].ToString()), j["asset"].ToString(), decimal.Parse(j["value"].ToString()), int.Parse(j["n"].ToString()));
                if (_dir.ContainsKey(j["asset"].ToString()))
                {
                    _dir[j["asset"].ToString()].Add(utxo);
                }
                else
                {
                    List<UTXO> l = new List<UTXO>();
                    l.Add(utxo);
                    _dir[j["asset"].ToString()] = l;
                }
            }
            return _dir;
        }
    }
}
