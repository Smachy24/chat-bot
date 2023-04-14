using System;
namespace ServerTCP
{
	/// <summary>
	/// Objet nous permettant d'écrire un message dans un fichier un log, il prend en compte la gestion de l'heure du message et son formatage.
	/// </summary>
	public class Logger
	{
        private static readonly string _logFileName = "LogClass.txt";
		private DateTime _logDateTime;
		private string _baseLog;
		private string _logMessage;
		StreamWriter LogWriter = new StreamWriter(_logFileName, true);

		/// <summary>
		/// A l'instantiation, le Logger Recupère notre message et la date d'instantiation puis l'écrit directement dans le fichier de log.
		/// </summary>
		/// <param name="message">le corp du message du log</param>
        public Logger(string message)
		{
			_logDateTime = DateTime.Now;
			_baseLog = "[" + _logDateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss") + "] : ";
			_logMessage = message;


            LogWriter.WriteLine(_baseLog + message);
			LogWriter.Close();
		}

    }
}

