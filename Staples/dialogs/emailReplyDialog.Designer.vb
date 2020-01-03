<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class emailReplyDialog
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtboxEmail = New System.Windows.Forms.TextBox()
        Me.cbTemplate = New System.Windows.Forms.ComboBox()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(12, 46)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(88, 13)
        Me.Label6.TabIndex = 9
        Me.Label6.Text = "Select Template:"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 18)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(85, 13)
        Me.Label5.TabIndex = 7
        Me.Label5.Text = "E-mail Address:"
        '
        'txtboxEmail
        '
        Me.txtboxEmail.Location = New System.Drawing.Point(133, 15)
        Me.txtboxEmail.Name = "txtboxEmail"
        Me.txtboxEmail.Size = New System.Drawing.Size(232, 22)
        Me.txtboxEmail.TabIndex = 6
        '
        'cbTemplate
        '
        Me.cbTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbTemplate.FormattingEnabled = True
        Me.cbTemplate.Location = New System.Drawing.Point(133, 43)
        Me.cbTemplate.Name = "cbTemplate"
        Me.cbTemplate.Size = New System.Drawing.Size(232, 21)
        Me.cbTemplate.TabIndex = 10
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(290, 77)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(75, 23)
        Me.btnSave.TabIndex = 11
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(12, 82)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(235, 13)
        Me.Label1.TabIndex = 12
        Me.Label1.Text = "Ensure that email is linked with proper template."
        '
        'emailReplyDialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(384, 113)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.cbTemplate)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtboxEmail)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "emailReplyDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Scan Email"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label6 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents txtboxEmail As TextBox
    Friend WithEvents cbTemplate As ComboBox
    Friend WithEvents btnSave As Button
    Friend WithEvents Label1 As Label
End Class
