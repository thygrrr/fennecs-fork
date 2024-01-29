﻿using System.Numerics;
using BenchmarkDotNet.Attributes;

namespace Benchmark;

[MemoryDiagnoser(true)]
public class V3Benchmarks
{
    [Params(1000, 1000000)] 
    public int entityCount { get; set; }

    private static readonly Random random = new(1337);

    private Vector3[] _input = null!;
    private float[] _output = null!;
    
    [GlobalSetup]
    public void Setup()
    {
        _input = Enumerable.Range(0, entityCount).Select(_ => new Vector3(random.Next(), random.Next(), random.Next())).ToArray();
        _output = new float[entityCount];

        _incrementDelegate = VectorIncrement;
    }

    //[Benchmark]
    public void PerItemDot()
    {
        for (var i = 0; i < entityCount; i++)
        {
            _output[i] = Vector3.Dot(_input[i], new Vector3(1, 2, 3));
        }
    }

    [Benchmark]
    public void PerItemIncrementArray()
    {
        var lim = _input.Length;
        for (var i = 0; i < lim; i++)
        {
            _input[i] += new Vector3(1, 2, 3);
        }
    }

    [Benchmark]
    public void PerItemIncrementSpan()
    {
        var span = _input.AsSpan();
        var lim = span.Length;
        for (var i = 0; i < lim; i++)
        {
            span[i] += new Vector3(1, 2, 3);
        }
    }

    [Benchmark]
    public void PerItemIncrementSpanRef()
    {
        var span = _input.AsSpan();
        foreach (ref var v in span)
        {
            v += new Vector3(1, 2, 3);
        }
    }

    private void VectorIncrement(ref Vector3 val)
    {
        val += new Vector3(1, 2, 3);   
    }

    [Benchmark]
    public void PerItemIncrementSpanCall()
    {
        var span = _input.AsSpan();
        foreach (ref var v in span)
        {
            VectorIncrement(ref v);
        }
    }

    private delegate void VectorIncrementDelegate(ref Vector3 val);

    private delegate void VectorIncrementDelegateIn(in Vector3 val);

    private VectorIncrementDelegate _incrementDelegate = null!;
    
    [Benchmark]
    public void PerItemIncrementSpanDelegate()
    {
        PerItemIncrementSpanDelegateImpl(_incrementDelegate);
    }

    [Benchmark]
    public void PerItemIncrementSpanLambda()
    {
        PerItemIncrementSpanDelegateImpl((ref Vector3 val) => { val += new Vector3(1, 2, 3); });
    }
    
    [Benchmark]
    public void PerItemIncrementSpanLocalDelegate()
    {
        VectorIncrementDelegate del = (ref Vector3 val) => { val += new Vector3(1, 2, 3); };
        PerItemIncrementSpanDelegateImpl(del);
    }

    [Benchmark]
    public void PerItemIncrementSpanLocalFunction()
    {
        PerItemIncrementSpanDelegateImpl(Del);
        return;

        void Del(ref Vector3 val)
        {
            val += new Vector3(1, 2, 3);
        }
    }


    private void PerItemIncrementSpanDelegateImpl(VectorIncrementDelegate del)
    {
        var span = _input.AsSpan();
        foreach (ref var v in span)
        {
            del(ref v);
        }
    }

    private void PerItemIncrementSpanDelegateImplIn(VectorIncrementDelegateIn del)
    {
        var span = _input.AsSpan();
        foreach (ref var v in span)
        {
            del(in v);
        }
    }

    public void PerItemDotParallel()
    {
        Parallel.For(0, entityCount, i => { _output[i] = Vector3.Dot(_input[i], new Vector3(1, 2, 3)); });
    }

    public void PerItemDotSpan()
    {
        var va = new Vector3(1, 2, 3);
        var input = _input.AsSpan();
        var output = _output.AsSpan();
        for (var i = 0; i < entityCount; i++)
        {
            output[i] = Vector3.Dot(input[i], va);
        }
    }
}