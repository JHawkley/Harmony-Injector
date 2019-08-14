using System;

using XRLCore = XRL.Core.XRLCore;
using XRLPopup = XRL.UI.Popup;
using TextBlock = XRL.UI.TextBlock;
using static HarmonyInjector.Tools;
using static HarmonyInjector.Constants;

namespace HarmonyInjector
{

    /// <summary>
    /// Provides basic support to display popups, especially in the main-menu and from
    /// the UI-thread.  If being called from the game-thread while the game is running,
    /// the normal popup infrastructure will be used automatically.
    /// </summary>
    public static class Popup
    {

        /// <summary>
        /// Whether a popup is showing or is queued to show.
        /// An exception is thrown if a request to show a popup occurs while one is already showing.
        /// </summary>
        public static bool Showing => showPending || ViewManager.ActiveView?.Name.StartsWith("Popup:") == true;

        /// <summary>
        /// Whether the game-state is in a place where a popup can be shown to the player properly.
        /// </summary>
        public static bool CanDoPopup
        {
            get
            {
                if (Showing) return false;
                if (UseRegularPopups) return true;
                if (GameManager.OverlayUIEnabled) return true;
                return false;
            }
        }

        private static global::GameManager GameManager => global::GameManager.Instance;

        private static QupKit.ViewManager ViewManager => QupKit.ViewManager.Instance;
        
        private static bool UseRegularPopups
        {
            get
            {
                // If we're on the game-thread and the game is running, we should be
                // able to show a dialog through the normal means fine.
                if (!OnGameThread) return false;
                if (GameManager.CurrentGameView != "Game") return false;
                return XRLCore.Core?.Game?.Running == true;
            }
        }

        /// <summary>
        /// Shows a simple dialog with an `Ok` button to the player.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="onOk">The action to execute when the dialog is dismissed.</param>
        /// <exception cref="InvalidOperationException">
        /// When `Showing` is `true`; another popup is already being shown.
        /// </exception>
        public static void ShowMessage(string message, Action onOk = null)
        {
            if (Showing)
                throw new InvalidOperationException("Another popup is already being shown.");
            
            if (onOk == null) onOk = NoopFn;

            if (UseRegularPopups)
            {
                XRLPopup.Show(message);
                onOk();
            }
            else if (GameManager.OverlayUIEnabled)
            {
                // If on the UI-thread, then we have to manage the popup manually.
                DoShow(() => {
                    var previousView = ViewManager.ActiveView?.Name;
                    Popup_MessageBox.Buttons = 1;
                    Popup_MessageBox.Default = 1;
                    Popup_MessageBox.Button1 = "Ok";
                    Popup_MessageBox.Text = new TextBlock(message, 80, 5000, false).GetRTFBlock();
                    Popup_MessageBox.Title = String.Empty;
                    Popup_MessageBox.action1 = HandleDismiss(onOk, previousView);
                    return "Popup:MessageBox";
                });
            }
            else onOk();
        }

        private static void DoShow(Func<string> configurePopup)
        {
            showPending = true;

            // Defer to next UI loop.
            QueueUITask(() => {
                // The `WaitFor` is for popups created while `XRLCore` is still initializing.
                // This ensures the popup shows only after the main-menu is completely setup.
                WaitFor(() => GameIsReady, () => {
                    // Show the popup...
                    ViewManager.SetActiveView(configurePopup(), bHideOldView: false);
                    showPending = false;
                    // ...and block the game-thread while it is shown.
                    QueueGameTask(() => { BlockWhile(() => Showing); });
                });
            });
        }

        private static Action HandleDismiss(Action action, string previousView) => () => {
            // The action can be called more than once under certain circumstances.
            // Got to prevent re-running if it has already been dismissed.
            if (!Showing) return;
            ViewManager.SetActiveView(previousView);
            action();
        };

        private static bool showPending = false;

        /// <summary>
        /// Provides access to fields on the internal `Popup_MessageBox` class.
        /// </summary>
        private static class Popup_MessageBox
        {

            public static int Buttons
            {
                get { return (int)GetField(nameof(Buttons)); }
                set { SetField(nameof(Buttons), value); }
            }

            public static int Default
            {
                get { return (int)GetField(nameof(Default)); }
                set { SetField(nameof(Default), value); }
            }

            public static string Button1
            {
                get { return (string)GetField(nameof(Button1)); }
                set { SetField(nameof(Button1), value); }
            }

            public static string Text
            {
                get { return (string)GetField(nameof(Text)); }
                set { SetField(nameof(Text), value); }
            }

            public static string Title
            {
                get { return (string)GetField(nameof(Title)); }
                set { SetField(nameof(Title), value); }
            }

            public static Action action1
            {
                get { return (Action)GetField(nameof(action1)); }
                set { SetField(nameof(action1), value); }
            }

            private static object GetField(string name) { return type.GetField(name).GetValue(null); }
            private static void SetField(string name, object value) { type.GetField(name).SetValue(null, value); }
            private static readonly Type type = Injector.GetUnityType("Popup_MessageBox");

        }

    }
}