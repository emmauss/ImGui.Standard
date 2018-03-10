using ImGuiOpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ImGuiCS.Demo
{
    static class Program {

        static OpenTKWindow Instance;

        [STAThread]
        static void Main(string[] args) {
            Queue<string> argq = new Queue<string>(args);
            while (argq.Count > 0) {
                string arg = argq.Dequeue();
                if (arg == "--debug")
                    Debugger.Launch();
            }

            /*ImGuiOpenTKCSWindow*/ Instance = new YourGameWindow();
            Instance.Run();
            Instance.Dispose();
        }

    }
}
