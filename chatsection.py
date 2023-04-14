import tkinter
import customtkinter

class ChatSection():
    def __init__(self, main_app,send_button_font, chat_text_font, frame_listener, user) -> None:
        self.main_app = main_app
        self.send_button_font = send_button_font
        self.chat_text_font = chat_text_font
        self.frame_listener = frame_listener
        self._user = user

        # colors
        self.primary_color = "#242424"
        self.recieve_color = "#0f0f0f"
        self.send_color = "#007acc"

        # footer frame wrapper
        self.bottom_frame = tkinter.Frame(main_app, padx=20, pady=20, width=500, background=self.primary_color)
        self.bottom_frame.pack(side="bottom")

        # Message section
        self.message_wrapper = tkinter.Frame(main_app, padx=20, pady=20, width=500, background=self.primary_color)

        # Add scrollbar
        self.message_scrollbar = customtkinter.CTkScrollbar(self.message_wrapper, orientation="vertical")
        # Create Canva
        self.message_canvas = tkinter.Canvas(self.message_wrapper, width=500, height=600, background=self.primary_color,yscrollcommand=self.message_scrollbar.set,highlightbackground=self.primary_color, highlightthickness=2)
        # Link scrollbar to canva
        self.message_scrollbar.configure(command=self.message_canvas.yview)
        self.message_scrollbar.pack(side="right", fill="y")

        self.message_canvas.pack(side="left", fill="both", expand=True)
        self.message_wrapper.pack(side="bottom", anchor="w")

        self.inputMessage = customtkinter.CTkEntry(master=self.bottom_frame, placeholder_text="Ecrivez votre message ici...", width=300) # Input text
        self.inputMessage.pack(side="left", anchor="w")
        self.button = customtkinter.CTkButton(master=self.bottom_frame, command=lambda : self.send_messsage(pseudo=self._user.get_pseudo()), text="Envoyer", font=self.send_button_font, width=100)
        self.button.pack(side="right", anchor="e")

        self.message_frame = tkinter.Frame(self.message_canvas, bg=self.primary_color)
        self.message_canvas.create_window((0, 0), window=self.message_frame, anchor="sw")  # Add message frame to canvas

        self.frame_listener.bind('<Return>', lambda event: self.send_messsage(pseudo=self._user.get_pseudo()))

    def send_messsage(self,event = None, pseudo = None ):
        """
            Envoie un message de l'utilisateur vers le chat.

            Parameter:
                event (): L'événement qui a déclenché l'envoi du message.
                pseudo (str): Le pseudo de l'utilisateur qui envoie le message.

            Returns:
                None
        """
        message = self.inputMessage.get()
        if len(message) == 0:
            return
        elif message.isspace():
            return
        #self.create_message_card(pseudo=pseudo,message=message)
        self._user.send_message(message)
        self.inputMessage.delete(0, "end")

    def create_message_card(self,pseudo, message, c_bg_color):
        """
            Crée une carte de message personnalisée pour afficher le pseudo et le message.

            Parameter:
                pseudo (str): Le pseudo de l'utilisateur qui a envoyé le message.
                message (str): Le contenu du message envoyé.
                c_bg_color (str): La couleur d'arrière-plan de la carte de message.

            Returns:
                None
        """
         # pseudo label
        pseudo_label = customtkinter.CTkLabel(master=self.message_frame, text=pseudo, wraplength=450, pady=10, padx=0, font=self.chat_text_font, text_color="#ffffff", bg_color=self.primary_color, anchor="w", justify="left")
        pseudo_label.pack(side="top", anchor="w")

        # message label
        message_label = customtkinter.CTkLabel(master=self.message_frame, text=message, wraplength=300, pady=10, padx=10, font=self.chat_text_font, text_color="#ffffff", bg_color=c_bg_color, justify="left")
        message_label.pack(side="top", anchor="w")

        # Update canvas scrollbar
        self.message_canvas.update_idletasks()
        self.message_canvas.configure(scrollregion=self.message_canvas.bbox("all"))

        # reset input value

    def display_messages(self):
        """
            Cette méthode affiche les messages reçus par l'utilisateur en créant une carte de message personnalisée avec le pseudo et le contenu du message.
        """
        while True:
            message = self._user.receive_message()
            print(message)

            private = False

            # Check Messages
            if "connecte" not in message and "recu" not in message and "aucun utilisateur" not in message and "veuillez indiquer" not in message and "un pseudo et un message" not in message:
                divided_message = None
                if "(private)" in message:
                    divided_message = message.split(" (private) : ", 1)
                    private = True
                elif (" group : ") in message:
                    divided_message = message.split(" group : ", 1)  
                elif "send :" in message:
                    divided_message = message.split(" send : ", 1)

                pseudo = divided_message[0]
                message = divided_message[1]

                # Bg colors
                if private:
                    c_bg_color = "#8e68ad" 
                elif pseudo != self._user.get_pseudo():
                    c_bg_color = self.recieve_color                   
                else:
                    c_bg_color = self.send_color

                self.create_message_card(pseudo, message, c_bg_color)

            # Message serveur
            if ("s'est deconnecte" in message) or ("aucun utilisateur" in message) or ("veuillez indiquer" in message) or ("un pseudo et un message") in message:
                c_bg_color = "#c9535b"
                self.create_message_card("SERVEUR", message, c_bg_color)
            elif "connecte" in message:  
                c_bg_color = "#54ba76"
                self.create_message_card("SERVEUR", message, c_bg_color)
