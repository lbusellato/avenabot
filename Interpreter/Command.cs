using Telegram.Bot.Args;

namespace avenabot.Interpreter
{
    public class Command
    {
        public delegate string CommandMethod(MessageEventArgs e);

        public CommandMethod cmd;
        public int ID;
        public string name;
        public string descr;
        public bool admin;
        public bool enabled;

        public Command(CommandMethod C, int cmdID, string cmdName, string cmdDescr, bool adminOnly = false, bool isEnabled = true)
        {
            ID = cmdID;
            cmd = C;
            name = cmdName;
            descr = cmdDescr;
            admin = adminOnly;
            enabled = isEnabled;
        }

        public string Execute(MessageEventArgs e)
        {
            return cmd(e);
        }
    }
}
