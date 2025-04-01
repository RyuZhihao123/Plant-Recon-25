from socket import *
import numpy as np
import time

tcp_server = socket(AF_INET, SOCK_STREAM)
address = ("127.0.0.1", 7878)
tcp_server.bind(address)
print("服务器已经开启!")

tcp_server.listen(3)  # 3 Clients max.
client_socket, clientAddr = tcp_server.accept()
print("收到客户端请求并建立链接!")

def PackageReturnMsg(eigenvectors, center, len1, len2, len3):

    return_msg = ""

    # 1. 特征值
    eigenvectors = eigenvectors.reshape(-1)
    for x in eigenvectors:
        return_msg += str(x) + " "

    # 2. 中心点
    for x in center:
        return_msg += str(x) + " "

    return_msg += str(len1) + " "
    return_msg += str(len2) + " "
    return_msg += str(len3) + " "

    return return_msg



while True:
    # Receive.
    recv_msg = client_socket.recv(10240)

    msg_type = recv_msg[0] - 48

    if msg_type == ord('A')-48:    # 如果接受字符为'A'，则打印消息在屏幕，用于debug
        print("接收打印:" + str(recv_msg))
        client_socket.send("abdbcbdsdas".encode("gbk"))

    # if msg_type == 0:   # 根据特征向量获取3个base dir
    #     recv_msg = recv_msg[2:]
    #     # print(recv_msg)
    #     points = np.array(recv_msg.split())

    #     points = points.astype(dtype=np.float64).reshape((-1, 3))
    #     print("-- Vertex num (in Server): ", points.shape)
    #     eigenvectors = GetEigenVector(points)
    #     center, len1, len2, len3 = GetCenterAndEigenLen(points, eigenvectors)
    #     print(eigenvectors)

    #     return_msg = PackageReturnMsg(eigenvectors, center, len1, len2, len3)

    #     print("---------")
    #     # Send.
    #     send_data = client_socket.send(return_msg.encode("gbk"))

    # if msg_type == 1:  # 以X,Y,Z轴作为3个base dir
    #     recv_msg = recv_msg[2:]
    #     # print(recv_msg)
    #     points = np.array(recv_msg.split())

    #     points = points.astype(dtype=np.float).reshape((-1, 3))

    #     eigenvectors = np.array([[0, 1, 0],
    #                              [1, 0, 0],
    #                              [0, 0, 1]]).astype(np.float)  # Y,X,Z
    #     center, len1, len2, len3 = GetCenter_Len_From_YXZ_Axis(points)
    #     print(eigenvectors)

    #     return_msg = PackageReturnMsg(eigenvectors, center, len1, len2, len3)

    #     print("---------")
    #     # Send.
    #     send_data = client_socket.send(return_msg.encode("gbk"))



