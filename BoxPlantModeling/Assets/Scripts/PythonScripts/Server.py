from socket import *

# 1.创建套接字
tcp_server = socket(AF_INET, SOCK_STREAM)

# 2.绑定ip，port
address = ("127.0.0.1", 3434)
tcp_server.bind(address)

print("服务器已经开启!")
# 3.启动被动连接
tcp_server.listen(5)

# 4.创建接收
client_socket, clientAddr = tcp_server.accept()

print("收到客户端请求并建立链接!")
while True:
    # 5.接收对方发送过来的数据
    recv_msg = client_socket.recv(10240)
    print("打印接收的数据：", recv_msg)
    # 6.发送数据给客户端

    print("---------")
    send_data = client_socket.send("这是给您的回复".encode("gbk"))


