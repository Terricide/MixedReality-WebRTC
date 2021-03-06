// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.MixedReality.WebRTC;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace TestAppUwp
{
    public class CategoryBase { }

    public class Category : CategoryBase
    {
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public Symbol Glyph { get; set; }
        public Type PageType { get; set; }
    }

    public class Separator : CategoryBase { }

    public class Header : CategoryBase
    {
        public string Name { get; set; }
    }

    [ContentProperty(Name = "ItemTemplate")]
    class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }

        internal DataTemplate HeaderTemplate = (DataTemplate)XamlReader.Load(
            @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                   <NavigationViewItemHeader Content='{Binding Name}' />
                  </DataTemplate>");

        internal DataTemplate SeparatorTemplate = (DataTemplate)XamlReader.Load(
            @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                    <NavigationViewItemSeparator />
                  </DataTemplate>");

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is Separator ? SeparatorTemplate : item is Header ? HeaderTemplate : ItemTemplate;
        }
    }

    /// <summary>
    /// Main application page with the navigation menu.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Collection of categories for the main navigation menu.
        /// </summary>
        public ObservableCollection<CategoryBase> Categories { get; } = new ObservableCollection<CategoryBase>()
        {
            new Category { Name = "Signaling", Glyph = Symbol.Map, PageType = typeof(SignalingPage) },
            new Category { Name = "Local Tracks", Glyph = Symbol.Library, PageType = typeof(TracksPage) },
            new Category { Name = "Session", Glyph = Symbol.VideoChat, PageType = typeof(SessionPage) },
            new Category { Name = "Media Player", Glyph = Symbol.Play, PageType = typeof(MediaPlayerPage) },
            new Category { Name = "Debug Logs", Glyph = Symbol.Memo, PageType = typeof(DebugConsolePage) },
        };

        public static string GetDeviceName()
        {
            return Environment.MachineName;
        }

        public MainPage()
        {
            RestoreSettings();

            this.InitializeComponent();

            // Insert items manually into navigation menu. It doesn't seem that there is a way
            // to force selecting a menu item when using data binding, so selecting the first
            // page below would crash (empty MenuItems list if assigning MenuItemsSource).
            //foreach (var catBase in Categories)
            //{
            //    if (catBase is Category cat)
            //    {
            //        var item = new NavigationViewItem()
            //        {
            //            Content = cat.Name,
            //            Icon = new SymbolIcon() { Symbol = cat.Glyph },
            //            DataContext = catBase
            //        };
            //        AutomationProperties.SetName(item, cat.Name);
            //        navigationView.MenuItems.Add(item);
            //    }
            //    else if (catBase is Header headerCat)
            //    {
            //        var item = new NavigationViewItemHeader()
            //        {
            //            Name = headerCat.Name,
            //            DataContext = catBase
            //        };
            //        AutomationProperties.SetName(item, headerCat.Name);
            //        navigationView.MenuItems.Add(item);
            //    }
            //    else if (catBase is Separator sepCat)
            //    {
            //        navigationView.MenuItems.Add(new NavigationViewItemSeparator());
            //    }
            //}
            //navigationView.MenuItemsSource = Categories;

            // Open the first page by default
            rootFrame.Navigate((Categories[0] as Category).PageType);
            //((NavigationViewItem)(navigationView.MenuItems[0])).IsSelected = true; // doesn't work...

            this.Loaded += OnLoaded;
            Application.Current.Suspending += App_Suspending;
            Application.Current.Resuming += App_Resuming;
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            // Save local and remote peer IDs for next launch for convenience
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            SessionModel sessionModel = SessionModel.Current;
            localSettings.Values["DssServerAddress"] = sessionModel.NodeDssSignaler.HttpServerAddress;
            localSettings.Values["LocalPeerID"] = sessionModel.NodeDssSignaler.LocalPeerId;
            localSettings.Values["RemotePeerID"] = sessionModel.NodeDssSignaler.RemotePeerId;
            localSettings.Values["PollTimeMs"] = sessionModel.NodeDssSignaler.PollTimeMs;
            //localSettings.Values["PreferredAudioCodec"] = PreferredAudioCodec;
            //localSettings.Values["PreferredAudioCodecExtraParamsLocal"] = PreferredAudioCodecExtraParamsLocalTextBox.Text;
            //localSettings.Values["PreferredAudioCodecExtraParamsRemote"] = PreferredAudioCodecExtraParamsRemoteTextBox.Text;
            //localSettings.Values["PreferredAudioCodec_Custom"] = PreferredAudioCodec_Custom.IsChecked.GetValueOrDefault() ? CustomPreferredAudioCodec.Text : "";
            //localSettings.Values["PreferredVideoCodec"] = PreferredVideoCodec;
            //localSettings.Values["PreferredVideoCodecExtraParamsLocal"] = PreferredVideoCodecExtraParamsLocalTextBox.Text;
            //localSettings.Values["PreferredVideoCodecExtraParamsRemote"] = PreferredVideoCodecExtraParamsRemoteTextBox.Text;
            //localSettings.Values["PreferredVideoCodec_Custom"] = PreferredVideoCodec_Custom.IsChecked.GetValueOrDefault() ? CustomPreferredVideoCodec.Text : "";
        }

        private void App_Resuming(object sender, object e)
        {
            RestoreSettings();
        }

        /// <summary>
        /// Check if this application instance is the first one launched on the host device.
        /// </summary>
        /// <returns><c>true</c> if the current application instance is the first and therefore only instance.</returns>
        private static bool IsFirstInstance()
        {
            var firstInstance = AppInstance.FindOrRegisterInstanceForKey("{44CD414E-B604-482E-8CFD-A9E09076CABD}");
            return firstInstance.IsCurrentInstance;
        }

        private void RestoreSettings()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            SessionModel sessionModel = SessionModel.Current;

            // Uncomment these lines if you want to connect a HoloLens (or any non-x64 device) to a
            // x64 PC.
            //var arch = System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
            //if (arch == "AMD64")
            //{
            //    sessionModel.NodeDssSignaler.LocalPeerId = "Pc";
            //    sessionModel.NodeDssSignaler.RemotePeerId = "Device";
            //}
            //else
            //{
            //    sessionModel.NodeDssSignaler.LocalPeerId = "Device";
            //    sessionModel.NodeDssSignaler.RemotePeerId = "Pc";
            //}

            // Get server address and peer ID from local settings if available.
            if (localSettings.Values.TryGetValue("DssServerAddress", out object dssServerAddress))
            {
                if (dssServerAddress is string str)
                {
                    sessionModel.NodeDssSignaler.HttpServerAddress = str;
                }
            }
            if (string.IsNullOrWhiteSpace(sessionModel.NodeDssSignaler.HttpServerAddress))
            {
                sessionModel.NodeDssSignaler.HttpServerAddress = "http://localhost:3000/";
            }

            if (localSettings.Values.TryGetValue("LocalPeerID", out object localObj))
            {
                if (localObj is string str)
                {
                    sessionModel.NodeDssSignaler.LocalPeerId = str;
                }
            }
            if (string.IsNullOrWhiteSpace(sessionModel.NodeDssSignaler.LocalPeerId))
            {
                sessionModel.NodeDssSignaler.LocalPeerId = GetDeviceName();
            }
            if (localSettings.Values.TryGetValue("RemotePeerID", out object remoteObj))
            {
                if (remoteObj is string str)
                {
                    sessionModel.NodeDssSignaler.RemotePeerId = str;
                }
            }
            if (localSettings.Values.TryGetValue("PollTimeMs", out object pollTimeObject))
            {
                if (pollTimeObject is int pollTimeMs)
                {
                    sessionModel.NodeDssSignaler.PollTimeMs = pollTimeMs;
                }
            }

            if (!IsFirstInstance())
            {
                // Swap the peer IDs. This way two instances launched on the same machine connect
                // to each other by default
                var tmp = sessionModel.NodeDssSignaler.LocalPeerId;
                sessionModel.NodeDssSignaler.LocalPeerId = sessionModel.NodeDssSignaler.RemotePeerId;
                sessionModel.NodeDssSignaler.RemotePeerId = tmp;
            }

            //if (localSettings.Values.TryGetValue("PreferredAudioCodec", out object preferredAudioObj))
            //{
            //    if (preferredAudioObj is string str)
            //    {
            //        switch (str)
            //        {
            //        case "":
            //        {
            //            PreferredAudioCodec_Default.IsChecked = true;
            //            break;
            //        }
            //        case "opus":
            //        {
            //            PreferredAudioCodec_OPUS.IsChecked = true;
            //            break;
            //        }
            //        default:
            //        {
            //            PreferredAudioCodec_Custom.IsChecked = true;
            //            CustomPreferredAudioCodec.Text = str;
            //            break;
            //        }
            //        }
            //    }
            //}
            //if (localSettings.Values.TryGetValue("PreferredAudioCodecExtraParamsLocal", out object preferredAudioParamsLocalObj))
            //{
            //    if (preferredAudioParamsLocalObj is string str)
            //    {
            //        PreferredAudioCodecExtraParamsLocalTextBox.Text = str;
            //    }
            //}
            //if (localSettings.Values.TryGetValue("PreferredAudioCodecExtraParamsRemote", out object preferredAudioParamsRemoteObj))
            //{
            //    if (preferredAudioParamsRemoteObj is string str)
            //    {
            //        PreferredAudioCodecExtraParamsRemoteTextBox.Text = str;
            //    }
            //}

            //if (localSettings.Values.TryGetValue("PreferredVideoCodec", out object preferredVideoObj))
            //{
            //    if (preferredVideoObj is string str)
            //    {
            //        switch (str)
            //        {
            //        case "":
            //        {
            //            PreferredVideoCodec_Default.IsChecked = true;
            //            break;
            //        }
            //        case "H264":
            //        {
            //            PreferredVideoCodec_H264.IsChecked = true;
            //            break;
            //        }
            //        case "VP8":
            //        {
            //            PreferredVideoCodec_VP8.IsChecked = true;
            //            break;
            //        }
            //        default:
            //        {
            //            PreferredVideoCodec_Custom.IsChecked = true;
            //            CustomPreferredVideoCodec.Text = str;
            //            break;
            //        }
            //        }
            //    }
            //}
            //if (localSettings.Values.TryGetValue("PreferredVideoCodecExtraParamsLocal", out object preferredVideoParamsLocalObj))
            //{
            //    if (preferredVideoParamsLocalObj is string str)
            //    {
            //        PreferredVideoCodecExtraParamsLocalTextBox.Text = str;
            //    }
            //}
            //if (localSettings.Values.TryGetValue("PreferredVideoCodecExtraParamsRemote", out object preferredVideoParamsRemoteObj))
            //{
            //    if (preferredVideoParamsRemoteObj is string str)
            //    {
            //        PreferredVideoCodecExtraParamsRemoteTextBox.Text = str;
            //    }
            //}
        }

        //private void OnPeerRenegotiationNeeded()
        //{
        //    RunOnMainThread(() => negotiationStatusText.Text = "Renegotiation needed");
        //}

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // This should move to the App, no need to wait for the main page loaded...
            await SessionModel.Current.InitializePeerConnectionAsync();

            // Automatically start polling for convenience
            SessionModel.Current.NodeDssSignaler.StartPollingAsync();

            //audioTrackComboBox.IsEnabled = false;
            //videoTrackComboBox.IsEnabled = false;

            // Populate the combo box with the VideoProfileKind enum
            {
                var values = Enum.GetValues(typeof(VideoProfileKind));
                //KnownVideoProfileKindComboBox.ItemsSource = values.Cast<VideoProfileKind>();
                //KnownVideoProfileKindComboBox.SelectedIndex = Array.IndexOf(values, VideoProfileKind.Unspecified);
            }

            //VideoCaptureDeviceList.SelectionChanged += VideoCaptureDeviceList_SelectionChanged;
            //KnownVideoProfileKindComboBox.SelectionChanged += KnownVideoProfileKindComboBox_SelectionChanged;
            //VideoProfileComboBox.SelectionChanged += VideoProfileComboBox_SelectionChanged;

            //videoPlayerElement.TransportControls = localVideoControls;



            //using (_sessionViewModel.GetNegotiationDeferral())
            //{
            //    // As a convenience, add 1 audio and 1 video transceivers
            //    // TODO - make that more flexible
            //    AddPendingTransceiver(MediaKind.Audio, "audio_transceiver_0");
            //    AddPendingTransceiver(MediaKind.Video, "video_transceiver_1");

            //    // It is CRUCIAL to add any data channel BEFORE the SDP offer is sent, if data channels are
            //    // to be used at all. Otherwise the SCTP will not be negotiated, and then all channels will
            //    // stay forever in the kConnecting state.
            //    // https://stackoverflow.com/questions/43788872/how-are-data-channels-negotiated-between-two-peers-with-webrtc
            //    await _peerConnection.AddDataChannelAsync(ChatChannelID, "chat", true, true);
            //}

            //chatInputBox.IsEnabled = true;
            //chatSendButton.IsEnabled = true;

            //_videoPlayer.CurrentStateChanged += OnMediaStateChanged;
            //_videoPlayer.MediaOpened += OnMediaOpened;
            //_videoPlayer.MediaFailed += OnMediaFailed;
            //_videoPlayer.MediaEnded += OnMediaEnded;
            //_videoPlayer.RealTimePlayback = true;
            //_videoPlayer.AutoPlay = false;

            // Bind the XAML UI control (videoPlayerElement) to the MediaFoundation rendering pipeline (_videoPlayer)
            // so that the former can render in the UI the video frames produced in the background by the latter.
            //videoPlayerElement.SetMediaPlayer(_videoPlayer);
        }

        //private void ChatList_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    if (e.ClickedItem is ChatChannelModel chat)
        //    {
        //        chatTextBox.Text = chat.FullText;
        //    }
        //}

        ///// <summary>
        ///// Callback on Send button from text chat clicker.
        ///// If connected, this sends the text message to the remote peer using
        ///// the previously opened data channel.
        ///// </summary>
        ///// <param name="sender">The object which invoked the event.</param>
        ///// <param name="e">Event arguments.</param>
        //private void ChatSendButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (string.IsNullOrWhiteSpace(chatInputBox.Text))
        //        return;

        //    var chat = SelectedChatChannel;
        //    if (chat == null)
        //        return;

        //    // Send the message through the data channel
        //    byte[] chatMessage = System.Text.Encoding.UTF8.GetBytes(chatInputBox.Text);
        //    chat.DataChannel.SendMessage(chatMessage);

        //    // Save and display in the UI
        //    var newLine = $"[local] {chatInputBox.Text}\n";
        //    chat.Text += newLine;
        //    chatTextBox.Text = chat.Text; // reassign or append? not sure...
        //    chatScrollViewer.ChangeView(chatScrollViewer.HorizontalOffset,
        //        chatScrollViewer.ScrollableHeight,
        //        chatScrollViewer.ZoomFactor); // scroll to end
        //    chatInputBox.Text = string.Empty;
        //}

        ///// <summary>
        ///// Callback on key down event invoked in the chat window, to handle
        ///// the "press Enter to send" text chat functionality.
        ///// </summary>
        ///// <param name="sender">The object which invoked the event.</param>
        ///// <param name="e">Event arguments.</param>
        //private void OnChatKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        //{
        //    if (e.Key == Windows.System.VirtualKey.Enter)
        //    {
        //        ChatSendButton_Click(this, null);
        //    }
        //}

        private void OnNavigationViewItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                rootFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                // TODO: use e.g. args.InvokedItemContainer.Tag to avoid string matching and be more reliable?
                foreach (var catBase in Categories)
                {
                    if (catBase is Category cat)
                    {
                        if (cat.PageType == null)
                        {
                            continue;
                        }
                        if ((args.InvokedItem as string) == cat.Name)
                        {
                            rootFrame.Navigate(cat.PageType);
                        }
                    }
                }
            }
        }
    }
}
