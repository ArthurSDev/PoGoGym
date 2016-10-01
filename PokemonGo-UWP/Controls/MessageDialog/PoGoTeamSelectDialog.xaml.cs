﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Template10.Common;
using Template10.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace PokemonGo_UWP.Controls
{
    public enum PoGoTeamSelectionDialogAnimation
    {
        None,
        Bottom,
        Fade
    }

    public sealed partial class PoGoTeamSelectionDialog : UserControl
    {
        public PoGoTeamSelectionDialog()
        {
            this.InitializeComponent();

            YellowButton.Click += TerminalButtonClick;
            BlueButton.Click += TerminalButtonClick;
            RedButton.Click += TerminalButtonClick;
            CancelButton.Click += TerminalButtonClick;
        }

        public void Show()
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                if (modal == null)
                {
                    return;
                }

                _formerModalBrush = modal.ModalBackground;
                modal.ModalBackground = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
                modal.ModalContent = this;
                modal.IsModal = true;

                // Subscribe to keyboard events
                InputPane.GetForCurrentView().Showing += OnKeyboardShowing;
                InputPane.GetForCurrentView().Hiding += OnKeyboardHiding;

                // Start the animation
                if (AnimationType == PoGoMessageDialogAnimation.Bottom)
                {
                    // preconditions
                    double Ytranslation = (modal.ActualHeight / 2) + 300;
                    DownwardsTranslationRange = Ytranslation;
                    DownwardsTranslation.Y = Ytranslation;

                    // animate
                    Storyboard sb = this.Resources["ShowMDBottomStoryboard"] as Storyboard;
                    sb.Begin();
                    sb.Completed += AppearCompleted;
                }
                else if (AnimationType == PoGoMessageDialogAnimation.Fade)
                {
                    // animate
                    Storyboard sb = this.Resources["ShowMDFadeStoryboard"] as Storyboard;
                    sb.Begin();
                    sb.Completed += AppearCompleted;
                }
            });
        }

        #region Propertys

        // Internal
        private bool _keyboardVisible = false;
        private Brush _formerModalBrush = null;

        public static readonly DependencyProperty DownwardsTranslationRangeProperty =
            DependencyProperty.Register(nameof(DownwardsTranslationRange), typeof(Double?), typeof(PoGoMessageDialog),
                new PropertyMetadata(0.0));

        public Double? DownwardsTranslationRange
        {
            get { return (Double?)GetValue(DownwardsTranslationRangeProperty); }
            set { SetValue(DownwardsTranslationRangeProperty, value); }
        }

        // Public

        public static readonly DependencyProperty DialogContentProperty =
            DependencyProperty.Register(nameof(DialogContent), typeof(Object), typeof(PoGoMessageDialog),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CoverBackgroundProperty =
            DependencyProperty.Register(nameof(CoverBackground), typeof(bool), typeof(PoGoMessageDialog), new PropertyMetadata(false));

        public Object DialogContent
        {
            get { return (Object)GetValue(DialogContentProperty); }
            set { SetValue(DialogContentProperty, value); }
        }

        public bool CoverBackground
        {
            get { return (bool)GetValue(CoverBackgroundProperty); }
            set {
                if(value)
                {
                    CoverGrid.Background = new SolidColorBrush((Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), "#55000000"));
                } else
                {
                    CoverGrid.Background = new SolidColorBrush((Color)XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), "Transparent"));
                }
                SetValue(CoverBackgroundProperty, value);
            }
        }

        private bool _backgroundTapInvokesCancel;
        public bool BackgroundTapInvokesCancel
        {
            get { return _backgroundTapInvokesCancel; }
            set { _backgroundTapInvokesCancel = value; }
        }

        public PoGoMessageDialogAnimation AnimationType { get; set; }

        #endregion

        #region Events

        public event RoutedEventHandler YellowInvoked
        {
            add { YellowButton.Click += value; }
            remove { YellowButton.Click -= value; }
        }

        public event RoutedEventHandler BlueInvoked
        {
            add { BlueButton.Click += value; }
            remove { BlueButton.Click -= value; }
        }

        public event RoutedEventHandler RedInvoked
        {
            add { RedButton.Click += value; }
            remove { RedButton.Click -= value; }
        }

        public event RoutedEventHandler CancelInvoked
        {
            add { CancelButton.Click += value; }
            remove { CancelButton.Click -= value; }
        }

        public event EventHandler<object> AppearCompleted;
        public event EventHandler<object> DisappearCompleted;

        private void Background_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if(_backgroundTapInvokesCancel && !_keyboardVisible)
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(CancelButton);
                IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProv.Invoke();
            }
        }

        private void TerminalButtonClick(object sender, RoutedEventArgs e)
        {
            WindowWrapper.Current().Dispatcher.Dispatch(() =>
            {
                var modal = Window.Current.Content as ModalDialog;
                if (modal == null)
                {
                    return;
                }

                // Start the animation
                if (AnimationType == PoGoMessageDialogAnimation.Bottom)
                {
                    // preconditions
                    double Ytranslation = (modal.ActualHeight / 2) + 300;
                    DownwardsTranslationRange = Ytranslation;
                    DownwardsTranslation.Y = Ytranslation;

                    // animate
                    Storyboard sb = this.Resources["HideMDBottomStoryboard"] as Storyboard;
                    sb.Begin();
                    sb.Completed += Cleanup;
                    sb.Completed += DisappearCompleted;
                }
                else if (AnimationType == PoGoMessageDialogAnimation.Fade)
                {
                    // animate
                    Storyboard sb = this.Resources["HideMDFadeStoryboard"] as Storyboard;
                    sb.Begin();
                    sb.Completed += Cleanup;
                    sb.Completed += DisappearCompleted;
                } else
                {
                    Cleanup(this, this);
                }
            });
        }

        private void OnKeyboardShowing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            _keyboardVisible = true;
        }

        private void OnKeyboardHiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            _keyboardVisible = false;
        }

        private void Cleanup(object sender, object e)
        {
            // Unsubscribe from keyboard events
            InputPane.GetForCurrentView().Showing -= OnKeyboardShowing;
            InputPane.GetForCurrentView().Hiding -= OnKeyboardHiding;

            var modal = Window.Current.Content as ModalDialog;
            if (modal == null)
            {
                return;
            }

            modal.ModalBackground = _formerModalBrush;
            modal.ModalContent = null;
            modal.IsModal = false;
        }

        #endregion
    }
}