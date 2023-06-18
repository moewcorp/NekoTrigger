using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace NekoTrigger
{
    public class Trigger
    {
        public bool enable = false;
        public string name = "";
        public Actions action = Actions.Command;
        public Sources source = Sources.ChatLog;
        public string regex = "";
        public string param = "";
        public Trigger() { }
        public Trigger(Trigger trigger)
        {
            this.enable = trigger.enable;
            this.name = trigger.name;
            this.action = trigger.action;
            this.source = trigger.source;
            this.regex = trigger.regex;
            this.param = trigger.param;
        }
    }
    public enum Actions
    {
        Command

    }
    
    public enum Sources
    {
        ChatLog,
        DalamudLog
    }

    //public static class TriggerExt
    //{
    //    public static string ToName(this Actions action)
    //    {
    //        return action switch
    //        {
    //            Actions.Command => "Command",
    //            _ => "NotExist",
    //        };
    //    }
    //}
}
