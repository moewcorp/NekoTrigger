

namespace NekoTrigger.Helper;

public class ChatHelper
{
    /// <summary>
    /// 向游戏内发送聊天框信息
    /// </summary>
    /// <param name="message"></param>
    public static void SendMessage(string message)
    {
        ECommons.Automation.Chat.Instance.SendMessage(message);
    }
}
