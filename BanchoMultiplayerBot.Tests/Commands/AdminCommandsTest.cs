using BanchoMultiplayerBot.Commands;
using BanchoMultiplayerBot.Constants;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BanchoMultiplayerBot.Tests.Commands;

[TestClass]
public class AdminCommandsTest
{
    [TestMethod]
    public void AddAdminCommand_Properties_AreCorrect()
    {
        var command = new AddAdminCommand();

        Assert.AreEqual("addadmin", command.Command);
        Assert.IsTrue(command.Administrator);
        Assert.IsTrue(command.AllowGlobal);
        Assert.AreEqual(1, command.MinimumArguments);
        Assert.AreEqual(BotMessages.Chat.Usage.AddAdmin, command.Usage);
        CollectionAssert.Contains(command.Aliases, "op");
        CollectionAssert.Contains(command.Aliases, "setadmin");
    }

    [TestMethod]
    public void RemoveAdminCommand_Properties_AreCorrect()
    {
        var command = new RemoveAdminCommand();

        Assert.AreEqual("removeadmin", command.Command);
        Assert.IsTrue(command.Administrator);
        Assert.IsTrue(command.AllowGlobal);
        Assert.AreEqual(1, command.MinimumArguments);
        Assert.AreEqual(BotMessages.Chat.Usage.RemoveAdmin, command.Usage);
        CollectionAssert.Contains(command.Aliases, "deop");
        CollectionAssert.Contains(command.Aliases, "deladmin");
    }

    [TestMethod]
    public void AdminListCommand_Properties_AreCorrect()
    {
        var command = new AdminListCommand();

        Assert.AreEqual("adminlist", command.Command);
        Assert.IsTrue(command.Administrator);
        Assert.IsTrue(command.AllowGlobal);
        Assert.AreEqual(0, command.MinimumArguments);
        Assert.IsNull(command.Usage);
        CollectionAssert.Contains(command.Aliases, "admins");
    }
}

