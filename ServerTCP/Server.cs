using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ServerTCP
{
    public class Server
    {
        private int _clientNb = 0;
        private readonly List<User> _users = new();
        private static List<Group> _groups = new();
        //private readonly static DateTime dateTime; 
        //private readonly static string baseLog = "[" + (dateTime = DateTime.Now).ToString("dddd, dd MMMM yyyy HH:mm:ss") + "] :";
        //private readonly static string logName = "Log.txt";

        public Server()
        {
            //File.WriteAllText(logName, baseLog + " SERVER STARTED\n");
            new Logger("SERVER STARTED");
            Console.WriteLine("Welcome to the server side !");
            // création d'un socket :  interNetwork : adresse de la famille Ipv4
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // création d'un point d'accés 
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 3042);
            // maintenant on connect les deux 
            serverSocket.Bind(ipEndPoint);

            // boucle qui tourne sur le thread principale et qui gère l'écoute de nouvelle connexion
            while (true)
            {
                Console.WriteLine("j'attend une connexion");
                serverSocket.Listen(10);
                Socket clientSocket = serverSocket.Accept();  // Quand le server identifie une connexion la fonction accept return un objet socket qui correspond au client qui vient de creer cette nouvelle connexion
                _clientNb++;

                User user = new User(clientSocket, _clientNb);
                _users.Add(user);

                // pour chaque nouvelle connexion on a un nouveau thread qui va gérer ce client a traver une fonction
                // le thread aura besoin d'au moins 1 info, le socket avec lequel travailler
                Thread clientThread;
                clientThread = new Thread(() => ClientConnection(user));
                clientThread.Start();
            }
        }


        // fonction qui tourne sur les thread secondaire créer qui gère l'écoute des message de chaque client
        private void ClientConnection(User user)
        {
            byte[] buffer = new byte[user.Socket.SendBufferSize];
            //Console.WriteLine("Initial size of buffer" + buffer.Count());
           
            //File.AppendAllText(logName, baseLog + " User : " + IPAddress.Parse(((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString()) + " connected\n");
            new Logger(" User : " + IPAddress.Parse(((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString()) + " connected");
            Console.WriteLine("connexion reussis");
            

            NetworkStream ns = new(user.Socket);
            TextReader tr = new StreamReader(ns);
            TextWriter tw = new StreamWriter(ns);
            // je rajoute un try catch pour la gestion des deconnexion sauvage coté client
            try
            {
               
                while(true)
                {
                    
                    // Reception
                    //int actualNumberOfBytesReveived = user.Socket.Receive(buffer); // remplie le buffer qu'on lui passe en parametre et  return un int qui correspond a la taille (nbr de byte) de ce qu'il vient de remplir dans le buffer
                    //byte[] sizeAdjustedBuffer = new byte[actualNumberOfBytesReveived];
                    //Array.Copy(buffer, sizeAdjustedBuffer, actualNumberOfBytesReveived);


                    int bytesRead = ns.Read(buffer, 0, buffer.Length);
                    string textReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    Console.WriteLine(textReceived.Length);
                    //Console.WriteLine(buffer.Length);
                    //Console.WriteLine(actualNumberOfBytesReveived);

                    //Console.WriteLine(textReceived);
                    //Console.WriteLine(textReceived);
                    //Console.WriteLine(textReceived);
                    //Console.WriteLine(textReceived == "dio");



                    // Traitement
                    if (user.Pseudo.Length == 0)
                    {
                        SetUserPseudo(user, textReceived);
                    }
                    else if(textReceived.StartsWith("/mp"))
                    {
                        SendPrivate(user, textReceived);
                    }
                    else
                    {
                        //group actions-----------------------------------------------
                        if (textReceived.StartsWith("/group create"))
                        {
                            Console.WriteLine("Group created");
                            _groups.Add(new Group(user, user.Pseudo));
                            foreach (Group g in _groups)
                            {
                                if (g._adminName == user.Pseudo)
                                {
                                    foreach (User u in _users)
                                    {
                                        if (u.Pseudo == user.Pseudo)
                                        {
                                            g._members.Add(u);
                                        }
                                    }
                                }
                            }
                        }
                        else if (textReceived.StartsWith("/group invite"))
                        {
                            Console.WriteLine("You invited a user");
                            string[] invitingMessage = textReceived.Split(" ");
                            if (invitingMessage.Length > 3)
                            {
                                string receiverPseudo = invitingMessage[2];
                                foreach (Group g in _groups)
                                {
                                    if (g._adminName == user.Pseudo)
                                    {
                                        foreach (User u in _users)
                                        {
                                            if (u.Pseudo == receiverPseudo)
                                            {
                                                g._members.Add(u);
                                            }
                                            u.Socket.Send(Encoding.UTF8.GetBytes(u.Pseudo + " joined the group\n"));
                                        }
                                    }
                                }
                            }
                        }
                        else if (textReceived.StartsWith("/group kick"))
                        {
                            Console.WriteLine("You kicked a user");
                            string[] invitingMessage = textReceived.Split(" ");
                            if (invitingMessage.Length > 3)
                            {
                                string receiverPseudo = invitingMessage[2];
                                foreach (Group g in _groups)
                                {
                                    if (g._adminName == user.Pseudo)
                                    {
                                        foreach (User u in _users)
                                        {
                                            if (u.Pseudo == receiverPseudo)
                                            {
                                                g._members.Remove(u);
                                            }
                                            u.Socket.Send(Encoding.UTF8.GetBytes(u.Pseudo + " has been kicked\n"));
                                        }
                                    }
                                }
                            }
                        }
                        else if (textReceived.StartsWith("/group delete"))
                        {
                            Console.WriteLine("You deleted your group");
                            foreach (Group g in _groups)
                            {
                                if (g._adminName == user.Pseudo)
                                {
                                    _groups.Remove(g);
                                    break;
                                }
                            }
                        }
                        else if(textReceived.StartsWith("/group leave"))
                        {
                            foreach (Group g in _groups)
                            {
                                foreach(User u in g._members)
                                {
                                    if (u.Pseudo == user.Pseudo)
                                    {
                                        g._members.Remove(u);
                                    }
                                    u.Socket.Send(Encoding.UTF8.GetBytes(u.Pseudo + " left the group\n"));
                                }
                            }
                        }
                        else if(textReceived.StartsWith("/group msg"))
                        {
                            string[] invitingMessage = textReceived.Split(" ");
                            if (invitingMessage.Length > 3)
                            {
                                string receiverPseudo = invitingMessage[2];
                                var place = textReceived.IndexOf(receiverPseudo) + receiverPseudo.Length + 1;
                                var message = textReceived.Substring(place);
                                foreach (Group g in _groups)
                                {
                                    foreach (User u in g._members)
                                    {
                                        if (g._members.Any(u => u.Pseudo == receiverPseudo))
                                        {
                                            u.Socket.Send(Encoding.UTF8.GetBytes(user.Pseudo + " send : " + message + "\n"));
                                            Console.WriteLine("message envoyé");
                                        }
                                    }
                                }
                            }
                        }
                        // end group actions---------------------------------------------------
                        SendToAll(user, textReceived, buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("connexion perdu");
                Console.WriteLine(user.Socket.RemoteEndPoint.ToString() + " s'est deconnecte");

                //File.AppendAllText(logName, baseLog + " User :" + IPAddress.Parse(((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString()) + " disconnected\n");
                new Logger(" User :" + IPAddress.Parse(((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString()) + " disconnected");
                user.Socket.Close();
                _users.Remove(user);
                _users.ForEach(u => u.Socket.Send(Encoding.UTF8.GetBytes("Un utilisateur s'est deconnecte\n")));

            }
        }

        private void SetUserPseudo(User user, string textReceived)
        {
            bool pseudoAvailable = true;
            Console.WriteLine("on verifie la dispo");
            if (_users.Any(x => x.Pseudo == textReceived))
            {
                pseudoAvailable = false;
                Console.WriteLine("meme pseudo trouve");
            }

            if (pseudoAvailable)
            {
                user.Pseudo = textReceived;
                user.Socket.Send(System.Text.Encoding.UTF8.GetBytes("Votre pseudo a bien ete set \n"));
                Console.WriteLine("un pseudo a ete affecte");
                _users.ForEach(u => u.Socket.Send(Encoding.UTF8.GetBytes("Un utilisateur s'est connecte\n")));
            }
            else
            {
                user.Socket.Send(System.Text.Encoding.UTF8.GetBytes("Pseudo deja prit"));
            }
        }

        private void SendToAll(User sender, string textReceived, byte[] buffer)
        {
            Console.WriteLine("envoie classique du message");
            // Reponse du server
            sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes("message bien recu \n"));
            foreach (User u in _users)
            {
                u.Socket.Send(System.Text.Encoding.UTF8.GetBytes(sender.Pseudo + " send : " + textReceived + "\n"));
            }
            Array.Clear(buffer, 0, buffer.Length);
        }


        private void SendPrivate(User sender, string textReceived)
        {
            Console.WriteLine("mode privée");
            string[] textReceivedWords = textReceived.Split(' ');
            if (textReceivedWords.Length > 2)
            {
                string pseudo = textReceivedWords[1];
                var place = textReceived.IndexOf(pseudo) + pseudo.Length + 1;
                var message = textReceived.Substring(place);



                bool isUserFound = false;
                foreach (User u in _users)
                {
                    if (u.Pseudo == pseudo)
                    {
                        Console.WriteLine("une cible trouvé");
                        u.Socket.Send(System.Text.Encoding.UTF8.GetBytes(sender.Pseudo + " (private) : " + message + "\n"));
                        sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes(sender.Pseudo + " (private) : " + message + "\n"));
                        isUserFound = true;
                        break;
                    }
                }

                if (isUserFound == false)
                {
                    sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes("aucun utilisateur correspondant\n"));
                }

            }
            else if (textReceivedWords.Length == 2)
            {
                sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes("veuillez indiquer message\n"));
            }
            else
            {
                sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes("Indiquez un pseudo et un message\n"));
            }
        }











    }

}
