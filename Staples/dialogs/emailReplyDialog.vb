Public Class emailReplyDialog
    Private Sub emailReplyDialog_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If cbTemplate.SelectedIndex = -1 Then
            cbTemplate.SelectedIndex = 0
        End If
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If String.IsNullOrWhiteSpace(txtboxEmail.Text) Then
            MsgBox("Email address cannot be empty.", MsgBoxStyle.Exclamation)
        Else
            DialogResult = DialogResult.OK
        End If
    End Sub
End Class