﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using iHentai.Apis.Core;
using iHentai.Apis.Core.Common;
using iHentai.Apis.Core.Models.Interfaces;
using iHentai.Apis.EHentai.Models;
using iHentai.Services;

namespace iHentai.Apis.EHentai
{
    [ApiKey(nameof(EHentai))]
    public class Apis : IHentaiApi, IWebApi, ICanLogin
    {
        public bool IsExhentaiMode { get; set; } = true;
        
        public string LoginWebViewUrl { get; } = "https://forums.e-hentai.org/index.php?act=Login";

        public ILoginData LoginDataGenerator => new LoginData();

        public Task<IInstanceData> Login(ILoginData data, CancellationToken token = default)
        {
            if (!(data is LoginData loginData)) throw new ArgumentException();
            return Login(loginData.UserName, loginData.Password, token);
        }
        
        public SearchOptionBase SearchOptionGenerator => new SearchOption();

        public string Host => IsExhentaiMode ? "exhentai.org" : "g.e-hentai.org";

        public async Task<(int MaxPage, IEnumerable<IGalleryModel> Gallery)> Gallery(IInstanceData data, int page = 0,
            SearchOptionBase searchOption = null, CancellationToken cancellationToken = default)
        {
            if (!(data is InstanceData instanceData)) throw new ArgumentException();
            Url req;
            if (searchOption != null && searchOption.SearchType == SearchTypes.Tag)
                req = $"https://{Host}/"
                    .AppendPathSegment("tag")
                    .AppendPathSegment(searchOption.Keyword);
            else
                req = $"https://{Host}/".SetQueryParams(searchOption?.ToDictionary());
            var res = await req.SetQueryParam("page", page)
                .GetHtmlAsync<GalleryListModel>(cancellationToken);
            return (res.MaxPage, res.Gallery.WithoutShit());
        }

        public async Task<IGalleryDetailModel> Detail(IInstanceData data, IGalleryModel model,
            CancellationToken cancellationToken = default)
        {
            if (!(model is GalleryModel item) || !(data is InstanceData instanceData))
                throw new ArgumentException();
            return await $"https://{Host}/"
                .AppendPathSegment("g")
                .AppendPathSegment(item.ID)
                .AppendPathSegment(item.Token)
                .GetHtmlAsync<GalleryDetailModel>(cancellationToken);
        }

        public string GetWebLink(IGalleryModel model)
        {
            if (!(model is GalleryModel item))
                return string.Empty;
            return $"https://{Host}/"
                .AppendPathSegment("g")
                .AppendPathSegment(item.ID)
                .AppendPathSegment(item.Token);
        }

        //public bool WebViewLoginHandler(string url, string cookie)
        //{
        //    if (!cookie.Contains("ipb_member_id") || !cookie.Contains("ipb_pass_hash")) return false;
        //    var memberid = Regex.Match(cookie, @"ipb_member_id=([^;]*)").Groups[1].Value;
        //    var passHash = Regex.Match(cookie, @"ipb_pass_hash=([^;]*)").Groups[1].Value;
        //    Cookie = new Dictionary<string, string>
        //    {
        //        {"ipb_member_id", memberid},
        //        {"ipb_pass_hash", passHash}
        //    };
        //    return true;
        //}

        //public async Task<bool> WebViewLoginFollowup(CancellationToken cancellationToken)
        //{
        //    if (!Cookie.ContainsKey("s"))
        //        Cookie = await UpdateCookie(Cookie, cancellationToken);
        //    return true;
        //}

        private async Task<IInstanceData> Login(string userName, string password,
            CancellationToken cancellationToken = default)
        {
            Dictionary<string, string> cookie = null;
            using (var loginClient = new FlurlClient {Settings = {HttpClientFactory = new DefaultHttpClientFactory()}})
            {
                using (var res = await "http://forums.e-hentai.org/index.php?act=Login&CODE=01&CookieDate=1"
                    .WithClient(loginClient).EnableCookies()
                    .PostUrlEncodedAsync(new
                    {
                        UserName = userName,
                        PassWord = password,
                        x = 0,
                        y = 0
                    }, cancellationToken))
                {
                    res.Headers.TryGetValues("Set-Cookie", out var cookies);
                    cookie = cookies
                        .Select(item =>
                            (Key: Regex.Matches(item, "([^=]*)=([^;]*);")[0].Groups[1].Value, Regex.Matches(item,
                                "([^=]*)=([^;]*);")[0].Groups[2].Value))
                        .Distinct(item => item.Key)
                        .Where(item => item.Key == "ipb_member_id" || item.Key == "ipb_pass_hash")
                        .ToDictionary(item => item.Key, item => item.Value);
                }

                if (!cookie.Any()) throw new KeyNotFoundException();
            }

            cookie = await UpdateCookie(cookie, cancellationToken);
            return new InstanceData
            {
                Cookies = cookie
            };
        }

        private async Task<Dictionary<string, string>> UpdateCookie(Dictionary<string, string> cookie,
            CancellationToken cancellationToken = default)
        {
            using (var loginClient = new FlurlClient {Settings = {HttpClientFactory = new DefaultHttpClientFactory()}})
            {
                using (var res = await "https://exhentai.org/uconfig.php".WithClient(loginClient).WithCookies(cookie)
                    .WithCookie("uconfig", string.Empty).GetAsync(cancellationToken))
                {
                    if (res.Headers.TryGetValues("Set-Cookie", out var cookies) &&
                        cookies.Any(item => item.StartsWith("s=")))
                        cookie = cookies
                            .Select(item =>
                                (Key: Regex.Matches(item, "([^=]*)=([^;]*);")[0].Groups[1].Value, Regex.Matches(item,
                                    "([^=]*)=([^;]*);")[0].Groups[2].Value))
                            .Distinct(item => item.Key)
                            .Where(item => item.Key == "s")
                            .ToDictionary(item => item.Key, item => item.Value)
                            .Concat(cookie)
                            .ToDictionary(item => item.Key, item => item.Value);
                    else
                        throw new ArgumentException();
                }
            }

            return cookie;
        }
    }
}