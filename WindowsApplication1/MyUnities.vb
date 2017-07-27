Imports MathNet.Numerics.LinearAlgebra.Double
Imports System.Exception
Imports MECMOD
Imports HybridShapeTypeLib
Imports SPATypeLib
Imports INFITF
Imports MathNet.Numerics.LinearAlgebra.Factorization
Namespace Myunities

    Public Class MyUnity
        Public Shared Function getChushaFrameFromLuosha(frames_luosha As List(Of DenseMatrix), points_chusha As List(Of Point)) As List(Of DenseMatrix)
            Dim frames_chusha As List(Of DenseMatrix) = frames_luosha
            Dim pos_chusha As DenseVector
            Dim point_i As Point
            Dim frame As DenseMatrix
            For i = 0 To frames_chusha.Count - 1
                frame = frames_luosha(i)
                point_i = points_chusha(i)
                point_i.Compute()
                pointToVector(points_chusha(i))
                pos_chusha = pointToVector(point_i)
                setOrigin(frame, pos_chusha)
                frames_chusha(i) = frame
            Next
            Return frames_chusha
        End Function
        Public Shared Function getChushaPointList(partdocument1 As PartDocument, points_luosha As List(Of Point),
                                                  curve As HybridShapeCurveExplicit, envelop As HybridShapeScaling) As List(Of Point)
            Dim hash_envelop As Integer = envelop.GetHashCode()
            Dim my_fact As HybridShapeFactory = partdocument1.Part.HybridShapeFactory
            Dim my_bench As SPATypeLib.SPAWorkbench = partdocument1.GetWorkbench("SPAWorkbench")
            Dim partBody As Body = partdocument1.Part.Bodies.Item(1)
            Dim tan_line As Line
            Dim intersection_i As HybridShapeIntersection
            Dim near_i As HybridShapeNear
            Dim point_i As Point
            Dim pos()
            ReDim pos(2)
            Dim points_chusha As List(Of Point) = New List(Of Point)
            For i = 0 To points_luosha.Count - 1
                tan_line = getTanLineByPointIndex(partdocument1, points_luosha, i, curve）
                tan_line.Compute()
                intersection_i = my_fact.AddNewIntersection(tan_line, envelop)
                partdocument1.Part.HybridBodies.Item(1).AppendHybridShape(intersection_i)
                intersection_i.Compute()
                near_i = my_fact.AddNewNear(intersection_i, points_luosha(i))
                near_i.Compute()
                partdocument1.Part.HybridBodies.Item(1).AppendHybridShape(near_i)
                my_bench.GetMeasurable(near_i).GetPoint(pos)
                point_i = my_fact.AddNewPointCoord(pos(0), pos(1), pos(2))
                point_i.Compute()
                points_chusha.Add(point_i)
            Next
            Return points_chusha
        End Function
        Public Shared Function getInterFrameListWithSurface(partDocument1 As PartDocument, frame_input As List(Of DenseMatrix),
                                                            surface As HybridShapeAssemble) As List(Of DenseMatrix)
            Dim my_fact As HybridShapeFactory = partDocument1.Part.HybridShapeFactory
        End Function
        Public Shared Function getFrameListOnSurface(partDocument1 As PartDocument, ByRef point_list As List(Of Point),
                                                     surface As HybridShapeAssemble, curve As HybridShapeCurveExplicit) As List(Of DenseMatrix)
            Dim my_fact As HybridShapeFactory = partDocument1.Part.HybridShapeFactory
            Dim frame_list As List(Of DenseMatrix) = New List(Of DenseMatrix)
            Dim normal_vec As DenseVector
            Dim tan_vec As DenseVector
            Dim third_vec As DenseVector
            Dim origin As DenseVector
            For i = 0 To point_list.Count - 1
                normal_vec = getNormalVecOut(partDocument1, point_list(i), surface)
                tan_vec = getTanVecByPointIndex(partDocument1, point_list, i, curve)
                third_vec = crossProduct(tan_vec, normal_vec)
                origin = pointToVector(point_list(i))
                Dim frame As DenseMatrix = New DenseMatrix(4)
                setX(frame, tan_vec)
                setY(frame, normal_vec)
                setZ(frame, third_vec)
                setOrigin(frame, origin)
                frame_list.Add(frame)
            Next
            Return frame_list
        End Function

        Public Shared Function getTanLineByPointIndex(partDocument1 As PartDocument, point_list As List(Of Point),
                                                      index As Integer, curve As HybridShapeCurveExplicit) As Line
            Dim nearlyTan_vec As DenseVector
            Dim my_fact As HybridShapeFactory = partDocument1.Part.HybridShapeFactory
            If index = 0 Then
                nearlyTan_vec = pointToVector(point_list(1)) - pointToVector(point_list(0))
            Else
                nearlyTan_vec = pointToVector(point_list(index)) - pointToVector(point_list(index - 1))
            End If
            Dim tan_vec As DenseVector
            Dim tan_line As HybridShapeLineTangency
            Dim dir()
            ReDim dir(2)
            tan_line = my_fact.AddNewLineTangency(curve, point_list.ElementAt(index), 100000, 0, False)
            tan_line.Compute()
            tan_line.GetDirection(dir)
            tan_vec = DenseVector.OfArray(New Double() {dir(0), dir(1), dir(2)})
            If tan_vec * nearlyTan_vec > 0 Then
                Return tan_line
            Else
                tan_line = my_fact.AddNewLineTangency(curve, point_list(index), 100000, 0, True)
                tan_line.Compute()
                Return tan_line
            End If
        End Function

        Private Shared Function getTanVecByPointIndex(partDocument1 As PartDocument, point_list As List(Of Point),
                                                      index As Integer, curve As HybridShapeCurveExplicit) As DenseVector
            Dim tan_vec As DenseVector
            Dim dir()
            ReDim dir(2)
            Dim tan_line As Line = getTanLineByPointIndex(partDocument1, point_list, index, curve)
            tan_line.Compute()
            tan_line.GetDirection(dir)
            tan_vec = DenseVector.OfArray(New Double() {dir(0), dir(1), dir(2)})
            Return tan_vec
        End Function

        Public Shared Function pointToVector(point As Point) As DenseVector
            Dim ordinate()
            ReDim ordinate(2)
            point.Compute()
            point.GetCoordinates(ordinate)
            Dim vec As DenseVector = DenseVector.OfArray(New Double() {ordinate(0), ordinate(1), ordinate(2)})
            Return vec
        End Function

        Public Shared Function appendMatrixList(ByRef content_to As List(Of DenseMatrix), ByRef content_from As List(Of DenseMatrix)) As List(Of DenseMatrix)
            For Each elem In content_from
                content_to.Add(elem)
            Next
            Return content_to
        End Function
        Private Shared Function getNormalVecOut(partDocument1 As PartDocument, point As Point, surface As HybridShapeAssemble) As DenseVector
            Dim my_fact As HybridShapeFactory = partDocument1.Part.HybridShapeFactory
            Dim line_normal As HybridShapeLineNormal = my_fact.AddNewLineNormal(surface, point, 10, 0, False)
            line_normal.Compute()
            Dim dir
            ReDim dir(2)
            line_normal.GetDirection(dir)
            Dim normal_vec As DenseVector = DenseVector.OfArray(New Double() {dir(0), dir(1), dir(2)})
            Dim point_ordinate()
            ReDim point_ordinate(2)
            point.Compute()
            point.GetCoordinates(point_ordinate)
            Dim point_vec As DenseVector = DenseVector.OfArray(New Double() {point_ordinate(0), point_ordinate(1), point_ordinate(2)})
            If normal_vec * point_vec < 0 Then
                normal_vec.Multiply(-1)
            End If
            Return normal_vec.Normalize(2)
        End Function


        Public Shared Function getPointListOnCurve(partDocument1 As PartDocument, point_begin As Point,
                                                   curve As HybridShapeCurveExplicit, distance As Double) As List(Of Point)
            Dim my_fact As HybridShapeFactory = partDocument1.Part.HybridShapeFactory
            Dim false_point As HybridShapePointOnCurve = my_fact.AddNewPointOnCurveFromPercent(curve, 0, False)
            false_point.Compute()
            Dim false_ordinate()
            ReDim false_ordinate(2)
            false_point.GetCoordinates(false_ordinate)
            Dim false_point_vec As DenseVector = DenseVector.OfArray(New Double() {false_ordinate(0), false_ordinate(1), false_ordinate(2)})

            Dim true_point As Point = my_fact.AddNewPointOnCurveFromPercent(curve, 0.5, True)
            true_point.Compute()
            Dim true_ordinate()
            ReDim true_ordinate(2)
            true_point.GetCoordinates(true_ordinate)
            Dim true_point_vec As DenseVector = DenseVector.OfArray(New Double() {true_ordinate(0), true_ordinate(1), true_ordinate(2)})

            point_begin.Compute()
            Dim begin_ordinate()
            ReDim begin_ordinate(2)
            point_begin.GetCoordinates(begin_ordinate)
            Dim begin_vec As DenseVector = DenseVector.OfArray(New Double() {begin_ordinate(0), begin_ordinate(1), begin_ordinate(2)})

            Dim flag As Boolean
            If (begin_vec - false_point_vec).L2Norm < (begin_vec - true_point_vec).L2Norm Then
                flag = False
            Else
                flag = True
            End If

            Dim my_bench As SPAWorkbench = partDocument1.GetWorkbench("SPAWorkbench")
            Dim length_curve = my_bench.GetMeasurable(curve).Length
            Dim num_point As Integer = length_curve / distance + 1
            Dim point_list As List(Of Point) = New List(Of Point)
            For i = 0 To num_point - 1
                Dim distance_i As Double = distance * i
                Dim point As Point = my_fact.AddNewPointOnCurveFromDistance(curve, distance_i, flag)
                point_list.Add(point)
            Next
            Return point_list
        End Function

        Public Shared Function getPointsList(alpha_0 As Double, alpha_2 As Double, L As Double, r As Double,
                                             angle As Double, rotationFlag As Integer, lamda_max As Double) As List(Of DenseVector)
            Dim alpha_1s As DenseVector = getAlphas(lamda_max, alpha_0, alpha_2, L / r)
            Dim A As DenseMatrix = New DenseMatrix(2)
            Dim bs As DenseMatrix
            Dim solutions_set As List(Of DenseVector) = New List(Of DenseVector)
            For i = 0 To alpha_1s.Count - 1
                A = MyUnity.getA(alpha_0, alpha_1s(i), alpha_2)
                Try
                    bs = MyUnity.getBs(A, L / r, angle, lamda_max)
                    Dim lamdas_set As DenseMatrix = 1 / (A.Inverse() * bs)
                    For ii = 0 To lamdas_set.ColumnCount - 1
                        Dim temp As DenseVector = DenseVector.OfArray(New Double() {alpha_1s(i), bs(0, ii),
                                                                  bs(1, ii), lamdas_set(0, ii), lamdas_set(1, ii)})
                        solutions_set.Add(temp)
                    Next
                Catch ex As Exception
                End Try
            Next
            solutions_set = MyUnity.filterSolutionsSet(solutions_set, lamda_max)
            Dim solution As DenseVector = MyUnity.getSolutionByCond(solutions_set)
            Dim points_list As List(Of DenseVector) = MyUnity.getPointsListBySolution(alpha_0, alpha_2, solution, r, rotationFlag)
            Return points_list
        End Function
        Public Shared Function getPointsListBySolution(alpha_0 As Double, alpha_2 As Double, solution As DenseVector, r As Double, rotationFlag As Integer) As List(Of DenseVector)
            Dim points_list As List(Of DenseVector) = New List(Of DenseVector)
            Dim num_points As Integer = 100
            Dim alpha_1 As Double = solution(0)
            Dim lamda_1 As Double = solution(3)
            Dim lamda_2 As Double = solution(4)
            Dim num_alpha_01 As Integer = Math.Abs(alpha_0 - alpha_1) / (Math.Abs(alpha_0 - alpha_1) + Math.Abs(alpha_1 - alpha_2)) * num_points
            Dim num_alpha_12 As Integer = num_points - num_alpha_01
            If num_alpha_01 < 3 Then
                num_alpha_01 = 3
            End If
            If num_alpha_12 < 3 Then
                num_alpha_12 = 3
            End If
            Dim alpha_01 As List(Of Double) = linspace(alpha_0, alpha_1, num_alpha_01).ToList()
            Dim alpha_12 As List(Of Double) = linspace(alpha_1, alpha_2, num_alpha_12).ToList()
            alpha_12.RemoveAt(0)
            For Each alpha_i In alpha_01
                Dim x_i As Double = r / lamda_1 * Math.Abs(1 / Math.Sin(alpha_i) - 1 / Math.Sin(alpha_0))
                Dim angle_0i As Double = 1 / lamda_1 * Math.Abs((Math.Log(Math.Tan(alpha_i / 2))) - Math.Log(Math.Tan(alpha_0 / 2)))
                Dim y_i As Double = r * Math.Cos(angle_0i * rotationFlag)
                Dim z_i As Double = r * Math.Sin(angle_0i * rotationFlag)
                points_list.Add(DenseVector.OfArray(New Double() {x_i, y_i, z_i}))
            Next
            Dim point_1 As DenseVector = points_list.Last()
            Dim angle_1 As Double = 1 / lamda_1 * Math.Abs((Math.Log(Math.Tan(alpha_1 / 2))) - Math.Log(Math.Tan(alpha_0 / 2)))
            For Each alpha_i In alpha_12
                Dim x_i As Double = point_1(0) + r / lamda_2 * Math.Abs(1 / Math.Sin(alpha_i) - 1 / Math.Sin(alpha_1))
                Dim angle_0i As Double = angle_1 + 1 / lamda_2 * Math.Abs(Math.Log(Math.Tan(alpha_i / 2)) - Math.Log(Math.Tan(alpha_1 / 2)))
                Dim y_i As Double = r * Math.Cos(angle_0i * rotationFlag)
                Dim z_i As Double = r * Math.Sin(angle_0i * rotationFlag)
                points_list.Add(DenseVector.OfArray(New Double() {x_i, y_i, z_i}))
            Next
            Return points_list
        End Function

        Public Shared Function getAlphas(lamda_max As Double, alpha_0 As Double, alpha_2 As Double, ratio As Double) As DenseVector
            Dim num_lamda As Double = 100
            Dim num_alpha As Double = 100
            Dim delta_lamda As Double = Math.Abs(lamda_max - 0) / num_lamda
            Dim lamda_range As DenseVector = linspace(0, lamda_max, num_lamda)
            lamda_range = getRangeByIndex(1, lamda_range.Count - 1, lamda_range)
            Dim alpha_range As DenseVector = linspace(0, Math.PI / 2, num_alpha)
            Dim delta_alpha As Double = Math.Abs(0 - Math.PI / 2) / num_alpha
            alpha_range = getRangeByIndex(1, alpha_range.Count - 2, alpha_range)
            For i = 0 To alpha_range.Count - 1
                If alpha_range(i) = alpha_0 Then
                    alpha_range(i) = alpha_0 - delta_alpha / 10
                ElseIf alpha_range(i) = alpha_2 Then
                    alpha_range(i) = alpha_2 - delta_alpha / 10
                End If
            Next
            Dim temp_alpha_range As List(Of Double) = New List(Of Double)
            For i = 0 To alpha_range.Count - 1
                If 1 / lamda_max * (Math.Abs(1 / Math.Sin(alpha_0) - 1 / Math.Sin(alpha_range(i))) +
                                             Math.Abs(1 / Math.Sin(alpha_range(i)) - 1 / Math.Sin(alpha_2))) <= ratio Then
                    temp_alpha_range.Add(alpha_range(i))
                End If
            Next
            If temp_alpha_range.Count = 0 Then
                Throw New Exception("no solution for alpha_1")
            End If
            alpha_range = DenseVector.OfArray(temp_alpha_range.ToArray())
            Return alpha_range
        End Function
        Public Shared Function getA(alpha_0 As Double, alpha_1 As Double, alpha_2 As Double) As DenseMatrix
            Dim A As DenseMatrix = New DenseMatrix(2)
            A(0, 0) = Math.Abs(1 / Math.Sin(alpha_0) - 1 / Math.Sin(alpha_1))
            A(0, 1) = Math.Abs(1 / Math.Sin(alpha_1) - 1 / Math.Sin(alpha_2))
            A(1, 0) = Math.Abs(Math.Log(Math.Tan(alpha_0 / 2)) - Math.Log(Math.Tan(alpha_1 / 2)))
            A(1, 1) = Math.Abs(Math.Log(Math.Tan(alpha_1 / 2)) - Math.Log(Math.Tan(alpha_2 / 2)))
            Return A
        End Function
        Public Shared Function getBs(A As DenseMatrix, ratio As Double, angle As Double, lamda_max As Double) As DenseMatrix
            Dim lamda1_min As Double = A(0, 0) / (ratio - A(0, 1) / lamda_max)
            Dim lamda1_max As Double = lamda_max
            Dim lamda1 As DenseVector = DenseVector.OfArray(New Double() {lamda1_min, lamda1_max})
            Dim b2s_range As DenseVector = ratio * A(1, 1) / A(0, 1) + (A(1, 0) - A(1, 1) * A(0, 0) / A(0, 1)) * 1 / lamda1
            Dim b2s_min As Double = b2s_range.Min()
            Dim b2s_max As Double = b2s_range.Max()
            Dim n_min As Integer = Math.Ceiling((b2s_min - angle) / (2 * Math.PI))
            Dim n_max As Integer = Math.Floor((b2s_max - angle) / (2 * Math.PI))
            If n_max < n_min Then
                Dim message As String = "No solution for angle between points"
                Throw New Exception(message)
            End If
            Dim bs_mat As DenseMatrix = New DenseMatrix(2, n_max - n_min + 1)
            For i = 0 To n_max - n_min
                bs_mat(0, i) = ratio
                bs_mat(1, i) = (n_min + i) * 2 * Math.PI + angle
            Next
            Return bs_mat
        End Function
        Public Shared Function filterSolutionsSet(origin As List(Of DenseVector), lamda_max As Double) As List(Of DenseVector)
            Dim output As List(Of DenseVector) = New List(Of DenseVector)
            For Each elem In origin
                If elem(3) <= lamda_max AndAlso elem(4) <= lamda_max Then
                    output.Add(elem)
                End If
            Next
            Return output
        End Function
        Public Shared Function getSolutionByCond(solutions_set As List(Of DenseVector)) As DenseVector

            Dim solutions_mat As DenseMatrix = listOfVecToMat(solutions_set)
            Dim min_angle As Double = solutions_mat.Row(2).Min()
            Dim min_angle_solutions As List(Of DenseVector) = New List(Of DenseVector)
            For Each ele In solutions_set
                If ele(2) = min_angle Then
                    min_angle_solutions.Add(ele)
                End If
            Next
            Dim min_angle_solutionsMat As DenseMatrix = listOfVecToMat(min_angle_solutions)
            Dim lamda_sum As DenseVector = min_angle_solutionsMat.Row(3) + min_angle_solutionsMat.Row(4)
            Dim index As Integer
            For i = 0 To lamda_sum.Count - 1
                Dim min_lamdaSum As Double = lamda_sum.Min()
                If lamda_sum(i) = min_lamdaSum Then
                    index = i
                End If
            Next
            Return min_angle_solutions(index)

        End Function
        Public Shared Function listOfVecToMat(list_vec As List(Of DenseVector)) As DenseMatrix
            Dim columns As Integer = list_vec.Count
            Dim rows As Integer = list_vec(0).Count
            Dim mat As DenseMatrix = New DenseMatrix(rows, columns)
            For i = 0 To columns - 1
                mat.SetColumn(i, list_vec(i).ToArray())
            Next
            Return mat
        End Function
        'Public Shared Function getAbsoluteAngle(frame As DenseMatrix, flag_rotation As Integer) As Double
        '    'flag_rotation = 1 代表沿着x轴逆时针， -1代表顺时针
        '    Dim y As Double = frame(1, 3)
        '    Dim z As Double = frame(2, 3)
        '    Dim angle As Double
        '    If y ^ 2 + z ^ 2 <= 0 Then
        '        Throw New Exception("infinite solutions in origin")
        '    ElseIf y = 0 AndAlso z > 0 Then
        '        angle = Math.PI / 2
        '    ElseIf y = 0 AndAlso z < 0 Then
        '        angle = Math.PI / -2
        '    End If
        '    If y > 0 AndAlso z >= 0 Then
        '        angle = Math.Atan(z / y)
        '    ElseIf y < 0 AndAlso z >= 0 Then
        '        angle = Math.Atan(z / y) + Math.PI / 2
        '    ElseIf y < 0 AndAlso z <= 0 Then
        '        angle = Math.Atan(z / y) + Math.PI / 2
        '    ElseIf y > 0 AndAlso z <= 0 Then
        '        angle = Math.Atan(z / y) + Math.PI * 2
        '    End If
        '    If flag_rotation = -1 Then
        '        angle = Math.PI * 2 - angle
        '    End If
        '    Return angle
        'End Function
        Private Shared Function linspace(valBegin As Double, valEnd As Double, num As Integer) As DenseVector
            Dim range As DenseVector = New DenseVector(num)
            Dim delta As Double = (valEnd - valBegin) / (num - 1)
            For i = 0 To num - 1
                range(i) = valBegin + delta * i
            Next i
            Return range
        End Function
        Private Shared Function getRangeByIndex(indBegin As Integer, indEnd As Integer, arrayVec As DenseVector) As DenseVector
            Dim result As DenseVector = New DenseVector(indEnd - indBegin + 1)
            For i = 0 To indEnd - indBegin
                result(i) = arrayVec(indBegin + i)
            Next
            Return result
        End Function
        Public Shared Function crossProduct(left As DenseVector, right As DenseVector) As DenseVector
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
        Public Shared Sub setOrigin(ByRef matrix As DenseMatrix, origin As Double())
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
        Public Shared Sub setOrigin(ByRef matrix As DenseMatrix, origin As DenseVector)
            Dim origin_Array As Double() = origin.ToArray()
            setOrigin(matrix, origin_Array)
        End Sub
        Public Shared Function getOrigin(ByRef matrix As DenseMatrix) As DenseVector
            Dim origin As DenseVector = DenseVector.OfArray(New Double() {matrix(0, 3), matrix(1, 3), matrix(2, 3)})
            Return origin
        End Function
        Public Shared Function getMatrixByOrigin(ByRef origin As DenseVector) As DenseMatrix
            Dim matrix As DenseMatrix = unitCoordinate()
            setOrigin(matrix, origin)
            Return matrix
        End Function
        Public Shared Function unitCoordinate() As DenseMatrix
            Dim matrix As DenseMatrix = New DenseMatrix(4)
            matrix(0, 0) = 1
            matrix(1, 1) = 1
            matrix(2, 2) = 1
            matrix(3, 3) = 1
            Return matrix
        End Function
        Public Shared Sub setX(ByRef matrix As DenseMatrix, X As Double())
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
        Public Shared Sub setX(ByRef matrix As DenseMatrix, X As DenseVector)
            Dim X_array As Double() = X.ToArray()
            setX(matrix, X_array)
        End Sub
        Public Shared Function getX(ByRef matrix As DenseMatrix) As DenseVector
            Dim X As DenseVector = DenseVector.OfArray(New Double() {matrix(0, 0), matrix(1, 0), matrix(2, 0)})
            Return X
        End Function

        Public Shared Sub setY(ByRef matrix As DenseMatrix, Y As Double())
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
        Public Shared Sub setY(ByRef matrix As DenseMatrix, Y As DenseVector)
            Dim Y_array As Double() = Y.ToArray()
            setY(matrix, Y_array)
        End Sub
        Public Shared Function getY(ByRef matrix As DenseMatrix) As DenseVector
            Dim Y As DenseVector = DenseVector.OfArray(New Double() {matrix(0, 1), matrix(1, 1), matrix(2, 1)})
            Return Y
        End Function

        Public Shared Sub setZ(ByRef matrix As DenseMatrix, Z As Double())
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
        Public Shared Sub setZ(ByRef matrix As DenseMatrix, Z As DenseVector)
            Dim Z_array As Double() = Z.ToArray()
            setZ(matrix, Z_array)
        End Sub
        Public Shared Function getZ(ByRef matrix As DenseMatrix) As DenseVector
            Dim Z As DenseVector = DenseVector.OfArray(New Double() {matrix(0, 2), matrix(1, 2), matrix(2, 2)})
            Return Z
        End Function
        Public Shared Function getAngleRad(rotationFlag As Double, y As Double, z As Double) As Double
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
        Public Shared Function connectAngles(ByVal absolute_angles As List(Of Double)) As List(Of Double)
            'Dim connected_angles As List(Of Double) = New List(Of Double)
            'connected_angles.Add(absolute_angles(0))
            'For i = 1 To absolute_angles.Count - 1
            '    Dim delta_angle As Double = absolute_angles(i) - absolute_angles(i - 1)
            '    If Math.Abs(delta_angle) < Math.PI Then
            '        connected_angles.Add(connected_angles(i - 1) + delta_angle)
            '    Else
            '        connected_angles.Add(connected_angles(i - 1) + delta_angle + 2 * Math.PI)
            '    End If
            'Next
            'Return connected_angles
            Dim connected_angles As List(Of Double) = New List(Of Double)
            connected_angles.Add(absolute_angles(0))
            Dim theta_1 As Double
            Dim theta_0 As Double
            For i = 1 To absolute_angles.Count - 1
                theta_1 = absolute_angles(i)
                theta_0 = connected_angles(i - 1)
                While theta_1 - theta_0 > Math.PI
                    theta_1 -= Math.PI * 2
                End While
                While theta_1 - theta_0 < -Math.PI
                    theta_1 += Math.PI * 2
                End While
                connected_angles.Add(theta_1 * 1)
            Next
            Return connected_angles
        End Function
        Public Shared Function rotx(angle_rad As Double) As DenseMatrix
            Dim result As DenseMatrix = unitCoordinate()
            result(1, 1) = Math.Cos(angle_rad)
            result(2, 1) = Math.Sin(angle_rad)
            result(1, 2) = Math.Sin(angle_rad) * -1
            result(2, 2) = Math.Cos(angle_rad)
            Return result
        End Function
        Public Shared Function quaternion2str(vec As DenseVector) As String
            Dim str As String
            str = "["
            For i = 0 To vec.Count - 1
                str = str + vec(i).ToString()
                If i < vec.Count - 1 Then
                    str = str + ","
                End If
            Next
            str = str + "]"
            Return str
        End Function
        Public Shared Sub output(writer As System.IO.StreamWriter, angles As List(Of Double), frames As List(Of DenseMatrix))
            Dim configration_str As String = "[0,0,0,0]"
            Dim frame As DenseMatrix = New DenseMatrix(4)
            Dim angle As Double
            For i = 0 To angles.Count - 1
                frame = frames(i)
                angle = angles(i)
                Dim quaternion_str As String = "[0.70711,0,0,-0.70711]" 'quaternion2str(rotMat2quatern(frame)) 0.70711 < 0, 0, -0.70711 >
                Dim pos_str As String = "[" + frame(0, 3).ToString() + "," + frame(1, 3).ToString() + "," + frame(2, 3).ToString() + "]"
                Dim angle_str As String = "[" + (angle * 180 / Math.PI / 10).ToString() + "*(-1),9E+09,9E+09,9E+09,9E+09,9E+09]"
                Dim data_str As String = "[" + pos_str + "," + quaternion_str + "," + configration_str + "," + angle_str + "]"
                If i = 0 Then
                    writer.WriteLine("[")
                    writer.WriteLine(data_str + ",")
                    Continue For
                End If
                If i = angles.Count - 1 Then
                    writer.WriteLine(data_str)
                    writer.WriteLine("];")
                    writer.WriteLine("Total:" + angles.Count.ToString())
                    writer.WriteLine("angle:" + ((angles(angles.Count - 1) - angles(0)) * 180 / Math.PI / 10).ToString() + "*10")
                    Continue For
                End If
                writer.WriteLine(data_str + ",")
            Next
        End Sub

        Public Shared Function rotMat2quatern(R As DenseMatrix) As DenseVector
            Dim K As DenseMatrix = New DenseMatrix(4)
            K(0, 0) = (1 / 3) * (R(0, 0) - R(1, 1) - R(2, 2))
            K(0, 1) = (1 / 3) * (R(1, 0) + R(0, 1))
            K(0, 2) = (1 / 3) * (R(2, 0) + R(0, 2))
            K(0, 3) = (1 / 3) * (R(1, 2) - R(2, 1))
            K(1, 0) = (1 / 3) * (R(1, 0) + R(0, 1))
            K(1, 1) = (1 / 3) * (R(1, 1) - R(0, 0) - R(2, 2))
            K(1, 2) = (1 / 3) * (R(2, 1) + R(1, 2))
            K(1, 3) = (1 / 3) * (R(2, 0) - R(0, 2))
            K(2, 0) = (1 / 3) * (R(2, 0) + R(0, 2))
            K(2, 1) = (1 / 3) * (R(2, 1) + R(1, 2))
            K(2, 2) = (1 / 3) * (R(2, 2) - R(0, 0) - R(1, 1))
            K(2, 3) = (1 / 3) * (R(0, 1) - R(1, 0))
            K(3, 0) = (1 / 3) * (R(1, 2) - R(2, 1))
            K(3, 1) = (1 / 3) * (R(2, 0) - R(0, 2))
            K(3, 2) = (1 / 3) * (R(0, 1) - R(1, 0))
            K(3, 3) = (1 / 3) * (R(0, 0) + R(1, 1) + R(2, 2))
            Dim e As Evd(Of Double) = K.Evd()
            Dim v As DenseMatrix = e.EigenVectors
            Dim q As DenseVector = New DenseVector(4)
            q(0) = v(3, 3)
            q(1) = v(0, 3)
            q(2) = v(1, 3)
            q(3) = v(2, 3)
            Return q
        End Function

    End Class

End Namespace
