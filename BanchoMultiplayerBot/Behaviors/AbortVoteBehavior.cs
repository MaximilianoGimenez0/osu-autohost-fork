using BanchoMultiplayerBot.Attributes;
using BanchoMultiplayerBot.Bancho.Commands;
using BanchoMultiplayerBot.Constants;
using BanchoMultiplayerBot.Data;
using BanchoMultiplayerBot.Interfaces;

namespace BanchoMultiplayerBot.Behaviors;

public class AbortVoteBehavior(BehaviorEventContext context) : IBehavior
{
    private readonly IVote _abortVote = context.Lobby.VoteProvider!.FindOrCreateVote("AbortVote", BotMessages.Chat.Votes.AbortDescription);

    [BanchoEvent(BanchoEventType.MatchAborted)]
    public void OnMatchAborted()
    {
        _abortVote.Abort();
    }
    
    [BanchoEvent(BanchoEventType.MatchFinished)]
    public void OnMatchFinished()
    {
        _abortVote.Abort();
    }
    
    [BotEvent(BotEventType.CommandExecuted, "Abort")]
    public async Task OnAbortCommandExecuted(CommandEventContext commandEventContext)
    {
        if (commandEventContext.Player == null)
        {
            return;
        }
        
        if (_abortVote.PlayerVote(commandEventContext.Player))
        {
            await context.ExecuteCommandAsync<MatchAbortCommand>();
        }
    }
}
