using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThinNeo;

namespace TransactionTest
{
    class Program
    {
        static string api = "http://localhost:20332/";

        static string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        async public static Task Demo()
        {
            string privateKey = "7c5a23ddb24c5eacb200874a32a64585bea7f1dd8efa23745e2b2a7753004c81";
            string targetAddr = "AZr9EpWFwx9dJZd7nncx5eGAB5zaW9iW1Y";
            
            byte[] prikey = ThinNeo.Helper.HexString2Bytes(privateKey);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
          
            Console.WriteLine("确认我执行到了这里");
            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(api, address);            

            //拼装交易体
            string[] targetAddrs = new string[1] { targetAddr };
            Transaction tran = makeTran(dir, targetAddrs, new Hash256(id_GAS), (decimal)1);
            tran.version = 0;
            tran.type = TransactionType.ContractTransaction;
            byte[] msg = tran.GetMessage();
            byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
            tran.AddWitness(signdata, pubkey, address);
            string txid = tran.GetHash().ToString();
            byte[] data = tran.GetRawData();
            string rawdata = ThinNeo.Helper.Bytes2HexString(data);

            byte[] postdata;
            var url = Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
            var result = await Helper.HttpPost(url, postdata);
            MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
            Console.WriteLine(resJO.ToString());
        }

        //拼交易体
        static Transaction makeTran(Dictionary<string, List<Utxo>> dir_utxos, string[] targetaddrs, ThinNeo.Hash256 assetid, decimal sendcount)
        {
            if (!dir_utxos.ContainsKey(assetid.ToString()))
                throw new Exception("no enough money.");

            List<Utxo> utxos = dir_utxos[assetid.ToString()];

            var tran = new Transaction();
            tran.type = TransactionType.ContractTransaction;
            tran.version = 0;//0 or 1
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
            utxos.Sort((a, b) =>
            {
                if (a.value > b.value)
                    return 1;
                else if (a.value < b.value)
                    return -1;
                else
                    return 0;
            });

            //十进制小数
            decimal count = decimal.Zero;
            string scraddr = "";
            List<ThinNeo.TransactionInput> list_inputs = new List<ThinNeo.TransactionInput>();
            for (var i = 0; i < utxos.Count; i++)
            {
                ThinNeo.TransactionInput input = new ThinNeo.TransactionInput();
                input.hash = utxos[i].txid;
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].value;
                scraddr = utxos[i].addr;
                if (count >= (sendcount))
                {
                    break;
                }
            }

            tran.inputs = list_inputs.ToArray();

            if (count >= sendcount)//输入大于等于输出
            {
                List<ThinNeo.TransactionOutput> list_outputs = new List<ThinNeo.TransactionOutput>();
                //输出
                if (sendcount > decimal.Zero && targetaddrs.Length > 0)
                {
                    foreach (string targetaddr in targetaddrs)
                    {
                        ThinNeo.TransactionOutput output = new ThinNeo.TransactionOutput();
                        output.assetId = assetid;
                        output.value = sendcount;
                        output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(targetaddr);
                        list_outputs.Add(output);
                    }
                }

                //找零
                var change = count - sendcount;
                if (change > decimal.Zero)
                {
                    ThinNeo.TransactionOutput outputchange = new ThinNeo.TransactionOutput();
                    outputchange.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(scraddr);
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

        static void MakeTransaction(Transaction tran) {
            byte[] msg = tran.GetMessage();
        }

        async static void AsyncLoop()
        {
            await Demo();                   
        }

        static void Main(string[] args)
        {
            AsyncLoop();
            Console.WriteLine("优先执行一下");
            Console.ReadKey();
        }
    }
}
