Imports System.Globalization
Imports System.IO
Imports System.Net.Security
Imports System.Net.Sockets
Imports System.Text
Imports System.Text.RegularExpressions



' LIST
' ^\*\sLIST\s\((.*?)\)\s\"\/\"\s(.*?)\n
' ^\$\s(.*?)\s(.*?)\s(.*?)\n


' STATUS
' ^\*\sSTATUS\s(.*?)\s\((.*?)\)
' ^\$\s(.*?)\s(.*?)\n


'Date
' ^Date:\s*(\d+)\s(\w+)\s(\d+)\s(\d+:\d+:\d+)\s(-|\+)(\d+)

' ^Date:\s*((\w+),\s*|)(\d+)\s(\w+)\s(\d+)\s(\d+:\d+:\d+)\s(-|\+)(\d+)

'From
' ^From:\s(.*?)<(.*?)>

'Subject
' ^Subject:\s((.|\n)*?)\)


Public Class IMAPClient
    Private _TCPClient As TcpClient
    Private _SSLStream As Stream
    Private ReadOnly _hostName As String
    Private ReadOnly _port As Integer
    Private ReadOnly _useSSL As Boolean
    Public isLoggedIn As Boolean = False
    Public username As String


    Public Event StatusChanged()
    Public Event CommandExecuted()
    Public Event ResponseArrived()

    Public Property CommandSingle As String
    Public Property Status As String
    Public Property StatusCode As StatusCodes

    Public Property ResponseCode As ResponseCodes


    Public EmailList As New List(Of Email)

    Sub New(hostName As String, port As Integer, Optional useSSL As Boolean = True)
        Status = "IMAPClient initialized" : StatusCode = StatusCodes.Idle

        If String.IsNullOrWhiteSpace(hostName) Then
            Status = "Hostname is empty or NULL" : StatusCode = StatusCodes.Failed
            Throw New Exception("Hostname is empty or NULL")
        End If
        If port < 100 Then
            Status = "Provide valid port number" : StatusCode = StatusCodes.Failed
            Throw New Exception("Provide valid port number")
        End If
        _hostName = hostName
        _port = port
        _useSSL = useSSL
    End Sub


    Async Function Connect() As Task(Of Boolean)
        Try
            _TCPClient = New TcpClient(_hostName, _port)

            Status = "Connecting to MailServer" : StatusCode = StatusCodes.Working
            RaiseEvent StatusChanged()

            If _useSSL = True Then
                _SSLStream = New Stream(_TCPClient.GetStream())
                _SSLStream.AuthenticateAsClient(_hostName)
                Dim rawResp = Await GetResponse(True)
                Dim resp = rawResp.Remove(0, 2).Split(" ")(0)
                If resp = "OK" Then

                    Status = "Connected to MailServer" : StatusCode = StatusCodes.Idle
                    RaiseEvent StatusChanged()

                    Return True
                End If
            End If
            _TCPClient.Close()
            _SSLStream.Close()
            Return False
        Catch ex As Exception
            Throw ex
        End Try
        Return False
    End Function

    Async Function Login(ByVal username As String, ByVal password As Password) As Task(Of Boolean)
        Try
            SendCommand("LOGIN", username, password.Show)
            Status = "Logging you in" : StatusCode = StatusCodes.Working
            RaiseEvent StatusChanged()
            Dim rawResp = (Await GetResponse()).Split(vbNewLine)
            Dim splitResp = rawResp(1).Trim().Remove(0, 2).Split(" ")
            If splitResp(0) = "OK" Then
                isLoggedIn = True
                Me.username = username
                Status = username & " logged in" : StatusCode = StatusCodes.Success
                RaiseEvent StatusChanged()
                Return True
            End If
        Catch ex As Exception
            Throw ex
        End Try
        Return False
    End Function

    Public Function SendCommand(commandName As String, ParamArray options As String()) As Boolean
        Try
            _SSLStream.WriteLine("$ " + commandName + " " + String.Join(" ", options))

            If commandName = "LOGIN" Then
                CommandSingle = "$ " + commandName + " " + options(0) + " " + New Password(options(1)).ToString()
            Else
                CommandSingle = "$ " + commandName + " " + String.Join(" ", options)
            End If


            RaiseEvent CommandExecuted()
            Return True
        Catch ex As Exception
            Status = "Failed to send command" : StatusCode = StatusCodes.Failed
            Throw ex
        End Try
        Return False
    End Function

    Private Async Function GetResponse(Optional connect As Boolean = False) As Task(Of String)
        Dim memoryStream As New MemoryStream()
        Dim fullresp = ""
        Dim str = ""
        Try
            While True

                If connect Then
                    If Regex.Match(str, "^\*\sOK", RegexOptions.Compiled).Success Then
                        ResponseCode = ResponseCodes.Success
                        RaiseEvent ResponseArrived()
                        Exit While
                    End If
                Else
                    If Regex.Match(str, "^\$\sOK", RegexOptions.Compiled).Success Then
                        ResponseCode = ResponseCodes.Success
                        RaiseEvent ResponseArrived()
                        Exit While
                    ElseIf Regex.Match(str, "^\$\sBAD", RegexOptions.Compiled).Success Then
                        ResponseCode = ResponseCodes.BadCommand
                        RaiseEvent ResponseArrived()
                        Exit While
                    End If
                End If

                str = ""
                While True
                    Dim num() As Byte = New Byte(1) {}
                    Await _SSLStream.ReadAsync(num, 0, 1)
                    If (num(0) <> 13) Then
                        If (num(0) = 10) Then
                            Exit While
                        End If
                        memoryStream.WriteByte(num(0))
                    End If
                End While
                str = Encoding.UTF8.GetString(memoryStream.ToArray())
                memoryStream.Position = 0
                memoryStream.SetLength(0)
                fullresp += str & vbNewLine
            End While
        Finally
            If (memoryStream IsNot Nothing) Then
                memoryStream.Dispose()
            End If
        End Try
        Return fullresp
    End Function

    Public Async Function BufferResponse() As Task(Of String)
        Try
            Dim s = ""
            Dim data() As Byte = New Byte(_TCPClient.ReceiveBufferSize - 1) {}
            Dim dataInt As Integer
            While Not _TCPClient.GetStream.DataAvailable
                Threading.Thread.Sleep(20)
            End While

            While _TCPClient.GetStream.DataAvailable
                dataInt = Await _SSLStream.ReadAsync(data, 0, data.Length)
                s += Encoding.ASCII.GetString(data)
                Threading.Thread.Sleep(20)
            End While
            Return s
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        Return Nothing
    End Function

    Public Async Function GetStatus(mailboxName As String, statusData As String) As Task(Of String)
        Try
            SendCommand("STATUS", mailboxName, statusData)
            Return Await GetResponse()
        Catch ex As Exception
            Throw ex
        End Try
        Return Nothing
    End Function

    Public Async Function GetMailbox(mailboxPath As String) As Task(Of String)
        Try
            SendCommand("SELECT", mailboxPath)
            Return Await GetResponse()
        Catch ex As Exception
            Throw ex
        End Try
        Return Nothing
    End Function

    Public Async Function ListAllEmail(mailboxPath As String, searchArg As String) As Task(Of String)
        Try
            SendCommand("LIST", ChrW(34) & mailboxPath & ChrW(34), ChrW(34) & searchArg & ChrW(34))
            Return Await GetResponse()
        Catch ex As Exception
            Throw ex
        End Try
        Return Nothing
    End Function

    Public Async Function SearchRecentUnseen(mailboxPath As String, from As String) As Task(Of List(Of Email))
        Try
            Await GetMailbox(mailboxPath) : _SSLStream.Flush()
            Dim dateNow As String = Now.Day & "-" & CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(Now.Month).ToLower & "-" & Now.Year

            SendCommand("SEARCH", "(UNSEEN)", "FROM", ChrW(34) & from & ChrW(34), "ON", ChrW(34) & dateNow & ChrW(34))

            Dim rawresp = Await GetResponse()
            Dim _res = rawresp.Split(vbNewLine)(0).Remove(0, 8)
            If _res <> "" Then
                Dim _uidList As String() = {}
                _uidList = _res.TrimStart(" ").Split(" ").Reverse().ToArray()

                For i = 0 To _uidList.Count - 1
                    EmailList.Add(New Email(_uidList(i)))
                Next
            End If


            Return EmailList
        Catch ex As Exception
            Status = "Some error occured while searching recent emails on " & Now : StatusCode = StatusCodes.Failed
            Throw ex
        End Try
        Return Nothing
    End Function

    Public Async Function FetchEmailContent(_email As Email) As Task(Of Email)
        Try
            Status = "Fetching email content" : StatusCode = StatusCodes.Working
            frmMain.ChangeStats("Fetching email content", frmMain.StatsModes.Working)

            SendCommand("FETCH", _email.UID, "(FLAGS INTERNALDATE RFC822.SIZE BODY[HEADER.FIELDS (DATE FROM SUBJECT)])")
            Dim _hResponse = Await GetResponse()

            SendCommand("FETCH", _email.UID, "(FLAGS INTERNALDATE RFC822.SIZE BODY[HEADER.FIELDS (SUBJECT)])")
            Dim _sResponse = Await GetResponse()

            SendCommand("FETCH", _email.UID, "(FLAGS INTERNALDATE RFC822.SIZE BODY[TEXT])")
            Dim s = Await GetResponse()

            _email.SetRawBody(s) : _email.SetRawHeader(_hResponse) : _email.SetRawSubject(_sResponse)

            Status = "Fetched email content" : StatusCode = StatusCodes.Success
            frmMain.ChangeStats("Idle", frmMain.StatsModes.Idle)
            Return _email
        Catch ex As Exception
            Status = "Some error occured while fetching email content" : StatusCode = StatusCodes.Failed
            Throw ex
        End Try
        Return Nothing
    End Function


    Public Enum StatusCodes As Integer
        Idle = 0
        Working = 1
        Success = 2
        Failed = 3
    End Enum

    Public Enum ResponseCodes As Integer
        Success = 0
        Failed = 1
        BadCommand = 2
    End Enum


    Public Class Password
        Private _pass As String

        Public Sub New(pass As String)
            _pass = pass
        End Sub

        Public Function Show() As String
            Return _pass
        End Function

        Public Overrides Function ToString() As String
            Return "<PASSWORD>"
        End Function
    End Class

End Class

Public Class Email
    Public ReadOnly Property UID As Integer
    Private Property Body As String
    Private Property Subject As String
    Private Property Header As String


    Dim _hDatePattern = "Date:\s*((\w+),\s*|)(\d+)\s(\w+)\s(\d+)\s(\d+:\d+:\d+)\s(-|\+)(\d+)"
    Dim _hFromPattern = "From:\s.*?<(.*?)>"
    Dim _hSubjectPattern = "Subject:\s((.|\n)*?)\)"


    Sub New(_uid As Integer)
        UID = _uid
    End Sub

    Public Sub SetRawBody(data As String)
        _Body = data
    End Sub

    Public Sub SetRawHeader(data As String)
        _Header = data
    End Sub

    Public Sub SetRawSubject(data As String)
        _Subject = data
    End Sub

    Function GetBody() As String
        If Body <> Nothing Then
            Dim m = Regex.Match(Body, "<html>(.|\n)*?<\/html>", RegexOptions.Compiled And RegexOptions.Multiline)
            If m.Success Then
                Return m.Value.Replace(vbNewLine, " ")
            Else
                Return Body.Replace(vbNewLine, " ")
            End If
        End If
        Return "NO BODY"
    End Function

    Public Function GetDate() As Date
        Dim _hDateMatch = Regex.Matches(Header, _hDatePattern)
        Dim _day As Integer = 1 : Dim _year As Integer = 2001 : Dim _month As String = "Jan" : Dim _time As String = "00:00:00"
        For Each m As Match In _hDateMatch
            _day = m.Groups(3).Value
            _month = m.Groups(4).Value
            _year = m.Groups(5).Value
            _time = m.Groups(6).Value
        Next
        Return Date.Parse(_day & " " & _month & " " & _year & " " & _time)
    End Function

    Public Function GetFrom() As String
        Dim _hFromMatch As Match = Regex.Match(Header, _hFromPattern)
        Return _hFromMatch.Groups(1).Value
    End Function

    Public Function GetSubject() As String
        Dim _sMatch As Match = Regex.Match(Subject, _hSubjectPattern)
        Return _sMatch.Groups(1).Value.Replace(vbLf, "").Replace(vbCr, "")
    End Function

    Public Overloads Function ToString() As String
        Return UID
    End Function
End Class

Public Class Stream : Inherits SslStream
    Private _stream As IO.Stream

    Public Sub New(ByRef Stream As IO.Stream)
        MyBase.New(Stream)
        _stream = Stream
    End Sub

    Sub WriteLine(ByVal message As String)
        If Not message.Substring(message.Count() - 1, 1) = vbCrLf Then
            message += (vbCrLf)
        End If
        Dim data As Byte() = Text.Encoding.UTF8.GetBytes(message)
        Write(data)
        Flush()
    End Sub

End Class
