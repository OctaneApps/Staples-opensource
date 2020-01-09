Imports System.IO
Imports System.Net.Sockets
Imports System.Text.RegularExpressions
Imports System.Threading
Imports StaplesCDN.Staples

Public Class CDN
    Dim ServerStatus As Boolean = False
    Dim ServerTrying As Boolean = False
    Dim Server As TcpListener
    Dim Clients As TcpClient

    ' IMAP SERVER CONFIG
    Private Const MailServer = "imap.gmail.com"
    Private Const PortServer = 993
    Private Const UseSSL = True

    ' STAPLES COMMAND
    Public Shared Event CommandArrived()
    Public Shared Event CommandProcessed()

    ' STAPLES
    Protected Friend Staples As IMAPClient

    Private EmailList As New List(Of Email)
    Private ScannerEmails As New List(Of Email)

    ' FETCH INIT
    Dim FetchThreadTimer As New Timers.Timer With {.Interval = 1}
    Dim FetchFlag As Boolean = False
    Dim IsFetching As Boolean = False

    ' SCANNER INIT
    Dim ScannerThreadTimer As New Timers.Timer With {.Interval = 1, .Enabled = True}
    Dim ScannerFlag As Boolean = False
    Dim _scannerResults As New List(Of List(Of ScannerResult))
    Dim Scanner As New Scanner

    Dim _donations As New List(Of StreamLabs.Donation)
    Dim _dID As New List(Of StreamLabs.Donation)


    Dim _emailsFound As New List(Of Email)
    Dim _isC = False





    Public Sub New()
        Staples = New IMAPClient(MailServer, PortServer, UseSSL)
        AddHandler FetchThreadTimer.Elapsed, AddressOf FetchThreadTimerTick
        AddHandler ScannerThreadTimer.Elapsed, AddressOf ScannerThreadTimerTick

        ' AddEmail("no-reply@paytm.com", AddTemplate("Paytm", "([a-zA-Z ]*)'s Paytm Wallet Linked to", "<font style=" & ChrW(34) & "font-weight: bold;font-size: 31px" & ChrW(34) & ">(\d+)<\/font>", "").ReturnObject)
        ' SaveScannerData("scanner.staples")

        LoadScannerData("scanner.staples")
    End Sub


    <Obsolete>
    Public Function StartCDN()
        If ServerStatus = False Then
            ServerTrying = True
            Try
                Server = New TcpListener(64555)
                Server.Start()
                ServerStatus = True
                ThreadPool.QueueUserWorkItem(AddressOf Handler_Client)
            Catch ex As Exception
                ServerStatus = False
            End Try
            ServerTrying = False
        End If
        Return True
    End Function

    Public Function StopCDN()
        If ServerStatus = True Then
            ServerTrying = True
            Try
                Clients.Close()
                Server.Stop()
                ServerStatus = False
            Catch ex As Exception
                StopCDN()
            End Try
            ServerTrying = False
        End If
        Return True
    End Function

    Public Async Sub Handler_Client(ByVal state As Object)
        Try
            Using Client As TcpClient = Server.AcceptTcpClient

                If ServerTrying = False Then
                    ThreadPool.QueueUserWorkItem(AddressOf Handler_Client)
                End If

                Clients = Client

                Dim TX As New StreamWriter(Client.GetStream)
                Dim RX As New StreamReader(Client.GetStream)
                Try
                    SendResponse("Connected to StaplesCDN")
                    If RX.BaseStream.CanRead = True Then
                        While RX.BaseStream.CanRead = True
                            Dim RawData As String = RX.ReadLine
                            If Client.Client.Connected = True AndAlso Client.Connected = True AndAlso Client.GetStream.CanRead = True Then
                                Dim RawDataLength As String = RawData.Length.ToString
                                Dim _command As New StaplesCommand
                                _command = CastCommand(RawData)
                                RaiseEvent CommandArrived()
                                SendResponse((Await ProcessCommand(_command)).ToString)
                                RaiseEvent CommandProcessed()
                            Else Exit While
                            End If
                        End While
                    End If
                Catch ex As Exception
                    If Clients Is Client Then
                        Clients = Nothing
                        Client.Close()
                    End If
                End Try
            End Using
        Catch ex As Exception

        End Try
    End Sub

    Public Function SendResponse(ByVal Data As String)
        If ServerStatus = True Then
            If Clients IsNot Nothing Then
                Try
                    Dim TX1 As New StreamWriter(Clients.GetStream)
                    TX1.WriteLine(Data)
                    TX1.Flush()
                Catch ex As Exception
                    SendResponse(Data)
                End Try
            End If
        End If
        Return True
    End Function


    ' CAST STAPLES COMMAND
    Private Function CastCommand(line As String) As StaplesCommand
        Dim _command As String = "" : Dim _parameters As New Dictionary(Of String, String) : Dim _badCommand As Boolean = True
        For Each commandMatch As Match In Regex.Matches(line, CommandDictionary.CommandPattern)
            If commandMatch.Success Then
                _command = commandMatch.Groups(1).Value
                If commandMatch.Groups(2).Value.Length > 0 Then
                    For Each parameterMatch As Match In Regex.Matches(commandMatch.Groups(3).Value, CommandDictionary.ParameterPattern)
                        _parameters.Add(parameterMatch.Groups(1).Value, parameterMatch.Groups(2).Value)
                    Next
                End If
                _badCommand = False
            End If
        Next
        Return New StaplesCommand(_command, _parameters, _badCommand)
    End Function


    ' PROCESS COMMAND
    Private Async Function ProcessCommand(command As StaplesCommand) As Task(Of CDNResponse)
        Dim _status = "Bad Command" : Dim _statusCode = 400 : Dim _response = "" : Dim _aResponse As New List(Of CDNResponse) : Dim _errorOnAction As String = ""
        Dim _command = command.Command : Dim _cdnResponse As CDNResponse
        If command.BadCommand <> True Then
            If _command = "LOAD_SETTINGS" Then
                If IsFetching = True Then
                    FetchFlag = False : ScannerFlag = False
                    FetchThreadTimer.Stop()
                    IsFetching = False
                    SendResponse(New CDNResponse("Ok", 200, "START_FETCHING", "* OK -> Fetching stopped.").ToString)
                End If
                If LoadSettings(command.Parameters.Item("username"), command.Parameters.Item("password")) = True Then
                    _status = "Ok" : _statusCode = 200 : _response = "* OK -> Setting loaded successfully."
                End If
            ElseIf _command = "START_FETCHING" Then
                If IsFetching = False Then
                    Dim _actionResponse As New List(Of CDNResponse) : Dim _fetchingStarted As Boolean = False
                    If Await Connect() = True Then
                        _actionResponse.Add(New CDNResponse("Ok", 200, "CONNECT", "* OK -> Connected to Staples Mail Client."))

                        If Await Staples.Login(EmailAddress, New IMAPClient.Password(Password, True)) = True Then
                            _actionResponse.Add(New CDNResponse("Ok", 200, "LOGIN", "* OK -> Login sucessful."))

                            ' Start Fetch Thread
                            FetchThreadTimer.Start()
                            IsFetching = True

                            _fetchingStarted = True

                        Else
                            _errorOnAction = "LOGIN"
                            _actionResponse.Add(New CDNResponse("Failed", 401, "LOGIN", "* FAILED -> Unauthorized access."))
                        End If
                    Else
                        _errorOnAction = "CONNECT"
                        _actionResponse.Add(New CDNResponse("Failed", 400, "CONNECT", "* FAILED -> Failed to connect Staples Mail Client."))
                    End If

                    _aResponse = _actionResponse

                    If _fetchingStarted = True Then
                        _status = "Ok" : _statusCode = 200 : _response = "* OK -> Fetching started."
                    Else
                        _status = "Failed" : _statusCode = 400 : _response = "* FAILED -> Fetching failed to start."
                    End If
                Else
                    _status = "Conflict" : _statusCode = 409 : _response = "* FAILED -> Fetching is already started."
                End If
            ElseIf _command = "STOP_FETCHING" Then
                FetchFlag = False : ScannerFlag = False
                FetchThreadTimer.Stop()
                IsFetching = False
                _status = "Ok" : _statusCode = 200 : _response = "* OK -> Fetching stopped."
            ElseIf _command = "STOP_CONNECTION" Then
                FetchFlag = False : ScannerFlag = False
                FetchThreadTimer.Stop()
                IsFetching = False
                EmailList.Clear() : ScannerEmails.Clear() : _scannerResults.Clear() : _emailsFound.Clear()
                Staples = New IMAPClient(MailServer, PortServer, UseSSL)
                SendResponse(New CDNResponse("Ok", 200, _command, "* OK -> Connection closed.").ToString)
                Clients = Nothing
                Clients.Close()
            ElseIf _command = "STOP_SERVER" Then
                FetchFlag = False : ScannerFlag = False
                FetchThreadTimer.Stop()
                IsFetching = False
                EmailList.Clear() : ScannerEmails.Clear() : _scannerResults.Clear() : _emailsFound.Clear()
                Staples = New IMAPClient(MailServer, PortServer, UseSSL)
                SendResponse(New CDNResponse("Ok", 200, _command, "* OK -> StaplesCDN stopped.").ToString)
                Close()
            End If
        Else

        End If
        _cdnResponse = New CDNResponse(_status, _statusCode, _command, _response, _errorOnAction, _aResponse)
        Return _cdnResponse
    End Function


    ' MAKE CONNECTION
    Private Async Function Connect() As Task(Of Boolean)
        Try
            If Await Staples.Connect() Then
                Return True
            End If
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    ' FETCH THREAD TIMER HANDLER
    Private Async Sub FetchThreadTimerTick()
        If FetchFlag = False Then
            FetchFlag = True
            _emailsFound.Clear()
            Await FetchEmail()
        End If
    End Sub

    Private Async Function FetchEmail() As Task
        Dim _status = "Service Unavailable" : Dim _statusCode = 503 : Dim _response = "Service unexpectedly closed. Fetching closed."
        Try
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
                    ' SendResponse(New CDNResponse("Ok", 200, "FETCHING_CONTENT", "* OK -> UID " & em.UID).ToString)
                Next
                ScannerFlag = True
            End If

            EmailList.Clear()
            FetchFlag = False
        Catch ex As Exception
            _emailsFound.Clear()
            FetchFlag = False : FetchThreadTimer.Stop()
            MsgBox(ex.Message & vbNewLine & vbNewLine & ex.StackTrace)
            SendResponse(New CDNResponse(_status, _statusCode, "START_FECTHING", _response).ToString)
        End Try
    End Function


    ' SCANNER THREAD TIMER
    Private Sub ScannerThreadTimerTick()
        If ScannerFlag = True Then

            ScannerFlag = False

            For Each _email As Email In ScannerEmails
                _scannerResults.Add(Scanner.Scan(CastEmail(_email.UID, _email.GetBody, _email.GetDate, GetEmail(_email.GetFrom))).ScannerResults)
            Next
            ScannerEmails.Clear()

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

            _donations.Sort(Function(t1, t2)
                                Return Date.Compare(t1.MailDate, t2.MailDate)
                            End Function)

            For Each dn As StreamLabs.Donation In _donations
                Dim _dataResult As New Dictionary(Of String, String)
                _dataResult.Add("name", dn.Name)
                _dataResult.Add("amount", dn.Amount)
                _dataResult.Add("message", dn.Message)
                _dataResult.Add("timestamp", dn.MailDate)
                _dataResult.Add("identifier", dn.Identifier)

                SendResponse(New CDNResponse("Donation", "Ok", 200, "START_FETCHING", "* OK -> Donation fetched.", _dataResult).ToString)
            Next

            _donations.Clear()

            '  For Each i In _dID
            '      _donations.Remove(i)
            '  Next
            '  _dID.Clear()

        End If
    End Sub
End Class


Public Class CDNResponse
    Public ReadOnly Property Type As String
    Public ReadOnly Property Status As String
    Public ReadOnly Property StatusCode As Integer
    Public ReadOnly Property Command As String
    Public ReadOnly Property Response As String
    Public ReadOnly Property ErrorOnAction As String
    Public ReadOnly Property ActionsResponse As New List(Of CDNResponse)
    Public ReadOnly Property DataResults As New Dictionary(Of String, String)

    Public Sub New()
    End Sub
    Public Sub New(_status As String, _statusCode As Integer, _command As String, _response As String)
        Type = "Process" : Status = _status : StatusCode = _statusCode : Response = _response : Command = _command
    End Sub
    Public Sub New(_status As String, _statusCode As Integer, _command As String, _response As String, _dataResults As Dictionary(Of String, String))
        Type = "Process" : Status = _status : StatusCode = _statusCode : Response = _response : Command = _command : DataResults = _dataResults
    End Sub
    Public Sub New(_type As String, _status As String, _statusCode As Integer, _command As String, _response As String)
        Type = _type : Status = _status : StatusCode = _statusCode : Response = _response : Command = _command
    End Sub
    Public Sub New(_type As String, _status As String, _statusCode As Integer, _command As String, _response As String, _dataResults As Dictionary(Of String, String))
        Type = _type : Status = _status : StatusCode = _statusCode : Response = _response : Command = _command : DataResults = _dataResults
    End Sub
    Public Sub New(_status As String, _statusCode As Integer, _command As String, _response As String, _errorOnAction As String, _data As List(Of CDNResponse))
        Type = "Process" : Status = _status : StatusCode = _statusCode : Response = _response : Command = _command : ErrorOnAction = _errorOnAction : ActionsResponse = _data
    End Sub
    Public Sub New(_status As String, _statusCode As Integer, _command As String, _response As String, _errorOnAction As String, _data As List(Of CDNResponse), _dataResults As Dictionary(Of String, String))
        Type = "Process" : Status = _status : StatusCode = _statusCode : Response = _response : Command = _command : ErrorOnAction = _errorOnAction : ActionsResponse = _data : DataResults = _dataResults
    End Sub
    Public Sub New(_type As String, _status As String, _statusCode As Integer, _command As String, _response As String, _errorOnAction As String, _data As List(Of CDNResponse))
        Type = _type : Status = _status : StatusCode = _statusCode : Response = _response : Command = _command : ErrorOnAction = _errorOnAction : ActionsResponse = _data
    End Sub
    Public Sub New(_type As String, _status As String, _statusCode As Integer, _command As String, _response As String, _errorOnAction As String, _data As List(Of CDNResponse), _dataResults As Dictionary(Of String, String))
        Type = _type : Status = _status : StatusCode = _statusCode : Response = _response : Command = _command : ErrorOnAction = _errorOnAction : ActionsResponse = _data : DataResults = _dataResults
    End Sub

    Public Overrides Function ToString() As String
        Dim _r As String = ""
        _r += "{"
        _r += ChrW(34) & "type" & ChrW(34) & ": " & ChrW(34) & Type & ChrW(34) & ", "
        _r += ChrW(34) & "status" & ChrW(34) & ": " & ChrW(34) & Status & ChrW(34) & ", "
        _r += ChrW(34) & "status code" & ChrW(34) & ": " & ChrW(34) & StatusCode & ChrW(34) & ", "
        _r += ChrW(34) & "timestamp" & ChrW(34) & ": " & ChrW(34) & Now & ChrW(34) & ", "
        _r += ChrW(34) & "command" & ChrW(34) & ": " & ChrW(34) & Command & ChrW(34) & ", "

        If ErrorOnAction <> "" Then
            _r += ChrW(34) & "errorOn" & ChrW(34) & ": " & ChrW(34) & ErrorOnAction & ChrW(34) & ", "
        End If

        If ActionsResponse.Count > 0 Then
            Dim _aRs As New List(Of String) : Dim _aR As String = ""
            For Each _actionResponse In ActionsResponse
                _aR += ChrW(34) & _actionResponse.Command & ChrW(34) & ": {"
                _aR += ChrW(34) & "type" & ChrW(34) & ": " & ChrW(34) & _actionResponse.Type & ChrW(34) & ", "
                _aR += ChrW(34) & "status" & ChrW(34) & ": " & ChrW(34) & _actionResponse.Status & ChrW(34) & ", "
                _aR += ChrW(34) & "status code" & ChrW(34) & ": " & ChrW(34) & _actionResponse.StatusCode & ChrW(34) & ", "
                _aR += ChrW(34) & "timestamp" & ChrW(34) & ": " & ChrW(34) & Now & ChrW(34) & ", "

                If _actionResponse.DataResults.Count > 0 Then
                    Dim _dR As New List(Of String)
                    For Each _result In _actionResponse.DataResults
                        _dR.Add(ChrW(34) & _result.Key & ChrW(34) & ": " & ChrW(34) & _result.Value & ChrW(34))
                    Next
                    _aR += ChrW(34) & "data" & ChrW(34) & ": {"
                    _aR += Join(_dR.ToArray(), ", ")
                    _aR += "}, "
                End If

                _aR += ChrW(34) & "response" & ChrW(34) & ": " & ChrW(34) & _actionResponse.Response & ChrW(34)
                _aR += "}"
                _aRs.Add(_aR)
                _aR = ""
            Next
            _r += Join(_aRs.ToArray(), ", ") & ", "
        End If

        If DataResults.Count > 0 Then
            Dim _dR As New List(Of String)
            For Each _result In DataResults
                _dR.Add(ChrW(34) & _result.Key & ChrW(34) & ": " & ChrW(34) & _result.Value & ChrW(34))
            Next
            _r += ChrW(34) & "data" & ChrW(34) & ": {"
            _r += Join(_dR.ToArray(), ", ")
            _r += "}, "
        End If

        _r += ChrW(34) & "response" & ChrW(34) & ": " & ChrW(34) & Response & ChrW(34)
        _r += "}"
        Return _r
    End Function



End Class


Public Class CommandDictionary
    Public Shared ReadOnly CommandPattern As String = "\$\s+(\w+)(\s*->\s*\{(.*?)\}|)"
    Public Shared ReadOnly ParameterPattern As String = "([a-zA-Z_$][a-zA-Z_$0-9]*):\s*\'(.*?)\'"

    ' COMMAND SYNTAX AND LIST
    '   $ LOAD_SETTINGS -> {username: '', password: ''}
    '   $ START_FETCHING
    '   $ STOP_FETCHING
    '   $ STOP_CONNECTION
    '   $ STOP_SERVER

    '   $ LOAD_SETTINGS -> {username: 'sharmamohinish67@gmail.com', password: '9619572358@mohinish'}

End Class


Public Class StaplesCommand
    Public ReadOnly Property Command As String
    Public ReadOnly Property Parameters As Dictionary(Of String, String)
    Public ReadOnly Property IsParameterLess As Boolean = True
    Public ReadOnly Property BadCommand As Boolean = False


    Public Sub New()
    End Sub
    Public Sub New(_command As String, _parameters As Dictionary(Of String, String), _badCommand As Boolean)
        Command = _command : Parameters = _parameters : BadCommand = _badCommand
        If _parameters.Count > 0 Then
            IsParameterLess = False
        End If
    End Sub

    Public Overrides Function ToString() As String
        Dim _p As New List(Of String)
        For Each p As KeyValuePair(Of String, String) In Parameters
            _p.Add(p.Key & ": '" & p.Value & "'")
        Next
        Return "$ " & Command & " -> " & "{" & Join(_p.ToArray, ", ") & "}"
    End Function

End Class


Public Module Settings
    Public Property EmailAddress As String
    Public Property Password As String



    Public Function LoadSettings(_emailAddress As String, _password As String) As Boolean
        EmailAddress = _emailAddress : Password = _password

        Return True
    End Function

End Module


Public Module StaplesMail

    Public Function Connect() As Boolean
        Return True
    End Function

    Public Function Login(username As String, password As String) As Boolean
        Return True
    End Function

End Module
