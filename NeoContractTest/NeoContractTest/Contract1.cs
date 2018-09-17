using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;
using Helper = Neo.SmartContract.Framework.Helper;

namespace NeoContractTest
{
    public class Contract1 : SmartContract
    {
        public delegate void deleTransfer(byte[] from, byte[] to, BigInteger value);
        [DisplayName("transfer")]
        public static event deleTransfer Transferred;

        public class TransferInfo {
            public byte[] from;
            public byte[] to;
            public BigInteger value;
        }

        static readonly byte[] superAdmin = Helper.ToScriptHash("AZ8p2L3AFBhd2qXrVn6eKP61dLQRS3ASVK");

        public static string name() {
            return "Li Zhan Credit";
        }

        public static string symbol() {
            return "LZC";
        }

        private const ulong factor = 100;
        private const ulong totalCoin = 10 * 100000000 * factor;
        public static byte decimals() {
            return 2;
        }

        public static BigInteger totalSupply() {
            return Storage.Get(Storage.CurrentContext, "totalSupply").AsBigInteger();
        }

        public static BigInteger balanceOf(byte[] who) {
            var keyAddress = new byte[] { 0x11 }.Concat(who);
            return Storage.Get(Storage.CurrentContext, keyAddress).AsBigInteger();
        }

        public static bool transfer(byte[] from, byte[] to, BigInteger value) {
            if (value <= 0) return false;
            if (from == to) return true;
            if (from.Length > 0) {
                var keyFrom = new byte[] { 0x11 }.Concat(from);
                BigInteger from_value = Storage.Get(Storage.CurrentContext, keyFrom).AsBigInteger();
                if (from_value < value) return false;
                if (from_value == value) Storage.Delete(Storage.CurrentContext, keyFrom);
                else Storage.Put(Storage.CurrentContext, keyFrom, from_value - value);
            }
            if (to.Length > 0) {
                var keyTo = new byte[] { 0x11 }.Concat(to);
                BigInteger to_value = Storage.Get(Storage.CurrentContext,keyTo).AsBigInteger();
                Storage.Put(Storage.CurrentContext, keyTo, to_value + value);
            }
            setTxInfo(from, to, value);
            Transferred(from, to, value);
            return true;
        }

        private static void setTxInfo(byte[] from, byte[] to, BigInteger value) {
            TransferInfo info = new TransferInfo();
            info.from = from;
            info.to = to;
            info.value = value;
            byte[] txinfo = Helper.Serialize(info);
            var txid = (ExecutionEngine.ScriptContainer as Transaction).Hash;
            var keytxid = new byte[] { 0x13 }.Concat(txid);
            Storage.Put(Storage.CurrentContext, keytxid, txinfo);
        }

        public static TransferInfo getTxInfo(byte[] txid) {
            byte[] keytxid = new byte[] { 0x13 }.Concat(txid);
            byte[] v = Storage.Get(Storage.CurrentContext, keytxid);
            if (v.Length == 0) return null;
            return Helper.Deserialize(v) as TransferInfo;
        }

        public static object Main(string method, object[] args) {
            var magicstr = "20180628";
            if (Runtime.Trigger == TriggerType.Verification)
            {
                return false;
            }
            else if (Runtime.Trigger == TriggerType.VerificationR)
            {
                return true;
            }
            else if (Runtime.Trigger == TriggerType.Application) {
                var callscript = ExecutionEngine.CallingScriptHash;
                if (method == "totalSupply") return totalSupply();
                if (method == "name") return name();
                if (method == "symbol") return symbol();
                if (method == "decimals") return decimals();
                if (method == "deploy") {
                    if (args.Length != 1) return false;
                    if (!Runtime.CheckWitness(superAdmin)) return false;
                    byte[] total_supply = Storage.Get(Storage.CurrentContext, "totalSupply");
                    if (total_supply.Length != 0) return false;
                    var keySuperAdmin = new byte[] { 0x11 }.Concat(superAdmin);
                    Storage.Put(Storage.CurrentContext, keySuperAdmin, totalCoin);
                    Storage.Put(Storage.CurrentContext, "totalSupply", totalCoin);

                    Transferred(null, superAdmin, totalCoin);
                }
                if (method == "balanceOf") {
                    if (args.Length != 1) return 0;
                    byte[] who = (byte[])args[0];
                    if (who.Length != 20) return false;
                    return balanceOf(who);
                }
                if (method == "transfer") {
                    if (args.Length != 3) return false;
                    byte[] from = (byte[])args[0];
                    byte[] to = (byte[])args[1];
                    if (from == to) return true;
                    if (from.Length != 20 || to.Length != 20) return false;
                    BigInteger value = (BigInteger)args[2];
                    if (!Runtime.CheckWitness(from)) return false;
                    if (ExecutionEngine.EntryScriptHash.AsBigInteger() != callscript.AsBigInteger()) return false;
                    if (!IsPayable(to)) return false;
                    return transfer(from, to, value);
                }
                if (method == "transfer_app") {
                    if (args.Length != 3) return false;
                    byte[] from = (byte[])args[0];
                    byte[] to = (byte[])args[1];
                    BigInteger value = (BigInteger)args[2];
                    if (from.AsBigInteger() != callscript.AsBigInteger()) return false;
                    return transfer(from, to, value);
                }
                if (method == "getTxInfo") {
                    if (args.Length != 1) return 0;
                    byte[] txid = (byte[])args[0];
                    return getTxInfo(txid);
                }
                if (method == "upgrade") {
                    if (!Runtime.CheckWitness(superAdmin)) return false;
                    if (args.Length != 1 && args.Length != 9) return false;
                    byte[] script = Blockchain.GetContract(ExecutionEngine.ExecutingScriptHash).Script;
                    byte[] new_script = (byte[])args[0];
                    if (script == new_script) return false;
                    byte[] parameter_list = new byte[] { 0x07, 0x10 };
                    byte return_type = 0x05;
                    bool need_storage = (bool)(object)05;
                    string name = "lzc";
                    string version = "1";
                    string author = "LiZhan";
                    string email = "0";
                    string description = "lzc";

                    if (args.Length == 9) {
                        parameter_list = (byte[])args[1];
                        return_type = (byte)args[2];
                        need_storage = (bool)args[3];
                        name = (string)args[4];
                        version = (string)args[5];
                        author = (string)args[6];
                        email = (string)args[7];
                        description = (string)args[8];
                    }
                    Contract.Migrate(new_script, parameter_list, return_type, need_storage, name, version, author, email, description);
                    return true;
                }
            }
            return false;
        }

        public static bool IsPayable(byte[] to) {
            var c = Blockchain.GetContract(to);
            if (c.Equals(null)) return true;
            return c.IsPayable;
        }

    }
}
