Imports System.Globalization
Imports System.Net
Imports System.Text.RegularExpressions
Imports Staples.Staples

Public Class frmMain
    Protected Friend Staples As IMAPClient

    Private _statsModeColor = {Color.Gray, Color.LightBlue, Color.ForestGreen, Color.IndianRed, Color.YellowGreen}
    Private _statsModeEnum As StatsModes = 0
    Private _statsText As String = "Started"

    Public Event TaskCompleted()

    Private EmailList As New List(Of Email)

    Private ScannerEmails As New List(Of Email)

    Dim _T As New Timer With {.Interval = 5000}
    Dim _emailsFound As New List(Of Email)
    Dim _isC = False

    Dim _c As Boolean = False
    Dim _s As Boolean = False

    Public IStreamLabs As New StreamLabs

    Private Async Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Try
            Staples = New IMAPClient(My.Settings.mailServer, My.Settings.port, My.Settings.useSSL)

            AddHandler Staples.StatusChanged, AddressOf StatusChanged
            AddHandler Staples.CommandExecuted, AddressOf CommandExecuted
            AddHandler Staples.ResponseArrived, AddressOf ResponseArrived



            If btnStart.Text = "Start" Then
                btnStart.Text = "Stop"
                btnSettings.Enabled = False

                Await MakeConnection()


            ElseIf btnStart.Text = "Stop" Then
                btnStart.Text = "Start"
                btnSettings.Enabled = True

                _c = False : _s = False
                tmrFetchEmail.Stop()
                ChangeStats("Stopped", StatsModes.Idle)
            End If
        Catch ex As Exception
            MsgBox(ex.Message & vbNewLine & vbNewLine & ex.StackTrace)
        End Try
    End Sub

    Private Async Function MakeConnection() As Task
        Try
            If Await Staples.Connect() Then
                ChangeStats("Connected to MailServer", StatsModes.Idle)

                IStreamLabs.LoadDetails(My.Settings.clientId, My.Settings.clientSecret, My.Settings.accessCode, My.Settings.accessToken, My.Settings.refreshToken)
                Dim resp As StreamLabs.StreamLabsResponse = Await IStreamLabs.RefreshConnection
                If resp.Status <> StreamLabs.StreamLabsResponse.StatusCode.Ok Then
                    Dim respC As StreamLabs.StreamLabsResponse = Await IStreamLabs.Connect(My.Settings.clientId, My.Settings.clientSecret)

                    If respC.IsClientValid = False Or respC.Status = StreamLabs.StreamLabsResponse.StatusCode.Unauthorized Then
                        settingsForm.lblStreamLabsStatus.Text = "Please verify your credentials" : settingsForm.lblStreamLabsStatus.ForeColor = Color.Salmon
                        ChangeStats("Please verify your credentials in setting under StreamLabs", StatsModes.Critical)
                        txtLogs.AppendText("Please verify your credentials in setting under StreamLabs" & vbNewLine)
                        My.Settings.streamLabsStatus = False
                        settingsForm.btnConnectStreamLabs.Visible = True
                        btnSettings.Enabled = True
                        btnStart.Text = "Start"
                        My.Settings.Save()
                        Exit Function
                    ElseIf respC.Status = StreamLabs.StreamLabsResponse.StatusCode.Ok Then
                        Activate()
                        settingsForm.lblStreamLabsStatus.Text = "Connected" : settingsForm.lblStreamLabsStatus.ForeColor = Color.Green
                        ChangeStats("Connected to StreamLabs", StatsModes.Success)
                        txtLogs.AppendText("Connected to StreamLabs" & vbNewLine)
                        settingsForm.txtboxStreamLabsCode.Text = respC.AccessCode
                        My.Settings.accessToken = respC.AccessToken
                        My.Settings.refreshToken = respC.RefreshToken
                        My.Settings.streamLabsStatus = True
                        settingsForm.btnConnectStreamLabs.Visible = False
                        My.Settings.Save()
                    End If
                Else
                    My.Settings.streamLabsStatus = True
                    settingsForm.lblStreamLabsStatus.Text = "Connected" : settingsForm.lblStreamLabsStatus.ForeColor = Color.Green
                    ChangeStats("Connected to StreamLabs", StatsModes.Success)
                    txtLogs.AppendText("Connected to StreamLabs" & vbNewLine)
                    My.Settings.Save()
                End If

                If resp.IsClientValid = False Or resp.Status = StreamLabs.StreamLabsResponse.StatusCode.Unauthorized Then
                    settingsForm.lblStreamLabsStatus.Text = "Please verify your credentials" : settingsForm.lblStreamLabsStatus.ForeColor = Color.Salmon
                    My.Settings.streamLabsStatus = False
                ElseIf resp.Status = StreamLabs.StreamLabsResponse.StatusCode.Ok Then
                    settingsForm.lblStreamLabsStatus.Text = "Connect Refreshed" : settingsForm.lblStreamLabsStatus.ForeColor = Color.Green
                    settingsForm.txtboxStreamLabsCode.Text = resp.AccessCode
                    My.Settings.accessToken = resp.AccessToken
                    My.Settings.refreshToken = resp.RefreshToken
                    My.Settings.streamLabsStatus = True
                End If

                If Await Staples.Login(My.Settings.username, New IMAPClient.Password(My.Settings.password, True)) = True Then
                    ChangeStats("Login Successful", StatsModes.Success)

                    tmrFetchEmail.Start()
                    ChangeStats("Fetching emails", StatsModes.Working)
                Else
                    ChangeStats("Failed to login. Please start again.", StatsModes.Critical)
                    btnSettings.Enabled = True
                    btnStart.Text = "Start"
                    settingsForm.LogChanged("Failed to login. Please start again." & " (" & "Failed" & ")")
                    txtLogs.AppendText("Failed to login. Please start again." & " (" & "Failed" & ")" & vbNewLine)
                End If
            End If
        Catch ex As Exception
            ChangeStats("Failed to login. Please start again.", StatsModes.Critical)
        End Try
    End Function

    Public Async Function FetchEmail() As Task
        Try
            If _statsModeEnum <> StatsModes.Working Then
                ChangeStats("Fetching emails", StatsModes.Working)
            End If

            For Each email In GetAllEmails()
                _emailsFound.Clear()
                _emailsFound = Await Staples.SearchRecentUnseen("INBOX", email.EmailAddress)
                If _emailsFound.Count > 0 Then
                    EmailList.AddRange(_emailsFound.ToArray)
                End If
            Next

            If EmailList.Count > 0 Then
                For Each em In EmailList
                    ScannerEmails.Add((Await Staples.FetchEmailContent(em)))
                Next
                _s = True
            End If
            EmailList.Clear()
            _c = False
        Catch ex As Exception
            _emailsFound.Clear()
            _c = False : tmrFetchEmail.Stop()
            ChangeStats("Stopped", StatsModes.Idle)
        End Try
    End Function

    Private Sub btnSettings_Click(sender As Object, e As EventArgs) Handles btnSettings.Click
        settingsForm.ShowDialog()
    End Sub

    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadScannerData("scanner.staples")

        If My.Settings.doneSettings <> True Then
            MsgBox("Initial settings is remaining. Please open settings and complete the procedure.", MsgBoxStyle.OkOnly, "Settings incomplete")
        End If
    End Sub

    Private Sub statusIndicator_Paint(sender As Object, e As PaintEventArgs) Handles statusIndicator.Paint
        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        If _statsModeEnum = StatsModes.Working Then
            e.Graphics.FillEllipse(Brushes.DarkSalmon, 0, 0, 10, 10)
            Dim t As New Timer With {.Enabled = True, .Interval = 200} : Dim i As Long = 0
            AddHandler t.Tick, Sub()
                                   If _statsModeEnum = StatsModes.Working Then
                                       i += 1
                                       If i Mod 2 Then
                                           statusIndicator.CreateGraphics.FillEllipse(Brushes.DarkSalmon, 0, 0, 10, 10)
                                       Else
                                           statusIndicator.CreateGraphics.FillEllipse(Brushes.DimGray, 0, 0, 10, 10)
                                       End If
                                   Else
                                       i = 0
                                       statusIndicator.CreateGraphics.FillEllipse(New SolidBrush(_statsModeColor(_statsModeEnum)), 0, 0, 10, 10)
                                       t.Stop()
                                   End If
                               End Sub
        Else
            e.Graphics.FillEllipse(New SolidBrush(_statsModeColor(_statsModeEnum)), 0, 0, 10, 10)
        End If
    End Sub

    Private Sub statusTextIndicator_Paint(sender As Object, e As PaintEventArgs) Handles statusTextIndicator.Paint
        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        e.Graphics.DrawString(_statsText, Font, Brushes.Black, 0, 0)
    End Sub

    Protected Friend Sub ChangeStatsMode(mode As StatsModes)
        _statsModeEnum = mode
        statusIndicator.Refresh()
    End Sub
    Protected Friend Sub ChangeStatsText(text As String)
        _statsText = text
        statusTextIndicator.Refresh()
    End Sub
    Protected Friend Sub ChangeStats(text As String, mode As StatsModes)
        ChangeStatsText(text) : ChangeStatsMode(mode)
    End Sub

    Protected Friend Enum StatsModes As Integer
        Started = 0
        Idle = 1
        Success = 2
        Critical = 3
        Warning = 4
        Working = 5
    End Enum

    Private Sub CommandExecuted()
        settingsForm.LogChanged(Staples.CommandSingle)
        txtLogs.AppendText(Staples.CommandSingle & vbNewLine)
    End Sub

    Private Sub ResponseArrived()
        settingsForm.LogChanged("MailServer responded with response code: " & Staples.ResponseCode & " (" & Staples.ResponseCode.ToString & ")")
        txtLogs.AppendText("MailServer responded with response code: " & Staples.ResponseCode & " (" & Staples.ResponseCode.ToString & ")" & vbNewLine)
    End Sub

    Private Sub StatusChanged()
        settingsForm.LogChanged(Staples.Status & " (" & Staples.StatusCode.ToString & ")")
        ChangeStatsText(Staples.Status)
        txtLogs.AppendText(Staples.Status & " (" & Staples.StatusCode.ToString & ")" & vbNewLine)
    End Sub

    Private Sub frmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If My.Settings.maintainLogs Then
            My.Computer.FileSystem.WriteAllText("logs.txt", settingsForm.txtLogs.Text & vbNewLine & vbNewLine & vbNewLine, True)
        End If
        My.Settings.Save()
    End Sub

    Private Async Sub Timer1_Tick(sender As Object, e As EventArgs) Handles tmrFetchEmail.Tick
        If _c = False Then
            _c = True
            _emailsFound.Clear()
            Await FetchEmail()
        End If
    End Sub


    Dim _scannerResults As New List(Of List(Of ScannerResult))
    Dim Scanner As New Scanner

    Dim _donations As New List(Of StreamLabs.Donation)
    Dim _dID As New List(Of StreamLabs.Donation)

    Private Async Sub tmrScanner_Tick(sender As Object, e As EventArgs) Handles tmrScanner.Tick
        If _s = True Then

            For Each _email As Email In ScannerEmails
                _scannerResults.Add(Scanner.Scan(CastEmail(_email.UID, _email.GetBody, _email.GetDate, GetEmail(_email.GetFrom))).ScannerResults)
            Next
            ScannerEmails.Clear()

            txtLogs.AppendText(_scannerResults.Count & vbNewLine)

            Dim _name As String = "" : Dim _amount As String = "" : Dim _message As String = "" : Dim _date As Date
            For Each sr As List(Of ScannerResult) In _scannerResults
                For Each s As ScannerResult In sr
                    _date = s.Email.MailDate.Add(TimeZone.CurrentTimeZone.GetUtcOffset(Date.Now))
                    If s.Name = "Name" Then
                        _name = s.Value
                    ElseIf s.Name = "Amount" Then
                        _amount = s.Value
                    ElseIf s.Name = "Message" Then
                        If s.Value <> "" Then
                            _message = s.Value
                        Else
                            _message = "Recieved " & _amount & " from " & _name
                        End If
                    End If
                Next
                _donations.Add(New StreamLabs.Donation(_name, Rnd, _amount, "INR", _message, "", 0, _date))
            Next

            _scannerResults.Clear()

            _s = False

            _donations.Sort(Function(t1, t2)
                                Return Date.Compare(t1.MailDate, t2.MailDate)
                            End Function)

            For Each dn As StreamLabs.Donation In _donations
                Dim resp As StreamLabs.StreamLabsResponse = Await IStreamLabs.PostDonation(dn)
                If resp.Status = StreamLabs.StreamLabsResponse.StatusCode.Ok Then
                    _dID.Add(dn)
                    rtbTest.AppendText(dn.ToString & vbNewLine & "----------------------------------------------" & vbNewLine & vbNewLine)
                End If
            Next

            For Each i In _dID
                _donations.Remove(i)
            Next
            _dID.Clear()

        End If
    End Sub

    Public Async Function HaveInternetConnection() As Task(Of Boolean)
        If My.Computer.Network.IsAvailable Then
            Try
                Dim IPHost As IPHostEntry = Await Dns.GetHostEntryAsync("www.google.com")
                Return True
            Catch
                Return False
            End Try
        Else
            Return False
        End If
    End Function

    Private Async Sub tmrInternet_Tick(sender As Object, e As EventArgs) Handles tmrInternet.Tick
        Dim _v = Await HaveInternetConnection()
        If _v = False Then
            If _c = True Then
                ChangeStats("Internet disconnected. Process will be resumed when internet is available.", StatsModes.Idle)
            Else
                ChangeStats("Internet disconnected.", StatsModes.Idle)
                '  btnStart.Enabled = False
            End If
        End If
    End Sub
End Class


' Paytm: no-reply@paytm.com
' Name: ([a-zA-Z ]*)'s Paytm Wallet Linked to
' Amount: <font style="font-weight: bold;font-size: 31px">(\d+)<\/font>
' Message: 