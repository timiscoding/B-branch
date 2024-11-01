using System.Globalization;
using Bbranch.Shared.TableData;

namespace Bbranch.CLI.Output;

public class PrintFullTable
{
    private static int ConsoleHeight => Console.WindowHeight - 1;
    private static int LongestBranchNameLength { get; set; }
    private static string? currentSearchTerm;

    public static void Print(List<GitBranch> branches)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        const int minimumBranchNameWidth = 14;

        if (branches.Count == 0)
        {
            Console.WriteLine("No branches found");
            return;
        }

        LongestBranchNameLength = Math.Max(
            minimumBranchNameWidth,
            branches.Max(x => x.Branch.Name.Length + 1)
        );

        PrintHeaders();

        if (DoesOutputFitScreen(branches.Count))
        {
            StartPaging(branches);
            return;
        }

        PrintBranchRows(branches, null);
    }

    private static void StartPaging(List<GitBranch> branches)
    {
        Console.Clear();

        PrintHeaders();
        PrintBranchRows(branches.Take(ConsoleHeight - 2).ToList(), currentSearchTerm);

        Pager.Start(
            SpawnWindowListenerThread,
            UpdateView,
            HandleSearch,
            branches
        );
    }

    private static void SpawnWindowListenerThread(List<GitBranch> branches, int scrollPosition)
    {
        int currentPosition = scrollPosition;
        Thread resizeListenerThread = new(() => ListenForWindowResize(branches, ref currentPosition))
        {
            IsBackground = true
        };

        resizeListenerThread.Start();
    }

    private static void ListenForWindowResize(List<GitBranch> branches, ref int scrollPosition)
    {
        int previousHeight = Console.WindowHeight;

        while (true)
        {
            if (Console.WindowHeight != previousHeight)
            {

                previousHeight = Console.WindowHeight;

                Console.Clear();
                PrintHeaders();

                // Update the scroll position to the bottom branch that is visible
                if (scrollPosition + ConsoleHeight - 2 > branches.Count)
                {
                    scrollPosition = Math.Max(0, branches.Count - ConsoleHeight + 2);
                }

                UpdateView(branches, scrollPosition, string.Empty);

                if (branches.Count < ConsoleHeight - 1)
                {
                    for (int i = branches.Count; i < ConsoleHeight - 2; i++)
                    {
                        Console.SetCursorPosition(0, i + 2);
                        Console.Write('~');
                    }

                    PrintEndPromt();
                }
                else
                {
                    PrintCommandPromt();
                }
            }

            Thread.Sleep(50);
        }
    }

    private static void UpdateView(List<GitBranch> branches, int scrollPosition, string? searchTerm)
    {
        if (searchTerm is null)
        {
            currentSearchTerm = string.Empty;
        }

        Console.SetCursorPosition(0, 2);
        PrintBranchRows(branches.Skip(scrollPosition).Take(ConsoleHeight - 2).ToList(), currentSearchTerm);
    }

    private static int HandleSearch(List<GitBranch> branches, int scrollPosition)
    {
        PrintSearchPromt();

        currentSearchTerm = Console.ReadLine() ?? string.Empty;

        Console.Clear();

        if (string.IsNullOrEmpty(currentSearchTerm)) return scrollPosition;

        Console.SetCursorPosition(0, 0);
        PrintHeaders();

        int firstMatchIndex = branches.FindIndex(x => x.Branch.Name.Contains(currentSearchTerm, StringComparison.OrdinalIgnoreCase));

        if (firstMatchIndex == -1) return scrollPosition;

        if (Math.Abs(ConsoleHeight - branches.Count - 1) > firstMatchIndex)
        {
            scrollPosition = firstMatchIndex;
        }
        else
        {
            scrollPosition = Math.Abs(branches.Count - ConsoleHeight + 2);
        }


        for (int i = scrollPosition; i < branches.Count; i++)
        {
            if (i > ConsoleHeight + scrollPosition - 3) break;

            PrintBranchRowWithHighlight(branches[i], currentSearchTerm);
        }

        if (branches.Count > ConsoleHeight) return scrollPosition;

        for (int i = branches.Count; i < ConsoleHeight - 1; i++)
        {
            Console.SetCursorPosition(0, i + 2);
            Console.Write('~');
        }

        return scrollPosition;
    }

    private static void PrintHeaders()
    {
        string branchHeader = " Branch name  ".PadRight(LongestBranchNameLength + 1);
        string underline = new('-', LongestBranchNameLength + 1);
        string[] headers = [" Ahead 󰜘 ", " Behind 󰜘 ", branchHeader, " Last commit  "];

        PrintColoredLine(headers, ConsoleColor.Yellow);

        Console.WriteLine(
            $"\n--------- | ---------- | {underline} | {new string('-', +15)}"
        );
    }

    private static void PrintBranchRows(List<GitBranch> branchTable, string? search)
    {
        foreach (var branch in branchTable)
        {
            PrintBranchRowWithHighlight(branch, search);
        }
    }
    private static void PrintColoredLine(string[] texts, ConsoleColor color)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            Console.ForegroundColor = color;
            Console.Write(texts[i]);
            Console.ResetColor();
            if (i < texts.Length - 1)
            {
                Console.Write(" | ");
            }
        }

        Console.ResetColor();
    }

    private static void PrintBranchRowWithHighlight(GitBranch branch, string? search)
    {
        var aHead = branch.AheadBehind.Ahead.ToString().PadRight(8);
        var behind = branch.AheadBehind.Behind.ToString().PadRight(9);
        var branchName = branch.Branch.Name.PadRight(LongestBranchNameLength);
        var lastCommitText = GetTimePrefix(branch.LastCommit);
        var description = branch.Description ?? string.Empty;

        Console.Write(new string(' ', Console.WindowWidth - 1) + "\r");

        Console.Write(' ');
        HighlightText(aHead.ToString(), search);
        Console.Write(" |  ");
        HighlightText(behind.ToString(), search);
        Console.Write(" |  ");

        if (branch.Branch.IsWorkingBranch)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
        }

        HighlightText(branchName, search);

        Console.ResetColor();

        Console.Write(" |  ");
        HighlightText(lastCommitText, search);
        Console.Write("    ");

        HighlightText(description, search);

        Console.WriteLine();
    }

    private static void HighlightText(string text, string? search)
    {
        int matchIndex;

        if (search is null)
        {
            matchIndex = -1;
        }
        else
        {
            matchIndex = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        }

        search = search?.Trim();

        if (matchIndex >= 0)
        {
            Console.Write(text[..matchIndex]);

            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(text.Substring(matchIndex, search!.Length));

            Console.ResetColor();
            Console.Write(text[(matchIndex + search.Length)..]);
        }
        else
        {
            Console.Write(text);
        }
    }

    private static string GetTimePrefix(DateTime lastCommit)
    {
        DateTime currentTime = DateTime.Now;

        DateTimeFormatInfo dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

        int days = (currentTime - lastCommit).Days;

        if (days == 0)
        {
            string timeFormat;

            if (dateTimeFormat.ShortTimePattern.Contains("tt"))
            {
                timeFormat = (lastCommit.Hour > 9 && lastCommit.Hour < 13) ? "h:mm tt" : "h:mm  tt";
            }
            else
            {
                timeFormat = "HH:mm";
            }

            string time = lastCommit.ToString(timeFormat, CultureInfo.CurrentCulture);

            if (currentTime.Day - lastCommit.Day == 1) return $"{time} yesterday";

            return $"{time} today";
        }

        string timeElapsed = days == 1 ? "day" : "days";

        int padLeft = 5 - days.ToString().Length;
        return $"{days} {new string(' ', padLeft)}{timeElapsed} ago";
    }

    private static bool DoesOutputFitScreen(int branchCount) => branchCount > ConsoleHeight;

    private static void PrintCommandPromt()
    {
        Console.SetCursorPosition(0, ConsoleHeight);
        Console.Write(":    ");
    }

    private static void PrintEndPromt()
    {
        Console.SetCursorPosition(0, ConsoleHeight);
        Console.BackgroundColor = ConsoleColor.White;
        Console.ForegroundColor = ConsoleColor.Black;
        Console.Write("(END)");
        Console.ResetColor();
    }

    private static void PrintSearchPromt()
    {
        Console.SetCursorPosition(0, ConsoleHeight);
        Console.Write("/    ");
        Console.SetCursorPosition(1, ConsoleHeight);
    }
}