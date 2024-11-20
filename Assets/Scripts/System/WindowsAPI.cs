using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowsAPI : MonoBehaviour
{
    private const int GWL_STYLE = -16;
    private const int WS_BORDER = 0x00800000;
    private const int WS_CAPTION = 0x00C00000;
    private const int WS_THICKFRAME = 0x00040000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    public static void RemoveWindowBorder()
    {
        IntPtr hwnd = GetActiveWindow();
        int style = GetWindowLong(hwnd, GWL_STYLE);

        // 테두리 및 제목 표시줄 제거
        style &= ~WS_BORDER;
        style &= ~WS_CAPTION;
        style &= ~WS_THICKFRAME;

        SetWindowLong(hwnd, GWL_STYLE, style);
    }
}
