using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImGuiNET;
using TKEventType = ImGuiOpenTK.TKEvent.Type;
using System.IO;
using System.Runtime.InteropServices;

namespace ImGuiOpenTK{
    public static class ImGuiOpenTKHelper {

        private static bool _Initialized = false;
        public static bool Initialized => _Initialized;

        public static void Init() {
            if (_Initialized)
                return;
            _Initialized = true;

            ImGuiIO io = ImGui.GetIO();
            io.KeyMap[ImGuiKey.Tab] = (int)Key.Tab;
            io.KeyMap[ImGuiKey.LeftArrow] = (int)Key.Left;
            io.KeyMap[ImGuiKey.RightArrow] = (int)Key.Right;
            io.KeyMap[ImGuiKey.UpArrow] = (int)Key.Up;
            io.KeyMap[ImGuiKey.DownArrow] = (int)Key.Down;
            io.KeyMap[ImGuiKey.PageUp] = (int)Key.PageUp;
            io.KeyMap[ImGuiKey.PageDown] = (int)Key.Down;
            io.KeyMap[ImGuiKey.Home] = (int)Key.Home;
            io.KeyMap[ImGuiKey.End] = (int)Key.End;
            io.KeyMap[ImGuiKey.Delete] = (int)Key.Delete;
            io.KeyMap[ImGuiKey.Backspace] = (int)Key.BackSpace;
            io.KeyMap[ImGuiKey.Enter] = (int)Key.Enter;
            io.KeyMap[ImGuiKey.Escape] = (int)Key.Escape;
            io.KeyMap[ImGuiKey.A] = (int)Key.A;
            io.KeyMap[ImGuiKey.C] = (int)Key.C;
            io.KeyMap[ImGuiKey.V] = (int)Key.V;
            io.KeyMap[ImGuiKey.X] = (int)Key.X;
            io.KeyMap[ImGuiKey.Y] = (int)Key.Y;
            io.KeyMap[ImGuiKey.Z] = (int)Key.Z;
            /*
            io.SetGetClipboardTextFn((userData) => );
            io.SetSetClipboardTextFn((userData, text) => SDL.SDL_SetClipboardText(text));*/

            // If no font added, add default font.
            if (io.FontAtlas.Fonts.Size == 0)
                io.FontAtlas.AddDefaultFont();
        }

        public static void NewFrame(ImVec2 size, ImVec2 scale, ref double g_Time) {
            ImGuiIO io = ImGui.GetIO();
            io.DisplaySize = size;
            io.DisplayFramebufferScale = scale;

            double currentTime = Environment.TickCount / 1000D;
            io.DeltaTime = g_Time > 0D ? (float) (currentTime - g_Time) : (1f/60f);
            g_Time = currentTime;

            //SDL.SDL_ShowCursor(io.MouseDrawCursor ? 0 : 1);

            ImGui.NewFrame();
        }

        public static void Render(ImVec2 size) {
            ImGui.Render();
            if (ImGui.IO.RenderDrawListsFn == IntPtr.Zero)
                RenderDrawData(ImGui.GetDrawData(), (int) Math.Round(size.X), (int) Math.Round(size.Y));
        }
        
        public unsafe static void RenderDrawData(ImDrawData drawData, int displayW, int displayH) {
            // We are using the OpenGL fixed pipeline to make the example code simpler to read!
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers.
            int lastTexture;
            GL.GetInteger(GetPName.TextureBinding2D, out lastTexture);
            Int4 lastViewport = new Int4();
            TypedReference tr = __makeref(lastViewport);
            IntPtr ptr = **(IntPtr**)(&tr);
            GL.GetInteger(GetPName.Viewport,(int*)ptr);
            Int4 lastScissorBox = new Int4();
            tr = __makeref(lastScissorBox);
            ptr = **(IntPtr**)(&tr);
            GL.GetInteger(GetPName.ScissorBox, (int*)ptr);

            GL.PushAttrib(AttribMask.EnableBit | AttribMask.ColorBufferBit | AttribMask.TransformBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);

            GL.UseProgram(0);

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            ImGuiIO io = ImGui.GetIO();
            ImGui.ScaleClipRects(drawData, io.DisplayFramebufferScale);

            // Setup orthographic projection matrix
            GL.Viewport(0, 0, displayW, displayH);
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(
                0.0f,
                io.DisplaySize.X / io.DisplayFramebufferScale.X,
                io.DisplaySize.Y / io.DisplayFramebufferScale.Y,
                0.0f,
                -1.0f,
                1.0f
            );
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Render command lists

            for (int n = 0; n < drawData.CmdListsCount; n++) {
                ImDrawList cmdList = drawData[n];
                ImVector<ImDrawVert> vtxBuffer = cmdList.VtxBuffer;
                ImVector<ushort> idxBuffer = cmdList.IdxBuffer;

                GL.VertexPointer(2, VertexPointerType.Float, ImDrawVert.Size,
                    new IntPtr((long) vtxBuffer.Data + ImDrawVert.PosOffset));
                GL.TexCoordPointer(2,TexCoordPointerType.Float, ImDrawVert.Size, 
                    new IntPtr((long) vtxBuffer.Data + ImDrawVert.UVOffset));
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, ImDrawVert.Size, 
                    new IntPtr((long) vtxBuffer.Data + ImDrawVert.ColOffset));

                long idxBufferOffset = 0;
                for (int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++) {
                    ImDrawCmd pcmd = cmdList.CmdBuffer[cmdi];
                    if (pcmd.UserCallback != IntPtr.Zero) {
                        pcmd.InvokeUserCallback(ref cmdList, ref pcmd);
                    } else {
                        GL.BindTexture(TextureTarget.Texture2D, (int) pcmd.TextureId);
                        GL.Scissor(
                            (int) pcmd.ClipRect.X,
                            (int) (io.DisplaySize.Y - pcmd.ClipRect.W),
                            (int) (pcmd.ClipRect.Z - pcmd.ClipRect.X),
                            (int) (pcmd.ClipRect.W - pcmd.ClipRect.Y)
                        );
                        GL.DrawElements(PrimitiveType.Triangles, (int) pcmd.ElemCount,DrawElementsType.UnsignedByte, 
                            new IntPtr((long) idxBuffer.Data + idxBufferOffset));
                    }
                    idxBufferOffset += pcmd.ElemCount * 2 /*sizeof(ushort)*/;
                }
            }

            // Restore modified state
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.BindTexture(TextureTarget.Texture2D, lastTexture);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.PopAttrib();
            GL.Viewport(lastViewport.X, lastViewport.Y, lastViewport.Z, lastViewport.W);
            GL.Scissor(lastScissorBox.X, lastScissorBox.Y, lastScissorBox.Z, lastScissorBox.W);
        }

        public static bool HandleEvent(TKEvent tKEvent) {
            ImGuiIO io = ImGui.GetIO();
            switch (tKEvent.EventType)
            {
                case TKEventType.Keyboard:
                    var KeyboardEvent = tKEvent as KeyBoardEvent;
                    switch (KeyboardEvent.Key) {
                        case Key.ControlLeft:
                        case Key.ControlRight:
                            io.CtrlPressed = KeyboardEvent.IsKeyDown;
                            break;
                        case Key.ShiftLeft:
                        case Key.ShiftRight:
                            io.ShiftPressed = KeyboardEvent.IsKeyDown;
                            break;
                        case Key.AltLeft:
                        case Key.AltRight:
                            io.AltPressed = KeyboardEvent.IsKeyDown;
                            break;
                        case Key.WinLeft:
                        case Key.WinRight:
                            io.SuperPressed = KeyboardEvent.IsKeyDown;
                            break;
                        default:
                            io.KeysDown[(int)KeyboardEvent.Key] = KeyboardEvent.IsKeyDown;
                            break;
                    }
                    break;
                case TKEventType.MouseButton:
                    var MouseButton = tKEvent as MouseButtonEvent;
                    switch (MouseButton.Button)
                    {
                        case OpenTK.Input.MouseButton.Left:
                            io.MouseDown[0] = MouseButton.IsButtonDown;
                            break;
                        case OpenTK.Input.MouseButton.Right:
                            io.MouseDown[1] = MouseButton.IsButtonDown;
                            break;
                        case OpenTK.Input.MouseButton.Middle:
                            io.MouseDown[2] = MouseButton.IsButtonDown;
                            break;
                    }
                    break;
                case TKEventType.MouseWheel:
                    var MouseWheel = tKEvent as MouseWheelEvent;
                    io.MouseWheel = MouseWheel.Value;
                    break;
                case TKEventType.MouseMotion:
                    var MouseMotion = tKEvent as MouseMotionEvent;
                    io.MousePosition = MouseMotion.Position;
                    break;
                case TKEventType.TextInput:
                    var TextEvent = tKEvent as TextInputEvent;
                    unsafe
                    {
                        ImGui.AddInputCharactersUTF8(TextEvent.Text);
                    }
                    break;

            }
            

           /*switch (mouse.GetState) {
                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    if (e.wheel.y > 0)
                        mouseWheel = 1;
                    if (e.wheel.y < 0)
                        mouseWheel = -1;
                    return true;
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    if (mousePressed == null)
                        return true;
                    if (e.button.button == SDL.SDL_BUTTON_LEFT && mousePressed.Length > 0)
                        mousePressed[0] = true;
                    if (e.button.button == SDL.SDL_BUTTON_RIGHT && mousePressed.Length > 1)
                        mousePressed[1] = true;
                    if (e.button.button == SDL.SDL_BUTTON_MIDDLE && mousePressed.Length > 2)
                        mousePressed[2] = true;
                    return true;
                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    unsafe
                    {
                        // THIS IS THE ONLY UNSAFE THING LEFT!
                        ImGui.AddInputCharactersUTF8(e.text.text);
                    }
                    return true;
                case SDL.SDL_EventType.SDL_KEYDOWN:
                case SDL.SDL_EventType.SDL_KEYUP:
                    int key = (int) e.key.keysym.sym & ~SDL.SDLK_SCANCODE_MASK;
                    io.KeysDown[key] = e.type == SDL.SDL_EventType.SDL_KEYDOWN;
                    SDL.SDL_Keymod keyModState = SDL.SDL_GetModState();
                    io.ShiftPressed = (keyModState & SDL.SDL_Keymod.KMOD_SHIFT) != 0;
                    io.CtrlPressed = (keyModState & SDL.SDL_Keymod.KMOD_CTRL) != 0;
                    io.AltPressed = (keyModState & SDL.SDL_Keymod.KMOD_ALT) != 0;
                    io.SuperPressed = (keyModState & SDL.SDL_Keymod.KMOD_GUI) != 0;
                    return true;
            }
            */
            return true;
        }

    }
}
