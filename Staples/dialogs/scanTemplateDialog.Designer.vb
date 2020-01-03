<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class scanTemplateDialog
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
        Me.Label5 = New System.Windows.Forms.Label()
        Me.txtboxName = New System.Windows.Forms.TextBox()
        Me.txtboxAmount = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtboxMessage = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.btnSave = New System.Windows.Forms.Button()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtboxTemplateName = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(10, 46)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(39, 13)
        Me.Label5.TabIndex = 9
        Me.Label5.Text = "Name:"
        '
        'txtboxName
        '
        Me.txtboxName.Location = New System.Drawing.Point(116, 43)
        Me.txtboxName.MaxLength = 3276700
        Me.txtboxName.Name = "txtboxName"
        Me.txtboxName.Size = New System.Drawing.Size(232, 22)
        Me.txtboxName.TabIndex = 2
        '
        'txtboxAmount
        '
        Me.txtboxAmount.Location = New System.Drawing.Point(116, 71)
        Me.txtboxAmount.MaxLength = 3276700
        Me.txtboxAmount.Name = "txtboxAmount"
        Me.txtboxAmount.Size = New System.Drawing.Size(232, 22)
        Me.txtboxAmount.TabIndex = 3
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(10, 74)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(51, 13)
        Me.Label1.TabIndex = 9
        Me.Label1.Text = "Amount:"
        '
        'txtboxMessage
        '
        Me.txtboxMessage.Location = New System.Drawing.Point(116, 99)
        Me.txtboxMessage.MaxLength = 3276700
        Me.txtboxMessage.Name = "txtboxMessage"
        Me.txtboxMessage.Size = New System.Drawing.Size(232, 22)
        Me.txtboxMessage.TabIndex = 4
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(10, 102)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(55, 13)
        Me.Label2.TabIndex = 9
        Me.Label2.Text = "Message:"
        '
        'btnSave
        '
        Me.btnSave.Location = New System.Drawing.Point(273, 135)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New System.Drawing.Size(75, 23)
        Me.btnSave.TabIndex = 12
        Me.btnSave.Text = "Save"
        Me.btnSave.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(12, 131)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(255, 34)
        Me.Label3.TabIndex = 13
        Me.Label3.Text = "Please enter proper Regular Expression pattern with value as a first match."
        '
        'txtboxTemplateName
        '
        Me.txtboxTemplateName.Location = New System.Drawing.Point(116, 15)
        Me.txtboxTemplateName.Name = "txtboxTemplateName"
        Me.txtboxTemplateName.Size = New System.Drawing.Size(232, 22)
        Me.txtboxTemplateName.TabIndex = 1
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(10, 18)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(87, 13)
        Me.Label4.TabIndex = 9
        Me.Label4.Text = "Template Name:"
        '
        'scanTemplateDialog
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(367, 170)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.txtboxMessage)
        Me.Controls.Add(Me.txtboxTemplateName)
        Me.Controls.Add(Me.txtboxAmount)
        Me.Controls.Add(Me.txtboxName)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "scanTemplateDialog"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Scan Template Pattern"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label5 As Label
    Friend WithEvents txtboxName As TextBox
    Friend WithEvents txtboxAmount As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents txtboxMessage As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents btnSave As Button
    Friend WithEvents Label3 As Label
    Friend WithEvents txtboxTemplateName As TextBox
    Friend WithEvents Label4 As Label
End Class
