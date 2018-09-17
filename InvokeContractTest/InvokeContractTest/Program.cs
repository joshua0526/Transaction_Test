using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ThinNeo;

namespace InvokeContractTest
{
    class Program
    {
        string api = "https://api.nel.group/api/testnet";
        string local = "http://127.0.0.1:20332/";

        string id_GAS = "0x602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7";

        public async Task publishContract() {
            string wif = "L1PSC3LRShi51xHAX2KN9oCFqETrZQhnzhKVu5zbrzdDpxF1LQz3";
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(api, address);

            byte[] script = System.IO.File.ReadAllBytes("0xccd651a5e7d9f4dc698353970df7b7180139cbbe.avm");
            Console.WriteLine("合约脚本："+ThinNeo.Helper.Bytes2HexString(script));
            Console.WriteLine("合约脚本Hash："+ThinNeo.Helper.Bytes2HexString(ThinNeo.Helper.GetScriptHashFromScript(script).data.ToArray().Reverse().ToArray()));
            byte[] parameter__list = ThinNeo.Helper.HexString2Bytes("0710");
            byte[] return_type = ThinNeo.Helper.HexString2Bytes("05");
            int need_storage = 1;
            int need_nep4 = 0;
            int need_canCharge = 4;
            string name = "mygas";
            string version = "1.0";
            string auther = "LZ";
            string email = "0";
            string description = "0";
            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder()) {
                var ss = need_storage | need_nep4 | need_canCharge;
                sb.EmitPushString(description);
                sb.EmitPushString(email);
                sb.EmitPushString(auther);
                sb.EmitPushString(version);
                sb.EmitPushString(name);
                sb.EmitPushNumber(ss);
                sb.EmitPushBytes(return_type);
                sb.EmitPushBytes(parameter__list);
                sb.EmitPushBytes(script);
                sb.EmitSysCall("Neo.Contract.Create");

                string scriptPublish = ThinNeo.Helper.Bytes2HexString(sb.ToArray());

                byte[] postdata;
                var url = Helper.MakeRpcUrlPost(api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(scriptPublish));
                var result = await Helper.HttpPost(url, postdata);
                //return;

                var consume = (((MyJson.Parse(result) as MyJson.JsonNode_Object)["result"] as MyJson.JsonNode_Array)[0] as MyJson.JsonNode_Object)["gas_consumed"].ToString();
                decimal gas_consumed = decimal.Parse(consume);
                ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
                extdata.script = sb.ToArray();

                extdata.gas = Math.Ceiling(gas_consumed - 10);

                ThinNeo.Transaction tran = makeTran(dir, null, new ThinNeo.Hash256(id_GAS), extdata.gas);
                tran.version = 1;
                tran.extdata = extdata;
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                byte[] signdata = ThinNeo.Helper.Sign(msg, prikey);
                tran.AddWitness(signdata, pubkey, address);
                string txid = tran.GetHash().ToString();
                byte[] data = tran.GetRawData();
                string rawdata = ThinNeo.Helper.Bytes2HexString(data);

                url = Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
                result = await Helper.HttpPost(url, postdata);

                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
                Console.WriteLine(resJO.ToString());
            }
        }

        public async Task ContractTransaction() {
            string wif = "L1PSC3LRShi51xHAX2KN9oCFqETrZQhnzhKVu5zbrzdDpxF1LQz3";
            byte[] prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wif);
            byte[] pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            string address = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);

            byte[] prikey1 = ThinNeo.Helper.GetPrivateKeyFromWIF("L3KK4jkgRzNEimdRxzzvD2p2vmPfkpnLC574tY2s5qY3M9LthHtb");
            byte[] pubkey1 = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey1);
            string address1 = ThinNeo.Helper.GetAddressFromPublicKey(pubkey1);

            Dictionary<string, List<Utxo>> dir = await Helper.GetBalanceByAddress(api, address1);

            using (ThinNeo.ScriptBuilder sb = new ThinNeo.ScriptBuilder())
            {
                MyJson.JsonNode_Array array = new MyJson.JsonNode_Array();
                array.AddArrayValue("(int)1");
                sb.EmitParamJson(array);
                sb.EmitPushString("deploy");
                sb.EmitAppCall(new Hash160("0xccd651a5e7d9f4dc698353970df7b7180139cbbe"));

                string scriptPublish = ThinNeo.Helper.Bytes2HexString(sb.ToArray());

                byte[] postdata;
                var url = Helper.MakeRpcUrlPost(api, "invokescript", out postdata, new MyJson.JsonNode_ValueString(scriptPublish));
                var result = await Helper.HttpPost(url, postdata);
                //return;

                var consume = (((MyJson.Parse(result) as MyJson.JsonNode_Object)["result"] as MyJson.JsonNode_Array)[0] as MyJson.JsonNode_Object)["gas_consumed"].ToString();
                decimal gas_consumed = decimal.Parse(consume);
                ThinNeo.InvokeTransData extdata = new ThinNeo.InvokeTransData();
                extdata.script = sb.ToArray();

                extdata.gas = Math.Ceiling(gas_consumed);

                ThinNeo.Transaction tran = makeTran(dir, null, new ThinNeo.Hash256(id_GAS), extdata.gas);
                tran.version = 1;
                tran.extdata = extdata;
                tran.type = ThinNeo.TransactionType.InvocationTransaction;
                byte[] msg = tran.GetMessage();
                byte[] signdata = ThinNeo.Helper.Sign(msg, prikey1);               
                tran.AddWitness(signdata, pubkey1, address1); 
                string txid = tran.GetHash().ToString();
                byte[] data = tran.GetRawData();
                string rawdata = ThinNeo.Helper.Bytes2HexString(data);

                url = Helper.MakeRpcUrlPost(api, "sendrawtransaction", out postdata, new MyJson.JsonNode_ValueString(rawdata));
                result = await Helper.HttpPost(url, postdata);

                MyJson.JsonNode_Object resJO = (MyJson.JsonNode_Object)MyJson.Parse(result);
                Console.WriteLine(resJO.ToString());
            }
        }

        ThinNeo.Transaction makeTran(Dictionary<string, List<Utxo>> dir_utxos, string targetaddr, ThinNeo.Hash256 assetid, decimal sendcount) {
            if (!dir_utxos.ContainsKey(assetid.ToString())) {
                throw new Exception("no enough money.");
            }
            List<Utxo> utxos = dir_utxos[assetid.ToString()];
            var tran = new ThinNeo.Transaction();
            tran.type = ThinNeo.TransactionType.ContractTransaction;
            tran.version = 0;
            tran.extdata = null;

            tran.attributes = new ThinNeo.Attribute[0];
            var scraddr = "";
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
            List<ThinNeo.TransactionInput> list_inputs = new List<ThinNeo.TransactionInput>();
            for (var i = 0; i < utxos.Count; i++) {
                ThinNeo.TransactionInput input = new ThinNeo.TransactionInput();
                input.hash = utxos[i].txid;
                input.index = (ushort)utxos[i].n;
                list_inputs.Add(input);
                count += utxos[i].value;
                scraddr = utxos[i].addr;
                if (count >= sendcount) {
                    break;
                }
            }
            tran.inputs = list_inputs.ToArray();
            if (count >= sendcount)
            {
                List<ThinNeo.TransactionOutput> list_outputs = new List<ThinNeo.TransactionOutput>();

                if (sendcount > decimal.Zero && targetaddr != null)
                {
                    ThinNeo.TransactionOutput output = new ThinNeo.TransactionOutput();
                    output.assetId = assetid;
                    output.value = sendcount;
                    output.toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(scraddr);
                    list_outputs.Add(output);
                }

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
            else {
                throw new Exception("no enough money.");
            }
            return tran;
        }

        public async void start() {
            //await publishContract();
            await ContractTransaction();
        }

        static void Main(string[] args)
        {
            Console.WriteLine(new BigInteger(ThinNeo.Helper.HexString2Bytes("b024764817")));
            //Console.WriteLine(ThinNeo.Helper.Bytes2HexString(Encoding.Default.GetBytes("00e8764817")));
            //Program p = new Program();
            //p.start();
            Console.ReadKey();                     
        }
    }
}
