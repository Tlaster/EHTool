﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp;

namespace iHentai.Helpers
{
    public class AutoList<TSource, T> : IncrementalLoadingCollection<TSource, T> where TSource : Microsoft.Toolkit.Collections.IIncrementalSource<T>
    {

        public TSource DataSource => Source;

        public AutoList(int itemsPerPage = 20, Action onStartLoading = null, Action onEndLoading = null, Action<Exception> onError = null) : base(itemsPerPage, onStartLoading, onEndLoading, onError)
        {
        }

        public AutoList(TSource source, int itemsPerPage = 20, Action onStartLoading = null, Action onEndLoading = null, Action<Exception> onError = null) : base(source, itemsPerPage, onStartLoading, onEndLoading, onError)
        {
        }
    }
}