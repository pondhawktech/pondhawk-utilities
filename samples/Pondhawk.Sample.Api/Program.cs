using Pondhawk.Sample.Api;

var app = Bootstrap.Build(args);
app.Run();

/// <summary>Assembly marker, so endpoint/handler discovery and test hosting can reference this assembly.</summary>
public partial class Program;
