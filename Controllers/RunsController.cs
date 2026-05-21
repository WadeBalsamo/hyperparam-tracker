using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaperCutDash.Data;
using PaperCutDash.Models;

namespace PaperCutDash.Controllers;

public class RunsController : Controller
{
    private readonly AppDb _db;
    public RunsController(AppDb db) { _db = db; }

    private async Task SeedIfEmpty()
    {
        if (await _db.Runs.AnyAsync()) return;

        _db.Runs.AddRange(new List<Run>
        {
            new()
            {
                Name = "demo-lecture-1",
                PositivePrompt = "explanation, diagram, key concept",
                NegativePrompt = "repetition, digression",
                Pace = 1.2,
                TargetOutputLenSeconds = 120,
                CoherenceWeight = 0.3,
                VarianceWeight = 0.5,
                MinimumSecsPerCut = 3,
                Padding = 7,
                EndPadding = 0.5,
                MaxGroups = 50,
                FilterNPercentIrrelevant = 0.1,
                CoherenceOverTimeliness = 1.04,
                DurationMicroparam = 2.5,
                ModelSize = "medium.en",
                EditedDuration = 112.4,
                Nonce = "abc123",
                Comments = new List<Comment>
                {
                    new() { Author = "alice", Body = "decent cut, lost a bit of context at  3:20" },
                    new() { Author = "bob", Body = "try lowering coherence to 0.25" },
                }
            },
            new()
            {
                Name = "demo-podcast-2",
                PositivePrompt = "main topic, conclusion",
                NegativePrompt = "uh, um, filler",
                Pace = 1.4,
                TargetOutputLenSeconds = 300,
                CoherenceWeight = 0.4,
                VarianceWeight = 0.4,
                MinimumSecsPerCut = 4,
                Padding = 5,
                EndPadding = 0.3,
                MaxGroups = 30,
                FilterNPercentIrrelevant = 0.15,
                CoherenceOverTimeliness = 1.02,
                DurationMicroparam = 2.0,
                ModelSize = "large",
                EditedDuration = 288.7,
                Nonce = "def456",
            },
            new()
            {
                Name = "demo-interview-3",
                PositivePrompt = "Start from the beginning how I liked engineering as a child..., I was always more enamoured by the technological wonder that was adobe premiere pro that let me edit tens of terrabytes of video. I realized I liked building tools more than using them.",
                NegativePrompt = "off-topic, noise",
                PosFilterPrompt = "speaker A",
                Pace = 1.0,
                TargetOutputLenSeconds = 180,
                CoherenceWeight = 0.5,
                VarianceWeight = 0.3,
                MinimumSecsPerCut = 2,
                Padding = 8,
                EndPadding = 0.5,
                MaxGroups = 40,
                PostFilter = true,
                PreFilter = true,
                PreFilterPosWeight = 1.5,
                FilterNPercentIrrelevant = 0.08,
                CoherenceOverTimeliness = 1.06,
                DurationMicroparam = 3.0,
                ModelSize = "medium.en",
                Quick = true,
                EditedDuration = 165.2,
                Nonce = "ghi789",
            },
        });

        await _db.SaveChangesAsync();
    }

    // --- Index: list runs + create form inline ---
    public async Task<IActionResult> Index()
    {
        await SeedIfEmpty();
        return View(await _db.Runs.Include(r => r.Comments).OrderByDescending(r => r.CreatedAt).ToListAsync());
    }

    // --- Create via inline form ---
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Run run, IFormFile? configFile)
    {
        if (string.IsNullOrWhiteSpace(run.Name))
        {
            ModelState.AddModelError("", "Name is required.");
            return RedirectToAction(nameof(Index));
        }

        // Parse config.json if provided  — fields that exist in the JSON
        // map directly onto the model by name.
        if (configFile is not null && configFile.Length > 0)
        {
            using var sr = new StreamReader(configFile.OpenReadStream());
            var json = await sr.ReadToEndAsync();
            run.ConfigJsonRaw = json;

            try
            {
                var doc = JsonDocument.Parse(json);
                foreach (var p in doc.RootElement.EnumerateObject())
                {
                    try
                    {
                        var prop = typeof(Run).GetProperty(p.Name);
                        if (prop is null || !prop.CanWrite) continue;

                        var propType = prop.PropertyType;
                        if (propType == typeof(double))
                            prop.SetValue(run, p.Value.GetDouble());
                        else if (propType == typeof(int))
                            prop.SetValue(run, p.Value.GetInt32());
                        else if (propType == typeof(bool))
                            prop.SetValue(run, p.Value.GetBoolean());
                        else if (propType == typeof(string))
                            prop.SetValue(run, p.Value.GetString());
                    }
                    catch { /* skip fields that dont parse */ }
                }
            }
            catch { ModelState.AddModelError("", "config.json is invalid JSON."); }
        }

        _db.Runs.Add(run);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // --- Inline comment ---
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Comment(Guid id, string? author, string body)
    {
        _db.Comments.Add(new Comment { RunId = id, Author = author ?? "anon", Body = body ?? "" });
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}