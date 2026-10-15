using Microsoft.Extensions.Time.Testing;

public class FakeTimeProviderTests
{
    [Fact]
    public void FakeTimer_BeforeExpires_DoesntFire()
    {
        var clock = new FakeTimeProvider();
        var rings = 0;
        using var timer = clock.CreateTimer(_ =>
            rings++, null, TimeSpan.FromMinutes(45), Timeout.InfiniteTimeSpan);
        clock.Advance(TimeSpan.FromMinutes(44));

        Assert.Equal(0, rings);
    }

    [Fact]
    public void FakeTimer_WhenExpires_Fires()
    {
        var clock = new FakeTimeProvider();
        var rings = 0;
        using var timer = clock.CreateTimer(_ =>
            rings++, null, TimeSpan.FromMinutes(45), Timeout.InfiniteTimeSpan);
        clock.Advance(TimeSpan.FromMinutes(45));

        Assert.Equal(1, rings);
    }
}