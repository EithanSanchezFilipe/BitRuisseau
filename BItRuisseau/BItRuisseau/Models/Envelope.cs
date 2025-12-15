using BitRuisseau.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace BitRuisseau.Models
{
    public class Envelope(string senderId, string receiverId, MessageType type, string? message)
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();
        public string SenderId { get; init; } = senderId;
        public string? ReceiverId { get; init; } = receiverId;
        public MessageType Type { get; init; } = type;
        public string Message { get; set; } = message;

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static Envelope? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Envelope>(json);
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}
