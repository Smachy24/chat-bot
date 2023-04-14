using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerTCP
{
    /// <summary>
    /// Type construit pour representer chaque nouveau socket client a un numéro d'index et un pseudo
    /// </summary>
    public class User
    {
        public Socket Socket { get; set; }
        public int Number { get; set; }
        public string Pseudo { get; set; }

        /// <summary>
        /// Construit un nouveau client. Son pseudo n'est pas encore définie.
        /// </summary>
        /// <param name="s">le socket de ce nouveu client</param>
        public User(Socket s, int num)
        {
            Socket = s;
            Number = num;
            // a la construction le pseudo est initialiser a une chaine vide, cela nous permettra d'identifier les client nouvelle connecté
            // pour qu'il puisse saisir leur pseudo.
            Pseudo = String.Empty;
        }
    }
}
