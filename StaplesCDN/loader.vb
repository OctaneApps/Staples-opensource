Imports System.Threading

Module loader

    Public StaplesCDN As New CDN

    Private Declare Auto Function ShowWindow Lib "user32.dll" (ByVal hWnd As IntPtr, ByVal nCmdShow As Integer) As Boolean
    Private Declare Auto Function GetConsoleWindow Lib "kernel32.dll" () As IntPtr
    Private Const SW_HIDE As Integer = 0

    <Obsolete>
    Sub Main()

        Dim hWndConsole As IntPtr
        hWndConsole = GetConsoleWindow()
        ShowWindow(hWndConsole, SW_HIDE)

        Console.WriteLine("CodeLaps (c) 2020." & vbNewLine & "Staples CDN" & vbNewLine & "──────────────────────────────────────────────────────────────")

        StaplesCDN.StartCDN()


        Dim c = Console.ReadLine()
        While c <> "EXIT"
            Select Case c
                Case "L"
                    StaplesCDN.SendResponse("qwqweqwe")
                    c = Console.ReadLine
                Case "/MSG"
                    StaplesCDN.SendResponse("/MSG")
                    c = Console.ReadLine
                Case Else
                    c = Console.ReadLine()
            End Select
        End While
    End Sub

    Sub Close()
        Environment.Exit(0)
    End Sub


End Module
