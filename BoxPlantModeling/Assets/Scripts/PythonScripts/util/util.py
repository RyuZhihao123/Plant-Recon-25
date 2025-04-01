import numpy as np

# 可能有的坑：
# 1. 特征向量是否根据特征值从大往小排序的
# 2. 中心位置计算：是否可能需要换成均值A_mean。目前是真正意义上的几何中心
# 3. 长度len，是否需要缩小一点点
#            Box的边长（注意：不是1/2边长，而是整个边长）


def GetEigenVector(A):
    A_mean = np.mean(A, axis=0)
    A = A - A_mean
    covariance_matrix = np.cov(A.T)

    eigen_values, eigen_vectors = np.linalg.eig(covariance_matrix)

    new_index = np.argsort(eigen_values)[::-1]
    eigen_vectors = eigen_vectors[:, new_index]
    eigen_values = eigen_values[new_index]

    # 单位化特征向量
    eigen_vectors[0] = eigen_vectors[0] / np.linalg.norm(eigen_vectors[0])
    eigen_vectors[1] = eigen_vectors[1] / np.linalg.norm(eigen_vectors[1])
    eigen_vectors[2] = eigen_vectors[2] / np.linalg.norm(eigen_vectors[2])

    print(eigen_vectors)
    return eigen_vectors


def GetCenterAndEigenLen(A, eigenvectors):

    dir1 = eigenvectors[0]
    dir2 = eigenvectors[1]
    dir3 = eigenvectors[2]

    A_mean = np.mean(A, axis=0)

    len1, delta1 = GetLen_AndShiftToMean_AlongWithDirection(A, dir1, A_mean)
    len2, delta2 = GetLen_AndShiftToMean_AlongWithDirection(A, dir2, A_mean)
    len3, delta3 = GetLen_AndShiftToMean_AlongWithDirection(A, dir3, A_mean)

    center = A_mean + delta1*dir1 + delta2*dir2 + delta3*dir3
    return center, len1, len2, len3


resultMax = None
resultMin = None


def GetLen_AndShiftToMean_AlongWithDirection(A, dir, A_mean):

    far_point = A_mean + 100000 * (-dir)  # 沿着负方向无限远的一个点

    max = -9999999.
    min = +9999999.

    for x in A:
        vector1 = x - far_point
        dot = np.dot(vector1, dir)

        if dot > max:
            global resultMax
            max = dot
            resultMax = x

        if dot < min:
            global resultMin
            min = dot
            resultMin = x

    center = np.dot(A_mean-far_point, dir)

    max -= center
    min -= center

    len = np.linalg.norm(resultMax-resultMin)

    return len, (max+min)/2.0


def GetCenter_Len_From_YXZ_Axis(A):

    minx = miny = minz = +9999999.
    maxx = maxy = maxz = -9999999.

    for pt in A:

        x = pt[0]
        y = pt[1]
        z = pt[2]

        if minx > x: minx = x
        if miny > y: miny = y
        if minz > z: minz = z

        if maxx < x: maxx = x
        if maxy < y: maxy = y
        if maxz < z: maxz = z

    center = np.array([
        (maxx + minx)/2., (maxy + miny)/2., (maxz + minz)/2.
    ])

    len1 = maxy - miny
    len2 = maxx - minx
    len3 = maxz - minz

    return center, len1, len2, len3


