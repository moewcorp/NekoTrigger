using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Xml.Linq;
using Dalamud.Game.Command;
using Dalamud.Hooking.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using ImGuiNET;
using ImGuiScene;
using NekoTrigger.Helper;
using Serilog;

namespace NekoTrigger.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    public MainWindow(Plugin plugin) : base(
        "Neko Trigger", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(475, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.Plugin = plugin; 
    }

    public void Dispose()
    {
    }


    private Trigger tmpTrigger = new Trigger();
    public override void Draw()
    {

        ImGui.BeginTabBar("NekoNeko");

        if (ImGui.BeginTabItem("Triggers")) {

            if (ImGui.Button("New##neko"))
            {
                Plugin.triggers.Add(tmpTrigger);
            }
            ImGui.SameLine();
            if (ImGui.Button("Save##neko"))
            {
                Plugin.Configuration.Tiggers = Plugin.triggers.ToArray();
                Plugin.Configuration.Save();
            }
            ImGui.SameLine();
            if (ImGui.Button("测试##neko"))
            {
                Log.Information("测试成功");
                ChatHelper.SendMessage("/e 测试成功！");
            }

            if (ImGui.BeginTable("TriggersTable", 6, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
            {
                ImGui.TableSetupColumn("Enable");
                ImGui.TableSetupColumn("Name"  );
                ImGui.TableSetupColumn("Source");
                ImGui.TableSetupColumn("Regex" );
                ImGui.TableSetupColumn("Action");
                ImGui.TableSetupColumn("Param" );
                ImGui.TableHeadersRow();
                int i=0;
                foreach (Trigger t in Plugin.triggers)
                {
                    ImGui.TableNextColumn();
                    ImGui.Checkbox($"##enable{i}", ref t.enable);
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(ImGui.CalcItemWidth() * 1.6f);
                    ImGui.InputText($"##name{i}",ref t.name,20);
                    ImGui.TableNextColumn();
                    //ImGui.ComboBox($"Source##{i}",ref t.source,,)
                    if (ImGui.BeginCombo("##{i}", t.source.ToString()))
                    {
                        foreach (Sources s in (Sources[])Enum.GetValues(typeof(Sources)))
                        {
                            if (ImGui.Selectable(s.ToString()))
                            {
                                t.source = s;
                            }
                        }
                        ImGui.EndCombo();
                    }
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(ImGui.CalcItemWidth() * 1.6f);
                    ImGui.InputText($"##regex{i}", ref t.regex, 100);
                    ImGui.TableNextColumn();
                    if (ImGui.BeginCombo("##{i}action", t.action.ToString()))
                    {
                        foreach (Actions s in (Actions[])Enum.GetValues(typeof(Actions)))
                        {
                            if (ImGui.Selectable(s.ToString()))
                            {
                                t.action = s;
                            }
                        }
                        ImGui.EndCombo();
                    }
                    ImGui.TableNextColumn();
                    ImGui.SetNextItemWidth(ImGui.CalcItemWidth()*1.6f);
                    ImGui.InputText($"##param{i}", ref t.param, 400);
                    i++;
                }
                ImGui.EndTable();
            }
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Log"))
        {
            LogTab.Draw();
            ImGui.EndTabItem();
        }


        ImGui.EndTabBar();
    }



    public void RightClickToCopyCmd(string cmd)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Right-click to copy command:\n  {cmd}");
        }

        if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            ImGui.SetClipboardText($"{cmd}");
        }
    }
}
