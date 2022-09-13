namespace CraftingInterpreters.Repl;

using System.Text;

public class ReplConsole
{
    readonly StringBuilder buffer = new();
    int cursor = 0;
    int historyCursor = 0;
    readonly List<string> history = new();
    string prompt = "";

    void GoToEnd()
    {
        while (cursor < buffer.Length)
        {
            var diff = buffer.Length - cursor;
            if (Console.CursorLeft == Console.BufferWidth)
            {
                Console.SetCursorPosition(0, Console.CursorTop + 1);
            }
            var moveRight = Math.Min(diff, Console.BufferWidth - Console.CursorLeft);
            Console.SetCursorPosition(Console.CursorLeft + moveRight, Console.CursorTop);
            cursor += moveRight;
        }
    }

    int ClearLine()
    {
        var moveLeft = Console.CursorLeft;
        var line = string.Join("", Enumerable.Repeat(" ", moveLeft));
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(line);
        Console.SetCursorPosition(0, Console.CursorTop);
        return moveLeft;
    }

    void ClearBuffer()
    {
        GoToEnd();
        var length = buffer.Length;
        buffer.Clear();
        cursor = 0;
        while (length > 0)
        {
            var removed = ClearLine();
            length -= removed;

            if (length > 0)
            {
                Console.SetCursorPosition(Console.BufferWidth, Console.CursorTop - 1);
            }
        }
    }

    void SetCursorPosition(int position)
    {
        if (position > buffer.Length || position < prompt.Length)
        {
            return;
        }
        GoToEnd();
        var currentPosition = buffer.Length;
        while (currentPosition > position)
        {
            var diff = currentPosition - position;
            if (Console.CursorLeft == 0)
            {
                Console.SetCursorPosition(Console.BufferWidth, Console.CursorTop - 1);
            }
            var moveLeft = Math.Min(diff, Console.BufferWidth - Console.CursorLeft);
            currentPosition -= moveLeft;
            Console.SetCursorPosition(Console.CursorLeft - moveLeft, Console.CursorTop);
        }
        cursor = position;
    }

    void WriteToBuffer(string str)
    {
        if (cursor == buffer.Length)
        {
            Console.Write(str);
            buffer.Append(str);
            cursor += str.Length;
        }
        else
        {
            buffer.Insert(cursor, str);
            var currentCursor = cursor + str.Length;
            var currentBuffer = buffer.ToString();
            ClearBuffer();
            WriteToBuffer(currentBuffer);
            SetCursorPosition(currentCursor);
        }
    }

    void DeleteAtCursor()
    {
        var removeAt = cursor - 1;
        if (removeAt < prompt.Length || buffer.Length == prompt.Length || removeAt >= buffer.Length)
        {
            return;
        }
        var newBuffer = new StringBuilder(buffer.ToString());
        ClearBuffer();
        newBuffer.Remove(removeAt, 1);
        WriteToBuffer(newBuffer.ToString());
        SetCursorPosition(removeAt);
    }

    void Reset()
    {
        historyCursor = history.Count;
        Console.WriteLine();
        cursor = 0;
        buffer.Clear();
        prompt = "";
    }

    public record InputResult;
    public record LineResult(string line) : InputResult;
    public record ControlResult(ConsoleKey key) : InputResult;

    public InputResult GetInput(string prompt)
    {
        Console.TreatControlCAsInput = true;
        this.prompt = prompt;
        Console.Write(prompt);
        cursor += prompt.Length;
        buffer.Append(prompt);

        while (true)
        {
            var key = Console.ReadKey(true);

            switch ((key.Modifiers, key.Key))
            {
                case (_, ConsoleKey.Backspace):
                    DeleteAtCursor();
                    break;
                case (_, ConsoleKey.LeftArrow):
                    SetCursorPosition(cursor - 1);
                    break;
                case (_, ConsoleKey.RightArrow):
                    SetCursorPosition(cursor + 1);
                    break;
                case (_, ConsoleKey.UpArrow):
                    {
                        if (historyCursor <= 0 || historyCursor - 1 >= history.Count)
                        {
                            break;
                        }
                        ClearBuffer();
                        WriteToBuffer(prompt + history[historyCursor]);
                        historyCursor -= 1;
                        break;
                    }
                case (_, ConsoleKey.DownArrow):
                    {
                        if (historyCursor + 1 == history.Count)
                        {
                            ClearBuffer();
                            WriteToBuffer(prompt);
                            historyCursor = history.Count;
                            break;
                        }
                        else if (historyCursor + 1 > history.Count)
                        {
                            break;
                        }
                        ClearBuffer();
                        WriteToBuffer(prompt + history[historyCursor]);
                        historyCursor += 1;
                        break;
                    }
                case (_, ConsoleKey.Enter):
                    {
                        var line = buffer.Remove(0, prompt.Length).ToString();
                        history.Add(line);
                        Reset();
                        return new LineResult(line);
                    }
                case (ConsoleModifiers.Control, _):
                    Console.Write($"^{key.Key}");
                    Reset();
                    return new ControlResult(key.Key);
                default:
                    WriteToBuffer(key.KeyChar.ToString());
                    break;
            }
        }
    }
}
