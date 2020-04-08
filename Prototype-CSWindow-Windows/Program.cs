using System;
using System.Net.Cache;
using System.Runtime.InteropServices;

namespace Prototype_CSWindow_Windows
{
    class Program
    {
        // From C:\Users\David\src\Avalonia\src\Windows\Avalonia.Win32\Interop\UnmanagedMethods.cs
        
        // const
        public const int CW_USEDEFAULT = unchecked((int)0x80000000);
        public const int PM_NOREMOVE = 0x00000000;

        // enum
        [Flags]
        public enum AttribMask : uint
        {
            GL_CURRENT_BIT = 0x00000001,
            GL_POINT_BIT = 0x00000002,
            GL_LINE_BIT = 0x00000004,
            GL_POLYGON_BIT = 0x00000008,
            GL_POLYGON_STIPPLE_BIT = 0x00000010,
            GL_PIXEL_MODE_BIT = 0x00000020,
            GL_LIGHTING_BIT = 0x00000040,
            GL_FOG_BIT = 0x00000080,
            GL_DEPTH_BUFFER_BIT = 0x00000100,
            GL_ACCUM_BUFFER_BIT = 0x00000200,
            GL_STENCIL_BUFFER_BIT = 0x00000400,
            GL_VIEWPORT_BIT = 0x00000800,
            GL_TRANSFORM_BIT = 0x00001000,
            GL_ENABLE_BIT = 0x00002000,
            GL_COLOR_BUFFER_BIT = 0x00004000,
            GL_HINT_BIT = 0x00008000,
            GL_EVAL_BIT = 0x00010000,
            GL_LIST_BIT = 0x00020000,
            GL_TEXTURE_BIT = 0x00040000,
            GL_SCISSOR_BIT = 0x00080000,
            GL_ALL_ATTRIB_BITS = 0x000fffff,
        }
        [Flags]
        public enum BeginMode : uint
        {
            GL_POINTS = 0x0000,
            GL_LINES = 0x0001,
            GL_LINE_LOOP = 0x0002,
            GL_LINE_STRIP = 0x0003,
            GL_TRIANGLES = 0x0004,
            GL_TRIANGLE_STRIP = 0x0005,
            GL_TRIANGLE_FAN = 0x0006,
            GL_QUADS = 0x0007,
            GL_QUAD_STRIP = 0x0008,
            GL_POLYGON = 0x0009,
        }
        [Flags]
        public enum ClassStyles : uint
        {
            CS_VREDRAW = 0x0001,
            CS_HREDRAW = 0x0002,
            CS_DBLCLKS = 0x0008,
            CS_OWNDC = 0x0020,
            CS_CLASSDC = 0x0040,
            CS_PARENTDC = 0x0080,
            CS_NOCLOSE = 0x0200,
            CS_SAVEBITS = 0x0800,
            CS_BYTEALIGNCLIENT = 0x1000,
            CS_BYTEALIGNWINDOW = 0x2000,
            CS_GLOBALCLASS = 0x4000,
            CS_IME = 0x00010000,
            CS_DROPSHADOW = 0x00020000
        }
        [Flags]
        public enum WindowStyles : uint
        {
            WS_BORDER = 0x800000,
            WS_CAPTION = 0xc00000,
            WS_CHILD = 0x40000000,
            WS_CLIPCHILDREN = 0x2000000,
            WS_CLIPSIBLINGS = 0x4000000,
            WS_DISABLED = 0x8000000,
            WS_DLGFRAME = 0x400000,
            WS_GROUP = 0x20000,
            WS_HSCROLL = 0x100000,
            WS_MAXIMIZE = 0x1000000,
            WS_MAXIMIZEBOX = 0x10000,
            WS_MINIMIZE = 0x20000000,
            WS_MINIMIZEBOX = 0x20000,
            WS_OVERLAPPED = 0x0,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUP = 0x80000000u,
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_SIZEFRAME = 0x40000,
            WS_SYSMENU = 0x80000,
            WS_TABSTOP = 0x10000,
            WS_VISIBLE = 0x10000000,
            WS_VSCROLL = 0x200000,
            WS_EX_DLGMODALFRAME = 0x00000001,
            WS_EX_NOPARENTNOTIFY = 0x00000004,
            WS_EX_TOPMOST = 0x00000008,
            WS_EX_ACCEPTFILES = 0x00000010,
            WS_EX_TRANSPARENT = 0x00000020,
            WS_EX_MDICHILD = 0x00000040,
            WS_EX_TOOLWINDOW = 0x00000080,
            WS_EX_WINDOWEDGE = 0x00000100,
            WS_EX_CLIENTEDGE = 0x00000200,
            WS_EX_CONTEXTHELP = 0x00000400,
            WS_EX_RIGHT = 0x00001000,
            WS_EX_LEFT = 0x00000000,
            WS_EX_RTLREADING = 0x00002000,
            WS_EX_LTRREADING = 0x00000000,
            WS_EX_LEFTSCROLLBAR = 0x00004000,
            WS_EX_RIGHTSCROLLBAR = 0x00000000,
            WS_EX_CONTROLPARENT = 0x00010000,
            WS_EX_STATICEDGE = 0x00020000,
            WS_EX_APPWINDOW = 0x00040000,
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,
            WS_EX_LAYERED = 0x00080000,
            WS_EX_NOINHERITLAYOUT = 0x00100000,
            WS_EX_LAYOUTRTL = 0x00400000,
            WS_EX_COMPOSITED = 0x02000000,
            WS_EX_NOACTIVATE = 0x08000000
        }
        public enum WindowsMessage : uint
        {
            WM_NULL = 0x0000,
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_MOVE = 0x0003,
            WM_SIZE = 0x0005,
            WM_ACTIVATE = 0x0006,
            WM_SETFOCUS = 0x0007,
            WM_KILLFOCUS = 0x0008,
            WM_ENABLE = 0x000A,
            WM_SETREDRAW = 0x000B,
            WM_SETTEXT = 0x000C,
            WM_GETTEXT = 0x000D,
            WM_GETTEXTLENGTH = 0x000E,
            WM_PAINT = 0x000F,
            WM_CLOSE = 0x0010,
            WM_QUERYENDSESSION = 0x0011,
            WM_QUERYOPEN = 0x0013,
            WM_ENDSESSION = 0x0016,
            WM_QUIT = 0x0012,
            WM_ERASEBKGND = 0x0014,
            WM_SYSCOLORCHANGE = 0x0015,
            WM_SHOWWINDOW = 0x0018,
            WM_WININICHANGE = 0x001A,
            WM_SETTINGCHANGE = WM_WININICHANGE,
            WM_DEVMODECHANGE = 0x001B,
            WM_ACTIVATEAPP = 0x001C,
            WM_FONTCHANGE = 0x001D,
            WM_TIMECHANGE = 0x001E,
            WM_CANCELMODE = 0x001F,
            WM_SETCURSOR = 0x0020,
            WM_MOUSEACTIVATE = 0x0021,
            WM_CHILDACTIVATE = 0x0022,
            WM_QUEUESYNC = 0x0023,
            WM_GETMINMAXINFO = 0x0024,
            WM_PAINTICON = 0x0026,
            WM_ICONERASEBKGND = 0x0027,
            WM_NEXTDLGCTL = 0x0028,
            WM_SPOOLERSTATUS = 0x002A,
            WM_DRAWITEM = 0x002B,
            WM_MEASUREITEM = 0x002C,
            WM_DELETEITEM = 0x002D,
            WM_VKEYTOITEM = 0x002E,
            WM_CHARTOITEM = 0x002F,
            WM_SETFONT = 0x0030,
            WM_GETFONT = 0x0031,
            WM_SETHOTKEY = 0x0032,
            WM_GETHOTKEY = 0x0033,
            WM_QUERYDRAGICON = 0x0037,
            WM_COMPAREITEM = 0x0039,
            WM_GETOBJECT = 0x003D,
            WM_COMPACTING = 0x0041,
            WM_WINDOWPOSCHANGING = 0x0046,
            WM_WINDOWPOSCHANGED = 0x0047,
            WM_COPYDATA = 0x004A,
            WM_CANCELJOURNAL = 0x004B,
            WM_NOTIFY = 0x004E,
            WM_INPUTLANGCHANGEREQUEST = 0x0050,
            WM_INPUTLANGCHANGE = 0x0051,
            WM_TCARD = 0x0052,
            WM_HELP = 0x0053,
            WM_USERCHANGED = 0x0054,
            WM_NOTIFYFORMAT = 0x0055,
            WM_CONTEXTMENU = 0x007B,
            WM_STYLECHANGING = 0x007C,
            WM_STYLECHANGED = 0x007D,
            WM_DISPLAYCHANGE = 0x007E,
            WM_GETICON = 0x007F,
            WM_SETICON = 0x0080,
            WM_NCCREATE = 0x0081,
            WM_NCDESTROY = 0x0082,
            WM_NCCALCSIZE = 0x0083,
            WM_NCHITTEST = 0x0084,
            WM_NCPAINT = 0x0085,
            WM_NCACTIVATE = 0x0086,
            WM_GETDLGCODE = 0x0087,
            WM_SYNCPAINT = 0x0088,
            WM_NCMOUSEMOVE = 0x00A0,
            WM_NCLBUTTONDOWN = 0x00A1,
            WM_NCLBUTTONUP = 0x00A2,
            WM_NCLBUTTONDBLCLK = 0x00A3,
            WM_NCRBUTTONDOWN = 0x00A4,
            WM_NCRBUTTONUP = 0x00A5,
            WM_NCRBUTTONDBLCLK = 0x00A6,
            WM_NCMBUTTONDOWN = 0x00A7,
            WM_NCMBUTTONUP = 0x00A8,
            WM_NCMBUTTONDBLCLK = 0x00A9,
            WM_NCXBUTTONDOWN = 0x00AB,
            WM_NCXBUTTONUP = 0x00AC,
            WM_NCXBUTTONDBLCLK = 0x00AD,
            WM_INPUT_DEVICE_CHANGE = 0x00FE,
            WM_INPUT = 0x00FF,
            WM_KEYFIRST = 0x0100,
            WM_KEYDOWN = 0x0100,
            WM_KEYUP = 0x0101,
            WM_CHAR = 0x0102,
            WM_DEADCHAR = 0x0103,
            WM_SYSKEYDOWN = 0x0104,
            WM_SYSKEYUP = 0x0105,
            WM_SYSCHAR = 0x0106,
            WM_SYSDEADCHAR = 0x0107,
            WM_UNICHAR = 0x0109,
            WM_KEYLAST = 0x0109,
            WM_IME_STARTCOMPOSITION = 0x010D,
            WM_IME_ENDCOMPOSITION = 0x010E,
            WM_IME_COMPOSITION = 0x010F,
            WM_IME_KEYLAST = 0x010F,
            WM_INITDIALOG = 0x0110,
            WM_COMMAND = 0x0111,
            WM_SYSCOMMAND = 0x0112,
            WM_TIMER = 0x0113,
            WM_HSCROLL = 0x0114,
            WM_VSCROLL = 0x0115,
            WM_INITMENU = 0x0116,
            WM_INITMENUPOPUP = 0x0117,
            WM_MENUSELECT = 0x011F,
            WM_MENUCHAR = 0x0120,
            WM_ENTERIDLE = 0x0121,
            WM_MENURBUTTONUP = 0x0122,
            WM_MENUDRAG = 0x0123,
            WM_MENUGETOBJECT = 0x0124,
            WM_UNINITMENUPOPUP = 0x0125,
            WM_MENUCOMMAND = 0x0126,
            WM_CHANGEUISTATE = 0x0127,
            WM_UPDATEUISTATE = 0x0128,
            WM_QUERYUISTATE = 0x0129,
            WM_CTLCOLORMSGBOX = 0x0132,
            WM_CTLCOLOREDIT = 0x0133,
            WM_CTLCOLORLISTBOX = 0x0134,
            WM_CTLCOLORBTN = 0x0135,
            WM_CTLCOLORDLG = 0x0136,
            WM_CTLCOLORSCROLLBAR = 0x0137,
            WM_CTLCOLORSTATIC = 0x0138,
            WM_MOUSEFIRST = 0x0200,
            WM_MOUSEMOVE = 0x0200,
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202,
            WM_LBUTTONDBLCLK = 0x0203,
            WM_RBUTTONDOWN = 0x0204,
            WM_RBUTTONUP = 0x0205,
            WM_RBUTTONDBLCLK = 0x0206,
            WM_MBUTTONDOWN = 0x0207,
            WM_MBUTTONUP = 0x0208,
            WM_MBUTTONDBLCLK = 0x0209,
            WM_MOUSEWHEEL = 0x020A,
            WM_XBUTTONDOWN = 0x020B,
            WM_XBUTTONUP = 0x020C,
            WM_XBUTTONDBLCLK = 0x020D,
            WM_MOUSEHWHEEL = 0x020E,
            WM_MOUSELAST = 0x020E,
            WM_PARENTNOTIFY = 0x0210,
            WM_ENTERMENULOOP = 0x0211,
            WM_EXITMENULOOP = 0x0212,
            WM_NEXTMENU = 0x0213,
            WM_SIZING = 0x0214,
            WM_CAPTURECHANGED = 0x0215,
            WM_MOVING = 0x0216,
            WM_POWERBROADCAST = 0x0218,
            WM_DEVICECHANGE = 0x0219,
            WM_MDICREATE = 0x0220,
            WM_MDIDESTROY = 0x0221,
            WM_MDIACTIVATE = 0x0222,
            WM_MDIRESTORE = 0x0223,
            WM_MDINEXT = 0x0224,
            WM_MDIMAXIMIZE = 0x0225,
            WM_MDITILE = 0x0226,
            WM_MDICASCADE = 0x0227,
            WM_MDIICONARRANGE = 0x0228,
            WM_MDIGETACTIVE = 0x0229,
            WM_MDISETMENU = 0x0230,
            WM_ENTERSIZEMOVE = 0x0231,
            WM_EXITSIZEMOVE = 0x0232,
            WM_DROPFILES = 0x0233,
            WM_MDIREFRESHMENU = 0x0234,
            WM_IME_SETCONTEXT = 0x0281,
            WM_IME_NOTIFY = 0x0282,
            WM_IME_CONTROL = 0x0283,
            WM_IME_COMPOSITIONFULL = 0x0284,
            WM_IME_SELECT = 0x0285,
            WM_IME_CHAR = 0x0286,
            WM_IME_REQUEST = 0x0288,
            WM_IME_KEYDOWN = 0x0290,
            WM_IME_KEYUP = 0x0291,
            WM_MOUSEHOVER = 0x02A1,
            WM_MOUSELEAVE = 0x02A3,
            WM_NCMOUSEHOVER = 0x02A0,
            WM_NCMOUSELEAVE = 0x02A2,
            WM_WTSSESSION_CHANGE = 0x02B1,
            WM_TABLET_FIRST = 0x02c0,
            WM_TABLET_LAST = 0x02df,
            WM_DPICHANGED = 0x02E0,
            WM_CUT = 0x0300,
            WM_COPY = 0x0301,
            WM_PASTE = 0x0302,
            WM_CLEAR = 0x0303,
            WM_UNDO = 0x0304,
            WM_RENDERFORMAT = 0x0305,
            WM_RENDERALLFORMATS = 0x0306,
            WM_DESTROYCLIPBOARD = 0x0307,
            WM_DRAWCLIPBOARD = 0x0308,
            WM_PAINTCLIPBOARD = 0x0309,
            WM_VSCROLLCLIPBOARD = 0x030A,
            WM_SIZECLIPBOARD = 0x030B,
            WM_ASKCBFORMATNAME = 0x030C,
            WM_CHANGECBCHAIN = 0x030D,
            WM_HSCROLLCLIPBOARD = 0x030E,
            WM_QUERYNEWPALETTE = 0x030F,
            WM_PALETTEISCHANGING = 0x0310,
            WM_PALETTECHANGED = 0x0311,
            WM_HOTKEY = 0x0312,
            WM_PRINT = 0x0317,
            WM_PRINTCLIENT = 0x0318,
            WM_APPCOMMAND = 0x0319,
            WM_THEMECHANGED = 0x031A,
            WM_CLIPBOARDUPDATE = 0x031D,
            WM_DWMCOMPOSITIONCHANGED = 0x031E,
            WM_DWMNCRENDERINGCHANGED = 0x031F,
            WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,
            WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,
            WM_GETTITLEBARINFOEX = 0x033F,
            WM_HANDHELDFIRST = 0x0358,
            WM_HANDHELDLAST = 0x035F,
            WM_AFXFIRST = 0x0360,
            WM_AFXLAST = 0x037F,
            WM_PENWINFIRST = 0x0380,
            WM_PENWINLAST = 0x038F,
            WM_TOUCH = 0x0240,
            WM_APP = 0x8000,
            WM_USER = 0x0400,

            WM_DISPATCH_WORK_ITEM = WM_USER,
        }
        public enum ShowWindowCommand
        {
            Hide = 0,
            Normal = 1,
            ShowMinimized = 2,
            Maximize = 3,
            ShowMaximized = 3,
            ShowNoActivate = 4,
            Show = 5,
            Minimize = 6,
            ShowMinNoActive = 7,
            ShowNA = 8,
            Restore = 9,
            ShowDefault = 10,
            ForceMinimize = 11
        }
        public enum PixelType : byte
        {
            PFD_TYPE_RGBA = 0,
            PFD_TYPE_COLORINDEX = 1,
        }
        [Flags]
        public enum PixelFormatDescriptorFlags : uint
        {
            PFD_DOUBLEBUFFER = 0x00000001,
            PFD_STEREO = 0x00000002,
            PFD_DRAW_TO_WINDOW = 0x00000004,
            PFD_DRAW_TO_BITMAP = 0x00000008,
            PFD_SUPPORT_GDI= 0x00000010,
            PFD_SUPPORT_OPENGL = 0x00000020,
            PFD_GENERIC_FORMAT = 0x00000040,
            PFD_NEED_PALETTE = 0x00000080,
            PFD_NEED_SYSTEM_PALETTE = 0x00000100,
            PFD_SWAP_EXCHANGE = 0x00000200,
            PFD_SWAP_COPY = 0x00000400,
            PFD_SWAP_LAYER_BUFFERS = 0x00000800,
            PFD_GENERIC_ACCELERATED = 0x00001000,
            PFD_SUPPORT_DIRECTDRAW = 0x00002000,
            PFD_DIRECT3D_ACCELERATED = 0x00004000,
            PFD_SUPPORT_COMPOSITION = 0x00008000,
            PFD_DEPTH_DONTCARE = 0x20000000,
            PFD_DOUBLEBUFFER_DONTCARE = 0x40000000,
            PFD_STEREO_DONTCARE = 0x80000000,
        }

        // struct
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }
        public struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] rgbReserved;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct PixelFormatDescriptor
        {
            public ushort Size;
            public ushort Version;
            public PixelFormatDescriptorFlags Flags;
            public PixelType PixelType;
            public byte ColorBits;
            public byte RedBits;
            public byte RedShift;
            public byte GreenBits;
            public byte GreenShift;
            public byte BlueBits;
            public byte BlueShift;
            public byte AlphaBits;
            public byte AlphaShift;
            public byte AccumBits;
            public byte AccumRedBits;
            public byte AccumGreenBits;
            public byte AccumBlueBits;
            public byte AccumAlphaBits;
            public byte DepthBits;
            public byte StencilBits;
            public byte AuxBuffers;
            public byte LayerType;
            private byte Reserved;
            public uint LayerMask;
            public uint VisibleMask;
            public uint DamageMask;

            //Use this function to make a new one with Size and Version already filled in.
            public static PixelFormatDescriptor Build()
            {
                var pfd = new PixelFormatDescriptor
                {
                    Size = (ushort)Marshal.SizeOf(typeof(PixelFormatDescriptor)),
                    Version = 1
                };

                return pfd;
            }
        }
        public struct POINT
        {
            public int X;
            public int Y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public int cbSize;
            public ClassStyles style;
            public WndProc lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        // delegate
        public delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);


        // gdi32.dll
        [DllImport("gdi32.dll")]
        static extern int ChoosePixelFormat(IntPtr hdc, ref PixelFormatDescriptor pfd);
        [DllImport("gdi32.dll")]
        static extern bool SetPixelFormat(IntPtr hdc, int format, in PixelFormatDescriptor pfd);
        [DllImport("gdi32.dll")]
        static extern bool SwapBuffers(IntPtr hdc);
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);

        // kernel32.dll
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        // Opengl32.dll
        [DllImport("opengl32.dll")]
        public static extern void glBegin(BeginMode mode);
        [DllImport("opengl32.dll")]
        public static extern void glClear(AttribMask mask);
        [DllImport("opengl32.dll")]
        public static extern void glClearColor(float red, float green, float blue, float alpha);
        [DllImport("opengl32.dll")]
        public static extern void glColor3f(float red, float green, float blue);
        [DllImport("opengl32.dll")]
        public static extern void glEnd();
        [DllImport("opengl32.dll")]
        public static extern void glFlush();
        [DllImport("opengl32.dll")]
        public static extern void glRotated(double angle, double x, double y, double z);
        [DllImport("opengl32.dll")]
        public static extern void glRotatef(float angle, float x, float y, float z);
        [DllImport("opengl32.dll")]
        public static extern void glVertex2i(int x, int y);
        [DllImport("opengl32.dll")]
        public static extern void glViewport(int x, int y, int width, int height);
        [DllImport("opengl32.dll")]
        public static extern IntPtr wglCreateContext(IntPtr hDC);
        [DllImport("opengl32.dll")]
        public static extern bool wglDeleteContext(IntPtr hRC);
        [DllImport("opengl32.dll")]
        public static extern bool wglMakeCurrent(IntPtr hDC, IntPtr hRC);



        // user32.dll
        [DllImport("user32.dll")]
        public static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(
            int dwExStyle,
            uint lpClassName,
            string lpWindowName,
            WindowStyles dwStyle,
            int x,
            int y,
            int nWidth,
            int nHeight,
            IntPtr hWndParent,
            IntPtr hMenu,
            IntPtr hInstance,
            IntPtr lpParam);
        [DllImport("user32.dll", EntryPoint = "DispatchMessageW")]
        public static extern IntPtr DispatchMessage(ref MSG lpmsg);
        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "GetMessageW")]
        public static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        [DllImport("user32.dll")]
        static extern void PostQuitMessage(int nExitCode);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "RegisterClassExW")]
        public static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommand nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG lpMsg);
        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        // MACROS
        static int LOWORD(IntPtr lParam)
        {
            return ((int)lParam) & 0x0000FFFF;            
        }
        static int HIWORD(IntPtr lParam)
        {
            return (int)lParam >> 16;            
        }

        // member variables
        private static IntPtr hDC;


        private static void Display()
        {
            //glClearColor(1.0f, 0.0f, 0.0f, 1.0f);
            glClear(AttribMask.GL_COLOR_BUFFER_BIT);
            glRotatef(1.0f, 0.0f, 0.0f, 1.0f);  // TODO use double everywhere
            glBegin(BeginMode.GL_TRIANGLES);
            glColor3f(1.0f, 0.0f, 0.0f); // TODO use byte
            glVertex2i(0, 1);
            glColor3f(0.0f, 1.0f, 0.0f); // TODO use byte
            glVertex2i(-1, -1);
            glColor3f(0.0f, 0.0f, 1.0f); // TODO use byte
            glVertex2i(1, -1);
            glEnd();
            glFlush();
            SwapBuffers(hDC);
        }

        private static IntPtr MyWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            // Can see codes: http://www.pinvoke.net/default.aspx/Constants/WM.html
            //      CreateWindowEx
            //          0x24  WM_GETMINMAXINFO
            //          0x81  WM_NCCREATE
            //          0x83  WM_NCCALCSIZE
            //          0x1   WM_CREATE
            //      ShowWindow 
            //          0x18  WM_SHOWWINDOW
            //          0x46  WM_WINDOWPOSCHANGING
            //          0x46  WM_WINDOWPOSCHANGING
            //          0x1C  WM_ACTIVATEAPP
            //          0x86  WM_NCACTIVATE
            //          0x7F  WM_GETICON
            //          0x7F  WM_GETICON
            //          0x7F  WM_GETICON
            //          0x6   WM_ACTIVATE
            //          0x281 WM_IME_SETCONTEXT
            //          0x282 WM_IME_NOTIFY
            //          0x3D  WM_GETOBJECT
            //          0x3D  WM_GETOBJECT
            //          0x7   WM_SETFOCUS
            //          0x85  WM_NCPAINT
            //          0x14  WM_ERASEBKGND
            //          0x47  WM_WINDOWPOSCHANGED
            //          0x5   WM_SIZE
            //          0x3   WM_MOVE
            //      UpdateWindow
            //          0xF   WM_PAINT
            //      PeekMessage/GetMessage/TranslateMessage/DispatchMessage
            //          0x7F  WM_GETICON
            //          0x88  WM_SYNCPAINT
            //          0x7F  WM_GETICON


            Console.WriteLine($"MyWndProc got msg 0x{msg:X}");

            switch ((WindowsMessage)msg)
            {
                //case WindowsMessage.WM_PAINT:
                //    Display();
                //    //var ps = new PAINTSTRUCT();
                //    //IntPtr hDC = BeginPaint(hWnd, out ps);
                //    //////TextOut(hDC, 0, 0, "Hello, Windows!", 15);
                //    //EndPaint(hWnd, ref ps);
                //    return IntPtr.Zero;
                case WindowsMessage.WM_SIZE:
                    glViewport(0, 0, LOWORD(lParam), HIWORD(lParam));
                    //PostMessage(hWnd, WM_PAINT, 0, 0);
                    return IntPtr.Zero;
                case WindowsMessage.WM_DESTROY:
                    PostQuitMessage(0);
                    return IntPtr.Zero;
                default:
                    return DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }
        static void Main(string[] args)
        {
            // Create window
            var wc = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<WNDCLASSEX>(),
                //hInstance = GetModuleHandle(null),
                lpfnWndProc = new WndProc(MyWndProc),
                lpszClassName = "SimpleWindow",
                //style = ClassStyles.CS_OWNDC,
            };
            ushort atom = RegisterClassEx(ref wc);
            if (atom == 0)
            {
                // System Error Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes

                int error = GetLastError();
                if (error == 87) Console.WriteLine("RegisterClassEx failed. GetLastError = {0}: The parameter is incorrect.", error);
                else Console.WriteLine("RegisterClassEx failed. GetLastError = {0}.", error);
                return;
            }
            IntPtr hWnd = CreateWindowEx(
                0,
                atom,
                null,
                WindowStyles.WS_OVERLAPPEDWINDOW,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                CW_USEDEFAULT,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);
            if (hWnd == IntPtr.Zero)
            {
                int error = GetLastError();
                Console.WriteLine("CreateWindowEx failed. GetLastError = {0}.", error);
                return;
            }

            // Set pixel format
            var pfd = PixelFormatDescriptor.Build();
            pfd.Flags = PixelFormatDescriptorFlags.PFD_DRAW_TO_WINDOW | 
                        PixelFormatDescriptorFlags.PFD_SUPPORT_OPENGL | 
                        PixelFormatDescriptorFlags.PFD_DOUBLEBUFFER;
            pfd.PixelType = PixelType.PFD_TYPE_RGBA;
            pfd.ColorBits = 24; // bits for color: 8 red + 8 blue + 8 green = 24
            hDC = GetDC(hWnd);
            int pf = ChoosePixelFormat(hDC, ref pfd);
            if (pf == 0)
            {
                int error = GetLastError();
                Console.WriteLine("ChoosePixelFormat failed. GetLastError = {0}.", error);
                return;
            }
            bool pixelFormatSet = SetPixelFormat(hDC, pf, in pfd);
            if (!pixelFormatSet)
            {
                int error = GetLastError();
                Console.WriteLine("SetPixelFormat failed. GetLastError = {0}.", error);
                return;
            }



            IntPtr hRC = wglCreateContext(hDC);
            if (hRC == IntPtr.Zero)
            {
                // System Error Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes
                int error = GetLastError();
                if (error == 2000) Console.WriteLine("wglCreateContext failed. GetLastError = {0}: The pixel format is invalid.", error);
                else Console.WriteLine("wglCreateContext failed. GetLastError = {0}.", error);
                return;
            }
            wglMakeCurrent(hDC, hRC);

            ShowWindow(hWnd, ShowWindowCommand.Show);
            //UpdateWindow(hWnd);

            while (true)
            {
                MSG msg;
                while (PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_NOREMOVE))
                {
                    if (GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
                    {
                        TranslateMessage(ref msg);
                        DispatchMessage(ref msg);
                    }
                    else // Got WM_QUIT
                    {
                        wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                        ReleaseDC(hWnd, hDC);
                        wglDeleteContext(hRC);
                        DestroyWindow(hWnd);
                        return;
                    }
                }
                Display();
            }
        }
    }
}
