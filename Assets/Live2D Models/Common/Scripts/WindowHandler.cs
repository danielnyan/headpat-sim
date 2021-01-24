using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowHandler : MonoBehaviour
{
    /*
    Taken from https://alastaira.wordpress.com/2015/06/15/creating-windowless-unity-applications/
    Modified by danielnyan

    References:
    https://msdn.microsoft.com/en-us/library/windows/desktop/ms633591%28v=vs.85%29.aspx
    http://pinvoke.net/default.aspx/Constants/Window%20styles.html
    https://msdn.microsoft.com/en-us/library/windows/desktop/aa969512%28v=vs.85%29.aspx 
    https://forums.codeguru.com/showthread.php?440817-Window-Styles
    */

    [SerializeField]
    private Material m_Material;

    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    // Define function signatures to import from Windows APIs

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int GWL_STYLE);

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    private bool transparent = false;
    private uint originalStyle;

    // Pass the output of the camera to the custom material
    // for chroma replacement
    void OnRenderImage(RenderTexture from, RenderTexture to)
    {
        Graphics.Blit(from, to, m_Material);
    }

    private void Start()
    {
        var hwnd = GetActiveWindow();
        originalStyle = GetWindowLong(hwnd, -16);
    }

    private void Update()
    {
#if !UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleTransparency();
        }
#endif
    }

    private void ToggleTransparency()
    {
        if (transparent)
        {
            var margins = new MARGINS() {
                cxLeftWidth = 0,
                cxRightWidth = 0,
                cyTopHeight = 0,
                cyBottomHeight = 0
            };

            // Get a handle to the window
            var hwnd = GetActiveWindow();

            // Set properties of the window
            // See: 
            // and 
            SetWindowLong(hwnd, -16, originalStyle);

            // Extend the window into the client area
            // See: 
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
        else
        {
            var margins = new MARGINS() { cxLeftWidth = -1 };
            var hwnd = GetActiveWindow();
            SetWindowLong(hwnd, -16, 0x80000000 | 0x10000000);
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
        transparent = !transparent;
    }
}