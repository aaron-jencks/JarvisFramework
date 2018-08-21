using System;
using System.Collections.Generic;
using System.Text;

namespace ModuleFramework
{
    public class CommandSyntaxInterpreter
    {
        /// <summary>
        /// Searches for any number of spaces in a string and returns them as a list of strings separated by spaces, and with the spaces removed.
        /// It will also parse for quoted strings ie. "Hello World!" will  be returned as "Hello World!" even though it has a space, because it's quoted.
        /// </summary>
        /// <param name="message">string to parse</param>
        /// <returns></returns>
        public static List<string> ParseMessageBySpace(string message)
        {
            List<string> temp = new List<string>(10);
            int index = 0;
            while (message.Contains(" "))
            {
                index = message.IndexOf(' ');
                if (message.StartsWith("\""))
                {
                    string tempStr = ParseQuoteString(ref message);
                    if (tempStr != "")
                        temp.Add(tempStr);
                }
                else
                {
                    temp.Add(message.Substring(0, index));
                    message = message.Remove(0, ++index);
                }
            }
            if (message != "")
                temp.Add(message);  // Add any remaining, since there is no space at the end
            return temp;
        }

        /// <summary>
        /// Returns the first quoted section in a string and removes it from the supplied string, and removes the quotation marks
        /// </summary>
        /// <param name="message">message to parse</param>
        /// <returns></returns>
        public static string ParseQuoteString(ref string message)
        {
            if (message.Contains("\""))
            {
                int index1 = message.IndexOf('\"');
                int index2 = message.IndexOf('\"', index1 + 1);
                if (index1 >= 0 && index2 >= 0)
                {
                    string temp = message.Substring(index1 + 1, index2 - index1 - 1);
                    message = message.Remove(index1, index2 - index1 + 1);
                    return temp;
                }
                else
                {
                    string temp = message;
                    message = "";
                    return temp;
                }
            }
            else
                return "";
        }
    }
}
