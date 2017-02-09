/* Copyright 2017 Cimpress

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. */


using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VP.FF.PT.Common.PlcCommunicationSimulator.Behavior
{
    /// <summary>
    /// Registeres global hotkeys to windows.
    /// Provides an .NET event based interface, but uses an internal thread for a message loop.
    /// </summary>
    public static class HotKeyManager
    {
        private static volatile MessageWindow _wnd;
        private static volatile IntPtr _hwnd;
        private static readonly ManualResetEvent WindowReadyEvent = new ManualResetEvent(false);
        private static int _id;

        private static bool _isInitialized;

        private static void Initialize()
        {
            var messageLoop = new Thread(() => Application.Run(new MessageWindow()))
            {
                Name = "MessageLoopThread",
                IsBackground = true
            };
            messageLoop.Start();

            _isInitialized = true;
        }

        /// <summary>
        /// Occurs when a registered hot key was pressed.
        /// </summary>
        public static event EventHandler<HotKeyEventArgs> HotKeyPressed;

        /// <summary>
        /// Registers a the hot key.
        /// The first call of this method will initilaize the HotKeyManager, if no key was registered it will not create a background thread.
        /// </summary>
        /// <param name="key">The key to register.</param>
        /// <param name="modifiers">The modifiers (e.g. CTRL or ALT key).</param>
        public static int RegisterHotKey(Keys key, KeyModifiers modifiers)
        {
            if (!_isInitialized)
                Initialize();

            WindowReadyEvent.WaitOne();
            int id = Interlocked.Increment(ref _id);
            _wnd.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), _hwnd, id, (uint) modifiers, (uint) key);
            return id;
        }

        public static void UnregisterHotKey(int id)
        {
            _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
        }

        private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key)
        {
            RegisterHotKey(hwnd, id, modifiers, key);
        }

        private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id)
        {
            UnregisterHotKey(_hwnd, id);
        }

        private static void OnHotKeyPressed(HotKeyEventArgs e)
        {
            if (HotKeyPressed != null)
            {
                HotKeyPressed(null, e);
            }
        }

        [DllImport("user32", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private class MessageWindow : Form
        {
            private const int WM_HOTKEY = 0x312;

            public MessageWindow()
            {
                _wnd = this;
                _hwnd = Handle;
                WindowReadyEvent.Set();
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    var e = new HotKeyEventArgs(m.LParam);
                    OnHotKeyPressed(e);
                }

                base.WndProc(ref m);
            }

            protected override void SetVisibleCore(bool value)
            {
                // Ensure the window never becomes visible
                base.SetVisibleCore(false);
            }
        }

        private delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);

        private delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);
    }

    public class HotKeyEventArgs : EventArgs
    {
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;

        public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam)
        {
            var param = (uint) hotKeyParam.ToInt64();
            Key = (Keys) ((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifiers) (param & 0x0000ffff);
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }
}
