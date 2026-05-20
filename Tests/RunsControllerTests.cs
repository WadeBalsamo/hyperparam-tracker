using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaperCutDash.Controllers;
using PaperCutDash.Data;
using PaperCutDash.Models;
using Xunit;

namespace PaperCutDash.Tests;

public class RunsControllerTests
{
    private AppDb CreateInMemoryDb()
    {
        var opts = new DbContextOptionsBuilder<AppDb>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDb(opts);
    }

    [Fact]
    public async Task Index_SeedsDataOnFirstLoad()
    {
        var db = CreateInMemoryDb();
        var controller = new RunsController(db);

        var result = await controller.Index() as ViewResult;

        Assert.NotNull(result);
        var runs = result.Model as List<Run>;
        Assert.NotNull(runs);
        Assert.Equal(3, runs.Count);
    }

    [Fact]
    public async Task Index_DoesNotSeedTwice()
    {
        var db = CreateInMemoryDb();
        var controller = new RunsController(db);

        await controller.Index();
        var countAfterFirst = await db.Runs.CountAsync();

        await controller.Index();
        var countAfterSecond = await db.Runs.CountAsync();

        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    [Fact]
    public async Task Create_AddsRunWithoutConfigFile()
    {
        var db = CreateInMemoryDb();
        var controller = new RunsController(db);

        var run = new Run { Name = "test-run", Pace = 1.5 };
        var result = await controller.Create(run, null) as RedirectToActionResult;

        Assert.NotNull(result);
        Assert.Equal(nameof(RunsController.Index), result.ActionName);

        var saved = await db.Runs.FirstOrDefaultAsync(r => r.Name == "test-run");
        Assert.NotNull(saved);
        Assert.Equal(1.5, saved.Pace);
    }

    [Fact]
    public async Task Create_RejectsEmptyName()
    {
        var db = CreateInMemoryDb();
        var controller = new RunsController(db);
        controller.ModelState.Clear();

        var run = new Run { Name = "  " };
        var result = await controller.Create(run, null);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_ParsesConfigJsonCorrectly()
    {
        var db = CreateInMemoryDb();
        var controller = new RunsController(db);
        controller.ModelState.Clear();

        var json = "{\"Pace\": 2.0, \"CoherenceWeight\": 0.5, \"TargetOutputLenSeconds\": 120}";
        var file = new FormFile(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)),
            0,
            json.Length,
            "configFile",
            "config.json");

        var run = new Run { Name = "with-config" };
        await controller.Create(run, file);

        var saved = await db.Runs.FirstOrDefaultAsync(r => r.Name == "with-config");
        Assert.NotNull(saved);
        Assert.Equal(2.0, saved.Pace);
        Assert.Equal(0.5, saved.CoherenceWeight);
        Assert.Equal(120, saved.TargetOutputLenSeconds);
    }

    [Fact]
    public async Task Create_SkipsUnknownJsonFields()
    {
        var db = CreateInMemoryDb();
        var controller = new RunsController(db);
        controller.ModelState.Clear();

        var json = "{\"Pace\": 1.8, \"UnknownField\": \"should-be-ignored\"}";
        var file = new FormFile(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)),
            0,
            json.Length,
            "configFile",
            "config.json");

        var run = new Run { Name = "with-unknown" };
        await controller.Create(run, file);

        var saved = await db.Runs.FirstOrDefaultAsync(r => r.Name == "with-unknown");
        Assert.NotNull(saved);
        Assert.Equal(1.8, saved.Pace);
    }

    [Fact]
    public async Task Comment_AddsCommentToRun()
    {
        var db = CreateInMemoryDb();
        var controller = new RunsController(db);

        var run = new Run { Name = "test", Id = Guid.NewGuid() };
        db.Runs.Add(run);
        await db.SaveChangesAsync();

        await controller.Comment(run.Id, "alice", "great run!");

        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Body == "great run!");
        Assert.NotNull(comment);
        Assert.Equal("alice", comment.Author);
        Assert.Equal(run.Id, comment.RunId);
    }

    [Fact]
    public async Task Comment_UsesAnonIfAuthorNull()
    {
        var db = CreateInMemoryDb();
        var controller = new RunsController(db);

        var run = new Run { Name = "test", Id = Guid.NewGuid() };
        db.Runs.Add(run);
        await db.SaveChangesAsync();

        await controller.Comment(run.Id, null, "anon comment");

        var comment = await db.Comments.FirstOrDefaultAsync(c => c.Body == "anon comment");
        Assert.NotNull(comment);
        Assert.Equal("anon", comment.Author);
    }
}
