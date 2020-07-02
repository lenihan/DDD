using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
// Defining input from the pipeline
// https://docs.microsoft.com/en-us/powershell/scripting/developer/cmdlet/adding-parameters-that-process-pipeline-input?view=powershell-7

// Samples here:
// C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0\Samples\sysmgmt\WindowsPowerShell\

namespace DDD
{
    partial class UIWindows : UI
    {
        IntPtr MyWndProc(IntPtr hWnd, WindowsMessage msg, IntPtr wParam, IntPtr lParam)
        {
            // Console.WriteLine($"MyWndProc: {hWnd}, {msg}, {wParam}, {lParam}");
            switch (msg)
            {
                case WindowsMessage.WM_GESTURE:
                    IntPtr hGestureInfo = lParam;
                    var gi = new GESTUREINFO();
                    gi.cbSize = (uint)Marshal.SizeOf(typeof(GESTUREINFO));
                    bool gestureInfoGotten = GetGestureInfo(hGestureInfo, ref gi);
                    if (!gestureInfoGotten) PrintErrorAndExit("GetGestureInfo");

                    if (gi.dwID == GestureID.GID_ROTATE)
                    {
                        double angleInRadians = GID_ROTATE_ANGLE_FROM_ARGUMENT(gi.ulArguments);
                        double angle = angleInRadians * 180.0 / Math.PI;
                        if (gi.dwFlags == GestureFlags.GF_BEGIN)
                        {
                            _zDegreesAtRotateGestureBegin = _zDegreesCurrent;
                        }
                        else 
                        {
                            _zDegreesCurrent = _zDegreesAtRotateGestureBegin - angle;
                        }
                        return IntPtr.Zero;
                    }
                    return DefWindowProc(hWnd, msg, wParam, lParam);

                case WindowsMessage.WM_TOUCH:
                    uint cInputs = LOWORD(wParam);
                    TOUCHINPUT[] pInputs = new TOUCHINPUT[cInputs];
                    IntPtr hTouchInput = lParam;
                    int cbSize = Marshal.SizeOf(typeof(TOUCHINPUT));
                    bool gotTouchInputInfo = GetTouchInputInfo(hTouchInput, cInputs, pInputs, cbSize);
                    if (!gotTouchInputInfo) PrintErrorAndExit("GetTouchInputInfo");
                    for (uint i = 0; i < cInputs; i++)
                    {
                        TOUCHINPUT ti = pInputs[i];
                        // TODO: do something with ti
                    }
                    bool touchInputHandleClosed = CloseTouchInputHandle(hTouchInput);
                    if (!touchInputHandleClosed) PrintErrorAndExit("CloseTouchInputHandle");
                    
                    return IntPtr.Zero;

               // Left Mouse
                case WindowsMessage.WM_LBUTTONDOWN:
                    _mouseLeftButtonDownPos.X = GET_X_LPARAM(lParam); 
                    _mouseLeftButtonDownPos.Y = GET_Y_LPARAM(lParam);
                    _xDegreesAtButtonDown = _xDegreesCurrent; 
                    _yDegreesAtButtonDown = _yDegreesCurrent; 
                    return IntPtr.Zero;
                
                // Right Mouse
                case WindowsMessage.WM_RBUTTONDOWN:
                    _mouseRightButtonDownPos.X = GET_X_LPARAM(lParam); 
                    _mouseRightButtonDownPos.Y = GET_Y_LPARAM(lParam); 
                    _zDegreesAtButtonDown = _zDegreesCurrent; 
                    return IntPtr.Zero;
                
                // Mouse move
                case WindowsMessage.WM_MOUSEMOVE:
                    _mouseMovePos.X = GET_X_LPARAM(lParam); 
                    _mouseMovePos.Y = GET_Y_LPARAM(lParam);
                    _mouseLeftButtonDown = (MouseKeyStateMasks)wParam == MouseKeyStateMasks.MK_LBUTTON ? true : false;
                    _mouseRightButtonDown = (MouseKeyStateMasks)wParam == MouseKeyStateMasks.MK_RBUTTON ? true : false;
                    return IntPtr.Zero;

                // Key down
                case WindowsMessage.WM_KEYDOWN:
                    switch ((uint)wParam)
                    {
                        // x axis
                        case VIRTUALKEY.VK_W:
                        case VIRTUALKEY.VK_UP:
                            if (_xaxis != 1)
                            {
                                _xaxis = 1;
                                _xDegreesAtButtonDown = _xDegreesCurrent;
                                _xaxisStart = DateTime.Now;
                            }
                            break;
                        case VIRTUALKEY.VK_S:
                        case VIRTUALKEY.VK_DOWN:
                            if (_xaxis != -1)
                            {
                                _xaxis = -1;
                                _xDegreesAtButtonDown = _xDegreesCurrent;
                                _xaxisStart = DateTime.Now;
                            }
                            break;
                        // y axis
                        case VIRTUALKEY.VK_A:
                        case VIRTUALKEY.VK_LEFT:
                            if (_yaxis != 1)
                            {
                                _yaxis = 1;
                                _yDegreesAtButtonDown = _yDegreesCurrent;
                                _yaxisStart = DateTime.Now;
                            }
                            break;
                        case VIRTUALKEY.VK_D:
                        case VIRTUALKEY.VK_RIGHT:
                            if (_yaxis != -1)
                            {
                                _yaxis = -1;
                                _yDegreesAtButtonDown = _yDegreesCurrent;
                                _yaxisStart = DateTime.Now;
                            }
                            break;
                        // z axis
                        case VIRTUALKEY.VK_Q:
                        case VIRTUALKEY.VK_OEM_COMMA:
                        case VIRTUALKEY.VK_PRIOR:
                            if (_zaxis != 1)
                            {
                                _zaxis = 1;
                                _zDegreesAtButtonDown = _zDegreesCurrent;
                                _zaxisStart = DateTime.Now;
                            }
                            break;
                        case VIRTUALKEY.VK_E:
                        case VIRTUALKEY.VK_OEM_PERIOD:
                        case VIRTUALKEY.VK_NEXT:
                            if (_zaxis != -1)
                            {
                                _zaxis = -1;
                                _zDegreesAtButtonDown = _zDegreesCurrent;
                                _zaxisStart = DateTime.Now;
                            }
                            break;
                    }
                    return IntPtr.Zero;
                // Key up
                case WindowsMessage.WM_KEYUP:
                    switch ((uint)wParam)
                    {
                        // x axis
                        case VIRTUALKEY.VK_W:
                        case VIRTUALKEY.VK_UP:
                        case VIRTUALKEY.VK_S:
                        case VIRTUALKEY.VK_DOWN:
                            _xaxis = 0;
                            break;
                        // y axis
                        case VIRTUALKEY.VK_A:
                        case VIRTUALKEY.VK_LEFT:
                        case VIRTUALKEY.VK_D:
                        case VIRTUALKEY.VK_RIGHT:
                            _yaxis = 0;
                            break;
                        // z axis
                        case VIRTUALKEY.VK_Q:
                        case VIRTUALKEY.VK_OEM_COMMA:
                        case VIRTUALKEY.VK_PRIOR:
                        case VIRTUALKEY.VK_E:
                        case VIRTUALKEY.VK_OEM_PERIOD:
                        case VIRTUALKEY.VK_NEXT:
                            _zaxis = 0;
                            break;
                        // show bounding box toggle
                        case VIRTUALKEY.VK_B:
                            _showBoundingBox = !_showBoundingBox;
                            break;
                        // reset
                        case VIRTUALKEY.VK_R:
                            _wld2cam = Matrix.Identity();
                            _xDegreesCurrent = 0.0;
                            _yDegreesCurrent = 0.0;
                            _zDegreesCurrent = 0.0;
                            break;
                        case VIRTUALKEY.VK_ESCAPE:
                            DestroyWindow(hWnd);
                            break;
                    }
                    return IntPtr.Zero;
                // Window resize
                case WindowsMessage.WM_SIZE:
                    const int x = 0;
                    const int y = 0;
                    _width = LOWORD(lParam);
                    _height = HIWORD(lParam);
                    glViewport(x, y, _width, _height);
                    return IntPtr.Zero;
                // Quit
                case WindowsMessage.WM_DESTROY:
                    PostQuitMessage(0);
                    return IntPtr.Zero;
                default:
                    return DefWindowProc(hWnd, msg, wParam, lParam);
            }
        }
        static void ClampAngleFrom0To360(ref double angle)
        {
            while (angle >= 360.0) angle -= 360.0;
            while (angle < 0.0) angle += 360.0;
        }

        private void Display(List<object> Objects, Point BoundingBoxMin, Point BoundingBoxMax)
        {
            /*
                Coordinate System Definitions
                
                Name    TLA   Notes
                ----    ---   -----
                Object  obj   Relative to an object  
                World   wld   Global space
                Camera  cam   Observers view. Looking at world origin. 
                Screen  scr   OpenGL. XY origin at center of screen. +X from left (-1) to right (+1). +Y from bottom (-1) to top (+1).
            */

            #region UPDATE WLD2CAM

            Matrix cam2wld = Matrix.Identity();
            cam2wld *= Matrix.RotateX(_xDegreesCurrent);
            cam2wld *= Matrix.RotateY(_yDegreesCurrent);
            cam2wld *= Matrix.RotateZ(_zDegreesCurrent);

            if (_xaxis != 0)
            {
                TimeSpan interval = DateTime.Now - _xaxisStart;
                double rot = interval.TotalMilliseconds % MillisecondsPerRotation / MillisecondsPerRotation;    // 0.0 to 1.0 rotation since button down
                double deg = 360.0 * rot * _xaxis;                                                              // degrees rotation since button down
                double newCurrent = deg + _xDegreesAtButtonDown;
                
                ClampAngleFrom0To360(ref newCurrent);

                double delta = _xDegreesCurrent - newCurrent;
                cam2wld *= Matrix.RotateX(delta);
                _xDegreesCurrent = newCurrent;
            }
            if (_yaxis != 0)
            {
                TimeSpan interval = DateTime.Now - _yaxisStart;
                double rot = interval.TotalMilliseconds % MillisecondsPerRotation / MillisecondsPerRotation;    // 0.0 to 1.0 rotation since button down
                double deg = 360.0 * rot * _yaxis;                                                              // degrees rotation since button down
                double newCurrent = deg + _yDegreesAtButtonDown;
                
                ClampAngleFrom0To360(ref newCurrent);

                double delta = _yDegreesCurrent - newCurrent;
                cam2wld *= Matrix.RotateY(delta);
                _yDegreesCurrent = newCurrent;
            }
            if (_zaxis != 0)
            {
                TimeSpan interval = DateTime.Now - _zaxisStart;
                double rot = interval.TotalMilliseconds % MillisecondsPerRotation / MillisecondsPerRotation;    // 0.0 to 1.0 rotation since button down
                double deg = 360.0 * rot * _zaxis;                                                              // degrees rotation since button down
                double newCurrent = deg + _zDegreesAtButtonDown;
                
                ClampAngleFrom0To360(ref newCurrent);

                double delta = _zDegreesCurrent - newCurrent;
                cam2wld *= Matrix.RotateZ(delta);
                _zDegreesCurrent = newCurrent;
            }
            if (_mouseLeftButtonDown)
            {
                int deltaPosX = _mouseLeftButtonDownPos.X - _mouseMovePos.X;
                double x = (double)deltaPosX / (double)_width;
                double newCurrentY = 360.0 * x + _yDegreesAtButtonDown;

                ClampAngleFrom0To360(ref newCurrentY);

                cam2wld *= Matrix.RotateY(newCurrentY - _yDegreesCurrent);
                _yDegreesCurrent = newCurrentY;

                int deltaPosY = _mouseLeftButtonDownPos.Y - _mouseMovePos.Y;
                double y = (double)deltaPosY / (double)_height;
                double newCurrentX = 360.0 * y + _xDegreesAtButtonDown;

                ClampAngleFrom0To360(ref newCurrentX);

                cam2wld *= Matrix.RotateX(newCurrentX - _xDegreesCurrent);
                _xDegreesCurrent = newCurrentX;
            }
            if (_mouseRightButtonDown)
            {
                int deltaPosX = _mouseRightButtonDownPos.X - _mouseMovePos.X;
                double x = (double)deltaPosX / (double)_width;
                double newCurrentZ = 360.0 * x + _zDegreesAtButtonDown;

                ClampAngleFrom0To360(ref newCurrentZ);

                cam2wld *= Matrix.RotateZ(newCurrentZ - _zDegreesCurrent);
                _zDegreesCurrent = newCurrentZ;
            }
            _wld2cam = cam2wld.Transpose();
            #endregion
            
            #region CAM2SCR
            Matrix cam2scr = Matrix.Identity();

            // fit bounding box within opengl window
            if (_maxDistance != 0)
            {
                const double OPENGL_MAX = 1.0;
                const double COVERAGE = 0.95;   // 1.0 == max coverage, 0.5 == half coverage, .95 looks about right
                double s = OPENGL_MAX / _maxDistance * COVERAGE;
                cam2scr *= Matrix.Scale(s, s, s);
            }
            
            // Keep 1:1 ratio, even when window is not square
            if (_width != 0 && _height != 0) 
            {
                double w2h = (double)_width/(double)_height;
                if (_width < _height)
                {
                    cam2scr *= Matrix.Scale(1.0, w2h, 1.0);
                }
                else 
                {
                    cam2scr *= Matrix.Scale(1.0/w2h, 1.0, 1.0);
                }
            }
            #endregion

            Matrix wld2scr = cam2scr * _wld2cam;


            glEnable(GetTarget.GL_DEPTH_TEST);  // TODO: This only needs to be done once
            glClear(AttribMask.GL_COLOR_BUFFER_BIT | AttribMask.GL_DEPTH_BUFFER_BIT);

            #region DRAW AXES
            {
                // Map axis to screen space
                Point origin_scr = wld2scr * Origin_wld;
                Point xaxis_scr = wld2scr * _xAxis_wld;
                Point yaxis_scr = wld2scr * _yAxis_wld;
                Point zaxis_scr = wld2scr * _zAxis_wld;

                glEnable(GetTarget.GL_LINE_SMOOTH);
                glHint(GetTarget.GL_LINE_SMOOTH_HINT, HintMode.GL_NICEST);
                glBegin(BeginMode.GL_LINES);

                    // x axis (red) - left
                    glColor3ub(_red.R, _red.G, _red.B);
                    glVertex3d(origin_scr.X, origin_scr.Y, origin_scr.Z);
                    glColor3ub(_black.R, _black.G, _black.B);
                    glVertex3d(xaxis_scr.X, xaxis_scr.Y, xaxis_scr.Z);

                    // y axis (green) - up
                    glColor3ub(_green.R, _green.G, _green.B);
                    glVertex3d(origin_scr.X, origin_scr.Y, origin_scr.Z);
                    glColor3ub(_black.R, _black.G, _black.B);
                    glVertex3d(yaxis_scr.X, yaxis_scr.Y, yaxis_scr.Z);

                    // z axis (blue) - towards user
                    glColor3ub(_blue.R, _blue.G, _blue.B);
                    glVertex3d(origin_scr.X, origin_scr.Y, origin_scr.Z);
                    glColor3ub(_black.R, _black.G, _black.B);
                    glVertex3d(zaxis_scr.X, zaxis_scr.Y, zaxis_scr.Z);

                glEnd();
            }
            #endregion

            #region DRAW OBJECTS
                foreach (object o in Objects)
                {
                    if (o is Point p_wld) 
                    {
                        Point p_scr = wld2scr * p_wld;
                        glPointSize(5.0f);
                        glEnable(GetTarget.GL_POINT_SMOOTH);
                        glHint(GetTarget.GL_POINT_SMOOTH_HINT, HintMode.GL_FASTEST);
                        glBegin(BeginMode.GL_POINTS);
                            glColor3ub(_yellow.R, _yellow.G, _yellow.B);
                            glVertex3d(p_scr.X, p_scr.Y, p_scr.Z);
                        glEnd();
                    }
                    else if (o is Vector v_wld)
                    {
                        Vector v_scr = wld2scr * v_wld;
                        Vector origin_scr = wld2scr * new Vector(0.0, 0.0, 0.0);
                        glEnable(GetTarget.GL_LINE_SMOOTH);
                        glHint(GetTarget.GL_LINE_SMOOTH_HINT, HintMode.GL_NICEST);
                        glBegin(BeginMode.GL_LINES);                        
                            glLineWidth(1.0f);
                            glColor3ub(_darkGray.R, _darkGray.G, _darkGray.B);
                            glVertex3d(origin_scr.X, origin_scr.Y, origin_scr.Z);
                            glColor3ub(_orange.R, _orange.G, _orange.B);
                            glVertex3d(v_scr.X, v_scr.Y, v_scr.Z);
                        glEnd();
                    }
                    else if (o is Matrix m_wld)
                    {
                        Matrix m_scr = wld2scr * m_wld;
                        Point origin_scr = m_scr * new Point(0.0, 0.0, 0.0);
                        Point xaxis_scr = m_scr * new Point(1.0, 0.0, 0.0);
                        Point yaxis_scr = m_scr * new Point(0.0, 1.0, 0.0);
                        Point zaxis_scr = m_scr * new Point(0.0, 0.0, 1.0);

                        glEnable(GetTarget.GL_LINE_SMOOTH);
                        glHint(GetTarget.GL_LINE_SMOOTH_HINT, HintMode.GL_NICEST);
                        glBegin(BeginMode.GL_LINES);

                            // x axis (red) - left
                            glColor3ub(_red.R, _red.G, _red.B);
                            glVertex3d(origin_scr.X, origin_scr.Y, origin_scr.Z);
                            glVertex3d(xaxis_scr.X, xaxis_scr.Y, xaxis_scr.Z);

                            // y axis (green) - up
                            glColor3ub(_green.R, _green.G, _green.B);
                            glVertex3d(origin_scr.X, origin_scr.Y, origin_scr.Z);
                            glVertex3d(yaxis_scr.X, yaxis_scr.Y, yaxis_scr.Z);

                            // z axis (blue) - towards user
                            glColor3ub(_blue.R, _blue.G, _blue.B);
                            glVertex3d(origin_scr.X, origin_scr.Y, origin_scr.Z);
                            glVertex3d(zaxis_scr.X, zaxis_scr.Y, zaxis_scr.Z);

                        glEnd();
                    }
                }
            #endregion

            #region DRAW BOUNDING BOX
            if (_showBoundingBox) 
            {
                Vector delta_bbox = BoundingBoxMax - BoundingBoxMin;
                if (delta_bbox.X != 0 && delta_bbox.Y != 0 && delta_bbox.Z != 0)
                {
                    // Map bounding box to screen space
                    Point min_scr = wld2scr * BoundingBoxMin;
                    Point max_scr = wld2scr * BoundingBoxMax;

                    Point xmin_scr = wld2scr * new Point(BoundingBoxMax.X, BoundingBoxMin.Y, BoundingBoxMin.Z);
                    Point ymin_scr = wld2scr * new Point(BoundingBoxMin.X, BoundingBoxMax.Y, BoundingBoxMin.Z);
                    Point zmin_scr = wld2scr * new Point(BoundingBoxMin.X, BoundingBoxMin.Y, BoundingBoxMax.Z);

                    Point xmax_scr = wld2scr * new Point(BoundingBoxMin.X, BoundingBoxMax.Y, BoundingBoxMax.Z);
                    Point ymax_scr = wld2scr * new Point(BoundingBoxMax.X, BoundingBoxMin.Y, BoundingBoxMax.Z);
                    Point zmax_scr = wld2scr * new Point(BoundingBoxMax.X, BoundingBoxMax.Y, BoundingBoxMin.Z);

                    glEnable(GetTarget.GL_LINE_SMOOTH);
                    glHint(GetTarget.GL_LINE_SMOOTH_HINT, HintMode.GL_NICEST);
                    glBegin(BeginMode.GL_LINES);

                        // x axis (red) - left
                        glColor3ub(_white.R, _white.G, _white.B);
                        glVertex3d(min_scr.X, min_scr.Y, min_scr.Z);
                        glVertex3d(xmin_scr.X, xmin_scr.Y, xmin_scr.Z);

                        // y axis (green) - up
                        glVertex3d(min_scr.X, min_scr.Y, min_scr.Z);
                        glVertex3d(ymin_scr.X, ymin_scr.Y, ymin_scr.Z);

                        // z axis (blue) - towards user
                        glVertex3d(min_scr.X, min_scr.Y, min_scr.Z);
                        glVertex3d(zmin_scr.X, zmin_scr.Y, zmin_scr.Z);

                        // max x white
                        glVertex3d(max_scr.X, max_scr.Y, max_scr.Z);
                        glVertex3d(xmax_scr.X, xmax_scr.Y, xmax_scr.Z);

                        // max y white
                        glVertex3d(max_scr.X, max_scr.Y, max_scr.Z);
                        glVertex3d(ymax_scr.X, ymax_scr.Y, ymax_scr.Z);

                        // max z white
                        glVertex3d(max_scr.X, max_scr.Y, max_scr.Z);
                        glVertex3d(zmax_scr.X, zmax_scr.Y, zmax_scr.Z);

                        // x axis (red) - left
                        glVertex3d(xmin_scr.X, xmin_scr.Y, xmin_scr.Z);
                        glVertex3d(ymax_scr.X, ymax_scr.Y, ymax_scr.Z);

                        glVertex3d(xmin_scr.X, xmin_scr.Y, xmin_scr.Z);
                        glVertex3d(zmax_scr.X, zmax_scr.Y, zmax_scr.Z);

                        // y axis (green) - up
                        glVertex3d(ymin_scr.X, ymin_scr.Y, ymin_scr.Z);
                        glVertex3d(xmax_scr.X, xmax_scr.Y, xmax_scr.Z);

                        glVertex3d(ymin_scr.X, ymin_scr.Y, ymin_scr.Z);
                        glVertex3d(zmax_scr.X, zmax_scr.Y, zmax_scr.Z);

                        // z axis (blue) - towards user
                        glVertex3d(zmin_scr.X, zmin_scr.Y, zmin_scr.Z);
                        glVertex3d(xmax_scr.X, xmax_scr.Y, xmax_scr.Z);

                        glVertex3d(zmin_scr.X, zmin_scr.Y, zmin_scr.Z);
                        glVertex3d(ymax_scr.X, ymax_scr.Y, ymax_scr.Z);

                    glEnd();
                }
            }
            #endregion

            glFlush();
        }
        static void PrintErrorAndExit(string cmd)
        {
            // System Error Codes: https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes
            int error = GetLastError();
            // Throw a terminating error for types that are not supported.
            string msg = string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0} returned error code 0x{1:X}. \nSee https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes", cmd, error);
            throw new System.InvalidOperationException(msg);
        }
        new public void ShowWindow(List<object> Objects, Point BoundingBoxMin, Point BoundingBoxMax, string Title)
        {
            // compute _maxDistance
            double maxX = Math.Max(Math.Abs(BoundingBoxMin.X), Math.Abs(BoundingBoxMax.X)); // max distance from origin in X direction
            double maxY = Math.Max(Math.Abs(BoundingBoxMin.Y), Math.Abs(BoundingBoxMax.Y)); // max distance from origin in Y direction
            double maxZ = Math.Max(Math.Abs(BoundingBoxMin.Z), Math.Abs(BoundingBoxMax.Z)); // max distance from origin in Z direction
            double maxAxis = Math.Max(Math.Max(maxX, maxY), maxZ);                          // overall max distance for any axis
            _maxDistance = Math.Sqrt(3 * maxAxis * maxAxis);                                // Distance from origin to (maxAxis, maxAxis, maxAxis)

            // initialize globals
            _xAxis_wld.X = _maxDistance;
            _xAxis_wld.Y = 0;
            _xAxis_wld.Z = 0;
            _yAxis_wld.X = 0;
            _yAxis_wld.Y = _maxDistance;
            _yAxis_wld.Z = 0;
            _zAxis_wld.X = 0;
            _zAxis_wld.Y = 0;
            _zAxis_wld.Z = _maxDistance;
            _showBoundingBox = false;

            _mouseLeftButtonDown = false;
            _mouseRightButtonDown = false;

            // Create window
            IntPtr hInstance = GetModuleHandle(null);
            if (hInstance == null) PrintErrorAndExit("GetModuleHandle");

            const string className = "DDDWindow";
            var wc = WNDCLASSEX.Build();
            wc.WndProc = new WndProc(MyWndProc);
            wc.ClassName = className;
            wc.Instance = hInstance;
            wc.Cursor = LoadCursor(IntPtr.Zero, IDC_STANDARD_CURSORS.IDC_ARROW);

            ushort atom = RegisterClassEx(ref wc);
            if (atom == 0) PrintErrorAndExit("RegisterClassEx");

            const int dwExStyle = 0;
            const int x = CW_USEDEFAULT;
            const int y = CW_USEDEFAULT;
            const int width = CW_USEDEFAULT;
            const int height = CW_USEDEFAULT;
            IntPtr hWndParent = IntPtr.Zero;
            IntPtr hMenu = IntPtr.Zero;
            IntPtr lpParam = IntPtr.Zero;

            IntPtr hWnd = CreateWindowEx(
                dwExStyle,
                atom,
                Title, 
                WindowStyles.WS_OVERLAPPEDWINDOW,
                x,
                y,
                width,
                height,
                hWndParent,
                hMenu,
                hInstance,
                lpParam);
            if (hWnd == IntPtr.Zero) PrintErrorAndExit("CreateWindowEx");

            const uint dwReserved = 0;
            const uint cIDs = 1;
            var pGestureConfig = new GESTURECONFIG[cIDs];            
            pGestureConfig[0].dwID = GestureID.GID_ROTATE;
            pGestureConfig[0].dwWant = GC_ROTATE;
            pGestureConfig[0].dwBlock = 0;
            uint cbSize = (uint)Marshal.SizeOf(typeof(GESTURECONFIG));
            bool gestureConfigSet = SetGestureConfig(hWnd, dwReserved, cIDs, pGestureConfig, cbSize);
            if (!gestureConfigSet) PrintErrorAndExit("SetGestureConfig");

            bool foregroundWindow = SetForegroundWindow(hWnd);
            if (!foregroundWindow) PrintErrorAndExit("SetForegroundWindow");

            // Set pixel format
            var pfd = PixelFormatDescriptor.Build();
            pfd.Flags = PixelFormatDescriptorFlags.PFD_DRAW_TO_WINDOW |
                        PixelFormatDescriptorFlags.PFD_SUPPORT_OPENGL |
                        PixelFormatDescriptorFlags.PFD_DOUBLEBUFFER;
            pfd.PixelType = PixelType.PFD_TYPE_RGBA;
            pfd.ColorBits = 24; // bits for color: 8 red + 8 blue + 8 green = 24

            IntPtr hDC = GetDC(hWnd);
            if (hDC == IntPtr.Zero) PrintErrorAndExit("GetDC");

            int pf = ChoosePixelFormat(hDC, ref pfd);
            if (pf == 0) PrintErrorAndExit("ChoosePixelFormat");

            bool pixelFormatSet = SetPixelFormat(hDC, pf, in pfd);
            if (!pixelFormatSet) PrintErrorAndExit("SetPixelFormat");

            IntPtr hRC = wglCreateContext(hDC);
            if (hRC == IntPtr.Zero) PrintErrorAndExit("wglCreateContext");
            
            bool makeCurrent = wglMakeCurrent(hDC, hRC);
            if (!makeCurrent) PrintErrorAndExit("wglMakeCurrent");

            ShowWindow(hWnd, ShowWindowCommand.Show);
                
            // Message loop
            bool running = true;
            while (running)
            {
                Display(Objects, BoundingBoxMin, BoundingBoxMax);

                bool buffersSwapped = SwapBuffers(hDC);
                if (!buffersSwapped) PrintErrorAndExit("SwapBuffers");
                
                // Do not use 100% of CPU. Yield to others for a minimal amount of time (1 millisecond)
                System.Threading.Tasks.Task.Delay(1).Wait();    

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
                        running = false;
                        break;
                    }
                }
            }

            bool makeCurrentZeroed = wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            if (!makeCurrentZeroed) PrintErrorAndExit("wglMakeCurrent");

            bool contextDeleted = wglDeleteContext(hRC);
            if (!contextDeleted) PrintErrorAndExit("wglDeleteContext");

            bool classUnregistered = UnregisterClass(className, hInstance);            
            if (!classUnregistered) PrintErrorAndExit("UnregisterClass");
        }        
    }
}