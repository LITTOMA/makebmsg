using System;
using System.IO;
using System.Windows.Forms;

namespace makebmsg
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: {0} [JSON File] [BMSG File]", Path.GetFileName(Application.ExecutablePath));
                return;
            }
            BinaryMessage bmsg = JSON.Parse<BinaryMessage>(File.ReadAllText(args[0]));
            bmsg.Save(args[1]);
        }
    }
}
