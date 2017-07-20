Imports MathNet.Numerics.LinearAlgebra.Double

Public Class coordinateMatrix
    Inherits DenseMatrix
    Public Sub New()
        MyBase.New(4)
        MyBase.SetColumn(0, New Double() {1, 0, 0, 0})
        MyBase.SetColumn(1, New Double() {0, 1, 0, 0})
        MyBase.SetColumn(2, New Double() {0, 0, 1, 0})
        MyBase.SetColumn(3, New Double() {0, 0, 0, 1})
    End Sub
    Public Sub New(ByRef matrix As DenseMatrix)
        MyBase.New(4)
        If matrix.RowCount <> 4 OrElse matrix.ColumnCount <> 4 Then
            Dim message As String = "initialization with densematrix of which size is not equal to 4."
            Throw New Exception(message)
        End If
        MyBase.SetSubMatrix(0, 0, matrix)
    End Sub
    Public Sub New(ByRef matrix As coordinateMatrix)
        MyBase.New(4)
        setOrigin(matrix.getOrigin())
        setX(matrix.getX())
        setY(matrix.getY())
        setZ(matrix.getZ())
    End Sub
    Public Sub setOrigin(origin As Double())
        If origin.Length = 3 Then
            origin = New Double() {origin(0), origin(1), origin(2), 1}
            MyBase.SetColumn(3, origin)
        ElseIf origin.Length = 4 Then
            MyBase.SetColumn(3, origin)
        Else
            Dim Message As String = "Vectors must have a length of 3 or 4."
            Throw New Exception(Message)
        End If

    End Sub
    Public Sub setOrigin(origin As DenseVector)
        Dim origin_Array As Double() = origin.ToArray()
        setOrigin(origin_Array)
    End Sub

    Public Sub setX(X As Double())
        If X.Length = 3 Then
            X = New Double() {X(0), X(1), X(2), 0}
            MyBase.SetColumn(0, X)
        ElseIf X.Length = 4 Then
            MyBase.SetColumn(0, X)
        Else
            Dim Message As String = "Vectors must have a length of 3 or 4."
            Throw New Exception(Message)
        End If
    End Sub
    Public Sub setX(X As DenseVector)
        Dim X_array As Double() = X.ToArray()
        setX(X_array)
    End Sub

    Public Sub setY(Y As Double())
        If Y.Length = 3 Then
            Y = New Double() {Y(0), Y(1), Y(2), 0}
            MyBase.SetColumn(1, Y)
        ElseIf Y.Length = 4 Then
            MyBase.SetColumn(1, Y)
        Else
            Dim Message As String = "Vectors must have a length of 3 or 4."
            Throw New Exception(Message)
        End If
    End Sub
    Public Sub setY(Y As DenseVector)
        Dim Y_array As Double() = Y.ToArray()
        setY(Y_array)
    End Sub

    Public Sub setZ(Z As Double())
        If Z.Length = 3 Then
            Z = New Double() {Z(0), Z(1), Z(2), 0}
            MyBase.SetColumn(2, Z)
        ElseIf Z.Length = 4 Then
            MyBase.SetColumn(2, Z)
        Else
            Dim Message As String = "Vectors must have a length of 3 or 4."
            Throw New Exception(Message)
        End If
    End Sub
    Public Sub setZ(Z As DenseVector)
        Dim Z_array As Double() = Z.ToArray()
        setZ(Z_array)
    End Sub



    Public Function getOrigin() As DenseVector
        Dim origin As DenseVector = New DenseVector(3)
        origin(0) = Me(0, 3)
        origin(1) = Me(1, 3)
        origin(2) = Me(2, 3)
        Return origin
    End Function

    Public Function getX() As DenseVector
        Dim X As DenseVector = New DenseVector(3)
        X(0) = Me(0, 0)
        X(1) = Me(1, 0)
        X(2) = Me(2, 0)
        Return X
    End Function

    Public Function getY() As DenseVector
        Dim Y As DenseVector = New DenseVector(3)
        Y(0) = Me(0, 1)
        Y(1) = Me(1, 1)
        Y(2) = Me(2, 1)
        Return Y
    End Function

    Public Function getZ() As DenseVector
        Dim Z As DenseVector = New DenseVector(3)
        Z(0) = Me(0, 2)
        Z(1) = Me(1, 2)
        Z(2) = Me(2, 2)
        Return Z
    End Function

    Overloads Shared Operator *(ByVal left As coordinateMatrix, ByVal right As coordinateMatrix) As coordinateMatrix

        Dim leftMatrix As DenseMatrix = left.arrayToDenseMatrix(left.ToArray())
        Dim rightMatrix As DenseMatrix = right.arrayToDenseMatrix(right.ToArray())
        Return New coordinateMatrix(leftMatrix * rightMatrix)

    End Operator
    Private Function arrayToDenseMatrix(arr As Double(,)) As DenseMatrix
        Dim matrix As DenseMatrix = New DenseMatrix(4)
        matrix(0, 0) = arr(0, 0)
        matrix(0, 1) = arr(0, 1)
        matrix(0, 2) = arr(0, 2)
        matrix(0, 3) = arr(0, 3)
        matrix(1, 0) = arr(1, 0)
        matrix(1, 1) = arr(1, 1)
        matrix(1, 2) = arr(1, 2)
        matrix(1, 3) = arr(1, 3)
        matrix(2, 0) = arr(2, 0)
        matrix(2, 1) = arr(2, 1)
        matrix(2, 2) = arr(2, 2)
        matrix(2, 3) = arr(2, 3)
        matrix(3, 0) = arr(3, 0)
        matrix(3, 1) = arr(3, 1)
        matrix(3, 2) = arr(3, 2)
        matrix(3, 3) = arr(3, 3)
        Return matrix
    End Function
End Class
