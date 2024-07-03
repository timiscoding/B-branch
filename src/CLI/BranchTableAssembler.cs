namespace CLI;

using Shared.TableData;
using Git.Options;
using CLI.Flags;

public class BranchTableAssembler
{
    internal static List<GitBranch> AssembleBranchTable(Dictionary<FlagType, string> arguments)
    {
        List<IOption> options = CreateOptions(arguments);
        CompositeOptionStrategy optionStrategies = new(options);

        AddBranchOptions(arguments, optionStrategies);
        AddLastCommitOption(optionStrategies);
        AddContainsOptions(arguments, optionStrategies);
        AddTrackOption(arguments, optionStrategies);
        AddWorkingBranchOption(optionStrategies);
        AddSortOption(arguments, optionStrategies);
        AddDescriptionOption(optionStrategies);
        AddPrintTopOption(arguments, optionStrategies);

        List<GitBranch> gitBranches = [];
        return optionStrategies.Execute(gitBranches);
    }

    private static List<IOption> CreateOptions(Dictionary<FlagType, string> arguments)
    {
        List<IOption> options = [];

        if (arguments.ContainsKey(FlagType.All))
        {
            IOption allOption = new BranchAllOptions();
            options.Add(allOption);

            return options;
        }

        if (arguments.ContainsKey(FlagType.Remote))
        {
            IOption remoteOption = new BranchRemoteOptions();
            options.Add(remoteOption);

            return options;
        }

        IOption localOption = new BranchLocalOptions();
        options.Add(localOption);

        return options;
    }

    private static void AddBranchOptions(Dictionary<FlagType, string> arguments, CompositeOptionStrategy optionStrategies)
    {
        if (arguments.ContainsKey(FlagType.All))
        {
            IOption allOption = new BranchAllOptions();
            optionStrategies.AddStrategyOption(allOption);

            return;
        }
        
        if (arguments.ContainsKey(FlagType.Remote))
        {
            IOption remoteOption = new BranchRemoteOptions();
            optionStrategies.AddStrategyOption(remoteOption);

            return;
        }

        IOption localOption = new BranchLocalOptions();
        optionStrategies.AddStrategyOption(localOption);
    }

    private static void AddLastCommitOption(CompositeOptionStrategy optionStrategies)
    {
        IOption lastCommitOption = new SetLastCommitOptions();
        optionStrategies.AddStrategyOption(lastCommitOption);
    }

    private static void AddContainsOptions(Dictionary<FlagType, string> arguments, CompositeOptionStrategy optionStrategies)
    {
        if (arguments.ContainsKey(FlagType.Contains))
        {
            var value = arguments[FlagType.Contains];
            IOption containsOption = new ContainsOption(value);
            optionStrategies.AddStrategyOption(containsOption);

            return;
        }

        if (arguments.ContainsKey(FlagType.NoContains))
        {
            var value = arguments[FlagType.NoContains];
            IOption noContainsOption = new NoContainsOption(value);
            optionStrategies.AddStrategyOption(noContainsOption);
        }
    }

    private static void AddTrackOption(Dictionary<FlagType, string> arguments, CompositeOptionStrategy optionStrategies)
    {
        if (arguments.ContainsKey(FlagType.Track))
        {
            var value = arguments[FlagType.Track];
            IOption trackOption = new TrackAheadBehindOption(value);
            optionStrategies.AddStrategyOption(trackOption);

            return;
        }

        IOption aheadBehindOption = new DefaultAheadBehindOption();
        optionStrategies.AddStrategyOption(aheadBehindOption);
    }

    private static void AddWorkingBranchOption(CompositeOptionStrategy optionStrategies)
    {
        IOption workingBranchOption = new WorkingBranchOption();
        optionStrategies.AddStrategyOption(workingBranchOption);
    }

    private static void AddSortOption(Dictionary<FlagType, string> arguments, CompositeOptionStrategy optionStrategies)
    {
        IOption sortOption;

        if (arguments.ContainsKey(FlagType.Sort))
        {
            var value = arguments[FlagType.Sort];

            if (value == "name")
            {
                sortOption = new SortByNameOptions();
                optionStrategies.AddStrategyOption(sortOption);
            }
            else if (value == "ahead")
            {
                sortOption = new SortByAheadOptions();
                optionStrategies.AddStrategyOption(sortOption);
            }
            else if (value == "behind")
            {
                sortOption = new SortByBehindOptions();
                optionStrategies.AddStrategyOption(sortOption);
            }

            return;
        }

        sortOption = new SortByLastEditedOptions();
        optionStrategies.AddStrategyOption(sortOption);
    }

    private static void AddDescriptionOption(CompositeOptionStrategy optionStrategies)
    {
        IOption descriptionOption = new DescriptionOption();
        optionStrategies.AddStrategyOption(descriptionOption);
    }

    private static void AddPrintTopOption(Dictionary<FlagType, string> arguments, CompositeOptionStrategy optionStrategies)
    {
        if (!arguments.ContainsKey(FlagType.PrintTop)) return;

        var topValue = Convert.ToInt32(arguments[FlagType.PrintTop]);
        IOption printTopOption = new TopOption(topValue);
        optionStrategies.AddStrategyOption(printTopOption);
    }

}