﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SnapshotAndAirdrop.Helper
{
    class HttpHelper
    {
        public static string MakeRpcUrlPost(string url, string method, out byte[] data, params JObject[] _params)
        {
            var json = new JObject();
            json["id"] = 1;
            json["jsonrpc"] = "2.0";
            json["method"] = method;
            StringBuilder sb = new StringBuilder();
            var array = new JArray();
            for (var i = 0; i < _params.Length; i++)
            {

                array.Add(_params[i]);
            }
            json["params"] = array;
            data = System.Text.Encoding.UTF8.GetBytes(json.ToString());
            return url;
        }
        public static string HttpGet(string url)
        {
            WebClient wc = new WebClient();
            return wc.DownloadString(url);
        }
        public static string HttpPost(string url, byte[] data)
        {
            WebClient wc = new WebClient();
            wc.Headers["content-type"] = "text/plain;charset=UTF-8";
            byte[] retdata;
            try
            {
                retdata = wc.UploadData(url, "POST", data);
            }
            catch (Exception e)
            {
                System.Threading.Thread.Sleep(100);
                return HttpPost(url, data);
            }
            return System.Text.Encoding.UTF8.GetString(retdata);
        }

        public static async Task<string> HttpGetAsyncy(string url)
        {
            WebClient wc = new WebClient();
            return await wc.DownloadStringTaskAsync(url);
        }
        public static async Task<string> HttpPostAsyncy(string url, byte[] data)
        {
            WebClient wc = new WebClient();
            wc.Headers["content-type"] = "text/plain;charset=UTF-8";
            byte[] retdata = await wc.UploadDataTaskAsync(url, "POST", data);
            return System.Text.Encoding.UTF8.GetString(retdata);
        }
    }
}
