﻿using AppKit;

namespace iHentai.MacOS
{
    internal static class MainClass
    {
        private static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.SharedApplication.Delegate = new AppDelegate();
            NSApplication.Main(args);
        }
    }
}