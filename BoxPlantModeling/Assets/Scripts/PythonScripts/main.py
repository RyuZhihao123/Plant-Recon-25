from socket import *

# 1.创建套接字
# https://blog.csdn.net/qq_26442553/article/details/94451871
tcp_socket = socket(AF_INET, SOCK_STREAM)


serve_ip = "127.0.0.1"
serve_port = 3434
tcp_socket.connect((serve_ip, serve_port))  # Start Server.


# 3.准备需要传送的数据
while True:
    send_data = input("请输入要发送的数据：")
    tcp_socket.send(send_data.encode("gbk"))  # 用的是send方法，不是sendto

    # 4.从服务器接收数据
    tcp_remsg = tcp_socket.recv(10240)  # 注意这个1024byte，大小根据需求自己设置
    print(tcp_remsg.decode("gbk"))  # 如果要乱码可以使用tcp_remsg.decode("gbk")
