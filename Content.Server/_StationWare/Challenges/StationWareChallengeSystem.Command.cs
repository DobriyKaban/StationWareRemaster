using System.Linq;
using Content.Server.Administration;
using Content.Server._StationWare.GameRules;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._StationWare.Challenges;

public sealed partial class StationWareChallengeSystem
{
    [AdminCommand(AdminFlags.Debug)]
    private void StartChallengeCommand(IConsoleShell shell, string argstr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError("Argument length must be 1");
            return;
        }

        if (!_prototype.TryIndex<ChallengePrototype>(args[0], out var wareEvent))
        {
            shell.WriteError($"Challenge prototype '{args[0]}' not found.");
            return;
        }

        var ruleQuery = EntityQueryEnumerator<StationWareRuleComponent>();
        if (!ruleQuery.MoveNext(out _, out _))
        {
            shell.WriteLine("StationWare game rule is not active. Starting it now...");
            var ruleId = "StationWare";
            _gameTicker.StartGameRule(ruleId);
        }

        ruleQuery = EntityQueryEnumerator<StationWareRuleComponent>();
        if (ruleQuery.MoveNext(out var ruleUid, out var wareRule))
        {
            if (wareRule.CurrentChallenge != null)
            {
                shell.WriteLine("Ending active challenge...");
                EndChallenge(wareRule.CurrentChallenge.Value);
            }

            var challengeUid = StartChallenge(wareEvent);
            wareRule.CurrentChallenge = challengeUid;
            shell.WriteLine($"Successfully started challenge: {args[0]}");
        }
        else
        {
            shell.WriteError("Failed to start/find StationWare game rule, starting challenge standalone...");
            StartChallenge(wareEvent);
        }
    }

    private CompletionResult StartChallengeCommandCompletions(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                _prototype.EnumeratePrototypes<ChallengePrototype>().Select(p => p.ID), "<prototype ID>");
        }

        return CompletionResult.Empty;
    }
}
