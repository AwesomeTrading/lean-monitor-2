using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Toolkit.Mvvm.Messaging;
using Panoptes.Model.Messages;
using System;

namespace Panoptes.Views.NewSession
{
    public partial class NewSessionWindow : Window
    {
        private readonly IMessenger _messenger;

        public NewSessionWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // TODO: Implement dependency injection for the messenger
            _messenger = (WeakReferenceMessenger)App.Current.Services.GetService(typeof(IMessenger)) ?? throw new NullReferenceException($"NewSessionWindow: '{nameof(_messenger)}' is null");
            _messenger.Register<NewSessionWindow, SessionOpenedMessage>(this, (r, _) =>
            {
                // Handle 'Call from invalid thread' exception
                Dispatcher.UIThread.InvokeAsync(() => r.Close());
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
