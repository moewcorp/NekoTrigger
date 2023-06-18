using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Game;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using NekoTrigger.Windows;
using System.Threading.Tasks;
using ImGuiNET;
using System.Text.RegularExpressions;
using System.Linq;
using Serilog.Events;
using Serilog.Core;
using System.Reflection;
using Lumina.Excel.GeneratedSheets;

namespace NekoTrigger
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "NekoTrigger";
        private DalamudPluginInterface pi { get; init; }
        private CommandManager cmd { get; init; }
        internal object pm { get; init; }
        internal object hm { get; init; }
        internal dynamic logger { get; init; }
        internal EventInfo eiLogLine { get; init; }
        internal Delegate lld { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("NekoTrigger");

        private MainWindow MainWindow { get; init; }

        internal List<Trigger> triggers { get; init; }
        
        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {   
            
            this.pi = pluginInterface;
            this.cmd = commandManager;
            this.logger = pi.GetType().Assembly.GetType("Dalamud.Logging.Internal.SerilogEventSink").GetProperty("Instance")?.GetValue(null);
            eiLogLine = logger.GetType().GetEvent("LogLine");
            Type tDelegate = eiLogLine.EventHandlerType;
            MethodInfo miHandler = typeof(Plugin).GetMethod("DalamudLogDispatch", BindingFlags.NonPublic | BindingFlags.Instance);
            lld = Delegate.CreateDelegate(tDelegate, this, miHandler);
            eiLogLine.GetAddMethod()?.Invoke(logger, new object[] { lld });
            

            DalamudApi.Initialize(this,pi);
            pi.UiBuilder.OpenConfigUi += DrawConfigUI;

            this.Configuration = this.pi.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.pi);

            MainWindow = new MainWindow(this);
            WindowSystem.AddWindow(MainWindow);
            this.pi.UiBuilder.Draw += DrawUI;

            this.cmd.AddHandler("/trigger", new CommandInfo(OnCommand) { HelpMessage = "Neko Trigger Panel" });
            this.cmd.AddHandler("/ncmd", new CommandInfo(OnCommand) { HelpMessage = "Send command to in-game chatbox" });

            DalamudApi.Framework.Update += ExecuteCommand;
            DalamudApi.ChatGui.ChatMessage += ChatLogDispatch;
            
            triggers= new List<Trigger>(Configuration.Tiggers);

            //logline += DalamudLogDispatch;

        }

        private static ConcurrentQueue<string> CommandQueue = new ConcurrentQueue<string>();
        public void EnqueueCommand(string command)
        {
            CommandQueue.Enqueue(command);
        }
        public void ExecuteCommand(Framework framework)
        {
            CommandQueue.TryDequeue(out string? cmd);
            if (cmd == null) return;
            if (!cmd.StartsWith('/')) return;
            try
            {
                Chat.Instance.SendMessage(cmd);
            }catch (Exception ex)
            {
                PluginLog.Error(ex.Message);
            }
            
        }

        private void ChatLogDispatch(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            foreach (var trigger in triggers.Select(t=>t).Where(t=>t.enable&&t.source==Sources.ChatLog))
            {
                DispatchTrigger(message.ToString(), trigger);
            }
        }
        private void DalamudLogDispatch(object sender, (string Line, LogEvent LogEvent) logEvent)
        {
            foreach (var trigger in triggers.Select(t => t).Where(t => t.enable && t.source == Sources.DalamudLog))
            {
                DispatchTrigger(logEvent.Line, trigger);
            }
        }
        private void DispatchTrigger(string logline,Trigger trigger)
        {
            
            Match m = Regex.Match(logline, trigger.regex, RegexOptions.IgnoreCase);

            if (m.Success)
            {
                Trigger proxyTrigger = new Trigger(trigger);
                for (int i = 1; i < m.Groups.Count; i++)
                {
                    proxyTrigger.param = proxyTrigger.param.Replace($"{{{i}}}", m.Groups[i].Value);
                }
                ExecuteTrigger(proxyTrigger);
            }
        }
        private void ExecuteTrigger(Trigger t)
        {
            switch (t.action)
            {
                case Actions.Command:EnqueueCommand(t.param);break;
            }
        }
        public void Dispose()
        {
            eiLogLine.GetRemoveMethod()?.Invoke(logger, new object[] { lld });

            DalamudApi.ChatGui.ChatMessage -= ChatLogDispatch;
            DalamudApi.Framework.Update -= ExecuteCommand;
            this.WindowSystem.RemoveAllWindows();
            this.cmd.RemoveHandler("/trigger");
            this.cmd.RemoveHandler("/ncmd");
            MainWindow.Dispose();
            DalamudApi.Dispose();
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            //MainWindow.IsOpen = true;
            switch (command)
            {
                case "/trigger":DrawConfigUI(); break;
                case "/ncmd":EnqueueCommand(args);break;
                default: ;break;
            }
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            MainWindow.Toggle();
        }
    }
}
