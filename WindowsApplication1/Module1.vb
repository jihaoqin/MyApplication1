Imports WindowsApplication1.Myunities
Imports MathNet.Numerics.LinearAlgebra.Double
Module Module1
    Sub Main()
        Dim alpha_0 As Double = Math.PI / 4
        Dim alpha_2 As Double = Math.PI / 2
        Dim L As Double = 100
        Dim r As Double = 20
        Dim lamda_max As Double = 0.2
        Dim angle As Double = Math.PI / 2
        'Dim alpha_1s As DenseVector = MyUnity.getAlphas(0.2, alpha_0, alpha_2, L / r)
        'Dim A As DenseMatrix = New DenseMatrix(2)
        'Dim bs As DenseMatrix
        'Dim solutions_set As List(Of DenseVector) = New List(Of DenseVector)
        'For i = 0 To alpha_1s.Count - 1
        '    A = MyUnity.getA(alpha_0, alpha_1s(i), alpha_2)
        '    Try
        '        bs = MyUnity.getBs(A, L / r, angle, lamda_max)
        '        Dim lamdas_set As DenseMatrix = 1 / (A.Inverse() * bs)
        '        For ii = 0 To lamdas_set.ColumnCount - 1
        '            Dim temp As DenseVector = DenseVector.OfArray(New Double() {alpha_1s(i), bs(0, ii),
        '                                                          bs(1, ii), lamdas_set(0, ii), lamdas_set(1, ii)})
        '            solutions_set.Add(temp)
        '        Next
        '    Catch ex As Exception
        '    End Try
        'Next
        'solutions_set = MyUnity.filterSolutionsSet(solutions_set, lamda_max)
        'Dim solution As DenseVector = MyUnity.getSolutionByCond(solutions_set)
        'Dim points_list As List(Of DenseVector) = MyUnity.getPointsListBySolution(alpha_0, alpha_2, solution, r, -1)
        MyUnity.getPointsList(alpha_0, alpha_2, L, r, angle, 1, lamda_max)
    End Sub
End Module
