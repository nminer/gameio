using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    internal struct SineWave
{
    internal readonly double Amplitude;
    internal readonly double OrdinaryFrequency;
    internal readonly double AngularFrequency;
    internal readonly double Phase;
    internal readonly double ShiftY;

    internal SineWave(double amplitude, double ordinaryFrequency, double phase, double shiftY)
    {
        Amplitude = amplitude;
        OrdinaryFrequency = ordinaryFrequency;
        AngularFrequency = 2 * Math.PI * ordinaryFrequency;
        Phase = phase;
        ShiftY = shiftY;
    }
}

public class RandomCurve
{
    private SineWave[] m_sineWaves;

    public RandomCurve(int components, double minY, double maxY, double flatness)
    {
        m_sineWaves = new SineWave[components];

        double totalPeakToPeakAmplitude = maxY - minY;
        double averagePeakToPeakAmplitude = totalPeakToPeakAmplitude / components;

        int prime = 1;
        Random r = new Random();
        for (int i = 0; i < components; i++)
        {
            // from 0.5 to 1.5 of averagePeakToPeakAmplitude 
            double peakToPeakAmplitude = averagePeakToPeakAmplitude * (r.NextDouble() + 0.5d);

            // peak amplitude is a hald of peak-to-peak amplitude
            double amplitude = peakToPeakAmplitude / 2d;

            // period should be a multiple of the prime number to avoid regularities
            prime = Utils.GetNextPrime(prime);
            double period = flatness * prime;

            // ordinary frequency is reciprocal of period
            double ordinaryFrequency = 1d / period;

            // random phase
            double phase = 2 * Math.PI * (r.NextDouble() + 0.5d);

            // shiftY is the same as amplitude
            double shiftY = amplitude;

            m_sineWaves[i] =
                new SineWave(amplitude, ordinaryFrequency, phase, shiftY);
        }
    }

    public double GetY(double x)
    {
        double y = 0;
        for (int i = 0; i < m_sineWaves.Length; i++)
            y += m_sineWaves[i].Amplitude * Math.Sin(m_sineWaves[i].AngularFrequency * x + m_sineWaves[i].Phase) + m_sineWaves[i].ShiftY;
        return y;
    }
}

internal static class Utils
{
    internal static int GetNextPrime(int i)
    {
        int nextPrime = i + 1;
        for (; !IsPrime(nextPrime); nextPrime++) ;
        return nextPrime;
    }

    private static bool IsPrime(int number)
    {
        if (number == 1) return false;
        if (number == 2) return true;

        for (int i = 2; i < number; ++i)
            if (number % i == 0) return false;

        return true;
    }
}
