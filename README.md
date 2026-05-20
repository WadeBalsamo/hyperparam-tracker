# PaperCutDash

A hyperparameter tracking dashboard for PaperCut, built with ASP.NET Core 8 MVC. record editing runs, tweak settings, and leave comments on how that changed outputs. This will help us to track what scikit-learn clustering hyperparams and NSP thresholds and coherence levels are best set for each preset when we add the 'MODES' feature, to create a preset for Podcasts, interviews, observational footage, and focus groups.

## Architecture

**MVC (Model-View-Controller)**: The app splits into three layers — Models (Run, Comment), a Controller (RunsController) that handles HTTP requests, and Views (Razor templates) that render HTML. The form on the index page creates new runs and accepts optional `config.json` files that map directly onto model properties via reflection.

**Data**: EF Core + SQLite. The database auto-creates on startup. Runs have many Comments (one-to-many relationship).

**How it works**: 
1. Index view displays all runs in an accordion, seeded with demo data on first load
2. Create form lets you input a run name + optional hyperparameters (inline form or JSON upload)
3. Comments section under each run lets the team leave notes

## Run it

```bash
dotnet run
```

Visit http://localhost:5029/Runs. runs default to basic hyperparameters (pace, coherence weight, prompts, etc.) but any field is optional.

## Config files

Takes json config files for logging comments per experiment run:
```json
{
  "Pace": 1.5,
  "CoherenceWeight": 0.4,
  "PositivePrompt": "key insights, examples"
}
```

The controller reflects over the Run model and assigns matching properties. Unknown fields are silently skipped.

## Tests

`dotnet test` runs the test suite (xUnit + EF In-Memory). Tests cover seeding, creating runs, adding comments, and JSON config parsing.
