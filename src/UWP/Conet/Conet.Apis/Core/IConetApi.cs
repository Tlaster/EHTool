﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using iHentai.Services;
using Newtonsoft.Json.Linq;

namespace Conet.Apis.Core
{
    public interface IConetApi : IApi, ICanLogin
    {
        Task<(long Cursor, IEnumerable Data)> HomeTimeline(IInstanceData data, int count = 20, long max_id = 0L, long since_id = 0L);

        Task<object> User(IInstanceData data, long uid);
    }
}