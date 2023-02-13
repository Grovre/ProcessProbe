﻿using System.Diagnostics;
using System.Reflection;
using ProcessProbe.MemoryInterface;

namespace ProcessProbe;

public class ProcessProbe
{
    private readonly IMemoryInterface _memory;
    private readonly Process _proc;

    public unsafe int Read<T>(nint address, out T value) where T : struct
    {
        EnforceTypeSafety<T>();

        value = default;

        Span<byte> buffer = new(&value, SizeOf<T>.Size);

        return _memory.Read(address, buffer);
    }

    public unsafe int Write<T>(nint address, out T value) where T : struct
    {
        EnforceTypeSafety<T>();

        Span<byte> buffer = new(&value, SizeOf<T>.Size);

        return _memory.Write(address, buffer);
    }

    public unsafe int ReadArray<T>(nint address, Span<T> array) where T : struct
    {
        EnforceTypeSafety<T>();

        fixed (void* bufferPtr = &array.GetPinnableReference())
        {
            Span<byte> buffer = new(bufferPtr, SizeOf<T>.Size * array.Length);

            return _memory.Read(address, buffer);
        }
    }

    public unsafe int WriteArray<T>(nint address, Span<T> array) where T : struct
    {
        EnforceTypeSafety<T>();

        fixed (void* bufferPtr = &array.GetPinnableReference())
        {
            Span<byte> buffer = new(bufferPtr, SizeOf<T>.Size * array.Length);

            return _memory.Write(address, buffer);
        }
    }

    private static void EnforceTypeSafety<T>() => EnforceTypeSafety(typeof(T));

    private static void EnforceTypeSafety(Type t)
    {
        if (t.IsPrimitive)
            return;

        if (t.IsByRef)
            throw new UnsafeTypeException("The given type cannot be a reference");

        FieldInfo[] fields = t.GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo f = fields[i];
            EnforceTypeSafety(f.FieldType);
        }
    }
}