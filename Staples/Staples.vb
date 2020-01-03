Imports System.IO
Imports System.Net
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Text.RegularExpressions

Public Class Staples

    Private Shared Templates As New List(Of Template)
    Private Shared Emails As New List(Of Email)

    Public Shared Event TemplatesListChanged()
    Public Shared Event EmailsListChanged()

    Public Class StreamLabs

        Private _uriPrefix As String = "http://127.0.0.1:55555/"
        Private _extendStreamUri As String = "staples/streamlabs"

        Private StreamLabsRedirectURL As String = _uriPrefix & _extendStreamUri
        Private Const StreamLabsauthorizationEndpoint As String = "https://streamlabs.com/api/v1.0/authorize"
        Private Const StreamLabsTokenEndpoint As String = "https://streamlabs.com/api/v1.0/token"
        Private Const StreamLabsDonationEndpoint As String = "https://streamlabs.com/api/v1.0/donations"

        Private _StreamLabsResponse As New StreamLabsResponse

        Private _accessToken As String = ""
        Private _refreshToken As String = ""
        Private _clientId As String = ""
        Private _clientSecretKey As String = ""
        Private _accessCode As String = ""
        Private _isClientValid As Boolean = False

        Public Sub New()
        End Sub
        Public Sub New(uriPrefix As String, extendStreamUri As String)
            _uriPrefix = uriPrefix : _extendStreamUri = extendStreamUri
        End Sub

        Public Async Function Connect(clientId As String, clientSecretKey As String) As Task(Of StreamLabsResponse)
            Dim _response As String = "" : Dim _status As Integer = 401 : Dim _isConnected As Boolean = False
            Try
                Dim hr As HttpWebRequest
                hr = WebRequest.CreateHttp(String.Format("{0}?response_type=code&scope=donations.create+donations.read&redirect_uri={1}&client_id={2}", StreamLabsauthorizationEndpoint, Uri.EscapeDataString(StreamLabsRedirectURL), clientId))
                hr.Method = "GET"

                Using result = hr.GetResponse()
                    Using sr = New StreamReader(result.GetResponseStream(), Encoding.UTF8)
                        sr.ReadToEnd()
                    End Using
                End Using

                Dim httpListener As New HttpListener()
                httpListener.Prefixes.Add(_uriPrefix)
                httpListener.Start()

                Process.Start(String.Format("{0}?response_type=code&scope=donations.create+donations.read&redirect_uri={1}&client_id={2}", StreamLabsauthorizationEndpoint, Uri.EscapeDataString(StreamLabsRedirectURL), clientId))

                Dim contextAsync As HttpListenerContext = Await httpListener.GetContextAsync()
                Dim response As HttpListenerResponse = contextAsync.Response
                Dim str2 As String = String.Format("<html><body>Please return to the app. <br> Close this window.<script>window.close();</script></body></html>", New Object(-1) {})
                Dim bytes As Byte() = Encoding.UTF8.GetBytes(str2)
                response.ContentLength64 = bytes.Length
                Dim outputStream As IO.Stream = response.OutputStream
                Await outputStream.WriteAsync(bytes, 0, bytes.Length).ContinueWith(Sub(task As Task)
                                                                                       outputStream.Close()
                                                                                       httpListener.[Stop]()
                                                                                   End Sub)

                If contextAsync.Request.QueryString.[Get]("code") IsNot Nothing Then
                    Dim code As String = contextAsync.Request.QueryString.[Get]("code")
                    httpListener.Close()
                    _accessCode = code
                    Try
                        Dim s As HttpWebRequest
                        Dim enc As UTF8Encoding
                        Dim postdata As String
                        Dim postdatabytes As Byte()
                        s = WebRequest.Create(StreamLabsTokenEndpoint)
                        enc = New UTF8Encoding()
                        postdata = "grant_type=authorization_code&client_id=" + clientId + "&client_secret=" + clientSecretKey + "&redirect_uri=" + StreamLabsRedirectURL + "&code=" + code
                        postdatabytes = enc.GetBytes(postdata)
                        s.Method = "POST"
                        s.ContentType = "application/x-www-form-urlencoded"
                        s.ContentLength = postdatabytes.Length
                        Using stream = s.GetRequestStream()
                            stream.Write(postdatabytes, 0, postdatabytes.Length)
                        End Using
                        Dim resp = ""
                        Using result = s.GetResponse()
                            Using sr = New StreamReader(result.GetResponseStream(), Encoding.UTF8)
                                resp = sr.ReadToEnd()
                            End Using
                        End Using

                        For Each m As Match In Regex.Matches(resp, ChrW(34) & "(.*?)" & ChrW(34) & ":(" & ChrW(34) & "(.*?)" & ChrW(34) & "|\d*)")
                            If m.Success Then
                                _response = "Connected to StreamLabs." : _status = 200
                                _isConnected = False : _isClientValid = True : _clientId = clientId : _clientSecretKey = clientSecretKey
                                If m.Groups(1).Value = "access_token" Then
                                    _accessToken = m.Groups(2).Value.Trim(ChrW(34))
                                ElseIf m.Groups(1).Value = "refresh_token" Then
                                    _refreshToken = m.Groups(2).Value.Trim(ChrW(34))
                                End If
                            Else
                                _response = "Unauthorized access." : _status = 401
                                _isConnected = False : _isClientValid = True
                            End If
                        Next

                    Catch ex As Exception
                        _response = "Unauthorized access." & vbNewLine & "Secret key is invalid. Please verify your secret key." : _status = 401
                        _isConnected = False : _isClientValid = True
                    End Try
                End If
            Catch ex As Exception
                _response = "Unauthorized access." & vbNewLine & "Credentials are invalid. Please verify your credentials." : _status = 401
                _isConnected = False : _isClientValid = False
            End Try

            _StreamLabsResponse = New StreamLabsResponse(_accesstoken, _refreshToken, _accessCode, _response, _status, _isClientValid, _isConnected)

            Return _StreamLabsResponse
        End Function

        Public Sub LoadDetails(clientId As String, clientSecretKey As String, accessCode As String, accesstoken As String, refreshToken As String)
            _clientId = clientId : _clientSecretKey = clientSecretKey : _accessCode = accessCode : _accesstoken = accesstoken : _refreshToken = refreshToken
        End Sub

        Public Async Function RefreshConnection() As Task(Of StreamLabsResponse)
            Dim _response As String = "" : Dim _status As Integer = 401 : Dim _isConnected As Boolean = False
            Try
                Dim s As HttpWebRequest
                Dim enc As UTF8Encoding
                Dim postdata As String
                Dim postdatabytes As Byte()
                s = WebRequest.Create(StreamLabsTokenEndpoint)
                enc = New UTF8Encoding()
                postdata = "grant_type=refresh_token&client_id=" + _clientId + "&client_secret=" + _clientSecretKey + "&redirect_uri=" + StreamLabsRedirectURL + "&refresh_token=" + _refreshToken
                postdatabytes = enc.GetBytes(postdata)
                s.Method = "POST"
                s.ContentType = "application/x-www-form-urlencoded"
                s.ContentLength = postdatabytes.Length
                Using stream = Await s.GetRequestStreamAsync()
                    Await stream.WriteAsync(postdatabytes, 0, postdatabytes.Length)
                End Using
                Dim resp = ""
                Using result = Await s.GetResponseAsync()
                    Using sr = New StreamReader(result.GetResponseStream(), Encoding.UTF8)
                        resp = Await sr.ReadToEndAsync()
                    End Using
                End Using

                For Each m As Match In Regex.Matches(resp, ChrW(34) & "(.*?)" & ChrW(34) & ":(" & ChrW(34) & "(.*?)" & ChrW(34) & "|\d*)")
                    If m.Success Then
                        _response = "Token Refreshed." : _status = 200
                        _isConnected = False : _isClientValid = True
                        If m.Groups(1).Value = "access_token" Then
                            _accessToken = m.Groups(2).Value.Trim(ChrW(34))
                        ElseIf m.Groups(1).Value = "refresh_token" Then
                            _refreshToken = m.Groups(2).Value.Trim(ChrW(34))
                        End If
                    Else
                        _response = "Bad Request." : _status = 400
                        _isConnected = False : _isClientValid = True
                    End If
                Next
            Catch ex As Exception
                _response = "Unauthorized access." & vbNewLine & "Credentials are invalid. Please verify your credentials." : _status = 401
                _isClientValid = False
            End Try
            Return New StreamLabsResponse(_accesstoken, _refreshToken, _accessCode, _response, _status, _isClientValid, _isConnected)
        End Function

        Public Async Function PostDonation(name As String, amount As String, currecnyCode As String, message As String, identifier As String) As Task(Of StreamLabsResponse)
            Dim _response As String = "" : Dim _status As Integer = 401 : Dim _isConnected As Boolean = False : Dim _donationId As String = "" : Dim _donation As New Donation
            Try
                Dim s As HttpWebRequest
                Dim enc As UTF8Encoding
                Dim postdata As String
                Dim postdatabytes As Byte()
                s = WebRequest.Create(StreamLabsDonationEndpoint)
                enc = New UTF8Encoding()
                postdata = "name=" & name & "&identifier=" & identifier & "&amount=" & amount & "&currency=" & currecnyCode & "&access_token=" & _accessToken & "&message=" & message
                postdatabytes = enc.GetBytes(postdata)
                s.Method = "POST"
                s.ContentType = "application/x-www-form-urlencoded"
                s.ContentLength = postdatabytes.Length
                Using stream = Await s.GetRequestStreamAsync()
                    Await stream.WriteAsync(postdatabytes, 0, postdatabytes.Length)
                End Using
                Dim resp = ""
                Using result = Await s.GetResponseAsync()
                    Using sr = New StreamReader(result.GetResponseStream(), Encoding.UTF8)
                        resp = Await sr.ReadToEndAsync()
                    End Using
                End Using

                For Each m As Match In Regex.Matches(resp, ChrW(34) & "(.*?)" & ChrW(34) & ":(" & ChrW(34) & "(.*?)" & ChrW(34) & "|\d*)")
                    If m.Success Then
                        _response = "Donation Posted." : _status = 200
                        _isConnected = False : _isClientValid = True
                        If m.Groups(1).Value = "donation_id" Then
                            _donationId = m.Groups(2).Value.Trim(ChrW(34))
                        End If
                        _donation = New Donation(name, identifier, amount, currecnyCode, message, _donationId, 200, Date.Now)
                    Else
                        _response = "Bad Request." : _status = 400
                        _isConnected = False : _isClientValid = True
                    End If
                Next
            Catch ex As Exception
                MsgBox(ex.Message)
                _response = "Unauthorized access." & vbNewLine & "Credentials are invalid. Please verify your credentials." : _status = 401
                _isClientValid = False
            End Try
            Return New StreamLabsResponse(_accessToken, _refreshToken, _accessCode, _response, _status, _isClientValid, _isConnected, _donation)
        End Function
        Public Async Function PostDonation(donation As Donation) As Task(Of StreamLabsResponse)
            Dim _response As String = "" : Dim _status As Integer = 401 : Dim _isConnected As Boolean = False : Dim _donationId As String = "" : Dim _donation As New Donation
            Try
                Dim s As HttpWebRequest
                Dim enc As UTF8Encoding
                Dim postdata As String
                Dim postdatabytes As Byte()
                s = WebRequest.Create(StreamLabsDonationEndpoint)
                enc = New UTF8Encoding()
                postdata = "name=" & donation.Name & "&identifier=" & donation.Identifier & "&amount=" & donation.Amount & "&currency=" & donation.CurrencyCode & "&access_token=" & _accessToken & "&message=" & donation.Message
                postdatabytes = enc.GetBytes(postdata)
                s.Method = "POST"
                s.ContentType = "application/x-www-form-urlencoded"
                s.ContentLength = postdatabytes.Length
                Using stream = Await s.GetRequestStreamAsync()
                    Await stream.WriteAsync(postdatabytes, 0, postdatabytes.Length)
                End Using
                Dim resp = ""
                Using result = Await s.GetResponseAsync()
                    Using sr = New StreamReader(result.GetResponseStream(), Encoding.UTF8)
                        resp = Await sr.ReadToEndAsync()
                    End Using
                End Using

                For Each m As Match In Regex.Matches(resp, ChrW(34) & "(.*?)" & ChrW(34) & ":(" & ChrW(34) & "(.*?)" & ChrW(34) & "|\d*)")
                    If m.Success Then
                        _response = "Donation Posted." : _status = 200
                        _isConnected = False : _isClientValid = True
                        If m.Groups(1).Value = "donation_id" Then
                            _donationId = m.Groups(2).Value.Trim(ChrW(34))
                        End If
                        _donation = New Donation(donation.Name, donation.Identifier, donation.Amount, donation.CurrencyCode, donation.Message, _donationId, 200, donation.MailDate)
                    Else
                        _response = "Bad Request." : _status = 400
                        _isConnected = False : _isClientValid = True
                    End If
                Next
            Catch ex As Exception
                _response = "Unauthorized access." & vbNewLine & "Credentials are invalid. Please verify your credentials." : _status = 401
                _isClientValid = False
            End Try
            Return New StreamLabsResponse(_accessToken, _refreshToken, _accessCode, _response, _status, _isClientValid, _isConnected, _donation)
        End Function

        Public Class StreamLabsResponse
            Public ReadOnly Property AccessToken As String = ""
            Public ReadOnly Property RefreshToken As String = ""
            Public ReadOnly Property AccessCode As String = ""
            Public ReadOnly Property Response As String = ""
            Public ReadOnly Property Status As StatusCode = 401
            Public ReadOnly Property IsClientValid As Boolean = False
            Public ReadOnly Property IsConnected As Boolean = False
            Public ReadOnly Property Donation As Donation = New Donation

            Public Sub New()
            End Sub
            Protected Friend Sub New(_accessToken As String, _refreshToken As String, _accessCode As String, _response As String, _status As StatusCode, _isClientValid As Boolean, _isConnected As Boolean)
                AccessToken = _accessToken : RefreshToken = _refreshToken : AccessCode = _accessCode : Response = _response : Status = _status : IsClientValid = _isClientValid : IsConnected = _isConnected
            End Sub
            Protected Friend Sub New(_accessToken As String, _refreshToken As String, _accessCode As String, _response As String, _status As StatusCode, _isClientValid As Boolean, _isConnected As Boolean, _donation As Donation)
                AccessToken = _accessToken : RefreshToken = _refreshToken : AccessCode = _accessCode : Response = _response : Status = _status : IsClientValid = _isClientValid : IsConnected = _isConnected : Donation = _donation
            End Sub

            Public Enum StatusCode As Integer
                Ok = 200
                BadRequest = 400
                Unauthorized = 401
            End Enum
        End Class

        Public Class Donation
            Public ReadOnly Property Name As String = ""
            Public ReadOnly Property Identifier As String = ""
            Public ReadOnly Property Amount As String = "0"
            Public ReadOnly Property CurrencyCode As String = "INR"
            Public ReadOnly Property Message As String = ""
            Public ReadOnly Property DonationId As String = "-1"
            Public ReadOnly Property Status As Integer = 400
            Public ReadOnly Property MailDate As Date

            Public Sub New()
            End Sub
            Public Sub New(_name As String, _identifier As String, _amount As String, _currencyCode As String, _message As String, _donationId As String, _status As Integer, _mailDate As Date)
                Name = _name : Identifier = _identifier : Amount = _amount : CurrencyCode = _currencyCode : Message = _message : DonationId = _donationId : Status = _status : MailDate = _mailDate
            End Sub

            Public Overrides Function ToString() As String
                Dim _s = ""
                _s += "From: " & Name & " (Name)" & vbNewLine
                _s += "Amount recieved: " & Amount & " (Amount)" & vbNewLine
                _s += "Recieved on: " & MailDate & " (Date)" & vbNewLine
                _s += "Message: " & Message & " (Message)"
                Return _s
            End Function
        End Class

    End Class


    <Serializable()>
    Public Class Template
        Public Property TemplateName As String
        Public Property Name As String
        Public Property Amount As String
        Public Property Message As String
        Public Property IsMessageAvailable As Boolean = True
        Public Property Variables As Dictionary(Of String, String)

        Public Sub New()
        End Sub
        Public Sub New(_templateName As String, _name As String, _amount As String, _message As String, _dictVariables As Dictionary(Of String, String))
            TemplateName = _templateName : Name = _name : Amount = _amount : Message = _message : Variables = _dictVariables
            If _message = "" Then
                IsMessageAvailable = False
            End If
        End Sub

        Public Function GetTemplateVariables() As List(Of TemplateVariable)
            Dim _templateVariableList As New List(Of TemplateVariable)
            _templateVariableList.AddRange({New TemplateVariable("Name", Name, True), New TemplateVariable("Amount", Amount, True), New TemplateVariable("Message", Message, True)})
            If Variables IsNot Nothing Then
                For Each _var In Variables
                    _templateVariableList.Add(New TemplateVariable(_var.Key, _var.Value, False))
                Next
            End If
            Return _templateVariableList
        End Function

        Public Overrides Function ToString() As String
            Return TemplateName
        End Function
    End Class

    <Serializable()>
    Public Class TemplateVariable
        Public Property Name As String
        Public Property Value As String
        Public ReadOnly Property IsStaplesDefault As Boolean

        Public Sub New()
        End Sub
        Public Sub New(_name As String, _value As String, _staplesDefault As Boolean)
            Name = _name : Value = _value : IsStaplesDefault = _staplesDefault
        End Sub
    End Class


    <Serializable()>
    Public Class Email
        Public Property Template As Template
        Public Property EmailAddress As String
        Public Property Body As String
        Public Property MailDate As Date
        Public Property UID As Integer

        Public Sub New()
        End Sub
        Public Sub New(_email As String, _template As Template)
            EmailAddress = _email : Template = _template
        End Sub

        Public Overrides Function ToString() As String
            Return EmailAddress
        End Function
    End Class


    Public Class Scanner

        Private _scannerResponse As New ScannerResponse

        Public Function Scan(email As Email) As ScannerResponse
            Dim _templateVariables = email.Template.GetTemplateVariables
            Dim _scannerResults As New List(Of ScannerResult)
            For Each var As TemplateVariable In _templateVariables
                Dim _regexMatch As Match = Regex.Match(email.Body, var.Value)
                _scannerResults.Add(New ScannerResult(var.Name, _regexMatch.Groups(1).Value, var.IsStaplesDefault, _regexMatch.Success, email, email.Template))
            Next

            Return New ScannerResponse(_scannerResults)
        End Function

        Public Class ScannerResponse
            Public ReadOnly Property ScannerResults As List(Of ScannerResult)

            Public Sub New()
            End Sub
            Public Sub New(_scannerResults As List(Of ScannerResult))
                ScannerResults = _scannerResults
            End Sub
        End Class
    End Class

    Public Class ScannerResult
        Public ReadOnly Property Name As String
        Public ReadOnly Property Value As String
        Public ReadOnly Property IsSuccess As Boolean
        Public ReadOnly Property IsStaplesDefault As Boolean
        Public ReadOnly Property Email As Email
        Public ReadOnly Property Template As Template

        Public Sub New()
        End Sub
        Public Sub New(_name As String, _value As String, _staplesDefault As Boolean, _isSuccess As Boolean, _email As Email, _template As Template)
            Name = _name : Value = _value : IsStaplesDefault = _staplesDefault : IsSuccess = _isSuccess : Email = _email : Template = _template
        End Sub
    End Class

    <Serializable()>
    Public Class ScannerData
        Public ReadOnly Property Templates As List(Of Template)
        Public ReadOnly Property Emails As List(Of Email)

        Public Sub New()
        End Sub
        Public Sub New(ByRef _templates As List(Of Template), ByRef _emails As List(Of Email))
            Templates = _templates : Emails = _emails
        End Sub
    End Class

    Public Shared Function GetAllTemplates() As List(Of Template)
        Return Templates
    End Function

    Public Shared Function GetAllEmails() As List(Of Email)
        Return Emails
    End Function


    Public Shared Function CastEmail(_uid As Integer, _body As String, _mailDate As Date, _email As Email) As Email
        Return New Email(_email.EmailAddress, _email.Template) With {.UID = _uid, .Body = _body, .MailDate = _mailDate}
    End Function


    Public Shared Function AddTemplate(_templateName As String, _name As String, _amount As String, _message As String, Optional _dictVariables As Dictionary(Of String, String) = Nothing) As CommonResponse
            Dim _response As String = "" : Dim _status As Integer = 0
            Dim _template As Template = New Template(_templateName, _name, _amount, _message, _dictVariables)
            If _templateName.Length > 0 Then
                If Templates.Count > 0 Then
                    Dim t As Template = Templates.Find(Function(a)
                                                           If a.TemplateName = _templateName Then
                                                               Return True
                                                           End If
                                                           Return Nothing
                                                       End Function)
                    If Not t Is Nothing Then
                        _response = "Template already exist." : _status = 3
                    Else
                        Templates.Add(_template)
                        _response = "Template added successfully." : _status = 2
                        RaiseEvent TemplatesListChanged()
                    End If
                Else
                    Templates.Add(_template)
                    _response = "Template added successfully." : _status = 2
                    RaiseEvent TemplatesListChanged()
                End If
            End If
            Return New CommonResponse(_response, _status, _template, Templates.IndexOf(_template))
        End Function
    Public Shared Function AddTemplate(index As Integer, _templateName As String, _name As String, _amount As String, _message As String, Optional _dictVariables As Dictionary(Of String, String) = Nothing) As CommonResponse
        Dim _response As String = "" : Dim _status As Integer = 0
        Dim _template As Template = New Template(_templateName, _name, _amount, _message, _dictVariables)
        If _templateName.Length > 0 Then
            If Templates.Count > 0 Then
                Dim t As Template = Templates.Find(Function(a)
                                                       If a.TemplateName = _templateName Then
                                                           Return True
                                                       End If
                                                       Return Nothing
                                                   End Function)
                If Not t Is Nothing Then
                    _response = "Template already exist." : _status = 3
                Else
                    Templates.Insert(index, _template)
                    _response = "Template added successfully." : _status = 2
                    RaiseEvent TemplatesListChanged()
                End If
            Else
                Templates.Add(_template)
                _response = "Template added successfully." : _status = 2
                RaiseEvent TemplatesListChanged()
            End If
        End If
        Return New CommonResponse(_response, _status, _template, Templates.IndexOf(_template))
    End Function

    Public Shared Function RemoveTemplate(_templateName As String, Optional _update As Boolean = False) As CommonResponse
            Dim _response As String = "" : Dim _template As New Template : Dim _index As Integer = -1
            If _templateName.Length > 0 Then
                If Templates.Count > 0 Then
                    Dim t As Template = Templates.Find(Function(a)
                                                           If a.TemplateName = _templateName Then
                                                               Return True
                                                           End If
                                                           Return Nothing
                                                       End Function)
                    If Not t Is Nothing Then
                        _template = t : _index = Templates.IndexOf(_template)
                        If _update = False Then
                            Dim _emails As New List(Of Email)

                            For Each email In Emails
                                If email.Template.TemplateName = t.TemplateName Then
                                    _emails.Add(email)
                                End If
                            Next
                            For Each email In _emails
                                Emails.Remove(email)
                            Next
                        End If
                        Templates.Remove(t)
                        _response = "Template and associated Emails with it are removed."
                    Else
                        _response = "Template does not exist."
                    End If
                Else
                    _response = "Template does not exist."
                End If
            End If
            Return New CommonResponse(_response,, _template, _index)
        End Function
    Public Shared Function RemoveTemplate(index As Integer, Optional _update As Boolean = False) As CommonResponse
        Dim _response As String = "" : Dim _template As New Template : Dim _status As Integer
        If index >= 0 And index <= Templates.Count - 1 Then
            If Templates.Count > 0 Then
                Dim _templateName = Templates(index).TemplateName
                Dim t As Template = Templates.Find(Function(a)
                                                       If a.TemplateName = _templateName Then
                                                           Return True
                                                       End If
                                                       Return Nothing
                                                   End Function)
                If Not t Is Nothing Then
                    _template = t
                    Templates.Remove(t)
                    If _update = False Then
                        For Each email In Emails
                            If email.Template.TemplateName = t.TemplateName Then
                                Emails.Remove(email)
                            End If
                        Next
                    End If

                    _response = "Template and associated Emails with it are removed." : _status = 2
                Else
                    _response = "Template does not exist." : _status = 3
                End If
            Else
                _response = "Template does not exist." : _status = 3
            End If
        End If
        Return New CommonResponse(_response, _status, _template, index)
    End Function

    Public Shared Function GetLinkedEmail(_templateName As String) As CommonResponse
            Dim _response As String = "" : Dim _index As Integer = -1 : Dim _emails As New List(Of Email) : Dim _emailAddress As New List(Of String)
            If _templateName.Length > 0 Then
                If Templates.Count > 0 Then
                    Dim t As Template = Templates.Find(Function(a)
                                                           If a.TemplateName = _templateName Then
                                                               Return True
                                                           End If
                                                           Return Nothing
                                                       End Function)
                    If Not t Is Nothing Then
                        _index = Templates.IndexOf(t)
                        For Each email In Emails
                            If email.Template.TemplateName = t.TemplateName Then
                                _emails.Add(email) : _emailAddress.Add(email.EmailAddress)
                            End If
                        Next
                        _response = "Linked Emails with (" & _templateName & ") template are returned as object array of emails as List(Of Emails) and email address as String()."
                    Else
                        _response = "Template does not exist."
                    End If
                Else
                    _response = "Template does not exist."
                End If
            End If
            Return New CommonResponse(_response,, {_emails, _emailAddress.ToArray()}, _index)
        End Function

    Public Shared Function AddEmail(_email As String, _template As Template) As CommonResponse
        Dim _response As String : Dim _status As Integer : Dim _returnObject As New Email : Dim _index As Integer = -1
        If Templates.Contains(_template) Then
            If _email <> "" Then
                _returnObject = New Email(_email, _template)
                Emails.Add(_returnObject)
                _index = Emails.IndexOf(_returnObject)
                _response = "Email added successfully and linked with (" & _template.TemplateName & ") template." : _status = 2
            Else
                _response = "Email address cannot be empty." : _status = 3
            End If
        Else
            _response = "Email cannot be added. Template (" & _template.TemplateName & ") does not exist." : _status = 3
        End If
        Return New CommonResponse(_response, _status, _returnObject, _index)
    End Function
    Public Shared Function AddEmail(index As Integer, _email As String, _template As Template) As CommonResponse
        Dim _response As String : Dim _status As Integer : Dim _returnObject As New Email : Dim _index As Integer = -1
        If Templates.Contains(_template) Then
            If _email <> "" Then
                _returnObject = New Email(_email, _template)
                Emails.Insert(index, _returnObject)
                _index = Emails.IndexOf(_returnObject)
                _response = "Email added successfully and linked with (" & _template.TemplateName & ") template." : _status = 2
            Else
                _response = "Email address cannot be empty." : _status = 3
            End If
        Else
            _response = "Email cannot be added. Template (" & _template.TemplateName & ") does not exist." : _status = 3
        End If
        Return New CommonResponse(_response, _status, _returnObject, _index)
    End Function

    Public Shared Function GetEmail(_emailAddress As String) As Email
        Dim _email As New Email
        If _emailAddress.Length > 0 Then
            If Emails.Count > 0 Then
                Dim t As Email = Emails.Find(Function(a)
                                                 If a.EmailAddress = _emailAddress Then
                                                     Return True
                                                 End If
                                                 Return Nothing
                                             End Function)
                If Not t Is Nothing Then
                    _email = t
                End If
            End If
        End If
        Return _email
    End Function

    Public Shared Function RemoveEmail(_email As String) As CommonResponse
        Dim _response As String = ""
        If _email.Length > 0 Then
            If Emails.Count > 0 Then
                Dim t As Email = Emails.Find(Function(a)
                                                 If a.EmailAddress = _email Then
                                                     Return True
                                                 End If
                                                 Return Nothing
                                             End Function)
                If Not t Is Nothing Then
                    Emails.Remove(t)
                    _response = "Email is removed."
                Else
                    _response = "Email does not exist."
                End If
            Else
                _response = "Email does not exist."
            End If
        End If
        Return New CommonResponse(_response)
    End Function
    Public Shared Function RemoveEmail(index As Integer) As CommonResponse
            Dim _response As String = "" : Dim _email As New Email : Dim _status As Integer
            If index >= 0 And index <= Emails.Count - 1 Then
                If Emails.Count > 0 Then
                    Emails.RemoveAt(index)
                    _response = "Email is removed." : _status = 2
                Else
                    _response = "Email does not exist." : _status = 3
                End If
            Else
                _response = "Email does not exist." : _status = 3
            End If
            Return New CommonResponse(_response, _status, _email, index)
        End Function


    Public Shared Function UpdateTemplate(_oldTemplateName As String, _newTemplateName As String, _name As String, _amount As String, _message As String, Optional _dictVariables As Dictionary(Of String, String) = Nothing) As CommonResponse
        Dim _response As String = "" : Dim _status As Integer = 0 : Dim _template As New Template
        If _oldTemplateName.Length > 0 Then
            If Templates.Count > 0 Then
                Dim t As Template = Templates.Find(Function(a)
                                                       If a.TemplateName = _oldTemplateName Then
                                                           Return True
                                                       End If
                                                       Return Nothing
                                                   End Function)
                If Not t Is Nothing Then
                    For Each email In Emails
                        If email.Template.TemplateName = _oldTemplateName Then
                            email.Template.TemplateName = _newTemplateName
                        End If
                    Next
                    t.TemplateName = _newTemplateName : t.Name = _name : t.Amount = _amount : t.Message = _message : t.Variables = _dictVariables
                    _template = t
                End If
            End If
        End If
        Return New CommonResponse(_response, _status, _template, Templates.IndexOf(_template))
    End Function

    Public Shared Function UpdateEmail(_emailOld As String, _emailNew As String, _templateNew As Template) As CommonResponse
            Dim _response As String = "" : Dim _status As Integer = 0 : Dim _email As New Email
            If _emailOld.Length > 0 Then
                If Emails.Count > 0 Then
                    Dim t As Email = Emails.Find(Function(a)
                                                     If a.EmailAddress = _emailOld Then
                                                         Return True
                                                     End If
                                                     Return Nothing
                                                 End Function)
                    If Not t Is Nothing Then
                        If _emailNew <> "" Then
                            t.EmailAddress = _emailNew : _status = 2
                        Else
                            _response = "Email cannot be empty." : _status = 3
                        End If
                        If _templateNew IsNot Nothing Then
                            t.Template = _templateNew : _status = 2
                        Else
                            _response = "Template cannot be nothing." : _status = 3
                        End If
                        _email = t
                    Else
                        _response = "Email does not exist." : _status = 3
                    End If
                Else
                    _response = "Email does not exist." : _status = 3
                End If
            End If
            Return New CommonResponse(_response, _status, _email, Emails.IndexOf(_email))
        End Function

    Public Enum EmailDataUpdateFlags As Integer
        EmailOnly = 0
        TemplateOnly = 1
        Both = 2
    End Enum

    Public Shared Sub SaveScannerData(filePath As String)
            Dim fs As FileStream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite)
            Dim encoder As New Formatters.Binary.BinaryFormatter()
            encoder.Serialize(fs, New ScannerData(Templates, Emails))
            fs.Close()
        End Sub

    Public Shared Sub LoadScannerData(filePath As String)
        If File.Exists(filePath) Then
            Dim fsFile As FileStream = File.Open(filePath, FileMode.Open, FileAccess.Read)
            If fsFile.Length <> 0 Then
                Try
                    Dim decoder As Formatters.Binary.BinaryFormatter = New Formatters.Binary.BinaryFormatter()
                    Dim sd As ScannerData = CType(decoder.Deserialize(fsFile), ScannerData)
                    Templates.Clear() : Emails.Clear()
                    Templates = sd.Templates : Emails = sd.Emails
                    fsFile.Close()
                Finally
                    If (Not IsNothing(fsFile)) Then
                        fsFile.Dispose()
                    End If
                End Try
            End If
        End If
    End Sub

    Public Class CommonResponse
            Public ReadOnly Response As String = ""
            Public ReadOnly Result As String = ""
            Public ReadOnly Status As StatusCode = 0
            Public ReadOnly Index As Integer = -1
            Public ReadOnly ReturnObject As Object

            Public Sub New()
            End Sub
            Protected Friend Sub New(_response As String, Optional _status As StatusCode = 0, Optional _returnObject As Object = Nothing, Optional _index As Integer = 0, Optional _result As String = "")
                Response = _response : Status = _status : ReturnObject = _returnObject : Index = _index : Result = _result
            End Sub

            Public Enum StatusCode As Integer
                NotSet = 0
                Ok = 1
                Success = 2
                Failed = 3
            End Enum
        End Class

    End Class
