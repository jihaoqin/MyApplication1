Imports MathNet.Numerics.LinearAlgebra.Double
Imports System.Exception
Namespace Myunities

    Public Class MyUnity

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
            Dim points_list As List(Of DenseVector) = MyUnity.getPointsListBySolution(alpha_0, alpha_2, solution, r, -1)
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

    End Class

End Namespace
