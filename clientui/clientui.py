import customtkinter
import tkinter
from chatsection import ChatSection
import sys

from client import Client

class Clientui(customtkinter.CTk):
    def __init__(self):
        super().__init__()

        self.geometry("500x700")
        self.title("Chatbot d'avril")
        self.minsize(500, 700)
        self.maxsize(500, 700)
        self._set_appearance_mode("dark")

        # color
        self.primary_color = "#242424"

        # Font parameter
        self.send_button_font = customtkinter.CTkFont(family="Arial", size=18, weight="bold")
        self.chat_text_font = customtkinter.CTkFont(family="Arial", size=18, weight="normal")

        self.create_menu()

    def create_chatpage(self):
        chat_page = tkinter.Frame(self, bg=self.primary_color)
        chat_page.pack(side="top")
        ChatSection(chat_page , self.primary_color, self.send_button_font, self.chat_text_font, self)
    
    def create_menu(self):
        page = tkinter.Frame(self, bg=self.primary_color, width=500, height=700,pady=275)
        page.pack(side="top")

        chat = customtkinter.CTkButton(master=page, text="Rejoindre le salon", command =lambda: self.go_chat(page))
        chat.pack(side="top")

        quit = customtkinter.CTkButton(master=page, text="Quitter", command =lambda: sys.exit()) # Remplacer par la fonction déconnecte
        quit.pack(side= "top")

    def go_chat(self, old_page):
        old_page.destroy()
        self.create_chatpage()

    
if __name__ == "__main__":
    app = Clientui()
    app.mainloop()