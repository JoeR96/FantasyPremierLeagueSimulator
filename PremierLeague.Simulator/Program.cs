// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using PremierLeague.Simulator;

var services = new ServiceCollection();
services.AddExternalHttpClient().AddSimulatorServices();

var sp = services.BuildServiceProvider();
var fpl = sp.GetService<FplService>();

var _ = await fpl.GetTeamsAsync();
Console.Write(_);

var premierLeaguesimulator = new PremierLeagueSimulator(fpl);

await premierLeaguesimulator.PrepareSeason();
premierLeaguesimulator.SimulateSeason();
