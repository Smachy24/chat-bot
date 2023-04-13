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
        private readonly static DateTime dateTime = DateTime.Now;
        private readonly static string baseLog = "[" + dateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss") + "] :";
        private readonly static string logName = "Log.txt";

        public Server()
        {
            File.WriteAllText(logName, baseLog + " SERVER STARTED\n");
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
            File.AppendAllText(logName, baseLog + " User : " + IPAddress.Parse(((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString()) + " connected\n");
            Console.WriteLine("connexion réussis");
            _users.ForEach(u => u.Socket.Send(Encoding.UTF8.GetBytes("Un utilisateur s'est connecté\n")));

            // mes objet pour me permettre de récupéré les message envoyé et envoyé des message
            // Le couple TexTReader NetworkStream nous prend en charge la conversion des bytes reçu en un string de bonne taille
            NetworkStream ns = new(user.Socket);
            TextReader tr = new StreamReader(ns);

            // je rajoute un try catch pour la gestion des deconnexion sauvage coté client
            try
            {
                while(true)
                {
                    // Reception
                    string textReceived = tr.ReadLine();

                    // Traitement
                    if (user.Pseudo.Length == 0)
                    {
                        SetUserPseudo(user, textReceived);
                    }
                    else
                    {
                        SendToAll(user, textReceived);
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
               // Console.WriteLine(user.Socket.RemoteEndPoint.ToString() + " s'est déconnecté"); //cause une erreur
                File.AppendAllText(logName, baseLog + " User :" + IPAddress.Parse(((IPEndPoint)user.Socket.RemoteEndPoint).Address.ToString()) + " disconnected\n");
                user.Socket.Close();
                _users.Remove(user);
                _users.ForEach(u => u.Socket.Send(Encoding.UTF8.GetBytes("Un utilisateur s'est déconnecté\n")));
            }
        }

        private void SetUserPseudo(User user, string textReceived)
        {
            bool pseudoAvailable = true;
            Console.WriteLine("on verifie la dispo");
            if (_users.Any(x => x.Pseudo == textReceived))
            {
                pseudoAvailable = false;
                Console.WriteLine("meme pseudo trouvé");
            }

            if (pseudoAvailable)
            {
                user.Pseudo = textReceived;
                user.Socket.Send(System.Text.Encoding.UTF8.GetBytes("Votre pseudo a bien été set \n"));
                Console.WriteLine("un pseudo a été affecté");
            }
            else
            {
                user.Socket.Send(System.Text.Encoding.UTF8.GetBytes("Pseudo déjà prit \n"));
            }
        }

        private void SendToAll(User sender, string textReceived)
        {
            Console.WriteLine("envoie classique du message");
            //Console.WriteLine("from (" + user.Number.ToString() + ") we got :" + textReceived);
            // Reponse du server
            sender.Socket.Send(System.Text.Encoding.UTF8.GetBytes("message bien reçu \n"));
            foreach (User u in _users)
            {
                u.Socket.Send(System.Text.Encoding.UTF8.GetBytes(sender.Pseudo + " send : " + textReceived + "\n"));
            }
            //Array.Clear(buffer, 0, buffer.Length);
        }












    }

}
