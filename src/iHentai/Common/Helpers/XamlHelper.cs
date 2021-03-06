﻿using System.Collections;
using System.Linq;
using Windows.UI.Xaml;

namespace iHentai.Common.Helpers
{
    internal static class XamlHelper
    {
        public static Visibility IsEmptyToVisibility(object any)
        {
            if (any is string stringValue)
            {
                return BoolToVisibility(string.IsNullOrEmpty(stringValue));
            }

            if (any is IEnumerable enumerable)
            {
                return BoolToVisibility(!enumerable.OfType<object>().Any());
            }

            return BoolToVisibility(false);
        }

        public static Visibility BoolToVisibility(bool visible)
        {
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Visibility InvertBoolToVisibility(bool collapsed)
        {
            return BoolToVisibility(!collapsed);
        }

        public static bool IsEqual(object item, object any)
        {
            return item == any;
        }

        public static bool IsZero(float number)
        {
            return number == 0f;
        }

        public static bool IsNonZero(float number)
        {
            return number != 0f;
        }

        public static string ElementThemeToString(ElementTheme theme)
        {
            return theme.ToString();
        }

        public static Visibility NonNullToVisibility(object? obj)
        {
            return BoolToVisibility(obj != null);
        }
    }
}