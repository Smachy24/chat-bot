import socket
import threading
import time

class Client:

    def __init__(self, server_host, server_port):
        self._server_host = server_host
        self._server_port = server_port
        self._sckt = socket.socket(socket.AF_INET, socket.SOCK_STREAM) #socket.AF_INET = IPV4 address, SOCK_STREAM = use TCP protocol
        self._pseudo = ""
    
    def get_server_host(self):
        """
        Return server IP address
        @return (str): server address IP
        """
        return self._server_host
    
    def get_server_port(self):
        """
        Return server port
        @return (int): server port
        """
        return self._server_port
    
    def get_pseudo(self):
        """
        Return user pseudo
        @return (str): user pseudo
        """
        return self._pseudo
    
    def receive_pseudo(self, tmp_pseudo):
        """
        
        """
        self._sckt.send((tmp_pseudo).encode())
        response = self._sckt.recv(1024).decode("utf8")
        return response
    
    def connect(self, pseudo, is_retry = False):
        if (is_retry == False):
            status = False
            self._sckt.connect((self._server_host,self._server_port)) #Connect socket to server
        
        tmp_pseudo = pseudo
        response = self.receive_pseudo(tmp_pseudo)
        #print("Reponse pseudo : ", response)

        if response=="Pseudo deja prit":
           return status
        
        status = True
        self._pseudo = tmp_pseudo

        return status
    
    def disconnect(self):
        self._sckt.close()

    def send_message(self, message):
        #message = input("Enter message : ")
        while(len(message)<= 0):
            message = input("Enter message : ")
        self._sckt.send(message.encode()) #Send data to socket

    def receive_message(self):

        while(True):
            response = self._sckt.recv(1024) # Limit to 1024 characters
            
            response = response.decode("utf-8", "ignore").strip().strip('\x00')
            
            if len(response)!=0:
                return response
            else:
                break
            time.sleep(3)
