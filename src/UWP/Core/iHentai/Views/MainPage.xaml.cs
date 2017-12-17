﻿using iHentai.Mvvm;
using iHentai.ViewModels;

namespace iHentai.Views
{
    public sealed partial class MainPage : IMvvmView<MainViewModel>
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public new MainViewModel ViewModel
        {
            get => (MainViewModel) base.ViewModel;
            set => base.ViewModel = value;
        }
    }
}