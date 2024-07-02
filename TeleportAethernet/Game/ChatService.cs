/*
https://git.annaclemens.io/ascclemens/XivCommon/src/branch/main/XivCommon/Functions/Chat.cs 
MIT License
Copyright (c) 2021 Anna Clemens
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


using System;
using System.Text;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace TeleportAethernet.Game;

public unsafe class ChatService
{
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9")]
    private readonly delegate* unmanaged<UIModule*, Utf8String*, nint, byte, void> ProcessChatBox = null!;

    internal static ChatService? instance;
    public static ChatService Instance
    {
        get
        {
            instance ??= new();
            return instance;
        }
    }

    internal ChatService()
    {
        DalamudServices.GameInteropProvider.InitializeFromAttributes(this);
    }

    public void SendMessageUnsafe(byte[] message)
    {
        if (ProcessChatBox == null)
            throw new InvalidOperationException("Could not find signature for chat sending");

        var mes = Utf8String.FromSequence(message);
        ProcessChatBox(UIModule.Instance(), mes, IntPtr.Zero, 0);
        mes->Dtor(true);
    }

    public void SendMessage(string message)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        if (bytes.Length == 0)
            throw new ArgumentException("message is empty", nameof(message));

        if (bytes.Length > 500)
            throw new ArgumentException("message is longer than 500 bytes", nameof(message));

        if (message.Length != SanitiseText(message).Length)
            throw new ArgumentException("message contained invalid characters", nameof(message));

        SendMessageUnsafe(bytes);
    }

    private static string SanitiseText(string text)
    {
        var uText = Utf8String.FromString(text);

        uText->SanitizeString( 0x27F, (Utf8String*)nint.Zero);
        var sanitised = uText->ToString();
        uText->Dtor(true);

        return sanitised;
    }
}
