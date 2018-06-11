using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ImGuiNET;
using ImGuiNETWidget;
using ImGuiOpenTK;

namespace CoreDemo
{
    public class YourGameWindow : ImGuiOpenTKWindow {

        //private ImGuiNETWidget.TextInputBuffer[] _TextInputBuffers;

        private MemoryEditor _MemoryEditor = new MemoryEditor();
        private byte[] _MemoryEditorData;

        private FileDialog _Dialog = new FileDialog(false, false, true, false, false, false);

        public YourGameWindow()
            : base("Your Game Window Title") {

            // Create any managed resources and set up the main game window here.
            _MemoryEditorData = new byte[1024];
            Random rnd = new Random();
            for (int i = 0; i < _MemoryEditorData.Length; i++) {
                _MemoryEditorData[i] = (byte) rnd.Next(255);
            }

            //////// OPTIONAL ////////
            // This affects the "underlying" SDL2Window and can be used for a quick game loop sketch.
            // Don't set those and only ImGui gets rendered / handled.
            // They're delegate fields so that one can override those from outside.

        }

        // Create any possibly unmanaged resources (textures, buffers) here.
        protected override void Create() {
            base.Create();

          //  _TextInputBuffers = ImGuiNETWidget.TextInputBuffer.CreateBuffers(2);
        }

        // Dispose any possibly unmanaged resources (textures, buffers) here.
        protected override void Dispose(bool disposing) {
           // ImGuiNETWidget.TextInputBuffer.DisposeBuffers(_TextInputBuffers);

            base.Dispose(disposing);
        }

        // This runs between ImGuiSDL2CSHelper.NewFrame and ImGuiSDL2CSHelper.Render.
        // Direct port of the example at https://github.com/ocornut/imgui/blob/master/examples/sdl_opengl2_example/main.cpp
        float f = 0.0f;
        bool show_test_window = true;
        bool show_another_window = false;
        bool show_file_dialog;
        System.Numerics.Vector3 clear_color = new System.Numerics.Vector3(114f/255f, 144f/255f, 154f/255f);
        public unsafe override void ImGuiLayout() {
            // 1. Show a simple window
            // Tip: if we don't call ImGui.Begin()/ImGui.End() the widgets appears in a window automatically called "Debug"
            {
                var io = ImGui.GetIO();
                ImGui.BeginWindow("Debug");
                ImGui.Text("Hello, world!");
                ImGui.SliderFloat("float", ref f, 0.0f, 1.0f, null, 1f);
                ImGui.ColorEdit3("clear color", ref clear_color, ColorEditFlags.Default);
                if (ImGui.Button("Test Window")) show_test_window = !show_test_window;
                if (ImGui.Button("Another Window")) show_another_window = !show_another_window;
                ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)",ImGui.GetIO().DeltaTime, 1f/ImGui.GetIO().DeltaTime));
                //ImGui.InputText("Text Input 1", _TextInputBuffers[0], _TextInputBuffers[0].Length,InputTextFlags.Default,new TextEditCallback();
                //ImGui.InputText("Text Input 2", _TextInputBuffers[1].Buffer, _TextInputBuffers[1].Length);
                if (ImGui.Button("Open File")) show_file_dialog = !show_file_dialog;
                ImGui.EndWindow();
            }

            // 2. Show another simple window, this time using an explicit Begin/End pair
            if (show_another_window) {
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 100), Condition.FirstUseEver);
                ImGui.BeginChild("Another Window", show_another_window);
                ImGui.Text("Hello");
                ImGui.EndChild();
            }

            // 3. Show the ImGui test window. Most of the sample code is in ImGui.ShowTestWindow()
            if (show_test_window) {
                ImGui.SetNextWindowPos(new System.Numerics.Vector2(650, 20), Condition.FirstUseEver,new System.Numerics.Vector2(0,0));
                ImGuiNative.igShowDemoWindow(ref show_test_window);
            }

            // 4. Show the memory editor and file dialog, just as an example.
            _MemoryEditor.Draw("Memory editor", _MemoryEditorData, _MemoryEditorData.Length);
            if (show_file_dialog) {
                string start = _Dialog.LastDirectory;
                _Dialog.ChooseFileDialog(true, _Dialog.LastDirectory, null, "Choose File", new System.Numerics.Vector2(500, 500), new System.Numerics.Vector2(50, 50), 1f);
               /* if (!string.IsNullOrEmpty(_Dialog.ChosenPath))
                    _TextInputBuffers[0].StringValue = _Dialog.ChosenPath;*/
            }
        }

        //////// OPTIONAL ////////

        // Processs any SDL2 events manually if required.
        // Return false to not allow the default event handler to process it.
        public bool MyEventHandler(OpenTKWindow _self, TKEvent e) {
            // We're replacing OnEvent and thus call ImGuiSDL2CSHelper.OnEvent manually.
            if (!ImGuiOpenTKHelper.HandleEvent(e))
                return false;

            // Any custom event handling can happen here.

            return true;
        }

        // Any custom game loop should end up here.
        // Setting the window.IsActive = false stops the loop.
        public void MyGameLoop(OpenTKWindow _self) {
            // This is the default implementation, except for the ClearColor not being 0.1f, 0.125f, 0.15f, 1f

            // Using minimal ImGuiSDL2CS.GL to provide access to OpenGL methods.
            // Alternatively, use SDL_GL_GetProcAddress on your own.
            GL.ClearColor(clear_color.X, clear_color.Y, clear_color.Z, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // This calls ImGuiSDL2CSHelper.NewFrame, the overridden ImGuiLayout, ImGuiSDL2CSHelper.Render and renders it.
            // ImGuiSDL2CSHelper.NewFrame properly sets up ImGuiIO and ImGuiSDL2CSHelper.Render renders the draw data.
            ImGuiRender();

            // Finally, swap.
            Swap();
        }

    }
}
