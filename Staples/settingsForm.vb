Imports System.IO
Imports System.Net
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Text.RegularExpressions
Imports Staples.Staples

Public Class settingsForm

    Private Const StreamLabsRedirectURL As String = "http://127.0.0.1:55555/staples/streamlabs"
    Private Const StreamLabsauthorizationEndpoint As String = "https://streamlabs.com/api/v1.0/authorize"
    Private Const StreamLabsTokenEndpoint As String = "https://streamlabs.com/api/v1.0/token"


    Private Sub settingsForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        cmbboxProtocol.SelectedIndex = 0

        If My.Settings.password <> "" Then
            txtboxPassword.Text = New IMAPClient.Password(My.Settings.password, True).Show()
        End If


        If My.Settings.streamLabsStatus Then
            btnConnectStreamLabs.Visible = False
            lblStreamLabsStatus.Text = "Connected" : lblStreamLabsStatus.ForeColor = Color.Green
        Else
            btnConnectStreamLabs.Visible = True
        End If


        listScanEmailFrom.Items.Clear() : listScanTemplates.Items.Clear()

        LoadScannerData("scanner.staples")
        listScanTemplates.Items.AddRange(GetAllTemplates.ToArray())
        listScanEmailFrom.Items.AddRange(GetAllEmails.ToArray())

        If My.Settings.doneSettings <> False Then
            btnSave.Visible = False
        Else
            btnSave.Visible = True
        End If

    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If My.Settings.doneSettings <> True Then
            My.Settings.doneSettings = True
        End If
        Close()
    End Sub

    Private Sub btnAddEmail_Click(sender As Object, e As EventArgs) Handles btnAddEmail.Click
        If listScanTemplates.Items.Count > 0 Then
            Dim emailReply As New emailReplyDialog
            emailReply.cbTemplate.Items.AddRange(GetAllTemplates.ToArray())
            If emailReply.ShowDialog() = DialogResult.OK Then
                listScanEmailFrom.Items.Add(AddEmail(emailReply.txtboxEmail.Text, emailReply.cbTemplate.SelectedItem).ReturnObject)
            End If
            emailReply.Close()
        Else
            MsgBox("Add template before adding email.", MsgBoxStyle.Critical)
        End If
    End Sub

    Private Sub listScanEmailFrom_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles listScanEmailFrom.MouseDoubleClick
        Dim s = TryCast(sender, ListBox)
        If s.SelectedIndex <> -1 Then
            Dim emailReply As New emailReplyDialog
            Dim email = CType(s.SelectedItem, Staples.Email)
            emailReply.txtboxEmail.Text = email.EmailAddress
            emailReply.cbTemplate.Items.AddRange(GetAllTemplates.ToArray())
            emailReply.cbTemplate.SelectedItem = email.Template
            If emailReply.ShowDialog() = DialogResult.OK Then
                Dim i = s.SelectedIndex
                listScanEmailFrom.Items.RemoveAt(i)
                listScanEmailFrom.Items.Insert(i, UpdateEmail(email.EmailAddress, emailReply.txtboxEmail.Text, emailReply.cbTemplate.SelectedItem).ReturnObject)
            End If
            emailReply.Close()
        End If
    End Sub

    Private Sub btnRemoveEmail_Click(sender As Object, e As EventArgs) Handles btnRemoveEmail.Click
        If listScanEmailFrom.SelectedIndex <> -1 Then
            If MsgBox("Are you sure that you want to delete this email?", MsgBoxStyle.Critical Or MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                RemoveEmail(listScanEmailFrom.SelectedItem.EmailAddress)
                listScanEmailFrom.Items.Remove(listScanEmailFrom.SelectedItem)
            End If
        End If
    End Sub

    Private Sub btnAddTemplate_Click(sender As Object, e As EventArgs) Handles btnAddTemplate.Click
        Dim scanTemplate As New scanTemplateDialog
        If scanTemplate.ShowDialog() = DialogResult.OK Then
            listScanTemplates.Items.Add(AddTemplate(scanTemplate.txtboxTemplateName.Text, scanTemplate.txtboxName.Text, scanTemplate.txtboxAmount.Text, scanTemplate.txtboxMessage.Text).ReturnObject)
        End If
    End Sub

    Private Sub listScanTemplates_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles listScanTemplates.MouseDoubleClick
        Dim s = TryCast(sender, ListBox)
        If s.SelectedIndex <> -1 Then
            Dim scanTemplate As New scanTemplateDialog
            Dim t As Template = CType(s.SelectedItem, Template)
            scanTemplate.txtboxTemplateName.Text = t.TemplateName
            scanTemplate.txtboxName.Text = t.Name
            scanTemplate.txtboxAmount.Text = t.Amount
            scanTemplate.txtboxMessage.Text = t.Message
            If scanTemplate.ShowDialog() = DialogResult.OK Then
                Dim i = s.SelectedIndex
                listScanTemplates.Items.RemoveAt(i)
                listScanTemplates.Items.Insert(i, UpdateTemplate(t.TemplateName, scanTemplate.txtboxTemplateName.Text, scanTemplate.txtboxName.Text, scanTemplate.txtboxAmount.Text, scanTemplate.txtboxMessage.Text).ReturnObject) 'AddTemplate(i, scanTemplate.txtboxTemplateName.Text, scanTemplate.txtboxName.Text, scanTemplate.txtboxAmount.Text, scanTemplate.txtboxMessage.Text).ReturnObject)

            End If
            scanTemplate.Close()
        End If
    End Sub

    Private Sub btnRemoveTemplate_Click(sender As Object, e As EventArgs) Handles btnRemoveTemplate.Click
        If listScanTemplates.SelectedIndex <> -1 Then

            Dim t As Template = CType(listScanTemplates.SelectedItem, Template)
            Dim resp = GetLinkedEmail(t.TemplateName).ReturnObject
            Dim emails As List(Of Staples.Email) = resp(0) : Dim emailAddress As String() = resp(1)

            If MsgBox("This template is linked to: " & vbNewLine & Join(emailAddress, vbNewLine) & vbNewLine & vbNewLine & "It will also be deleted from Scan Emails list." & vbNewLine & "Are you sure that you want to delete (" & t.TemplateName & ") Template?", MsgBoxStyle.Critical Or MsgBoxStyle.OkCancel) = MsgBoxResult.Ok Then
                RemoveTemplate(t.TemplateName)
                For Each email In emails
                    listScanEmailFrom.Items.Remove(email)
                Next
                listScanTemplates.Items.Remove(t)

            End If
        End If
    End Sub

    Private Sub settingsForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        SaveScannerData("scanner.staples")
        My.Settings.password = New IMAPClient.Password(txtboxPassword.Text).EncryptedPass
        My.Settings.Save()
    End Sub

    Public Sub LogChanged(log As String)
        If chkLogs.Checked Then
            txtLogs.Text += log & vbNewLine
            txtLogs.Text.TrimEnd(vbNewLine)
            txtLogs.ScrollToCaret()
        End If
    End Sub

    Private Async Sub btnConnectStreamLabs_Click(sender As Object, e As EventArgs) Handles btnConnectStreamLabs.Click
        Dim resp As StreamLabs.StreamLabsResponse = Await frmMain.IStreamLabs.Connect(txtboxStreamLabsId.Text, txtboxStreamLabsSecret.Text)

        If resp.IsClientValid = False Or resp.Status = StreamLabs.StreamLabsResponse.StatusCode.Unauthorized Then
            lblStreamLabsStatus.Text = "Please verify your credentials" : lblStreamLabsStatus.ForeColor = Color.Salmon
            My.Settings.streamLabsStatus = False
            btnConnectStreamLabs.Visible = True
        ElseIf resp.Status = StreamLabs.StreamLabsResponse.StatusCode.Ok Then
            lblStreamLabsStatus.Text = "Connected" : lblStreamLabsStatus.ForeColor = Color.Green
            txtboxStreamLabsCode.Text = resp.AccessCode
            My.Settings.accessToken = resp.AccessToken
            My.Settings.refreshToken = resp.RefreshToken
            My.Settings.streamLabsStatus = True
            btnConnectStreamLabs.Visible = False
        End If
    End Sub

    Private Async Sub btnRefresh_Click(sender As Object, e As EventArgs) Handles btnRefresh.Click
        frmMain.IStreamLabs.LoadDetails(My.Settings.clientId, My.Settings.clientSecret, My.Settings.accessCode, My.Settings.accessToken, My.Settings.refreshToken)

        Dim resp As StreamLabs.StreamLabsResponse = Await frmMain.IStreamLabs.RefreshConnection

        If resp.IsClientValid = False Or resp.Status = StreamLabs.StreamLabsResponse.StatusCode.Unauthorized Then
            lblStreamLabsStatus.Text = "Please verify your credentials" : lblStreamLabsStatus.ForeColor = Color.Salmon
            My.Settings.streamLabsStatus = False
            btnConnectStreamLabs.Visible = True
        ElseIf resp.Status = StreamLabs.StreamLabsResponse.StatusCode.Ok Then
            lblStreamLabsStatus.Text = "Connect Refreshed" : lblStreamLabsStatus.ForeColor = Color.Green
            txtboxStreamLabsCode.Text = resp.AccessCode
            My.Settings.accessToken = resp.AccessToken
            My.Settings.refreshToken = resp.RefreshToken
            My.Settings.streamLabsStatus = True
        End If
    End Sub

    Private Sub btnReset_Click(sender As Object, e As EventArgs) Handles btnReset.Click
        If MsgBox("Are you sure?" & vbNewLine & "This change cannot be undone.", MsgBoxStyle.Critical Or MsgBoxStyle.OkCancel, "Reset Settings") = MsgBoxResult.Ok Then
            My.Settings.Reset()
            If My.Settings.doneSettings <> False Then
                btnSave.Visible = False
            Else
                btnSave.Visible = True
            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Process.Start("https://streamlabs.com/dashboard/#/apps/register")
    End Sub

    Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
        MsgBox("Not yet implemented.", MsgBoxStyle.Information, "Update")
    End Sub

    Private Sub btnDiscord_Click(sender As Object, e As EventArgs) Handles btnDiscord.Click
        Process.Start("https://discord.gg/ffkevXD")
    End Sub
End Class