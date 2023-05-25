using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;

namespace DayzConnection;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private string ResolveHostname(string hostNameOrAddress)
    {
        IPHostEntry hostEntry;

        try
        {
            hostEntry = Dns.GetHostEntry(hostNameOrAddress);
        }
        catch
        {
            return hostNameOrAddress;
        }

        return hostEntry.HostName;
    }

    private void Refresh(string text)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PingResultEditor.Text += text + Environment.NewLine;

            SemanticScreenReader.Announce(PingResultEditor.Text);
        });
    }

    private void OnTracerouteClicked(object sender, EventArgs e)
    {
        PingResultEditor.Text = "";
        SemanticScreenReader.Announce(PingResultEditor.Text);

        var hostname = "195.18.27.235";
        var maxTtl = 30;
        var timeout = 1000;
        const int bufferSize = 32;
        var buffer = new byte[bufferSize];
        new Random().NextBytes(buffer);

        var stopwatch = new Stopwatch();

        var pinger = new Ping();

        _ = Task.Factory.StartNew(() =>
        {
            for (int ttl = 1; ttl <= maxTtl; ttl++)
            {
                stopwatch.Start();

                var pingOptions = new PingOptions(ttl, true);
                PingReply reply = null;

                try
                {
                    reply = pinger.Send(hostname, timeout, buffer, pingOptions);
                }
                catch (Exception exception)
                {
                    Refresh(exception.Message);

                    break;
                }

                if (reply != null)
                {
                    switch (reply.Status)
                    {
                        case IPStatus.TtlExpired:
                            {
                                var hostname = ResolveHostname(reply.Address.ToString());
                                Refresh($"[{ttl}] {hostname} in {stopwatch.ElapsedMilliseconds} ms");
                                stopwatch.Restart();
                                continue;
                            }
                        case IPStatus.TimedOut:
                            {
                                Refresh($"[{ttl}] Timeout.");
                                stopwatch.Restart();
                                continue;
                            }
                        case IPStatus.Success:
                            {
                                stopwatch.Stop();
                                Refresh($"[{ttl}] Successful trace route to {hostname} in {stopwatch.ElapsedMilliseconds} ms");
                            }
                            break;
                        default:
                            {                                
                                Refresh($"Default  - Time: {stopwatch.ElapsedMilliseconds} ms");
                                stopwatch.Restart();
                            }
                            break;
                    }
                }
                break;
            }
        });
    }

    private void OnPingClicked(object sender, EventArgs e)
    {
        PingResultEditor.Text = "";
        SemanticScreenReader.Announce(PingResultEditor.Text);

        var pinger = new Ping();

        var address = "195.18.27.235";
        var timeout = 1000;

        const int bufferSize = 32;
        var buffer = new byte[bufferSize];
        new Random().NextBytes(buffer);

        PingReply reply = pinger.Send(address, timeout, buffer);

        var hostname = ResolveHostname(reply.Address.ToString());

        Refresh($"Reply from {hostname} ({address}) in {reply.RoundtripTime} ms");        
    }
}

