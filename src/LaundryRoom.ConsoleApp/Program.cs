using Microsoft.Extensions.Time.Testing;

var clock = new FakeTimeProvider(new DateTimeOffset(2026, 1, 7, 8, 0, 0, TimeSpan.Zero));

var tariff = Tariff.From(Money.From(30));
var journal = new Journal(clock);

var green = new Room("green");
var washer = new Washer(Guid.NewGuid(), green, clock);
var dryer = new Dryer(Guid.NewGuid(), green, clock);
var fan = new Fan(Guid.NewGuid(), green);

var washerMeter = new SimulatedMeter(clock, 0.5m);
var dryerMeter = new SimulatedMeter(clock, 1.0m);
var fanMeter = new SimulatedMeter(clock, 0.4m);

washer.ProgramStarted += program => washerMeter.Start();
washer.ProgramCompleted += program => washerMeter.Stop();
washer.ProgramInterrupted += program => washerMeter.Stop();

dryer.ProgramStarted += program => dryerMeter.Start();
dryer.ProgramCompleted += program => dryerMeter.Stop();
dryer.ProgramInterrupted += program => dryerMeter.Stop();

fan.Started += program => fanMeter.Start();
fan.Stopped += program => fanMeter.Stop();

journal.Attach(washer, washerMeter, green);
journal.Attach(dryer, dryerMeter, green);
journal.Attach(fan, fanMeter, green);

var card3B = new Card(Guid.NewGuid());
card3B.Assign(Flat.From("3B"));
var card2A = new Card(Guid.NewGuid());
card2A.Assign(Flat.From("2A"));