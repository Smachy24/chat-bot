from client import Client
import sys
import threading
import time

# host local '10.57.33.239'

HOST = '192.168.1.85'  #IP address server machine
PORT = 3042

client = Client(HOST,PORT)

menu_options = {"1":"Send message", "q": "quit"}

def show_menu():
    print("--MENU--")
    for item in menu_options.items():
        print(item[0], ":", item[1])

def choose_option():

    while(True):
        show_menu()
        choice = input("Choose option")
        if(choice =="1"):
            client.send_message()
            
        
        elif(choice =="q" or choice == "Q"):
            client.disconnect()
            sys.exit()

# send_message_thread = threading.Thread(target=choose_option)
# send_message_thread.start()
# send_message_thread.join()

receive_message_thread = threading.Thread(target=client.receive_message)
receive_message_thread.start()


client.send_message()



    

    

    
    


# client.send_message("test1")
# client.send_message("test2")
# client.send_message("test3")
# client.send_message("test")


