using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GameCore.Tools
{
    public class Logging
    {
        // https://stackoverflow.com/questions/12556767/how-do-i-get-the-current-line-number

        public static string GetTrace(string message, bool writeToConsole = false,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string callingFilePath = null,
            [CallerMemberName] string caller = null)
        {
            string report = message + " - " + caller + "() " + callingFilePath + ":" + lineNumber;
            if (writeToConsole)
                Console.WriteLine(report);
            return report;
        }

    }
}
