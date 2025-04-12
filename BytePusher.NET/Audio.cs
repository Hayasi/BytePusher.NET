using Silk.NET.OpenAL;

namespace BytePusher.NET;

internal class Audio
{
    private readonly AL al;
    private readonly ALContext alc;
    private readonly unsafe Device* device;
    private readonly unsafe Context* context;
    private readonly uint source;
    private readonly uint[] buffers;
    private readonly Queue<byte[]> pendingSamples = new();
    private const int BufferSize = 256;
    private const int SampleRate = BufferSize * 60;
    private const int BufferCount = 3;

    public Audio()
    {
        unsafe
        {
            alc = ALContext.GetApi();
            al = AL.GetApi();

            device = alc.OpenDevice(null);
            context = alc.CreateContext(device, null);
            alc.MakeContextCurrent(context);
            alc.ProcessContext(context);

            source = al.GenSource();
            buffers = new uint[BufferCount];
            fixed (uint* ptr = buffers)
            {
                al.GenBuffers(BufferCount, ptr);

                var silence = new byte[BufferSize];
                Array.Fill(silence, (byte)128);
                foreach (uint buffer in buffers)
                {
                    fixed (byte* data = silence)
                    {
                        al.BufferData(buffer, BufferFormat.Mono8, data, BufferSize, SampleRate);
                    }
                }

                al.SourceQueueBuffers(source, BufferCount, ptr);
            }

            al.SourcePlay(source);
        }
    }

    public void Update(byte[] samples)
    {
        pendingSamples.Enqueue(samples);
        ProcessBuffers();
    }

    private unsafe void ProcessBuffers()
    {
        al.GetSourceProperty(source, GetSourceInteger.BuffersProcessed, out int processed);
        if (processed == 0) return;

        uint[] processedBuffers = new uint[processed];
        fixed (uint* ptr = processedBuffers)
        {
            al.SourceUnqueueBuffers(source, processed, ptr);

            foreach (uint buffer in processedBuffers)
            {
                byte[] data;
                if (pendingSamples.Count > 0)
                {
                    data = pendingSamples.Dequeue();
                }
                else
                {
                    data = new byte[BufferSize];
                    Array.Fill(data, (byte)128);
                }
                fixed (byte* dataPtr = data)
                {
                    al.BufferData(buffer, BufferFormat.Mono8, dataPtr, BufferSize, SampleRate);
                }
            }

            al.SourceQueueBuffers(source, processed, ptr);
        }

        al.GetSourceProperty(source, GetSourceInteger.SourceState, out int state);
        if ((SourceState)state != SourceState.Playing)
        {
            al.SourcePlay(source);
        }
    }

    public void Close()
    {
        unsafe
        {
            al.DeleteSource(source);
            fixed (uint* ptr = buffers)
                al.DeleteBuffers(BufferCount, ptr);
            alc.DestroyContext(context);
            alc.CloseDevice(device);
        }
    }
}
