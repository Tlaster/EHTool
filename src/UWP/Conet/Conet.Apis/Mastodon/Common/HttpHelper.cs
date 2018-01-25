﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Conet.Apis.Mastodon.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Conet.Apis.Mastodon.Common
{
    internal static class HttpHelper
    {
        public const string HTTPS = "https://";
        public const string HTTP = "http://";
        private static string UrlEncode(string url, IEnumerable<(string Key, string Value)> param)
            => param != null ? 
            $"{url}?{string.Join("&", param.Where(CheckForValue).Select(kvp => $"{kvp.Key}={kvp.Value}"))}" : 
            url;

        private static bool CheckForValue<T>((string Key, T Value) kvp)
        {
            return !string.IsNullOrEmpty(kvp.Value.ToString()) && (!int.TryParse(kvp.Value.ToString(), out var intValue) || intValue > 0) && (!bool.TryParse(kvp.Value.ToString(), out var boolValue) || boolValue);
        }

        public static IEnumerable<(string, T)> ArrayEncode<T>(string paramName, params T[] values)
        {
            paramName = $"{paramName}[]";
            return values.Select(value => (paramName, value));
        }

        private static HttpClient GetHttpClient(string token, string tokenType = "Bearer") => string.IsNullOrEmpty(token)
            ? new HttpClient()
            : new HttpClient
            {
                DefaultRequestHeaders = {Authorization = new AuthenticationHeaderValue(tokenType, token)}
            };

        public static async Task<string> GetAsync(string url, string token, IEnumerable<(string Key, string Value)> param)
        {
            using (var client = GetHttpClient(token))
                return CheckForError(await client.GetStringAsync(UrlEncode(url, param)));
        }
        public static async Task<T> GetAsync<T>(string url, string token, IEnumerable<(string Key, string Value)> param) => JsonConvert.DeserializeObject<T>(await GetAsync(url, token, param));

        public static async Task<ArrayModel<T>> GetArrayAsync<T>(string url, string token, long max_id = 0, long since_id = 0, params (string Key, string Value)[] param)
        {
            var p = new List<(string Key, string Value)>
            {
                (nameof(max_id), max_id.ToString()),
                (nameof(since_id), since_id.ToString())
            };
            if (param != null)
                p.AddRange(param);
            return await GetArrayAsync<T>(url, token, p);
        }

        public static async Task<ArrayModel<T>> GetArrayAsync<T>(string url, string token, IEnumerable<(string Key, string Value)> param)
        {
            using (var client = GetHttpClient(token))
            using (var res = await client.GetAsync(UrlEncode(url, param)))
            {
                if (res.Headers.TryGetValues("Link", out var values))
                {
                    var links = values.FirstOrDefault().Split(',').Select(s => Regex.Match(s, "<.*(max_id|since_id)=([0-9]*)(.*)>; rel=\"(.*)\"").Groups).ToList();
                    long.TryParse(links.FirstOrDefault(m => m[1].Value == "max_id")?[2]?.Value, out var maxId);
                    long.TryParse(links.FirstOrDefault(m => m[1].Value == "since_id")?[2]?.Value, out var sinceId);
                    return new ArrayModel<T>
                    {
                        MaxId = maxId,
                        SinceId = sinceId,
                        Result = JsonConvert.DeserializeObject<List<T>>(await res.Content.ReadAsStringAsync())
                    };
                }
                else
                {
                    return new ArrayModel<T>
                    {
                        MaxId = 0,
                        SinceId = 0,
                        Result = JsonConvert.DeserializeObject<List<T>>(await res.Content.ReadAsStringAsync())
                    };
                }
            }
        }

        public static async Task<TModel> PostAsync<TModel, TValue>(string url, string token, IEnumerable<(string Key, TValue Value)> param)
             => JsonConvert.DeserializeObject<TModel>(await PostAsync(url, token, param));

        public static async Task<string> PostAsync<TValue>(string url, string token, IEnumerable<(string Key, TValue Value)> param)
        {
            using (var client = GetHttpClient(token))
            {
                if (param == null)
                    param = new List<(string Key, TValue Value)>();
                if (param.Select(p => p.Value).Any(item => item is HttpContent))
                {
                    using (var formData = new MultipartFormDataContent())
                    {
                        foreach (var item in param)
                            if (item.Value is StreamContent)
                                formData.Add(item.Value as StreamContent, item.Key, "file");
                            else if (CheckForValue(item))
                                formData.Add(item.Value as HttpContent, item.Key);
                        using (var res = await client.PostAsync(url, formData))
                            return CheckForError(await res.Content.ReadAsStringAsync());
                    }
                }
                client.Timeout = TimeSpan.FromSeconds(30);
                var items = param.Where(CheckForValue).Select(item => new KeyValuePair<string, string>(item.Key, item.Value.ToString()));
                using (var formData = new FormUrlEncodedContent(items))
                using (var res = await client.PostAsync(url, formData))
                    return CheckForError(await res.Content.ReadAsStringAsync());
            }
        }



        public static async Task<string> DeleteAsync(string url, string token)
        {
            using (var client = GetHttpClient(token))
            using (var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod("DELETE"), url)))
                return CheckForError(await response.Content.ReadAsStringAsync());
        }

        public static async Task<string> PatchAsync<TValue>(string url, string token, IEnumerable<(string Key, TValue Value)> param)
            where TValue : HttpContent
        {
            using (var formData = new MultipartFormDataContent())
            {
                foreach (var item in param)
                {
                    if (item.Value is StreamContent)
                        formData.Add(new StringContent(Convert.ToBase64String(await item.Value.ReadAsByteArrayAsync())), item.Key);
                    else
                        formData.Add(item.Value, item.Key);
                }
                using (var client = GetHttpClient(token))
                using (var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), url) { Content = formData }))
                    return CheckForError(await response.Content.ReadAsStringAsync());
            }
        }

        private static string CheckForError(string json)
        {
            if (string.IsNullOrEmpty(json))
                return json;
            if (json.StartsWith("<html>"))
            {
                throw new MastodonException("Return json is not valid");
            }
            try
            {
                var jobj = JsonConvert.DeserializeObject<JObject>(json);
                if (jobj.TryGetValue("error", out var token))
                {
                    throw new MastodonException(token.Value<string>());
                }
            }
            catch (InvalidCastException e)
            {

            }
            return json;
        }
    }
}