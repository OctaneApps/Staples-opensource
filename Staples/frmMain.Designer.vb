<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.Label18 = New System.Windows.Forms.Label()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnSettings = New System.Windows.Forms.Button()
        Me.Label22 = New System.Windows.Forms.Label()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.statusIndicator = New System.Windows.Forms.PictureBox()
        Me.rtbTest = New System.Windows.Forms.RichTextBox()
        Me.statusTextIndicator = New System.Windows.Forms.PictureBox()
        Me.tmrFetchEmail = New System.Windows.Forms.Timer(Me.components)
        Me.txtLogs = New System.Windows.Forms.TextBox()
        Me.tmrScanner = New System.Windows.Forms.Timer(Me.components)
        Me.tmrInternet = New System.Windows.Forms.Timer(Me.components)
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.statusIndicator, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.statusTextIndicator, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'Label18
        '
        Me.Label18.AutoSize = True
        Me.Label18.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label18.Location = New System.Drawing.Point(12, 17)
        Me.Label18.Name = "Label18"
        Me.Label18.Size = New System.Drawing.Size(61, 13)
        Me.Label18.TabIndex = 8
        Me.Label18.Text = "Donations"
        '
        'btnStart
        '
        Me.btnStart.Location = New System.Drawing.Point(217, 12)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(75, 23)
        Me.btnStart.TabIndex = 9
        Me.btnStart.Text = "Start"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'btnSettings
        '
        Me.btnSettings.Location = New System.Drawing.Point(298, 12)
        Me.btnSettings.Name = "btnSettings"
        Me.btnSettings.Size = New System.Drawing.Size(75, 23)
        Me.btnSettings.TabIndex = 10
        Me.btnSettings.Text = "Settings"
        Me.btnSettings.UseVisualStyleBackColor = True
        '
        'Label22
        '
        Me.Label22.AutoSize = True
        Me.Label22.Font = New System.Drawing.Font("Segoe UI", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label22.ForeColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(83, Byte), Integer), CType(CType(173, Byte), Integer), CType(CType(193, Byte), Integer))
        Me.Label22.Location = New System.Drawing.Point(32, 419)
        Me.Label22.Name = "Label22"
        Me.Label22.Size = New System.Drawing.Size(76, 20)
        Me.Label22.TabIndex = 21
        Me.Label22.Text = "CodeLaps"
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = Global.Staples.My.Resources.Resources.logo
        Me.PictureBox1.Location = New System.Drawing.Point(12, 417)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(20, 24)
        Me.PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.PictureBox1.TabIndex = 20
        Me.PictureBox1.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(114, 421)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(10, 13)
        Me.Label1.TabIndex = 23
        Me.Label1.Text = "|"
        '
        'statusIndicator
        '
        Me.statusIndicator.BackColor = System.Drawing.Color.Transparent
        Me.statusIndicator.Location = New System.Drawing.Point(129, 424)
        Me.statusIndicator.Name = "statusIndicator"
        Me.statusIndicator.Size = New System.Drawing.Size(11, 11)
        Me.statusIndicator.TabIndex = 24
        Me.statusIndicator.TabStop = False
        '
        'rtbTest
        '
        Me.rtbTest.BackColor = System.Drawing.Color.White
        Me.rtbTest.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.rtbTest.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rtbTest.ForeColor = System.Drawing.Color.Black
        Me.rtbTest.Location = New System.Drawing.Point(15, 41)
        Me.rtbTest.Name = "rtbTest"
        Me.rtbTest.ReadOnly = True
        Me.rtbTest.Size = New System.Drawing.Size(358, 370)
        Me.rtbTest.TabIndex = 25
        Me.rtbTest.Text = ""
        '
        'statusTextIndicator
        '
        Me.statusTextIndicator.BackColor = System.Drawing.Color.Transparent
        Me.statusTextIndicator.Location = New System.Drawing.Point(144, 422)
        Me.statusTextIndicator.Name = "statusTextIndicator"
        Me.statusTextIndicator.Size = New System.Drawing.Size(623, 15)
        Me.statusTextIndicator.TabIndex = 24
        Me.statusTextIndicator.TabStop = False
        '
        'tmrFetchEmail
        '
        Me.tmrFetchEmail.Interval = 1
        '
        'txtLogs
        '
        Me.txtLogs.Location = New System.Drawing.Point(385, 12)
        Me.txtLogs.Multiline = True
        Me.txtLogs.Name = "txtLogs"
        Me.txtLogs.ReadOnly = True
        Me.txtLogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLogs.Size = New System.Drawing.Size(382, 399)
        Me.txtLogs.TabIndex = 27
        '
        'tmrScanner
        '
        Me.tmrScanner.Enabled = True
        Me.tmrScanner.Interval = 1
        '
        'tmrInternet
        '
        Me.tmrInternet.Enabled = True
        Me.tmrInternet.Interval = 2000
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(787, 448)
        Me.Controls.Add(Me.txtLogs)
        Me.Controls.Add(Me.rtbTest)
        Me.Controls.Add(Me.statusTextIndicator)
        Me.Controls.Add(Me.statusIndicator)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label22)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.btnSettings)
        Me.Controls.Add(Me.btnStart)
        Me.Controls.Add(Me.Label18)
        Me.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "frmMain"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Staples"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.statusIndicator, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.statusTextIndicator, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label18 As Label
    Friend WithEvents btnStart As Button
    Friend WithEvents btnSettings As Button
    Friend WithEvents Label22 As Label
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents Label1 As Label
    Friend WithEvents statusIndicator As PictureBox
    Friend WithEvents rtbTest As RichTextBox
    Friend WithEvents statusTextIndicator As PictureBox
    Friend WithEvents tmrFetchEmail As Timer
    Friend WithEvents txtLogs As TextBox
    Friend WithEvents tmrScanner As Timer
    Friend WithEvents tmrInternet As Timer
End Class
