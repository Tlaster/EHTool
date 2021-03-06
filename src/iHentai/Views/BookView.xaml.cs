﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace iHentai.Views
{
    public sealed partial class BookView : UserControl
    {
        public static readonly DependencyProperty CoverFirstProperty = DependencyProperty.Register(
            "CoverFirst", typeof(bool), typeof(BookView), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(object), typeof(BookView),
            new PropertyMetadata(default, OnItemsSourceChanged));

        public static readonly DependencyProperty LeftTemplateProperty = DependencyProperty.Register(
            "LeftTemplate", typeof(DataTemplate), typeof(BookView),
            new PropertyMetadata(default(DataTemplate), PropertyChangedCallback));

        public static readonly DependencyProperty RightTemplateProperty = DependencyProperty.Register(
            "RightTemplate", typeof(DataTemplate), typeof(BookView),
            new PropertyMetadata(default(DataTemplate), PropertyChangedCallback));

        public static readonly DependencyProperty DataTemplateSelectorProperty = DependencyProperty.Register(
            "DataTemplateSelector", typeof(DataTemplateSelector), typeof(BookView),
            new PropertyMetadata(default, OnDataTemplateSelectorChanged));

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            "SelectedIndex", typeof(int), typeof(BookView), new PropertyMetadata(default));

        public BookView()
        {
            InitializeComponent();
            var leftSelector = Resources["LeftPageTemplateSelector"] as PageTemplateSelector;
            leftSelector.Template = LeftTemplate;
            var rightSelector = Resources["RightPageTemplateSelector"] as PageTemplateSelector;
            rightSelector.Template = RightTemplate;
        }

        public bool CoverFirst
        {
            get => (bool) GetValue(CoverFirstProperty);
            set => SetValue(CoverFirstProperty, value);
        }

        public DataTemplate LeftTemplate
        {
            get => (DataTemplate) GetValue(LeftTemplateProperty);
            set => SetValue(LeftTemplateProperty, value);
        }

        public DataTemplate RightTemplate
        {
            get => (DataTemplate) GetValue(RightTemplateProperty);
            set => SetValue(RightTemplateProperty, value);
        }

        public int SelectedIndex
        {
            get => (int) GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public DataTemplateSelector DataTemplateSelector
        {
            get => (DataTemplateSelector) GetValue(DataTemplateSelectorProperty);
            set => SetValue(DataTemplateSelectorProperty, value);
        }

        internal ObservableCollection<BookViewItem> FlipViewSource { get; } = new ObservableCollection<BookViewItem>();

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is BookView view)
            {
                if (e.Property == LeftTemplateProperty)
                {
                    var selector = view.Resources["LeftPageTemplateSelector"] as PageTemplateSelector;
                    selector.Template = e.NewValue as DataTemplate;
                }

                if (e.Property == RightTemplateProperty)
                {
                    var selector = view.Resources["RightPageTemplateSelector"] as PageTemplateSelector;
                    selector.Template = e.NewValue as DataTemplate;
                }
            }
        }

        private static void OnDataTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BookView).OnDataTemplateSelectorChanged(e.NewValue as DataTemplateSelector);
        }

        private void OnDataTemplateSelectorChanged(DataTemplateSelector value)
        {
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as BookView).OnItemsSourceChanged(e.NewValue, e.OldValue);
        }

        private void OnItemsSourceChanged(object newValue, object oldValue)
        {
            if (oldValue is INotifyCollectionChanged oldNotifyCollectionChanged)
            {
                oldNotifyCollectionChanged.CollectionChanged -= OnItemsSourceChanged;
            }

            if (newValue is INotifyCollectionChanged newNotifyCollectionChanged)
            {
                newNotifyCollectionChanged.CollectionChanged += OnItemsSourceChanged;
            }

            if (newValue is IEnumerable enumerable)
            {
                FlipViewSource.Clear();
                var resultIndex = -1;
                var index = 0;
                if (CoverFirst)
                {
                    index++;
                    resultIndex++;
                    FlipViewSource.Add(new BookViewItem {Left = null});
                }

                foreach (var item in enumerable)
                {
                    if (index % 2 == 0)
                    {
                        resultIndex++;
                        FlipViewSource.Add(new BookViewItem {Left = item});
                    }
                    else
                    {
                        FlipViewSource[resultIndex].Right = item;
                    }

                    index++;
                }
                BookFlipView.SetBinding(FlipView.SelectedIndexProperty, new Binding
                {
                    Source = this,
                    Path = new PropertyPath(nameof(SelectedIndex)),
                    Converter = new IndexConverter(index - 1, FlipViewSource.Count - 1),
                    Mode = BindingMode.TwoWay,
                });
            }
        }

        private void OnItemsSourceChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //TODO:
        }
    }

    internal class IndexConverter : IValueConverter
    {
        public IndexConverter(int max, int bookMax)
        {
            Max = max;
            BookMax = bookMax;
        }

        public int Max { get; }
        public int BookMax { get; }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Math.Min(System.Convert.ToInt32(Math.Round(System.Convert.ToSingle(value) / 2f)), BookMax);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Math.Min((int) value * 2, Max);
        }
    }

    internal class PageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template { get; set; }
        public DataTemplateSelector Selector { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (Selector != null)
            {
                return Selector.SelectTemplate(item, container);
            }

            if (item != null)
            {
                return Template;
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    internal class BookViewItem : INotifyPropertyChanged
    {
        public object Left { get; set; }
        public object Right { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}