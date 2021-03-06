using System;
using System.Collections.Generic;
using System.Text;
using Yad.Net.Messaging.Common;

namespace Yad.Net.Client
{
    public static class Utils
    {
        public static Message CreateMessageWithSenderId(MessageType type)
        {
            Message message = MessageFactory.Create(type);
            message.SenderId = ClientPlayerInfo.SenderId;
            return message;
        }
    }
}
