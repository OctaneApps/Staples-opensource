Public Class scanTemplateDialog
    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If String.IsNullOrWhiteSpace(txtboxTemplateName.Text) Then
            MsgBox("Template name cannot be empty.", MsgBoxStyle.Exclamation)
        Else
            DialogResult = DialogResult.OK
        End If
    End Sub
End Class