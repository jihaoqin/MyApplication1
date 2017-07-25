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
Imports WindowsApplication1.Myunities.MyUnity





Public Class Form1
    Dim CATIA As Application
    Dim partDocument1 As PartDocument
    Dim lamda_max As Double
    Dim distance_point As Double
    Dim frames_chusha As List(Of DenseMatrix)

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
        distance_point = 20
        TextBox2.Text = distance_point.ToString()
        Dim frames_chusha As List(Of DenseMatrix) = New List(Of DenseMatrix)
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
        sFilter(0) = "ZeroDim"
        myselection.SelectElement2(sFilter, "请选择起点", False)
        Dim point_begin As Object = myselection.Item(1).Value
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
        sFilter(0) = "ZeroDim"
        myselection.SelectElement2(sFilter, "请选择终点", False)
        Dim point_end As Object = myselection.Item(1).Value
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

        myselection.Clear()
        sFilter(0) = "BiDim"
        myselection.SelectElement2(sFilter, "请选择支撑面", False)
        Dim support_spline As HybridShapeAssemble = myselection.Item(1).Value
        '------------------

        Dim O As DenseVector = A_inAxis + AB_inAxis * (AB_inAxis * (pb - A_inAxis)) / AB_inAxis.Norm(2) ^ 2
        Dim x As DenseVector = AB_inAxis.Normalize(2)
        If x * (pe - O) < 0 Then
            x = -1 * x
        End If
        Dim y As DenseVector = (pb - O).Normalize(2)
        Dim z As DenseVector = MyUnity.crossProduct(x, y)

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
        If MyUnity.crossProduct(pb, dir_b) * x > 0 Then
            rotationFlag = 1
        Else
            rotationFlag = -1
        End If

        Dim OB As DenseMatrix = OriO.Inverse() * OriB
        Dim OE As DenseMatrix = OriO.Inverse() * OriE

        Dim length As Double = OE(0, 3)
        Dim angle As Double = getAngleRad(rotationFlag, OE(1, 3), OE(2, 3))
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
        Dim spline As HybridShapeSpline = fac.AddNewSpline()
        Dim partBody As Body = partDocument1.Part.Bodies.GetItem("PartBody")
        spline.SetClosing(0)
        spline.SetSplineType(0)
        spline.SetSupport(support_spline)
        For Each Ori_point_vec In Ori_points_list
            Dim point_new As HybridShapePointCoord = fac.AddNewPointCoord(Ori_point_vec(0), Ori_point_vec(1), Ori_point_vec(2))
            point_new.Compute()
            'partBody.InsertHybridShape(point_new)
            spline.AddPointWithConstraintExplicit(point_new, Nothing, -1, 1, Nothing, 0)
            'partDocument1.Part.InWorkObject = point_new
        Next
        spline.Compute()
        partDocument1.Part.Bodies.GetItem("PartBody").InsertHybridShape(spline)

        partDocument1.Part.InWorkObject = spline
        partDocument1.Part.Update()
    End Sub
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim partBody As Body = partDocument1.Part.Bodies.GetItem("PartBody")
        Dim my_selection As Selection = partDocument1.Selection
        'my_selection.Clear()
        Dim sFilter(0)
        sFilter(0) = "Point"
        my_selection.SelectElement2(sFilter, "请选择曲线的起始点", False)
        Dim point_0 As Point = my_selection.Item(1).Value

        my_selection.Clear()
        sFilter(0) = "MonoDim"
        my_selection.SelectElement2(sFilter, "请选择曲线", False)
        Dim curve As HybridShapeAssemble = my_selection.Item(1).Value

        my_selection.Clear()
        sFilter(0) = "BiDim"
        my_selection.SelectElement2(sFilter, "请选择芯模面", False)
        Dim mandrel As HybridShapeAssemble = my_selection.Item(1).Value

        my_selection.Clear()
        my_selection.SelectElement2(sFilter, "请选择出纱点包络面", False)
        Dim envelope As HybridShapeScaling = my_selection.Item(1).Value
        Dim points_luosha As List(Of Point) = New List(Of Point)
        Dim points_chusha As List(Of Point) = New List(Of Point)
        Dim frames_luosha As List(Of DenseMatrix) = New List(Of DenseMatrix)
        points_luosha = MyUnity.getPointListOnCurve(partDocument1, point_0, curve, distance_point)
        For i = 0 To points_luosha.Count - 1
            Dim point_i As Point = points_luosha(i)
            point_i.Compute()
            partBody.InsertHybridShape(point_i)
        Next
        points_chusha = MyUnity.getChushaPointList(partDocument1, points_luosha, curve, envelope)
        For Each point In points_chusha
            partBody.InsertHybridShape(point)
        Next
        frames_luosha = MyUnity.getFrameListOnSurface(partDocument1, points_luosha, mandrel, curve)
        frames_chusha = MyUnity.getChushaFrameFromLuosha(frames_luosha, points_chusha)
        partDocument1.Part.Update()
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        distance_point = Convert.ToDouble(TextBox2.Text)
    End Sub



    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        lamda_max = Convert.ToDouble(TextBox1.Text)
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim angles_zhuzhou As List(Of Double)
        Dim flag_rotation As Integer = 1
        Dim angle As Double
        Dim absolute_angles As List(Of Double) = New List(Of Double)
        For Each frame In frames_chusha
            angle = MyUnity.getAngleRad(flag_rotation, frame(1, 3), frame(2, 3))
            absolute_angles.Add(angle)
        Next
        angles_zhuzhou = MyUnity.connectAngles(absolute_angles)
        Dim frames_D_chusha As List(Of DenseMatrix) = New List(Of DenseMatrix) 'D代表动坐标系
        Dim D_J As DenseMatrix
        For i = 0 To frames_chusha.Count - 1  ' frames_chusha是在静坐标系I中
            D_J = MyUnity.rotx(angles_zhuzhou(i))
            frames_D_chusha.Add(D_J * frames_chusha(i))
        Next
    End Sub
End Class


