﻿namespace avenabot.Interpreter
{
    public class Command
    {
        public delegate string CommandMethod(string message, string sender);

        public CommandMethod cmd;
        public int ID;
        public string name;
        public string descr;
        public bool admin;

        public Command(CommandMethod C, int cmdID, string cmdName, string cmdDescr, bool adminOnly = false)
        {
            ID = cmdID;
            cmd = C;
            name = cmdName;
            descr = cmdDescr;
            admin = adminOnly;
        }

        public string Execute(string message, string sender)
        {
            return cmd(message, sender);
        }
    }
}