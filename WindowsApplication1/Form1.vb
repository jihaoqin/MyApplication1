Imports SURFACEMACHINING
Imports CATStrSettingsTypeLib
Imports StrTypeLib
Imports SPATypeLib
Imports SMTypeLib
Imports SimulationTypeLib
Imports SIM
Imports CATRsc2
Imports ProductStructureTypeLib
Imports PRISMATICMACHINING
Imports PPR
Imports PCBITF
Imports PARTITF
Imports OSMInterfacesTypeLib
Imports NavigatorTypeLib
Imports MECMOD
Imports MANUFACTURING
Imports KnowledgewareTypeLib
Imports KinTypeLib
Imports INFITF
Imports HybridShapeTypeLib
Imports GenKwe
Imports FittingTypeLib
Imports ElectricalTypeLib
Imports ElecSchematicTypeLib
Imports DRAFTINGITF
Imports LAYOUT2DITF
Imports DNBSimulation
Imports DNBSimIO
Imports DNBSimAct
Imports DNBRobot
Imports DNBIgpResourceProgram
Imports DNBReporting
Imports DNBMHIItf
Imports DNBManufacturingLayoutItf
Imports DNBIgripSim
Imports DNBIgpTagPath
Imports SWKHumanModelingItf
Imports DNBPert
Imports DNBFastener
Imports DNBDpmItf
Imports DNBBIW
Imports DNBDevice
Imports DNBDeviceActivity
Imports DNBD5I
Imports DNBASY
Imports PROCESSITF
Imports ComponentsCatalogsTypeLib
Imports AnnotationTypeLib
Imports CATTooling
Imports CATStk
Imports CATSmInterfacesTypeLib
Imports CATSmarTeamInteg
Imports SHEITF
Imports CATSdeSetting
Imports CATSchematicTypeLib
Imports CATRsc
Imports CATRma
Imports CATRpmReporterTypeLib
Imports CATRdg
Imports CATPspPlantShipTypeLib
Imports CATOBM
Imports CATMultiCAD
Imports CATMat
Imports DNBIPD
Imports CATInstantCollabItf
Imports CATImm
Imports CATIdeSettings
Imports CATV4IInteropTypeLib
Imports CATHumanPackaging
Imports CATFunctSystem
Imports CATEdbTypeLib
Imports CATDrmRmsSettings
Imports CATDataExch
Imports BehaviorTypeLib
Imports CATAssemblyTypeLib
Imports CATArrangementTypeLib
Imports SAMITF
Imports CAT3DXml
Imports CATCompositesMat
Imports CATIA_APP_ITF
Imports AECRTypeLib
Imports System.IO
Imports MathNet.Numerics.LinearAlgebra.Double
Imports tools
Imports System
Imports WindowsApplication1.Myunities





Public Class Form1
    Dim CATIA As Application
    Dim partDocument1 As PartDocument


    Dim lamda_max As Double

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        '-----------------------------------------------------------------------
        '打开并获取CATIA
        '-----------------------------------------------------------------------

        Try
            CATIA = GetObject(, "CATIA.Application")
        Catch
            CATIA = CreateObject("CATIA.Application")
        End Try
        CATIA.Visible = True
        partDocument1 = CATIA.ActiveDocument

        lamda_max = 0.2
        TextBox1.Text = lamda_max.ToString()

    End Sub




    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim something As Type = partDocument1.GetType()
        '------------------
        ' 获取初始条件
        Dim myselection As Selection
        Dim axisCyc As HybridShapeAxisLine
        myselection = partDocument1.Selection
        myselection.Clear()
        Dim sFilter
        ReDim sFilter(0)
        sFilter(0) = "HybridShapeAxisLine"
        myselection.SelectElement2(sFilter, "请选择轴线", False)
        axisCyc = myselection.Item(1).Value

        Dim Workbench As SPAWorkbench = partDocument1.GetWorkbench("SPAWorkbench")
        Dim axis_points(8)
        Workbench.GetMeasurable(axisCyc).GetPointsOnCurve(axis_points)
        Dim A_inAxis As DenseVector = DenseVector.OfArray(
            New Double() {axis_points(0), axis_points(1), axis_points(2)})
        Dim B_inAxis As DenseVector = DenseVector.OfArray(
            New Double() {axis_points(6), axis_points(7), axis_points(8)})
        Dim AB_inAxis As DenseVector = B_inAxis - A_inAxis


        myselection.Clear()
        sFilter(0) = "Point"
        myselection.SelectElement2(sFilter, "请选择起点", False)
        Dim point_begin As Point = myselection.Item(1).Value
        Dim pos_pointb(2)
        Workbench.GetMeasurable(point_begin).GetPoint(pos_pointb)
        Dim pb As DenseVector = DenseVector.OfArray(New Double() {pos_pointb(0), pos_pointb(1), pos_pointb(2)})
        myselection.Clear()
        sFilter(0) = "Line"
        myselection.SelectElement2(sFilter, "请选择起点方向", False)
        Dim line_b As Line = myselection.Item(1).Value
        Dim direction_b(2)
        Workbench.GetMeasurable(line_b).GetDirection(direction_b)
        Dim dir_b As DenseVector = DenseVector.OfArray(New Double() {direction_b(0), direction_b(1), direction_b(2)})


        myselection.Clear()
        sFilter(0) = "Point"
        myselection.SelectElement2(sFilter, "请选择终点", False)
        Dim point_end As Point = myselection.Item(1).Value
        Dim pos_pointe(2)
        Workbench.GetMeasurable(point_end).GetPoint(pos_pointe)
        Dim pe As DenseVector = DenseVector.OfArray(New Double() {pos_pointe(0), pos_pointe(1), pos_pointe(2)})
        myselection.Clear()
        sFilter(0) = "Line"
        myselection.SelectElement2(sFilter, "请选择终点方向", False)
        Dim line_e As Line = myselection.Item(1).Value
        Dim direction_e(2)
        Workbench.GetMeasurable(line_e).GetDirection(direction_e)
        Dim dir_e As DenseVector = DenseVector.OfArray(New Double() {direction_e(0), direction_e(1), direction_e(2)})
        '------------------

        Dim O As DenseVector = A_inAxis + AB_inAxis * (AB_inAxis * (pb - A_inAxis)) / AB_inAxis.Norm(2) ^ 2
        Dim x As DenseVector = AB_inAxis.Normalize(2)
        If x * (pe - O) < 0 Then
            x = -1 * x
        End If
        Dim y As DenseVector = (pb - O).Normalize(2)
        Dim z As DenseVector = crossProduct(x, y)

        If x * dir_b < 0 Then
            dir_b = dir_b * -1
        End If
        Dim OriB As DenseMatrix = unitCoordinate()
        setOrigin(OriB, pb)

        Dim OriE As DenseMatrix = unitCoordinate()
        setOrigin(OriE, pe)

        Dim OriO As DenseMatrix = New DenseMatrix(4)
        setOrigin(OriO, O)
        setX(OriO, x)
        setY(OriO, y)
        setZ(OriO, z)
        Dim rotationFlag As Integer
        If crossProduct(pb, dir_b) * x > 0 Then
            rotationFlag = 1
        Else
            rotationFlag = -1
        End If

        Dim OB As DenseMatrix = OriO.Inverse() * OriB
        Dim OE As DenseMatrix = OriO.Inverse() * OriE

        Dim length As Double = OE(0, 3)
        Dim angle As Double = getAngelRad(-1 * rotationFlag, OE(2, 3), OE(1, 3)) '好好考虑下这个函数
        Dim alpha_b As Double = Math.Acos(Math.Abs(dir_b * AB_inAxis) / (dir_b.L2Norm * AB_inAxis.L2Norm))
        Dim alpha_e As Double = Math.Acos(Math.Abs(dir_e * AB_inAxis) / (dir_e.L2Norm * AB_inAxis.L2Norm))
        Dim R As Double = (pb - O).L2Norm
        Dim fac As HybridShapeFactory = partDocument1.Part.HybridShapeFactory
        Dim O_points_list As List(Of DenseVector) = MyUnity.getPointsList(alpha_b, alpha_e, length, R, angle, rotationFlag, lamda_max)
        Dim Ori_points_list As List(Of DenseVector) = New List(Of DenseVector)
        For Each O_point In O_points_list
            O_point = DenseVector.OfArray(New Double() {O_point(0), O_point(1), O_point(2), 1})
            Ori_points_list.Add(OriO * O_point)
        Next
        For i = 0 To O_points_list.Count - 1
            Ori_points_list(i) = DenseVector.OfArray(New Double() {Ori_points_list(i)(0), Ori_points_list(i)(1), Ori_points_list(i)(2)})
        Next
        For Each Ori_point_vec In Ori_points_list
            Dim point_new As HybridShapePointCoord = fac.AddNewPointCoord(Ori_point_vec(0), Ori_point_vec(1), Ori_point_vec(2))
            partDocument1.Part.Bodies.Item("PartBody").InsertHybridShape(point_new)
            partDocument1.Part.InWorkObject = point_new
        Next
        partDocument1.Part.Update()
    End Sub

    Private Function crossProduct(left As DenseVector, right As DenseVector) As DenseVector
        If (left.Count <> 3 OrElse right.Count <> 3) Then
            Dim Message As String = "Vectors must have a length of 3."
            Throw New Exception(Message)
        End If
        Dim result As DenseVector = DenseVector.OfArray(New Double() {0, 0, 0})

        result(0) = left(1) * right(2) - left(2) * right(1)
        result(1) = -left(0) * right(2) + left(2) * right(0)
        result(2) = left(0) * right(1) - left(1) * right(0)

        Return result
    End Function
    Private Function unitCoordinate() As DenseMatrix
        Dim matrix As DenseMatrix = New DenseMatrix(4)
        matrix(0, 0) = 1
        matrix(1, 1) = 1
        matrix(2, 2) = 1
        matrix(3, 3) = 1
        Return matrix
    End Function
    Private Sub setOrigin(ByRef matrix As DenseMatrix, origin As Double())
        If origin.Length = 3 Then
            origin = New Double() {origin(0), origin(1), origin(2), 1}
            matrix.SetColumn(3, origin)
        ElseIf origin.Length = 4 Then
            matrix.SetColumn(3, origin)
        Else
            Dim Message As String = "Vectors must have a length of 3 or 4."
            Throw New Exception(Message)
        End If

    End Sub
    Private Sub setOrigin(ByRef matrix As DenseMatrix, origin As DenseVector)
        Dim origin_Array As Double() = origin.ToArray()
        setOrigin(matrix, origin_Array)
    End Sub
    Private Function getOrigin(ByRef matrix As DenseMatrix) As DenseVector
        Dim origin As DenseVector = DenseVector.OfArray(New Double() {matrix(0, 3), matrix(1, 3), matrix(2, 3)})
        Return origin
    End Function
    Private Function getMatrixByOrigin(ByRef origin As DenseVector) As DenseMatrix
        Dim matrix As DenseMatrix = unitCoordinate()
        setOrigin(matrix, origin)
    End Function

    Private Sub setX(ByRef matrix As DenseMatrix, X As Double())
        If X.Length = 3 Then
            X = New Double() {X(0), X(1), X(2), 0}
            matrix.SetColumn(0, X)
        ElseIf X.Length = 4 Then
            matrix.SetColumn(0, X)
        Else
            Dim Message As String = "Vectors must have a length of 3 or 4."
            Throw New Exception(Message)
        End If
    End Sub
    Private Sub setX(ByRef matrix As DenseMatrix, X As DenseVector)
        Dim X_array As Double() = X.ToArray()
        setX(matrix, X_array)
    End Sub
    Private Function getX(ByRef matrix As DenseMatrix) As DenseVector
        Dim X As DenseVector = DenseVector.OfArray(New Double() {matrix(0, 0), matrix(1, 0), matrix(2, 0)})
        Return X
    End Function

    Private Sub setY(ByRef matrix As DenseMatrix, Y As Double())
        If Y.Length = 3 Then
            Y = New Double() {Y(0), Y(1), Y(2), 0}
            matrix.SetColumn(1, Y)
        ElseIf Y.Length = 4 Then
            matrix.SetColumn(1, Y)
        Else
            Dim Message As String = "Vectors must have a length of 3 or 4."
            Throw New Exception(Message)
        End If
    End Sub
    Private Sub setY(ByRef matrix As DenseMatrix, Y As DenseVector)
        Dim Y_array As Double() = Y.ToArray()
        setY(matrix, Y_array)
    End Sub
    Private Function getY(ByRef matrix As DenseMatrix) As DenseVector
        Dim Y As DenseVector = DenseVector.OfArray(New Double() {matrix(0, 1), matrix(1, 1), matrix(2, 1)})
        Return Y
    End Function

    Private Sub setZ(ByRef matrix As DenseMatrix, Z As Double())
        If Z.Length = 3 Then
            Z = New Double() {Z(0), Z(1), Z(2), 0}
            matrix.SetColumn(2, Z)
        ElseIf Z.Length = 4 Then
            matrix.SetColumn(2, Z)
        Else
            Dim Message As String = "Vectors must have a length of 3 or 4."
            Throw New Exception(Message)
        End If
    End Sub
    Private Sub setZ(ByRef matrix As DenseMatrix, Z As DenseVector)
        Dim Z_array As Double() = Z.ToArray()
        setZ(matrix, Z_array)
    End Sub
    Private Function getZ(ByRef matrix As DenseMatrix) As DenseVector
        Dim Z As DenseVector = DenseVector.OfArray(New Double() {matrix(0, 2), matrix(1, 2), matrix(2, 2)})
        Return Z
    End Function

    Private Function getAngelRad(rotationFlag As Double, y As Double, z As Double) As Double
        If y ^ 2 + z ^ 2 = 0 Then
            Dim message As String = "norm of input equals to 0"
            Throw New Exception(message)
        End If
        Dim angle As Double
        If y = 0 Then
            If z > 0 Then
                angle = Math.PI / 2
            Else
                angle = Math.PI / -2
            End If
        End If

        If y > 0 AndAlso z >= 0 Then
            angle = Math.Atan(z / y)
        ElseIf y < 0 AndAlso z >= 0 Then
            angle = Math.Atan(z / y) + Math.PI
        ElseIf y < 0 AndAlso z < 0 Then
            angle = Math.Atan(z / y) + Math.PI
        ElseIf y > 0 AndAlso z < 0 Then
            angle = Math.Atan(z / y) + Math.PI * 2
        End If
        If rotationFlag = 1 Then
            Return angle
        Else
            Return 2 * Math.PI - angle
        End If
    End Function

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        lamda_max = Convert.ToDouble(TextBox1.Text)
    End Sub
End Class


