Imports System.Reflection
Imports System.IO
Public Class frmMain
    Const CLOSE_SIZE As Integer = 12
    Public Sub New()

        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        For Each mTabPage As TabPage In TabControlList.TabPages
            For Each ctl As Control In mTabPage.Controls
                If TypeOf ctl Is TreeView Then
                    Dim mTreeView As TreeView = CType(ctl, TreeView)
                    mTreeView.Nodes.Add(mTabPage.Text)

                    mTreeView.ShowLines = True
                    mTreeView.ShowPlusMinus = True
                    mTreeView.ExpandAll()
                End If
            Next
        Next
        msg.Text = ""
        TabControlDetail.TabPages.Clear()
        TabControlDetail.DrawMode = TabDrawMode.OwnerDrawFixed
        TabControlDetail.Padding = New System.Drawing.Point(CLOSE_SIZE, 0)
        'TabControlDetail.DrawItem += New DrawItemEventHandler(this.tabControl2_DrawItem)
        'TabControlDetail.MouseDown += New System.Windows.Forms.MouseEventHandler(this.tabControl2_MouseDown)
        AddTabForm()
    End Sub
    Public Sub AddTabForm()
        Dim paths() As String = Directory.GetFiles(Application.StartupPath & "\DllTabForms", "*.dll")
        Dim RootNode As TreeNode = Me.TreeViewMe.Nodes(0)
        For Each path As String In paths

            Try
                Dim DllAssembly As Assembly = Assembly.LoadFrom(path)
                Dim DllTypes() As Type = DllAssembly.GetTypes()
                For Each DllType As Type In DllTypes
                    'Dim DllPropertyInfo As PropertyInfo = DllType.GetProperty("FormGUID")
                    If Not DllType.GetInterface("IRecognize") Is Nothing Then
                        Dim DllTabForm As Object = DllAssembly.CreateInstance(DllType.FullName)
                        'Dim IsMainForm As Boolean = CallByName(DllTabForm, "IsMainForm", CallType.Get)
                        Dim FormName As String = CallByName(DllTabForm, "FormName", CallType.Get)
                        Dim ChildNode As TreeNode = RootNode.Nodes.Add(FormName)
                        ChildNode.Tag = path & "|" & DllType.FullName

                    End If
                Next
            Catch ex As Exception
                Me.msg.Text = "错误信息：" & ex.Message
            End Try
        Next

    End Sub

    Private Sub TabControlDetail_DrawItem(sender As Object, e As DrawItemEventArgs)
        Try
            Dim myTabRect As Rectangle = TabControlDetail.GetTabRect(e.Index)
            '先添加TabPage属性   
            e.Graphics.DrawString(TabControlDetail.TabPages(e.Index).Text, Me.TabControlDetail.Font, SystemBrushes.ControlText, myTabRect.X + 2, myTabRect.Y + 2)

            '再画一个矩形框
            Using p As Pen = New Pen(Color.Black)
                myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2)
                myTabRect.Width = CLOSE_SIZE
                myTabRect.Height = CLOSE_SIZE
                e.Graphics.DrawRectangle(p, myTabRect)
            End Using

            '填充矩形框
            Dim recColor As Color = IIf(e.State = DrawItemState.Selected, Color.Orange, Color.DarkGray)
            Using b As Brush = New SolidBrush(recColor)
                e.Graphics.FillRectangle(b, myTabRect)
            End Using
            '画关闭符号
            Using p As Pen = New Pen(Color.White)
                '画"/"线
                Dim p1 As Point = New Point(myTabRect.X + 3, myTabRect.Y + 3)
                Dim p2 As Point = New Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + myTabRect.Height - 3)
                e.Graphics.DrawLine(p, p1, p2)

                '画"/"线
                Dim p3 As Point = New Point(myTabRect.X + 3, myTabRect.Y + myTabRect.Height - 3)
                Dim p4 As Point = New Point(myTabRect.X + myTabRect.Width - 3, myTabRect.Y + 3)
                e.Graphics.DrawLine(p, p3, p4)
            End Using

            e.Graphics.Dispose()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub TabControlDetail_MouseDown(sender As Object, e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            Dim x As Integer = e.X
            Dim y As Integer = e.Y

            '计算关闭区域   
            Dim myTabRect As Rectangle = TabControlDetail.GetTabRect(TabControlDetail.SelectedIndex)

            myTabRect.Offset(myTabRect.Width - (CLOSE_SIZE + 3), 2)
            myTabRect.Width = CLOSE_SIZE
            myTabRect.Height = CLOSE_SIZE

            '如果鼠标在区域内就关闭选项卡   
            Dim isClose As Boolean = (x > myTabRect.X) And (x < myTabRect.Right) And (y > myTabRect.Y) And (y < myTabRect.Bottom)
            If isClose Then TabControlDetail.TabPages.Remove(TabControlDetail.SelectedTab)
        End If
    End Sub

    Private Sub TreeViewMe_AfterSelect(sender As Object, e As TreeViewEventArgs)

    End Sub

    Private Sub TreeViewMe_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs)
        Try
            If TreeViewMe.SelectedNode.Level > 0 Then
                For Each DetailPage As TabPage In Me.TabControlDetail.TabPages
                    If DetailPage.Text = TreeViewMe.SelectedNode.Text Then
                        Me.TabControlDetail.SelectedTab() = DetailPage
                        Exit Sub
                    End If
                Next

                Dim NodeInfo() As String = Split(TreeViewMe.SelectedNode.Tag, "|")
                Dim DllAssembly As Assembly = Assembly.LoadFile(NodeInfo(0))
                Dim DllTabForm As Object = DllAssembly.CreateInstance(NodeInfo(1))
                Dim f As Form = CType(DllTabForm, Form)
                f.FormBorderStyle = FormBorderStyle.None
                f.AutoScroll = True
                f.AutoScrollMinSize = New Size(TabControlDetail.Width * 0.8, TabControlDetail.Height * 0.8)
                f.Dock = DockStyle.Fill
                f.TopLevel = False
                f.Show()
                Dim mTab As New TabPage
                mTab.Text = TreeViewMe.SelectedNode.Text
                mTab.AutoScroll = True
                mTab.Controls.Add(f)

                Me.TabControlDetail.Font = New Font("微软雅黑", 10.5)
                Me.TabControlDetail.TabPages.Add(mTab)
                Me.TabControlDetail.SelectedTab() = mTab
            End If
        Catch ex As Exception
            msg.Text = ex.Message
        End Try
    End Sub
End Class
