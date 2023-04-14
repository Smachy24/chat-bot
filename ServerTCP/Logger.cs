using System;
namespace ServerTCP
{
	public class Logger
	{
        private static readonly string _logFileName = "LogClass.txt";
		private DateTime _logDateTime;
		private string _baseLog;
		private string _logMessage;
		StreamWriter LogWriter = new StreamWriter(_logFileName, true);

        public Logger(string message)
		{
			_logDateTime = DateTime.Now;
			_baseLog = "[" + _logDateTime.ToString("dddd, dd MMMM yyyy HH:mm:ss") + "] : ";
			_logMessage = message;


            LogWriter.WriteLine(_baseLog + message);
			LogWriter.Close();
		}

		//public void purgeLo  // faire une fonction pour purger les log
    }
}

