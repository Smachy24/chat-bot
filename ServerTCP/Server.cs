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
    /// <summary>
    /// Class server qui gère la reception des connexions et des messages de clients.
    /// Répartie les multiple client sur différent threads avec une fonction qui gère le traitement a effectué en fonction du message reçu.
    /// </summary>
    public class Server
    {
        private int _clientNb = 0;
        // Collection qui garde en mémoire tout nos client actuellement connectés.
        private readonly List<User> _users = new();
        private static List<Group> _groups = new();

        /// <summary>
        /// CONSTRUCTEUR DU SERVEUR : vient initialiser le socket de notre serveur et le passer dans une écoute continuelle de nouvelle connexion.
        /// A chaque nouvelle connexion, un nouvelle user est créer et stocker en mémoire puis son sa gestion est basculé sur un nouveau thread.
        /// A la construction le serveur va donc occuper le thread principale.
        /// </summary>
        public Server()
        {
            // Création d'un log donnant les information sur la création du Server
            new Logger("SERVER STARTED");
            Console.WriteLine("Welcome to the server side !");
            // création d'un socket :  interNetwork : adresse de la famille Ipv4
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // création d'un point d'accés 
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 3042);
            // maintenant on connect les deux 
            serverSocket.Bind(ipEndPoint);

            // Boucle qui tourne sur le thread principale et qui gère l'écoute de nouvelle connexion.
            // Cette boucle occupe le thread principale.
            while (true)
            {
                Console.WriteLine("j'attend une connexion");
                serverSocket.Listen(10);
                Socket clientSocket = serverSocket.Accept();  // Quand le server identifie une connexion la fonction accept return un objet socket qui correspond au client qui vient de creer cette nouvelle connexion
                _clientNb++;

                User user = new User(clientSocket, _clientNb);
                _users.Add(user);

                // pour chaque nouvelle connexion on a un nouveau thread qui va gérer ce client a traver une fonction dédié
                // le thread aura besoin d'au moins 1 info, le socket avec lequel travailler
                Thread clientThread;
                clientThread = new Thread(() => ClientConnection(user));
                clientThread.Start();
            }
        }


        /// <summary>
        /// Gère la communication avec un client. Ecoute l'arrivé de message et en fonction de son contenu,
        /// va appeler la fonction pour le traitement adapté.
        /// Gère egualement la fin de communication/perte de communication avec le dit client et envoie un message d'erreur
        /// </summary>
        /// <param name="user">Le client gérer.</param>
        private void ClientConnection(User user)
        {
            // Création d'un log donnant les information sur la connexion de ce client.
            new Logger(" User : " + IPAddress.Parse(((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString()) + " connected");
            Console.WriteLine("connexion reussis");

            
            byte[] buffer = new byte[user.Socket.SendBufferSize];
            // Objet utilisé pour la reception des donnée envoyé par le socket de ce client.
            NetworkStream ns = new(user.Socket);
            //TextReader tr = new StreamReader(ns);
            //TextWriter tw = new StreamWriter(ns);

            // je rajoute un try catch pour la gestion des deconnexion sauvage coté client
            try
            {
               // Boucle pour l'écoute d'arrivé de nouveau message du client, et appel du traitement approprié en fonction de son contenu
                while(true)
                {
                    // remplie le buffer qu'on lui passe en parametre et  return un int qui correspond a la taille (nbr de byte) de ce qu'il vient de remplir dans le buffer
                    int bytesRead = ns.Read(buffer, 0, buffer.Length);
                    // on convertie en un string les bytes du buffer, en commançant au premier et en s'arretant au nombre de bytes reelement reçu
                    // Cela nous permet de ne pas avoir un string composé de nombreux caractères vide.
                    string textReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Traitement
                    if (user.Pseudo.Length == 0)
                    {
                        SetUserPseudo(user, textReceived);
                    }
                    else if(textReceived.StartsWith("/mp"))
                    {
                        SendPrivate(user, textReceived, buffer);
                    }
                    else if (textReceived.StartsWith("/group"))
                    {
                        GroupActions(user, textReceived);
                    }
                    else
                    {
                        SendToAll(user, textReceived, buffer);
                    }
                }
            }
            catch (Exception ex) 
            {
                /// une erreur est detecté et nous fait sortir du try executant notre boucle while.
                Console.WriteLine(ex.Message);
            }
            finally
            {
                // la connexion avec le client a été perdu :
                // on le retire de la liste des utilisateur en mémoire et créer message et log de deconnexion.
                Console.WriteLine("connexion perdu");
                Console.WriteLine(user.Socket.RemoteEndPoint.ToString() + " s'est deconnecte");

                // Création d'un log donnant les information sur la déconnexion de ce client.
                new Logger(" User :" + IPAddress.Parse(((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString()) + " disconnected");
                user.Socket.Close();
                _users.Remove(user);
                _users.ForEach(u => u.Socket.Send(Encoding.UTF8.GetBytes("Un utilisateur s'est deconnecte\n")));

            }
        }

        /// <summary>
        /// Cette fonction recupéré le message d'un user dont le champs pseudo est encore vide pour lui affecté en tant que Pseudo
        /// plutot que le distribuer aux autre utilisateurs.
        /// Elle vérifie au préalable si ce message correspond a un pseudo d'un des utilisateur en mémoire. Si oui elle envoie un message d'erreur.
        /// Si non le message est affecté en pseudo.
        /// </summary>
        /// <param name="user">l'utilisateur qui a envoyé le message</param>
        /// <param name="textReceived">Le message qui a été envoyé, correspondant ici au pseudo désiré</param>
        private void SetUserPseudo(User user, string textReceived)
        {
            bool pseudoAvailable = true;
            Console.WriteLine("on verifie la dispo");
            // Utilisation de linq sur notre collection de user pour trouver une correspondance
            if (_users.Any(x => x.Pseudo == textReceived))
            {
                pseudoAvailable = false;
                Console.WriteLine("meme pseudo trouve");
            }

            if (pseudoAvailable) // aucune correspondance trouvé, le pseudo est disponible
            {
                user.Pseudo = textReceived;
                user.Socket.Send(System.Text.Encoding.UTF8.GetBytes("Votre pseudo a bien ete set \n"));
                Console.WriteLine("un pseudo a ete affecte");
                _users.ForEach(u => u.Socket.Send(Encoding.UTF8.GetBytes("Un utilisateur s'est connecte\n")));
            }
            else // correspondace avec un pseudo déjà existant trouvé
            {
                user.Socket.Send(System.Text.Encoding.UTF8.GetBytes("Pseudo deja prit"));
            }
        }

        /// <summary>
        /// Distribue le message reçu a tout les utilisateur connecté present en mémoire en précisant le user source de ce message et en nettoyant le buffer après distribution.
        /// </summary>
        /// <param name="sender">L'utilisateur qui a envoyé le message.</param>
        /// <param name="textReceived">Le message qui a été envoyé.</param>
        /// <param name="buffer">le buffer contenant les bytes d'information reçu</param>
        private void SendToAll(User sender, string textReceived, byte[] buffer)
        {
            Console.WriteLine("envoie classique du message");
            // Reponse du server
            sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes("message bien recu \n"));
            // boucle qui distribue le message sur les socket de tout les user en memoire.
            foreach (User u in _users)
            {
                u.Socket.Send(System.Text.Encoding.UTF8.GetBytes(sender.Pseudo + " send : " + textReceived + "\n"));
            }
            // On vide le buffer.
            Array.Clear(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Decoupe le message reçu, pour récupéré a part le pseudo de la cible et le corp du message et cherche dans _users un user avec un pseudo identique
        /// pour lui envoyer. Un message d'erreur est envoyé si aucune correspondance n'est trouvé ou si le message ne contient pas toute les informations.
        /// </summary>
        /// <param name="sender">L'utilisateur qui a envoyé le message</param>
        /// <param name="textReceived">le message envoyé respectant le format : /mp (pseudoCible) (message)".</param>
        /// <param name="buffer">le buffer contenant les bytes d'information reçu</param>
        private void SendPrivate(User sender, string textReceived, byte[] buffer)
        {
            Console.WriteLine("mode privée");
            // découpage du message réçu pour récupéré les information qu'il contient
            string[] textReceivedWords = textReceived.Split(' ');
            // un message correct doit contenir au moins 3 mots : l'identifiant /mp, un pseudo, un message
            if (textReceivedWords.Length > 2) 
            {
                string pseudo = textReceivedWords[1];
                var place = textReceived.IndexOf(pseudo) + pseudo.Length + 1;
                var message = textReceived.Substring(place);



                bool isUserFound = false;
                foreach (User u in _users)
                {
                    // boucle pour chercher une correspondance entre le pseudo précisé et ceux de nos user en mémoire
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
            else if (textReceivedWords.Length == 2) // il manque un mot correspondant au message
            {
                sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes("veuillez indiquer message\n"));
            }
            else // il manque 2 mot correspondant au pseudo et au message
            {
                sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes("Indiquez un pseudo et un message\n"));
            }
            // On vide le buffer.
            Array.Clear(buffer, 0, buffer.Length);
        }

        public void GroupActions(User user, string textReceived)
        {
            //group actions-----------------------------------------------
            if (textReceived.StartsWith("/group create"))
            {
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
                string[] invitingMessage = textReceived.Split(" ");
                if (invitingMessage.Length > 2)
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
                                    Console.WriteLine(g._members[1].Pseudo);
                                    Console.WriteLine("You invited a user");
                                }
                                //u.Socket.Send(Encoding.UTF8.GetBytes(u.Pseudo + " joined the group\n"));
                            }
                        }
                    }
                }
            }
            else if (textReceived.StartsWith("/group kick"))
            {
                Console.WriteLine("You kicked a user");
                string[] invitingMessage = textReceived.Split(" ");
                if (invitingMessage.Length > 2)
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
                                //u.Socket.Send(Encoding.UTF8.GetBytes(u.Pseudo + " has been kicked\n"));
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
            else if (textReceived.StartsWith("/group leave"))
            {
                foreach (Group g in _groups)
                {
                    foreach (User u in g._members)
                    {
                        if (u.Pseudo == user.Pseudo)
                        {
                            Console.WriteLine("fze00");
                            g._members.Remove(u);
                            break;
                        }
                        //u.Socket.Send(Encoding.UTF8.GetBytes(u.Pseudo + " left the group\n"));
                    }
                }
            }
            else if (textReceived.StartsWith("/group msg"))
            {
                string[] invitingMessage = textReceived.Split(" ");
                Console.WriteLine(invitingMessage.Length);
                if (invitingMessage.Length > 2)
                {

                    //string receiverPseudo = invitingMessage[1];
                    //var place = textReceived.IndexOf(receiverPseudo) + receiverPseudo.Length + 1;
                    var message = textReceived.Substring(11);
                    Console.WriteLine(message);
                    Console.WriteLine(_groups.Count);

                    foreach (Group g in _groups)
                    {
                        Console.WriteLine(g._members.Count);

                        foreach (User u in g._members)
                        {


                            u.Socket.Send(Encoding.UTF8.GetBytes(user.Pseudo + " group : " + message + "\n"));

                            //if (g._members.Any(u => u.Pseudo == user.Pseudo))
                            //{
                            //    u.Socket.Send(Encoding.UTF8.GetBytes(user.Pseudo + " send : " + message + "\n"));

                            //}
                        }
                    }
                }
            }
            // end group actions---------------------------------------------------
        }









    }

}
