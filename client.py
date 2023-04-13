import socket
import threading
import time

class Client:

    def __init__(self, server_host, server_port):
        self._server_host = server_host
        self._server_port = server_port
        self._sckt = socket.socket(socket.AF_INET, socket.SOCK_STREAM) #socket.AF_INET = IPV4 address, SOCK_STREAM = use TCP protocol
        self.connect()
        self._pseudo = ""
    
    def get_server_host(self):
        return self._server_host
    
    def get_server_port(self):
        return self._server_port
    
    def get_pseudo(self):
        return self._pseudo
    
    def create_pseudo(self):
        tmp_pseudo = input("Enter pseudo : ")
        
        while(len(tmp_pseudo)>20 or len(tmp_pseudo) <= 0 or " " in tmp_pseudo):
            tmp_pseudo = input("Enter pseudo : ")
        
        return tmp_pseudo
    
    def receive_pseudo(self, tmp_pseudo):
        self._sckt.send((tmp_pseudo + "\n").encode())
        response = self._sckt.recv(1024).decode("utf8")
        return response
    
    def connect(self):
        self._sckt.connect((self._server_host,self._server_port)) #Connect socket to server
        
        tmp_pseudo = self.create_pseudo()
        response = self.receive_pseudo(tmp_pseudo)
        print("Reponse pseudo : ", response)

        while response=="Pseudo déjà prit \n":
           tmp_pseudo = self.create_pseudo()
           response = self.receive_pseudo(tmp_pseudo)
        
        self._pseudo = tmp_pseudo
    
    def disconnect(self):
        self._sckt.close()

    def send_message(self):
        message = input("Enter message : ")
        while(len(message)<= 0):
            message = input("Enter message : ")    
        self._sckt.send(message.encode()) #Send data to socket


    def receive_message(self):
        while(True):
            response = self._sckt.recv(1024) # Limit to 1024 characters
            response = response.decode("utf-8", "ignore").strip().strip('\x00')
            

            if len(response)!=0:
                print("Condition bonne")
                print(response)
            time.sleep(3)
