﻿using Conet.Apis.Core.Models.Interfaces;

namespace Conet.Apis.Mastodon
{
    public class LoginData : ILoginData
    {
        public string Domain { get; set; }
    }
}