import customtkinter
import tkinter
from chatsection import ChatSection
import threading
import sys
from client import Client

HOST = '10.57.33.117'  #IP address server machine
PORT = 3042

class Clientui(customtkinter.CTk):
    def __init__(self):
        super().__init__()

        self.geometry("500x700")
        self.title("Chatbot d'avril")
        self.minsize(500, 700)
        self.maxsize(500, 700)
        self._set_appearance_mode("dark")

        self._pseudo = None
        self._user = Client(HOST,PORT)

        # color
        self.primary_color = "#242424"
        

        # Font parameter
        self.send_button_font = customtkinter.CTkFont(family="Arial", size=18, weight="bold")
        self.chat_text_font = customtkinter.CTkFont(family="Arial", size=18, weight="normal")

        self.create_menu()

        self._reload_pseudo = False

        
        self.label_error = tkinter.Label(master=self,text= "Test", font=self.chat_text_font)
    def create_chatpage(self):
        """
            Crée une page de chat et l'affiche.
        """
        chat_page = tkinter.Frame(self, bg=self.primary_color)
        chat_page.pack(side="top")
        chat_section = ChatSection(chat_page, self.send_button_font, self.chat_text_font, self, self._user)
        threading.Thread(target=chat_section.display_messages).start()
    
    def create_menu(self):
        """
            Crée une page de menu avec des boutons pour rejoindre le salon de discussion ou quitter l'application.
        """
        page = tkinter.Frame(self, bg=self.primary_color, width=500, height=700,pady=275)
        page.pack(side="top")

        chat = customtkinter.CTkButton(master=page, text="Rejoindre le salon", command =lambda: self.go_pseudo(page))
        chat.pack(side="top")

        quit = customtkinter.CTkButton(master=page, text="Quitter", command =lambda: sys.exit()) # Remplacer par la fonction déconnecte
        quit.pack(side= "top")

    def create_pseudo_menu(self):
        """
            Crée un formulaire permettant à l'utilisateur d'entrer son pseudo pour se connecter au salon de discussion.

        """
        page = tkinter.Frame(self, bg=self.primary_color, width=500, height=700,pady=275)
        page.pack(side="top")

        label_pseudo = customtkinter.CTkLabel(master=page, text="Entrez votre pseudo", font=self.chat_text_font)
        label_pseudo.pack(side="top")

        input_pseudo = customtkinter.CTkEntry(master=page, placeholder_text="Pseudo...") # On entre le pseudo
        input_pseudo.pack(side="top")
        
        chat = customtkinter.CTkButton(master=page, text="Rejoindre le salon", command =lambda: self.check_pseudo(input_pseudo.get(), page)) # Quand on clique on va vers le chat
        chat.pack(side="top")

    
    def check_pseudo(self, pseudo, page):
        """
            Vérifie si le pseudo fourni est valide et tente de se connecter au serveur avec le pseudo fourni.
            
            Parameter:
                pseudo (str): Le pseudo fourni par l'utilisateur.
                page (tkinter.Frame): La page parent du bouton.
            
            Returns:
                None

        """
        if(len(pseudo)>20 or len(pseudo) <= 0 or " " in pseudo): # Verif côté client 
            self.generate_error("Pas d'espace et pas plus de 20 caractères")
        else:
            if (self._user.connect(pseudo,self._reload_pseudo)): # Verif côté sersveur
                if self.label_error.winfo_ismapped():
                    self.label_error.destroy()
                self.go_chat(page)
            else:
                self.generate_error("Pseudo déjà pris :(")
                self._reload_pseudo = True
            
            

    def generate_error(self, message):
        """
            Affiche un message d'erreur sur l'interface graphique.

            Parameter:
                message (str): Le message d'erreur à afficher.

            Returns:
                None
        """
        if self.label_error.winfo_ismapped():
            self.label_error.destroy()
        self.label_error = tkinter.Label(master=self,text= message, font=self.chat_text_font)
        self.label_error.pack(side="top")
            
    
    def go_chat(self, old_page):
        """
            Détruit la page actuelle et crée la page de chat où l'utilisateur pourra rejoindre le salon de discussion.

            Parameter:
                old_page (tkinter.Frame): La page actuelle qui doit être détruite.

            Returns:
                None
        """
        old_page.destroy()
        self.create_chatpage()
        


    def go_pseudo(self, old_page):
        """
            Détruit la page actuelle et crée la page de connexion où l'utilisateur pourra entrer son pseudo.

            Parameter:
                old_page (tkinter.Frame): La page actuelle qui doit être détruite.

            Returns:
                None
        """
        old_page.destroy()
        self.create_pseudo_menu()

    def on_closing(self):
        """
           Déconnecte l'utilisateur et ferme le script python
        """
        if tkinter.messagebox.askokcancel("Quit", "Voulez-vous vous déconnecter ?"):
            self._user.disconnect()
            app.destroy()
            sys.exit()
    
if __name__ == "__main__":
    app = Clientui()
    app.protocol("WM_DELETE_WINDOW", app.on_closing) #Execute function on_closing() when tkinter window is closed
    app.mainloop()
    