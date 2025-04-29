// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using PremierLeague.Simulator;
using PremierLeague.Simulator.Features.Fpl;

var services = new ServiceCollection();
services.AddExternalHttpClient().AddSimulatorServices();

var sp = services.BuildServiceProvider();
var fpl = sp.GetService<FplService>();

var premierLeaguesimulator = new PremierLeagueSimulator(fpl);

await premierLeaguesimulator.PrepareSeason();
premierLeaguesimulator.SimulateSeason();
