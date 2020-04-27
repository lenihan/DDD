using System;
using System.Collections.Generic;
using System.Management.Automation;             // Windows PowerShell namespace
using System.Runtime.InteropServices;
// Defining input from the pipeline
// https://docs.microsoft.com/en-us/powershell/scripting/developer/cmdlet/adding-parameters-that-process-pipeline-input?view=powershell-7

// Samples here:
// C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0\Samples\sysmgmt\WindowsPowerShell\

namespace DDD
{
    internal static class NativeMethods
    {
    //internal class MinWinDef
    //{
        // Macros
        public static int LOWORD(IntPtr lParam)
        {
            return ((int)lParam) & 0x0000FFFF;
        }
        public static int HIWORD(IntPtr lParam)
        {
            return ((int)lParam >> 16) & 0x0000FFFF;
        }
    //}
    //class WinUser
    //{
        // Macros
        public const int CW_USEDEFAULT = unchecked((int)0x80000000);
        public const int PM_NOREMOVE = 0x00000000;
    //}
    //class Kernel
    //{
        // DllImport
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    //}
    //class Gdi
    //{
        // enum
        [Flags]
        public enum PixelFormatDescriptorFlags : uint
        {
            PFD_DOUBLEBUFFER = 0x00000001,
            PFD_STEREO = 0x00000002,
            PFD_DRAW_TO_WINDOW = 0x00000004,
            PFD_DRAW_TO_BITMAP = 0x00000008,
            PFD_SUPPORT_GDI = 0x00000010,
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
        public enum PixelType : byte
        {
            PFD_TYPE_RGBA = 0,
            PFD_TYPE_COLORINDEX = 1,
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



        // DllImport
        [DllImport("gdi32.dll")]
        public static extern int ChoosePixelFormat(IntPtr hdc, ref PixelFormatDescriptor pfd);
        [DllImport("gdi32.dll")]
        public static extern bool SetPixelFormat(IntPtr hdc, int format, in PixelFormatDescriptor pfd);
        [DllImport("gdi32.dll")]
        public static extern bool SwapBuffers(IntPtr hdc);
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);
    //}
    //class User
    //{
        // enum
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

        // delegate
        public delegate IntPtr WndProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam);

        // struct
        public struct POINT
        {
            public int X;
            public int Y;
        }
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
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }
        [StructLayout(LayoutKind.Sequential)]
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
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WNDCLASSEX
        {
            public int Size;
            public ClassStyles Style;
            public WndProc WndProc;
            public int ClsExtra;
            public int WndExtra;
            public IntPtr Instance;
            public IntPtr Icon;
            public IntPtr Cursor;
            public IntPtr Background;
            public string MenuName;
            public string ClassName;
            public IntPtr IconSm;
            public static WNDCLASSEX Build()
            {
                var wc = new WNDCLASSEX
                {
                    Size = (ushort)Marshal.SizeOf(typeof(WNDCLASSEX)),
                };
                return wc;
            }
        }

        // DllImport
        [DllImport("user32.dll")]
        public static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
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
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetClassInfoEx(IntPtr hInstance, string lpClassName, ref WNDCLASSEX lpWndClass);        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll", EntryPoint = "GetMessageW")]
        public static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "RegisterClassExW")]
        public static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);
        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommand nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG lpMsg);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);

    //}    
    //class OpenGL
    //{
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
        [Flags()]
        public enum GetTarget : uint
        {
            GL_CURRENT_COLOR = 0x0B00,
            GL_CURRENT_INDEX = 0x0B01,
            GL_CURRENT_NORMAL = 0x0B02,
            GL_CURRENT_TEXTURE_COORDS = 0x0B03,
            GL_CURRENT_RASTER_COLOR = 0x0B04,
            GL_CURRENT_RASTER_INDEX = 0x0B05,
            GL_CURRENT_RASTER_TEXTURE_COORDS = 0x0B06,
            GL_CURRENT_RASTER_POSITION = 0x0B07,
            GL_CURRENT_RASTER_POSITION_VALID = 0x0B08,
            GL_CURRENT_RASTER_DISTANCE = 0x0B09,
            GL_POINT_SMOOTH = 0x0B10,
            GL_POINT_SIZE = 0x0B11,
            GL_POINT_SIZE_RANGE = 0x0B12,
            GL_POINT_SIZE_GRANULARITY = 0x0B13,
            GL_LINE_SMOOTH = 0x0B20,
            GL_LINE_WIDTH = 0x0B21,
            GL_LINE_WIDTH_RANGE = 0x0B22,
            GL_LINE_WIDTH_GRANULARITY = 0x0B23,
            GL_LINE_STIPPLE = 0x0B24,
            GL_LINE_STIPPLE_PATTERN = 0x0B25,
            GL_LINE_STIPPLE_REPEAT = 0x0B26,
            GL_LIST_MODE = 0x0B30,
            GL_MAX_LIST_NESTING = 0x0B31,
            GL_LIST_BASE = 0x0B32,
            GL_LIST_INDEX = 0x0B33,
            GL_POLYGON_MODE = 0x0B40,
            GL_POLYGON_SMOOTH = 0x0B41,
            GL_POLYGON_STIPPLE = 0x0B42,
            GL_EDGE_FLAG = 0x0B43,
            GL_CULL_FACE = 0x0B44,
            GL_CULL_FACE_MODE = 0x0B45,
            GL_FRONT_FACE = 0x0B46,
            GL_LIGHTING = 0x0B50,
            GL_LIGHT_MODEL_LOCAL_VIEWER = 0x0B51,
            GL_LIGHT_MODEL_TWO_SIDE = 0x0B52,
            GL_LIGHT_MODEL_AMBIENT = 0x0B53,
            GL_SHADE_MODEL = 0x0B54,
            GL_COLOR_MATERIAL_FACE = 0x0B55,
            GL_COLOR_MATERIAL_PARAMETER = 0x0B56,
            GL_COLOR_MATERIAL = 0x0B57,
            GL_FOG = 0x0B60,
            GL_FOG_INDEX = 0x0B61,
            GL_FOG_DENSITY = 0x0B62,
            GL_FOG_START = 0x0B63,
            GL_FOG_END = 0x0B64,
            GL_FOG_MODE = 0x0B65,
            GL_FOG_COLOR = 0x0B66,
            GL_DEPTH_RANGE = 0x0B70,
            GL_DEPTH_TEST = 0x0B71,
            GL_DEPTH_WRITEMASK = 0x0B72,
            GL_DEPTH_CLEAR_VALUE = 0x0B73,
            GL_DEPTH_FUNC = 0x0B74,
            GL_ACCUM_CLEAR_VALUE = 0x0B80,
            GL_STENCIL_TEST = 0x0B90,
            GL_STENCIL_CLEAR_VALUE = 0x0B91,
            GL_STENCIL_FUNC = 0x0B92,
            GL_STENCIL_VALUE_MASK = 0x0B93,
            GL_STENCIL_FAIL = 0x0B94,
            GL_STENCIL_PASS_DEPTH_FAIL = 0x0B95,
            GL_STENCIL_PASS_DEPTH_PASS = 0x0B96,
            GL_STENCIL_REF = 0x0B97,
            GL_STENCIL_WRITEMASK = 0x0B98,
            GL_MATRIX_MODE = 0x0BA0,
            GL_NORMALIZE = 0x0BA1,
            GL_VIEWPORT = 0x0BA2,
            GL_MODELVIEW_STACK_DEPTH = 0x0BA3,
            GL_PROJECTION_STACK_DEPTH = 0x0BA4,
            GL_TEXTURE_STACK_DEPTH = 0x0BA5,
            GL_MODELVIEW_MATRIX = 0x0BA6,
            GL_PROJECTION_MATRIX = 0x0BA7,
            GL_TEXTURE_MATRIX = 0x0BA8,
            GL_ATTRIB_STACK_DEPTH = 0x0BB0,
            GL_CLIENT_ATTRIB_STACK_DEPTH = 0x0BB1,
            GL_ALPHA_TEST = 0x0BC0,
            GL_ALPHA_TEST_FUNC = 0x0BC1,
            GL_ALPHA_TEST_REF = 0x0BC2,
            GL_DITHER = 0x0BD0,
            GL_BLEND_DST = 0x0BE0,
            GL_BLEND_SRC = 0x0BE1,
            GL_BLEND = 0x0BE2,
            GL_LOGIC_OP_MODE = 0x0BF0,
            GL_INDEX_LOGIC_OP = 0x0BF1,
            GL_COLOR_LOGIC_OP = 0x0BF2,
            GL_AUX_BUFFERS = 0x0C00,
            GL_DRAW_BUFFER = 0x0C01,
            GL_READ_BUFFER = 0x0C02,
            GL_SCISSOR_BOX = 0x0C10,
            GL_SCISSOR_TEST = 0x0C11,
            GL_INDEX_CLEAR_VALUE = 0x0C20,
            GL_INDEX_WRITEMASK = 0x0C21,
            GL_COLOR_CLEAR_VALUE = 0x0C22,
            GL_COLOR_WRITEMASK = 0x0C23,
            GL_INDEX_MODE = 0x0C30,
            GL_RGBA_MODE = 0x0C31,
            GL_DOUBLEBUFFER = 0x0C32,
            GL_STEREO = 0x0C33,
            GL_RENDER_MODE = 0x0C40,
            GL_PERSPECTIVE_CORRECTION_HINT = 0x0C50,
            GL_POINT_SMOOTH_HINT = 0x0C51,
            GL_LINE_SMOOTH_HINT = 0x0C52,
            GL_POLYGON_SMOOTH_HINT = 0x0C53,
            GL_FOG_HINT = 0x0C54,
            GL_TEXTURE_GEN_S = 0x0C60,
            GL_TEXTURE_GEN_T = 0x0C61,
            GL_TEXTURE_GEN_R = 0x0C62,
            GL_TEXTURE_GEN_Q = 0x0C63,
            GL_PIXEL_MAP_I_TO_I = 0x0C70,
            GL_PIXEL_MAP_S_TO_S = 0x0C71,
            GL_PIXEL_MAP_I_TO_R = 0x0C72,
            GL_PIXEL_MAP_I_TO_G = 0x0C73,
            GL_PIXEL_MAP_I_TO_B = 0x0C74,
            GL_PIXEL_MAP_I_TO_A = 0x0C75,
            GL_PIXEL_MAP_R_TO_R = 0x0C76,
            GL_PIXEL_MAP_G_TO_G = 0x0C77,
            GL_PIXEL_MAP_B_TO_B = 0x0C78,
            GL_PIXEL_MAP_A_TO_A = 0x0C79,
            GL_PIXEL_MAP_I_TO_I_SIZE = 0x0CB0,
            GL_PIXEL_MAP_S_TO_S_SIZE = 0x0CB1,
            GL_PIXEL_MAP_I_TO_R_SIZE = 0x0CB2,
            GL_PIXEL_MAP_I_TO_G_SIZE = 0x0CB3,
            GL_PIXEL_MAP_I_TO_B_SIZE = 0x0CB4,
            GL_PIXEL_MAP_I_TO_A_SIZE = 0x0CB5,
            GL_PIXEL_MAP_R_TO_R_SIZE = 0x0CB6,
            GL_PIXEL_MAP_G_TO_G_SIZE = 0x0CB7,
            GL_PIXEL_MAP_B_TO_B_SIZE = 0x0CB8,
            GL_PIXEL_MAP_A_TO_A_SIZE = 0x0CB9,
            GL_UNPACK_SWAP_BYTES = 0x0CF0,
            GL_UNPACK_LSB_FIRST = 0x0CF1,
            GL_UNPACK_ROW_LENGTH = 0x0CF2,
            GL_UNPACK_SKIP_ROWS = 0x0CF3,
            GL_UNPACK_SKIP_PIXELS = 0x0CF4,
            GL_UNPACK_ALIGNMENT = 0x0CF5,
            GL_PACK_SWAP_BYTES = 0x0D00,
            GL_PACK_LSB_FIRST = 0x0D01,
            GL_PACK_ROW_LENGTH = 0x0D02,
            GL_PACK_SKIP_ROWS = 0x0D03,
            GL_PACK_SKIP_PIXELS = 0x0D04,
            GL_PACK_ALIGNMENT = 0x0D05,
            GL_MAP_COLOR = 0x0D10,
            GL_MAP_STENCIL = 0x0D11,
            GL_INDEX_SHIFT = 0x0D12,
            GL_INDEX_OFFSET = 0x0D13,
            GL_RED_SCALE = 0x0D14,
            GL_RED_BIAS = 0x0D15,
            GL_ZOOM_X = 0x0D16,
            GL_ZOOM_Y = 0x0D17,
            GL_GREEN_SCALE = 0x0D18,
            GL_GREEN_BIAS = 0x0D19,
            GL_BLUE_SCALE = 0x0D1A,
            GL_BLUE_BIAS = 0x0D1B,
            GL_ALPHA_SCALE = 0x0D1C,
            GL_ALPHA_BIAS = 0x0D1D,
            GL_DEPTH_SCALE = 0x0D1E,
            GL_DEPTH_BIAS = 0x0D1F,
            GL_MAX_EVAL_ORDER = 0x0D30,
            GL_MAX_LIGHTS = 0x0D31,
            GL_MAX_CLIP_PLANES = 0x0D32,
            GL_MAX_TEXTURE_SIZE = 0x0D33,
            GL_MAX_PIXEL_MAP_TABLE = 0x0D34,
            GL_MAX_ATTRIB_STACK_DEPTH = 0x0D35,
            GL_MAX_MODELVIEW_STACK_DEPTH = 0x0D36,
            GL_MAX_NAME_STACK_DEPTH = 0x0D37,
            GL_MAX_PROJECTION_STACK_DEPTH = 0x0D38,
            GL_MAX_TEXTURE_STACK_DEPTH = 0x0D39,
            GL_MAX_VIEWPORT_DIMS = 0x0D3A,
            GL_MAX_CLIENT_ATTRIB_STACK_DEPTH = 0x0D3B,
            GL_SUBPIXEL_BITS = 0x0D50,
            GL_INDEX_BITS = 0x0D51,
            GL_RED_BITS = 0x0D52,
            GL_GREEN_BITS = 0x0D53,
            GL_BLUE_BITS = 0x0D54,
            GL_ALPHA_BITS = 0x0D55,
            GL_DEPTH_BITS = 0x0D56,
            GL_STENCIL_BITS = 0x0D57,
            GL_ACCUM_RED_BITS = 0x0D58,
            GL_ACCUM_GREEN_BITS = 0x0D59,
            GL_ACCUM_BLUE_BITS = 0x0D5A,
            GL_ACCUM_ALPHA_BITS = 0x0D5B,
            GL_NAME_STACK_DEPTH = 0x0D70,
            GL_AUTO_NORMAL = 0x0D80,
            GL_MAP1_COLOR_4 = 0x0D90,
            GL_MAP1_INDEX = 0x0D91,
            GL_MAP1_NORMAL = 0x0D92,
            GL_MAP1_TEXTURE_COORD_1 = 0x0D93,
            GL_MAP1_TEXTURE_COORD_2 = 0x0D94,
            GL_MAP1_TEXTURE_COORD_3 = 0x0D95,
            GL_MAP1_TEXTURE_COORD_4 = 0x0D96,
            GL_MAP1_VERTEX_3 = 0x0D97,
            GL_MAP1_VERTEX_4 = 0x0D98,
            GL_MAP2_COLOR_4 = 0x0DB0,
            GL_MAP2_INDEX = 0x0DB1,
            GL_MAP2_NORMAL = 0x0DB2,
            GL_MAP2_TEXTURE_COORD_1 = 0x0DB3,
            GL_MAP2_TEXTURE_COORD_2 = 0x0DB4,
            GL_MAP2_TEXTURE_COORD_3 = 0x0DB5,
            GL_MAP2_TEXTURE_COORD_4 = 0x0DB6,
            GL_MAP2_VERTEX_3 = 0x0DB7,
            GL_MAP2_VERTEX_4 = 0x0DB8,
            GL_MAP1_GRID_DOMAIN = 0x0DD0,
            GL_MAP1_GRID_SEGMENTS = 0x0DD1,
            GL_MAP2_GRID_DOMAIN = 0x0DD2,
            GL_MAP2_GRID_SEGMENTS = 0x0DD3,
            GL_TEXTURE_1D = 0x0DE0,
            GL_TEXTURE_2D = 0x0DE1,
            GL_FEEDBACK_BUFFER_POINTER = 0x0DF0,
            GL_FEEDBACK_BUFFER_SIZE = 0x0DF1,
            GL_FEEDBACK_BUFFER_TYPE = 0x0DF2,
            GL_SELECTION_BUFFER_POINTER = 0x0DF3,
            GL_SELECTION_BUFFER_SIZE = 0x0DF4,
        }        
        [Flags()]
        public enum HintMode : uint
        {
            GL_DONT_CARE = 0x1100,
            GL_FASTEST = 0x1101,
            GL_NICEST = 0x1102,
        }

        // DllImport
        [DllImport("opengl32.dll")]
        public static extern void glBegin(BeginMode mode);
        [DllImport("opengl32.dll")]
        public static extern void glClear(AttribMask mask);
        [DllImport("opengl32.dll")]
        public static extern void glClearColor(float red, float green, float blue, float alpha);
        [DllImport("opengl32.dll")]
        public static extern void glColor3ub(byte red, byte green, byte blue);
        [DllImport("opengl32.dll")]
        public static extern void glColor3f(float red, float green, float blue);
        [DllImport("opengl32.dll")]
        public static extern void glEnable(GetTarget cap);
        [DllImport("opengl32.dll")]
        public static extern void glEnd();
        [DllImport("opengl32.dll")]
        public static extern void glFlush();
        [DllImport("opengl32.dll")]
        public static extern void glHint(GetTarget target, HintMode mode);
        [DllImport("opengl32.dll")]
        public static extern void glLineWidth(float width);
        [DllImport("opengl32.dll")]
        public static extern void glPointSize(float size);
        [DllImport("opengl32.dll")]
        public static extern void glRotated(double angle, double x, double y, double z);
        [DllImport("opengl32.dll")]
        public static extern void glRotatef(float angle, float x, float y, float z);
        [DllImport("opengl32.dll")]
        public static extern void glVertex2i(int x, int y);
        [DllImport("opengl32.dll")]
        public static extern void glVertex3d(double x, double y, double z);        
        [DllImport("opengl32.dll")]
        public static extern void glViewport(int x, int y, int width, int height);
        [DllImport("opengl32.dll")]
        public static extern IntPtr wglCreateContext(IntPtr hDC);
        [DllImport("opengl32.dll")]
        public static extern bool wglDeleteContext(IntPtr hRC);
        [DllImport("opengl32.dll")]
        public static extern bool wglMakeCurrent(IntPtr hDC, IntPtr hRC);
    //}

    }

    [Cmdlet(VerbsData.Out, "3d")]
    [Alias("o3d")]
    public class Out3dCommand : Cmdlet
    {
        List<object> _objects = new List<object>();
#region OPENGL             
        private static void Display(List<object> objects)
        {
            // TODO: point array doesn't allow modification            
            //glClearColor(1.0f, 0.0f, 0.0f, 1.0f);
            NativeMethods.glClear(NativeMethods.AttribMask.GL_COLOR_BUFFER_BIT);
            // OpenGL.glRotatef(1.0f, 0.0f, 0.0f, 1.0f);  // TODO use double everywhere
            // OpenGL.glBegin(OpenGL.BeginMode.GL_TRIANGLES);

            #region DRAW AXES
            const double MaxValue = 3.40282347E+38; // NOTE: This is the max value for float. Using max value for double did not draw. Not sure why.
            //NativeMethods.glLineWidth(10.0f);
            //NativeMethods.glEnable(NativeMethods.GetTarget.GL_LINE_SMOOTH);
            //NativeMethods.glHint(NativeMethods.GetTarget.GL_LINE_SMOOTH_HINT, NativeMethods.HintMode.GL_NICEST);

            NativeMethods.glBegin(NativeMethods.BeginMode.GL_LINES);
            // x axis (red) - left
            NativeMethods.glColor3ub(255, 0, 0);
            NativeMethods.glVertex3d(0.0, 0.0, 0.0);
            NativeMethods.glVertex3d(MaxValue, 0.0, 0.0);

            // y axis (green) - up
            NativeMethods.glColor3ub(0, 255, 0);
            NativeMethods.glVertex3d(0.0, 0.0, 0.0);
            NativeMethods.glVertex3d(0.0, MaxValue, 0.0);

            // z axis (blue) - towards user
            NativeMethods.glColor3ub(0, 0, 255);
            NativeMethods.glVertex3d(0.0, 0.0, 0.0);
            NativeMethods.glVertex3d(0.0, 0.0, MaxValue);
            NativeMethods.glEnd();
            #endregion

            #region DRAW POINTS
            NativeMethods.glPointSize(10);
            NativeMethods.glEnable(NativeMethods.GetTarget.GL_POINT_SMOOTH);
            NativeMethods.glHint(NativeMethods.GetTarget.GL_POINT_SMOOTH_HINT, NativeMethods.HintMode.GL_FASTEST);
            NativeMethods.glBegin(NativeMethods.BeginMode.GL_POINTS);
            foreach (object o in objects)
            {
                if (o is Point p) 
                {
                    NativeMethods.glColor3ub(255, 0, 0);
                    NativeMethods.glVertex3d(p.X, p.Y, p.Z);
                }
            }
            NativeMethods.glEnd();
            #endregion

            NativeMethods.glFlush();
        }
#endregion         
#region WIN32        
        private static IntPtr MyWndProc(IntPtr hWnd, NativeMethods.WindowsMessage msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case NativeMethods.WindowsMessage.WM_SIZE:
                    NativeMethods.glViewport(0, 0, NativeMethods.LOWORD(lParam), NativeMethods.HIWORD(lParam));
                    return IntPtr.Zero;
                case NativeMethods.WindowsMessage.WM_DESTROY:
                    NativeMethods.PostQuitMessage(0);
                    return IntPtr.Zero;
                default:
                    return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }
        private void PrintErrorAndExit(string cmd)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            // System Error Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes
            int error = NativeMethods.GetLastError();
            // Throw a terminating error for types that are not supported.
            string msg = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0} returned error code 0x{1:X}. \nSee https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes", cmd, error);
            ErrorRecord er = new ErrorRecord(
                new FormatException(msg),
                "LastError",
                ErrorCategory.NotSpecified,
                null);
            this.ThrowTerminatingError(er);
#pragma warning restore CA1303 // Do not pass literals as localized parameters            
        }
#endregion                
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public object[] InputObject;
        protected override void BeginProcessing()
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("BeginProcessing");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }
        private void ProcessObject(object input)
        {
            if (input is Point ||
                input is Matrix ||
                input is Vector)
            {
                _objects.Add(input);
            }
            else
            {
#pragma warning disable CA1303 // Do not pass literals as localized parameters            

                // Throw a terminating error for types that are not supported.
                ErrorRecord error = new ErrorRecord(
                    new FormatException("Invalid data type for Out-3d"),
                    "DataNotQualifiedFor3d",
                    ErrorCategory.InvalidType,
                    null);
                this.ThrowTerminatingError(error);
#pragma warning restore CA1303 // Do not pass literals as localized parameters            
            }
        }
        protected override void ProcessRecord()
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("ProcessRecord");
            if (InputObject == null)
            {
                Console.WriteLine("Got null");                
            }
            else if (InputObject.Length == 0)
            {
                 Console.WriteLine("Got empty array of type {0}", InputObject[0].GetType().ToString());
            }
            else if (InputObject.Length == 1) 
            {
                Console.WriteLine("Got array length 1 of type {0}", InputObject[0].GetType().ToString());
                ProcessObject(InputObject[0]);
            }
            else 
            {   
                Console.WriteLine("Got array length {0} containing...", InputObject.Length);
                foreach(object obj in InputObject) 
                {
                    Console.WriteLine("    Type: {0}", obj.GetType().ToString());
                    ProcessObject(obj);
                }
            }
#pragma warning restore CA1303 // Do not pass literals as localized parameters            
        }
        protected override void EndProcessing()
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("EndProcessing");
            if (_objects.Count == 0) return;
#region WIN32
            // Create window
            IntPtr hInst = NativeMethods.GetModuleHandle(null);
            if (hInst == null) PrintErrorAndExit("GetModuleHandle");

            const string className = "DDDWindow";
            var wc = NativeMethods.WNDCLASSEX.Build();
            wc.WndProc = new NativeMethods.WndProc(MyWndProc);
            wc.ClassName = className;
            wc.Instance = hInst;
            ushort atom = NativeMethods.RegisterClassEx(ref wc);
            if (atom == 0) PrintErrorAndExit("RegisterClassEx");

            IntPtr hWnd = NativeMethods.CreateWindowEx(
                0,
                atom,
                null,
                NativeMethods.WindowStyles.WS_OVERLAPPEDWINDOW,
                NativeMethods.CW_USEDEFAULT,
                NativeMethods.CW_USEDEFAULT,
                NativeMethods.CW_USEDEFAULT,
                NativeMethods.CW_USEDEFAULT,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);
            if (hWnd == IntPtr.Zero) PrintErrorAndExit("CreateWindowEx");

            // Set pixel format
            var pfd = NativeMethods.PixelFormatDescriptor.Build();
            pfd.Flags = NativeMethods.PixelFormatDescriptorFlags.PFD_DRAW_TO_WINDOW |
                        NativeMethods.PixelFormatDescriptorFlags.PFD_SUPPORT_OPENGL |
                        NativeMethods.PixelFormatDescriptorFlags.PFD_DOUBLEBUFFER;
            pfd.PixelType = NativeMethods.PixelType.PFD_TYPE_RGBA;
            pfd.ColorBits = 24; // bits for color: 8 red + 8 blue + 8 green = 24

            IntPtr hDC = NativeMethods.GetDC(hWnd);
            if (hDC == IntPtr.Zero) PrintErrorAndExit("GetDC");

            int pf = NativeMethods.ChoosePixelFormat(hDC, ref pfd);
            if (pf == 0) PrintErrorAndExit("ChoosePixelFormat");

            bool pixelFormatSet = NativeMethods.SetPixelFormat(hDC, pf, in pfd);
            if (!pixelFormatSet) PrintErrorAndExit("SetPixelFormat");

            IntPtr hRC = NativeMethods.wglCreateContext(hDC);
            if (hRC == IntPtr.Zero) PrintErrorAndExit("wglCreateContext");
            
            bool makeCurrent = NativeMethods.wglMakeCurrent(hDC, hRC);
            if (!makeCurrent) PrintErrorAndExit("wglMakeCurrent");

            NativeMethods.ShowWindow(hWnd, NativeMethods.ShowWindowCommand.Show);
                
            // Message loop
            bool running = true;
            while (running)
            {
                Display(_objects);

                bool buffersSwapped = NativeMethods.SwapBuffers(hDC);
                if (!buffersSwapped) PrintErrorAndExit("SwapBuffers");

                NativeMethods.MSG msg;
                while (NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0, 0, NativeMethods.PM_NOREMOVE))
                {
                    if (NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0) != 0)
                    {
                        NativeMethods.TranslateMessage(ref msg);
                        NativeMethods.DispatchMessage(ref msg);
                    }
                    else // Got WM_QUIT
                    {                        
                        running = false;
                        break;
                    }
                }
            }

            bool makeCurrentZeroed = NativeMethods.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            if (!makeCurrentZeroed) PrintErrorAndExit("wglMakeCurrent");

            bool contextDeleted = NativeMethods.wglDeleteContext(hRC);
            if (!contextDeleted) PrintErrorAndExit("wglDeleteContext");

            bool classUnregistered = NativeMethods.UnregisterClass(className, hInst);            
            if (!classUnregistered) PrintErrorAndExit("UnregisterClass");
            
            Console.WriteLine("EndProcessing - DONE");
#endregion        
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }
    }
}