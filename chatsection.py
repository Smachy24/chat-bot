import tkinter
import customtkinter

class ChatSection():
    def __init__(self, main_app, primary_color, send_button_font, chat_text_font, frame_listener, user) -> None:
        self.main_app = main_app
        self.primary_color = primary_color
        self.send_button_font = send_button_font
        self.chat_text_font = chat_text_font
        self.frame_listener = frame_listener
        self._user = user

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
        self.button = customtkinter.CTkButton(master=self.bottom_frame, command=lambda : self.send_messsage(pseudo=self._pseudo), text="Envoyer", font=self.send_button_font, width=100)
        self.button.pack(side="right", anchor="e")

        self.message_frame = tkinter.Frame(self.message_canvas, bg=self.primary_color)
        self.message_canvas.create_window((0, 0), window=self.message_frame, anchor="sw")  # Add message frame to canvas

        self.frame_listener.bind('<Return>', lambda event: self.send_messsage(pseudo=self._pseudo))

    def send_messsage(self,event = None, pseudo = None ):
        message = self.inputMessage.get()
        if len(message) == 0:
            return
        self.create_message_card(pseudo=pseudo,message=message)
        self.inputMessage.delete(0, "end")

    def create_message_card(self,pseudo, message):
         # pseudo label
        pseudo = self._pseudo  # Pseudo à rendre dynamique
        pseudo_label = customtkinter.CTkLabel(master=self.message_frame, text=pseudo, wraplength=450, pady=10, padx=0, font=self.chat_text_font, text_color="#ffffff", bg_color=self.primary_color, anchor="w", justify="left")
        pseudo_label.pack(side="top", anchor="w")

        # message label
        message_label = customtkinter.CTkLabel(master=self.message_frame, text=message, wraplength=300, pady=10, padx=10, font=self.chat_text_font, text_color="#ffffff", bg_color="#007acc", justify="left")
        message_label.pack(side="top", anchor="w")

        # Update canvas scrollbar
        self.message_canvas.update_idletasks()
        self.message_canvas.configure(scrollregion=self.message_canvas.bbox("all"))

        # reset input value