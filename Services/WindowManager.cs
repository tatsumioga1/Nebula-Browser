using System.Collections.Generic;

namespace Nebula.Services
{
    public static class WindowManager
    {
        public static List<BrowserWindow> OpenWindows { get; }
            = new();
    }
}