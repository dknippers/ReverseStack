#if DEBUG

using StackToBottom.Extensions;
using System.Text;
using UnityEngine;

namespace StackToBottom.Debugging;

internal static class DebugExtensions
{
    private static readonly Dictionary<int, Dictionary<string, string>> _logs = [];

    internal static void Log(this GameCard card, string key, string message)
    {
        var id = card.GetInstanceID();

        if (_logs.TryGetValue(id, out var messages) &&
            messages.TryGetValue(key, out var prevMsg) &&
            prevMsg == message)
        {
            return;
        }

        Debug.Log(card.GetDebugName() + " | " + message);

        if (messages == null)
        {
            _logs[id] = messages = [];
        }

        messages[key] = message;
    }

    internal static string GetDebugName(this GameCard? card)
    {
        if (card is null) return "NULL";

        return new StringBuilder()
            .Append('[')
            .Append(Math.Abs(card.GetInstanceID()))
            .Append(']')
            .Append(' ')
            .Append(card.GetName())
            .ToString();
    }
}

#endif