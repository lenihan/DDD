using System;
using System.Runtime.InteropServices;

namespace DDD
{
    partial class UIWindows : UI
    {
#region DELEGATE        
        public delegate IntPtr WndProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam);
        public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);
#endregion        
#region MACRO
        public const uint GC_ALLGESTURES = 0x00000001;
        public const uint GC_ZOOM = 0x00000001;
        public const uint GC_PAN = 0x00000001;
        public const uint GC_PAN_WITH_SINGLE_FINGER_VERTICALLY = 0x00000002;
        public const uint GC_PAN_WITH_SINGLE_FINGER_HORIZONTALLY = 0x00000004;
        public const uint GC_PAN_WITH_GUTTER = 0x00000008;
        public const uint GC_PAN_WITH_INERTIA = 0x00000010;
        public const uint GC_ROTATE = 0x00000001;
        public const uint GC_TWOFINGERTAP = 0x00000001;
        public const uint GC_PRESSANDTAP = 0x00000001;
        public const uint GC_ROLLOVER = GC_PRESSANDTAP;
        static double GID_ROTATE_ANGLE_FROM_ARGUMENT(ulong arg)
        {
            return (((double)(arg) / 65535.0) * 4.0 * 3.14159265) - 2.0 * 3.14159265;
        }   
        static ushort LOWORD(IntPtr lParam)
        {
            return System.Convert.ToUInt16(lParam.ToInt32() & 0x0000FFFF);
        }
        static ushort HIWORD(IntPtr lParam)
        {
            return System.Convert.ToUInt16((lParam.ToInt32() >> 16) & 0x0000FFFF);
        }
        static int GET_X_LPARAM(IntPtr lParam)
        {
            return (int)(short)LOWORD(lParam);
        }
        static int GET_Y_LPARAM(IntPtr lParam)
        {
            return (int)(short)HIWORD(lParam);
        }
        const int CW_USEDEFAULT = unchecked((int)0x80000000);
        const int PM_NOREMOVE = 0x00000000;
#endregion
#region STRUCT
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
        [StructLayout(LayoutKind.Sequential)]
        public struct GESTURECONFIG
        {
            public GestureID dwID;
            public uint dwWant;
            public uint dwBlock;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct GESTUREINFO
        {
            public uint cbSize;
            public GestureFlags dwFlags;
            public GestureID dwID;
            public IntPtr hwndTarget;
            public POINTS ptsLocation;
            public uint dwInstanceID;
            public uint dwSequenceID;
            public ulong ulArguments;
            public uint cbExtraArgs;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            public static MONITORINFO Build()
            {
                var mi = new MONITORINFO
                {
                    cbSize = Marshal.SizeOf(typeof(MONITORINFO)),
                    rcMonitor = new RECT(),
                    rcWork = new RECT()
                };
                return mi;
            }
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
        public struct POINT
        {
            public int x;
            public int y;
        }
        public struct POINTS
        {
            public short x;
            public short y;
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
        public struct TOUCHINPUT
        {
            public int x;
            public int y;
            public IntPtr hSource;
            public uint dwID;
            public uint dwFlags;
            public uint dwMask;
            public uint dwTime;
            public UIntPtr dwExtraInfo;
            public uint cxConact;
            public uint cyContact;
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
                    Size = Marshal.SizeOf(typeof(WNDCLASSEX)),
                };
                return wc;
            }
        }
#endregion
#region ENUM
        public enum VIRTUALKEY : uint
        {
            /* Virtual Keys, Standard Set */
            VK_LBUTTON = 0x01,
            VK_RBUTTON = 0x02,
            VK_CANCEL = 0x03,
            VK_MBUTTON = 0x04,    /* NOT contiguous with L & RBUTTON */

            //#if(_WIN32_WINNT >= 0x0500)
            VK_XBUTTON1 = 0x05,   /* NOT contiguous with L & RBUTTON */
            VK_XBUTTON2 = 0x06,   /* NOT contiguous with L & RBUTTON */
            //#endif /* _WIN32_WINNT >= 0x0500 */

            /* 0x07 : unassigned */
            VK_BACK = 0x08,
            VK_TAB = 0x09,

            /* 0x0A - 0x0B : reserved */
            VK_CLEAR = 0x0C,
            VK_RETURN = 0x0D,

            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_MENU = 0x12,
            VK_PAUSE = 0x13,
            VK_CAPITAL = 0x14,

            VK_KANA = 0x15,
            VK_HANGEUL = 0x15,  /* old name - should be here for compatibility */
            VK_HANGUL = 0x15,
            VK_JUNJA = 0x17,
            VK_FINAL = 0x18,
            VK_HANJA = 0x19,
            VK_KANJI = 0x19,

            VK_ESCAPE = 0x1B,

            VK_CONVERT = 0x1C,
            VK_NONCONVERT = 0x1D,
            VK_ACCEPT = 0x1E,
            VK_MODECHANGE = 0x1F,

            VK_SPACE = 0x20,
            VK_PRIOR = 0x21,
            VK_NEXT = 0x22,
            VK_END = 0x23,
            VK_HOME = 0x24,
            VK_LEFT = 0x25,
            VK_UP = 0x26,
            VK_RIGHT = 0x27,
            VK_DOWN = 0x28,
            VK_SELECT = 0x29,
            VK_PRINT = 0x2A,
            VK_EXECUTE = 0x2B,
            VK_SNAPSHOT = 0x2C,
            VK_INSERT = 0x2D,
            VK_DELETE = 0x2E,
            VK_HELP = 0x2F,

            /*  VK_LWIN = 0x5B,CII '0' - '9' (0x30 - 0x39)
            /* 0x40 : unassigned * VK_A - VK_Z are the same as ASCII 'A' - 'Z' (0x41 - 0x5A) */
            VK_LWIN = 0x5B,
            VK_RWIN = 0x5C,
            VK_APPS = 0x5D,

            /* 0x5E : reserved */
            VK_SLEEP = 0x5F,

            VK_NUMPAD0 = 0x60,
            VK_NUMPAD1 = 0x61,
            VK_NUMPAD2 = 0x62,
            VK_NUMPAD3 = 0x63,
            VK_NUMPAD4 = 0x64,
            VK_NUMPAD5 = 0x65,
            VK_NUMPAD6 = 0x66,
            VK_NUMPAD7 = 0x67,
            VK_NUMPAD8 = 0x68,
            VK_NUMPAD9 = 0x69,
            VK_MULTIPLY = 0x6A,
            VK_ADD = 0x6B,
            VK_SEPARATOR = 0x6C,
            VK_SUBTRACT = 0x6D,
            VK_DECIMAL = 0x6E,
            VK_DIVIDE = 0x6F,
            VK_F1 = 0x70,
            VK_F2 = 0x71,
            VK_F3 = 0x72,
            VK_F4 = 0x73,
            VK_F5 = 0x74,
            VK_F6 = 0x75,
            VK_F7 = 0x76,
            VK_F8 = 0x77,
            VK_F9 = 0x78,
            VK_F10 = 0x79,
            VK_F11 = 0x7A,
            VK_F12 = 0x7B,
            VK_F13 = 0x7C,
            VK_F14 = 0x7D,
            VK_F15 = 0x7E,
            VK_F16 = 0x7F,
            VK_F17 = 0x80,
            VK_F18 = 0x81,
            VK_F19 = 0x82,
            VK_F20 = 0x83,
            VK_F21 = 0x84,
            VK_F22 = 0x85,
            VK_F23 = 0x86,
            VK_F24 = 0x87,

            /* 0x88 - 0x8F : unassigned */
            VK_NUMLOCK = 0x90,
            VK_SCROLL = 0x91,

            /* NEC PC-9800 kbd definitions */
            VK_OEM_NEC_EQUAL = 0x92,   // '=' key on numpad

            /* Fujitsu/OASYS kbd definitions */
            VK_OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
            VK_OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
            VK_OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
            VK_OEM_FJ_LOYA = 0x95,   // 'Left OYAYUBI' key
            VK_OEM_FJ_ROYA = 0x96,   // 'Right OYAYUBI' key

            /* 0x97 - 0x9F : unassigned */
            /* VK_L* & VK_R* - left and right Alt, Ctrl and Shift virtual keys. * Used only as parameters to GetAsyncKeyState() and GetKeyState(). * No other API or message will distinguish left and right keys in this way. */
            VK_LSHIFT = 0xA0,
            VK_RSHIFT = 0xA1,
            VK_LCONTROL = 0xA2,
            VK_RCONTROL = 0xA3,
            VK_LMENU = 0xA4,
            VK_RMENU = 0xA5,

            //#if(_WIN32_WINNT >= 0x0500)
            VK_BROWSER_BACK = 0xA6,
            VK_BROWSER_FORWARD = 0xA7,
            VK_BROWSER_REFRESH = 0xA8,
            VK_BROWSER_STOP = 0xA9,
            VK_BROWSER_SEARCH = 0xAA,
            VK_BROWSER_FAVORITES = 0xAB,
            VK_BROWSER_HOME = 0xAC,

            VK_VOLUME_MUTE = 0xAD,
            VK_VOLUME_DOWN = 0xAE,
            VK_VOLUME_UP = 0xAF,
            VK_MEDIA_NEXT_TRACK = 0xB0,
            VK_MEDIA_PREV_TRACK = 0xB1,
            VK_MEDIA_STOP = 0xB2,
            VK_MEDIA_PLAY_PAUSE = 0xB3,
            VK_LAUNCH_MAIL = 0xB4,
            VK_LAUNCH_MEDIA_SELECT = 0xB5,
            VK_LAUNCH_APP1 = 0xB6,
            VK_LAUNCH_APP2 = 0xB7,

            //#endif /* _WIN32_WINNT >= 0x0500 */

            /* 0xB8 - 0xB9 : reserved */
            VK_OEM_1 = 0xBA,   // ',:' for US
            VK_OEM_PLUS = 0xBB,   // '+' any country
            VK_OEM_COMMA = 0xBC,   // ',' any country
            VK_OEM_MINUS = 0xBD,   // '-' any country
            VK_OEM_PERIOD = 0xBE,   // '.' any country
            VK_OEM_2 = 0xBF,   // '/?' for US
            VK_OEM_3 = 0xC0,   // '`~' for US

            /* 0xC1 - 0xD7 : reserved */
            /* 0xD8 - 0xDA : unassigned */
            VK_OEM_4 = 0xDB,  //  '[{' for US
            VK_OEM_5 = 0xDC,  //  '\|' for US
            VK_OEM_6 = 0xDD,  //  ']}' for US
            VK_OEM_7 = 0xDE,  //  ''"' for US
            VK_OEM_8 = 0xDF,

            /* 0xE0 : reserved */
            /* Various extended or enhanced keyboards */
            VK_OEM_AX = 0xE1,     //  'AX' key on Japanese AX kbd
            VK_OEM_102 = 0xE2,    //  "<>" or "\|" on RT 102-key kbd.
            VK_ICO_HELP = 0xE3,   //  Help key on ICO
            VK_ICO_00 = 0xE4,     //  00 key on ICO

            //#if(WINVER >= 0x0400)
            VK_PROCESSKEY = 0xE5,
            //#endif /* WINVER >= 0x0400 */

            VK_ICO_CLEAR = 0xE6,
            //#if(_WIN32_WINNT >= 0x0500)
            VK_PACKET = 0xE7,
            //#endif /* _WIN32_WINNT >= 0x0500 */

            /* 0xE8 : unassigned */
            /* Nokia/Ericsson definitions */
            VK_OEM_RESET = 0xE9,
            VK_OEM_JUMP = 0xEA,
            VK_OEM_PA1 = 0xEB,
            VK_OEM_PA2 = 0xEC,
            VK_OEM_PA3 = 0xED,
            VK_OEM_WSCTRL = 0xEE,
            VK_OEM_CUSEL = 0xEF,
            VK_OEM_ATTN = 0xF0,
            VK_OEM_FINISH = 0xF1,
            VK_OEM_COPY = 0xF2,
            VK_OEM_AUTO = 0xF3,
            VK_OEM_ENLW = 0xF4,
            VK_OEM_BACKTAB = 0xF5,

            VK_ATTN = 0xF6,
            VK_CRSEL = 0xF7,
            VK_EXSEL = 0xF8,
            VK_EREOF = 0xF9,
            VK_PLAY = 0xFA,
            VK_ZOOM = 0xFB,
            VK_NONAME = 0xFC,
            VK_PA1 = 0xFD,
            VK_OEM_CLEAR = 0xFE,

            /* 0xFF : reserved */
            /* missing letters and numbers for convenience */
            VK_0 = 0x30,
            VK_1 = 0x31,
            VK_2 = 0x32,
            VK_3 = 0x33,
            VK_4 = 0x34,
            VK_5 = 0x35,
            VK_6 = 0x36,
            VK_7 = 0x37,
            VK_8 = 0x38,
            VK_9 = 0x39,
            /* 0x40 : unassigned */
            VK_A = 0x41,
            VK_B = 0x42,
            VK_C = 0x43,
            VK_D = 0x44,
            VK_E = 0x45,
            VK_F = 0x46,
            VK_G = 0x47,
            VK_H = 0x48,
            VK_I = 0x49,
            VK_J = 0x4A,
            VK_K = 0x4B,
            VK_L = 0x4C,
            VK_M = 0x4D,
            VK_N = 0x4E,
            VK_O = 0x4F,
            VK_P = 0x50,
            VK_Q = 0x51,
            VK_R = 0x52,
            VK_S = 0x53,
            VK_T = 0x54,
            VK_U = 0x55,
            VK_V = 0x56,
            VK_W = 0x57,
            VK_X = 0x58,
            VK_Y = 0x59,
            VK_Z = 0x5A
        }
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
        public enum IDC_STANDARD_CURSORS 
        {
            IDC_ARROW = 32512,
            IDC_IBEAM = 32513,
            IDC_WAIT = 32514,
            IDC_CROSS = 32515,
            IDC_UPARROW = 32516,
            IDC_SIZE = 32640,
            IDC_ICON = 32641,
            IDC_SIZENWSE = 32642,
            IDC_SIZENESW = 32643,
            IDC_SIZEWE = 32644,
            IDC_SIZENS = 32645,
            IDC_SIZEALL = 32646,
            IDC_NO = 32648,
            IDC_HAND = 32649,
            IDC_APPSTARTING = 32650,
            IDC_HELP = 32651
        }
        [Flags]
        public enum GestureFlags : uint
        {
            GF_BEGIN = 0x00000001,
            GF_INERTIA = 0x00000002,
            GF_END = 0x00000004            
        }
        public enum GestureID : uint
        {
            GID_BEGIN = 1,
            GID_END = 2,
            GID_ZOOM = 3,
            GID_PAN = 4,
            GID_ROTATE = 5,
            GID_TWOFINGERTAP = 6,
            GID_PRESSANDTAP = 7,
            GID_ROLLOVER = GID_PRESSANDTAP
        }
        public enum MouseKeyStateMasks : byte
        {
            MK_LBUTTON = 0x0001,
            MK_RBUTTON = 0x0002,
            MK_SHIFT = 0x0004,
            MK_CONTROL = 0x0008,
            MK_MBUTTON = 0x0010,
            MK_XBUTTON1 = 0x0020,
            MK_XBUTTON2 = 0x0040
        }
        public enum MonitorOptions : uint
        {
            MONITOR_DEFAULTTONULL = 0x00000000,
            MONITOR_DEFAULTTOPRIMARY = 0x00000001,
            MONITOR_DEFAULTTONEAREST = 0x00000002
        }
        public enum RegisterTouchFlags 
        {
            TWF_NONE = 0x00000000,
            TWF_FINETOUCH = 0x00000001,     // Specifies that hWnd prefers noncoalesced touch input.
            TWF_WANTPALM = 0x00000002       // Setting this flag disables palm rejection which reduces delays for getting WM_TOUCH messages.
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
            WM_GESTURE = 0x0119,
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
        public enum HintMode : uint
        {
            GL_DONT_CARE = 0x1100,
            GL_FASTEST = 0x1101,
            GL_NICEST = 0x1102,
        }
#endregion
#region DLLIMPORT
        [DllImport("gdi32.dll")]
        public static extern int ChoosePixelFormat(IntPtr hdc, ref PixelFormatDescriptor pfd);
        [DllImport("gdi32.dll")]
        public static extern int DescribePixelFormat(IntPtr hdc, int format, uint bytes, ref PixelFormatDescriptor pfd);
        [DllImport("gdi32.dll")]
        public static extern bool DPtoLP(IntPtr hdc, [In, Out] POINT [] lpPoints, int nCount);
        [DllImport("gdi32.dll")]
        public static extern int GetPixelFormat(IntPtr hdc);
        [DllImport("gdi32.dll")]
        public static extern int SetMapMode(IntPtr hdc, int fnMapMode);

        [DllImport("gdi32.dll")]
        public static extern bool SetPixelFormat(IntPtr hdc, int format, ref PixelFormatDescriptor pfd);
        public static bool HACK_SetPixelFormat(IntPtr hdc, int format, ref PixelFormatDescriptor pfd)
        {
            // Hack to force opengl32.dll to overwrite gdi32.dll 'SetPixelFormat'.
            // Without this hack, SetPixelFormat will not work in Windows Sandbox or VMWare Fusion.
            // Learned from...
            // https://stackoverflow.com/questions/199016/wglcreatecontext-in-c-sharp-failing-but-not-in-managed-c
            LoadLibrary("opengl32.dll");
            return SetPixelFormat(hdc, format, ref pfd);
        }

        [DllImport("gdi32.dll")]
        public static extern bool SwapBuffers(IntPtr hdc);
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart, string lpString, int cbString);
        
        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);       
        
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
        public static extern void glDisable(GetTarget cap);
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
        
        [DllImport("user32.dll")]
        public static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);
        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);        
        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);        
        [DllImport("user32.dll")]
        public static extern bool CloseTouchInputHandle(IntPtr hTouchInput);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateWindowEx(int dwExStyle, uint lpClassName, string lpWindowName, WindowStyles dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);
        [DllImport("user32.dll", EntryPoint = "DispatchMessageW")]
        public static extern IntPtr DispatchMessage(ref MSG lpmsg);
        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool DestroyWindow(IntPtr hwnd);
        [DllImport("user32.dll")]
        public static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);
        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);
        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        public static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool GetClassInfoEx(IntPtr hInstance, string lpClassName, ref WNDCLASSEX lpWndClass);        
        [DllImport("user32.dll")]
        public static extern IntPtr GetCapture();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", EntryPoint = "GetMessageW")]
        public static extern sbyte GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        [DllImport("user32.dll")]
        public static extern bool GetGestureInfo(IntPtr hGestureInfo, ref GESTUREINFO pGestureInfo);
        [DllImport("user32.dll")] // Learned from here: https://csharp.hotexamples.com/site/file?hash=0xd551f7506b56063e7c47779993980c8a93ce6a2a04c1a2d26de34cc2b1b5d754&fullName=src/WMTouchForm.cs&project=EXOPC/EXOxtender
        public static extern bool GetTouchInputInfo(IntPtr hTouchInput, uint cInputs, [In, Out] TOUCHINPUT [] pInputs, int cbSize);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadCursor(IntPtr hInstance, IDC_STANDARD_CURSORS lpCursorName);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr MonitorFromPoint(POINT pt, MonitorOptions dwFlags);        
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOptions dwFlags);        
        [DllImport("user32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);
        [DllImport("user32.dll")]
        public static extern bool PhysicalToLogicalPoint(IntPtr hWnd, ref POINT lpPoint);
        [DllImport("user32.dll")]
        public static extern bool PhysicalToLogicalPointForPerMonitorDPI(IntPtr hWnd, ref POINT lpPoint);
        [DllImport("user32.dll")]
        public static extern void PostQuitMessage(int nExitCode);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "RegisterClassExW")]
        public static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);
        [DllImport("user32.dll", SetLastError=true)]
        public static extern bool RegisterTouchWindow(IntPtr hWnd, RegisterTouchFlags flags);        
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();        
        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);        
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern bool SetGestureConfig(IntPtr hWnd, uint dwReserved, uint cIDs, [In, Out] GESTURECONFIG [] pGestureConfig, uint cbSize);
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommand nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG lpMsg);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool UnregisterClass(string lpClassName, IntPtr hInstance);
        [DllImport("user32.dll")]
        public static extern bool UpdateWindow(IntPtr hWnd);
#endregion        
    }
}