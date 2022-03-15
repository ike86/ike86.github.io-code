using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;

namespace MyFirstCodeAnalyzer;

public static class Program
{
    private const string SolutionFilePath = @"..\..\..\..\MyFirstCodeAnalyzer.Target\MyFirstCodeAnalyzer.Target.sln";

    public static async Task Main()
    {
        var instance = GetVisualStudioInstance();
        using var workspace = CreateMsBuildWorkspace(instance);
        var solution = await OpenSolution(workspace);
        await Analyze(solution);
    }

    private static VisualStudioInstance GetVisualStudioInstance()
    {
        var visualStudioInstances =
            MSBuildLocator.QueryVisualStudioInstances().ToArray();
        var instance = visualStudioInstances.Length == 1
            ? visualStudioInstances[0]
            : SelectVisualStudioInstance(visualStudioInstances);

        Console.WriteLine(
            $"Using MSBuild at '{instance.MSBuildPath}' to load projects.");
        return instance;
    }

    private static MSBuildWorkspace CreateMsBuildWorkspace(
        VisualStudioInstance instance)
    {
        // NOTE: Be sure to register an instance with the MSBuildLocator 
        //       before calling MSBuildWorkspace.Create()
        //       otherwise, MSBuildWorkspace won't MEF compose.
        MSBuildLocator.RegisterInstance(instance);

        var workspace = MSBuildWorkspace.Create();
        workspace.WorkspaceFailed +=
            (_, e) => Console.WriteLine(e.Diagnostic.Message);
        return workspace;
    }

    private static async Task<Solution> OpenSolution(MSBuildWorkspace workspace)
    {
        Console.WriteLine($"Loading solution '{SolutionFilePath}'");

        // Attach progress reporter so we print projects as they are loaded.
        var solution =
            await workspace.OpenSolutionAsync(
                SolutionFilePath,
                new ConsoleProgressReporter());
        Console.WriteLine($"Finished loading solution '{SolutionFilePath}'");
        return solution;
    }

    private static VisualStudioInstance SelectVisualStudioInstance(
        VisualStudioInstance[] visualStudioInstances)
    {
        Console.WriteLine(
            "Multiple installs of MSBuild detected please select one:");
        for (int i = 0; i < visualStudioInstances.Length; i++)
        {
            Console.WriteLine($"Instance {i + 1}");
            Console.WriteLine($"    Name: {visualStudioInstances[i].Name}");
            Console.WriteLine(
                $"    Version: {visualStudioInstances[i].Version}");
            Console.WriteLine(
                $"    MSBuild Path: {visualStudioInstances[i].MSBuildPath}");
        }

        while (true)
        {
            var userResponse = Console.ReadLine();
            if (int.TryParse(userResponse, out int instanceNumber)
                && instanceNumber > 0
                && instanceNumber <= visualStudioInstances.Length)
            {
                return visualStudioInstances[instanceNumber - 1];
            }

            Console.WriteLine("Input not accepted, try again.");
        }
    }
    
    private static async Task Analyze(Solution solution)
    {
        Console.WriteLine();
        Console.WriteLine("Analyzing the solution...");
        _ = solution;
        await Task.CompletedTask;
    }

    private class ConsoleProgressReporter : IProgress<ProjectLoadProgress>
    {
        public void Report(ProjectLoadProgress loadProgress)
        {
            var projectDisplay = Path.GetFileName(loadProgress.FilePath);
            if (loadProgress.TargetFramework != null)
            {
                projectDisplay += $" ({loadProgress.TargetFramework})";
            }

            Console.WriteLine(
                $"{loadProgress.Operation,-15} {loadProgress.ElapsedTime,-15:m\\:ss\\.fffffff} {projectDisplay}");
        }
    }
}