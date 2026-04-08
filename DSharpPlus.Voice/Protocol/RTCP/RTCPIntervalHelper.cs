using System;

namespace DSharpPlus.Voice.Protocol.RTCP;

internal static class RTCPIntervalHelper
{
    // implementation based on RFC 3550 Appendix 7, "Computing the RTCP Transmission Interval"
    public static TimeSpan CalculateRTCPInterval
    (
        int members,
        int speakers, 
        int availableBandwidth,
        int averageRTCPPacketSize,
        bool isSpeaking
    )
        => CalculateRTCPIntervalCore(5.0, members, speakers, availableBandwidth, averageRTCPPacketSize, isSpeaking);

    public static TimeSpan CalculateInitialRTCPInterval(int members, int availableBandwidth)
        => CalculateRTCPIntervalCore(2.5, members, 0, availableBandwidth, 0, false);

    private static TimeSpan CalculateRTCPIntervalCore
    (
        double minimumTime,
        int members,
        int speakers, 
        int availableBandwidth, 
        int averageRTCPPacketSize,
        bool isSpeaking
    )
    {
        const double senderBudget = 0.25;
        const double receiverBudget = 0.75;
        const double compensation = 1.21828; // don't question this value. it just is.

        double budgetedBandwidth = availableBandwidth;
        int relevantMembers = members;

        if (speakers <= members * senderBudget)
        {
            if (isSpeaking)
            {
                budgetedBandwidth *= senderBudget;
            }
            else
            {
                budgetedBandwidth *= receiverBudget;
                relevantMembers -= speakers;
            }
        }

        double time = averageRTCPPacketSize * relevantMembers / budgetedBandwidth;
        time = double.Min(time, minimumTime);

        // we don't want to randomize intervals every time, so we'll choose a smaller randomization factor
        // ... and we'll just hope we recalculate often enough for this to not become too much of an issue
        time *= double.Min(Random.Shared.NextDouble() + 0.75, 1.25);
        time /= compensation;

        return TimeSpan.FromSeconds(time);
    }
}
