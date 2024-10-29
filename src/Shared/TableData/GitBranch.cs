namespace Bbranch.Shared.TableData;

public class GitBranch
{
    public Branch Branch { get; private set; }
    public AheadBehind AheadBehind { get; private set; }
    public DateTime LastCommit { get; private set; }
    public string? Description { get; private set; }

    public GitBranch(
        AheadBehind aheadBehind,
        Branch branch,
        DateTime lastCommit,
        string description
    )
    {
        SetAheadBehind(aheadBehind);
        SetBranch(branch);
        SetLastCommit(lastCommit);
        SetDescription(description);
    }

    public GitBranch SetAheadBehind(AheadBehind aheadBehind)
    {
        if (aheadBehind.Ahead < 0 || aheadBehind.Behind < 0)
        {
            throw new ArgumentException("Ahead and Behind should be positive integers");
        }

        AheadBehind = aheadBehind;

        return this;
    }

    public GitBranch SetBranch(Branch branch)
    {
        if (string.IsNullOrEmpty(branch.Name))
        {
            throw new ArgumentException("Branch name should not be empty");
        }

        Branch = branch;

        return this;
    }

    public GitBranch SetLastCommit(DateTime lastCommit)
    {
        if (lastCommit == DateTime.MinValue)
        {
            throw new ArgumentException("Last commit should not be empty");
        }

        LastCommit = lastCommit;

        return this;
    }

    public GitBranch SetDescription(string description)
    {
        Description = description ?? string.Empty;

        return this;
    }

    public static GitBranch Default()
    {
        return new GitBranch(
            new AheadBehind { Ahead = 0, Behind = 0 },
            new Branch { Name = "branchName", IsWorkingBranch = false },
            DateTime.MaxValue,
            string.Empty
        );
    }
}