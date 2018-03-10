using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ImGuiOpenTK
{
    public class OpenTKWindow : GameWindow {
        
        protected IntPtr _GLContext;
        public IntPtr GLContext => _GLContext;

        //public GameWindowFlags Flags =>;

        public Action<OpenTKWindow> OnLoop;
        public Func<OpenTKWindow, TKEvent, bool> OnEvent;
        public bool IsAlive = false;

        public OpenTKWindow(
            string title = "OpenTK Window",
            int width = 800, int height = 600
            ) : base(width,height,GraphicsMode.Default,title,GameWindowFlags.Default) {
            Keyboard.KeyDown += Keyboard_KeyDown;
            Keyboard.KeyUp += Keyboard_KeyUp;
            Mouse.ButtonDown += Mouse_ButtonDown;
            Mouse.ButtonUp += Mouse_ButtonUp;
        }

        private void Mouse_ButtonUp(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            MouseButtonEvent mouseButtonEvent = new MouseButtonEvent(false, e.Button);
            OnEvent(this, mouseButtonEvent);
        }

        private void Mouse_ButtonDown(object sender, OpenTK.Input.MouseButtonEventArgs e)
        {
            MouseButtonEvent mouseButtonEvent = new MouseButtonEvent(true, e.Button);
            OnEvent(this, mouseButtonEvent);
        }

        private void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            KeyBoardEvent keyBoardEvent = new KeyBoardEvent(false, e.Key);
            OnEvent(this, keyBoardEvent);
        }

        private void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            KeyBoardEvent keyBoardEvent = new KeyBoardEvent(true, e.Key);
            OnEvent(this, keyBoardEvent);
        }

        public bool IsVisible =>Visible;
        public void Show() =>Visible = true;
        public void Hide() =>Visible = false;

        public virtual void Start() {
            Show();
            Run(60, 60);
        }

        protected override void OnLoad(EventArgs e)
        {
            IsAlive = true;
            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            IsAlive = false;
            base.OnClosing(e);
        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            OnLoop.Invoke(this);
            base.OnRenderFrame(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
        }

        public virtual void Swap() =>SwapBuffers();

        protected override void Dispose(bool manual)
        {
            base.Dispose(manual);

        }

        ~OpenTKWindow() {
            Dispose(false);
        }
        
        public override void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
