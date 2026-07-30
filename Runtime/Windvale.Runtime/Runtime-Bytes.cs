using System.Collections.Immutable;

namespace Windvale.Runtime;

internal abstract class Runtimeˉbyteˉnode
{
    private static readonly Runtimeˉbyteˉnode EMPTY = new Leafˉnode(
        ImmutableArray<byte>.Empty,
        0,
        0);

    public abstract int Length { get; }

    public abstract int Height { get; }

    public abstract byte Read(int offset);

    protected abstract void Copyˉrangeˉto(Span<byte> destination, int sourceˉoffset, int length);

    public void Copyˉto(Span<byte> destination, int sourceˉoffset, int length, int destinationˉoffset = 0)
    {
        if (sourceˉoffset < 0 || length < 0 || length > Length - sourceˉoffset ||
            destinationˉoffset < 0 || length > destination.Length - destinationˉoffset)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        Copyˉrangeˉto(destination.Slice(destinationˉoffset, length), sourceˉoffset, length);
    }

    public static Runtimeˉbyteˉnode From(ImmutableArray<byte> storage, int offset, int length)
    {
        if (storage.IsDefault || offset < 0 || length < 0 || length > storage.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        return length == 0 ? EMPTY : new Leafˉnode(storage, offset, length);
    }

    public static Runtimeˉbyteˉnode Slice(Runtimeˉbyteˉnode source, int offset, int length)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (offset < 0 || length < 0 || length > source.Length - offset)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }
        if (length == 0)
        {
            return EMPTY;
        }
        if (offset == 0 && length == source.Length)
        {
            return source;
        }
        if (source is Leafˉnode Leaf)
        {
            return new Leafˉnode(Leaf.Storage, checked(Leaf.Offset + offset), length);
        }

        var Branch = (Branchˉnode)source;
        if (offset >= Branch.Left.Length)
        {
            return Slice(Branch.Right, offset - Branch.Left.Length, length);
        }
        if (length <= Branch.Left.Length - offset)
        {
            return Slice(Branch.Left, offset, length);
        }

        var Leftˉlength = Branch.Left.Length - offset;
        return Concat(
            Slice(Branch.Left, offset, Leftˉlength),
            Slice(Branch.Right, 0, length - Leftˉlength));
    }

    public static Runtimeˉbyteˉnode Concat(Runtimeˉbyteˉnode left, Runtimeˉbyteˉnode right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        if (left.Length == 0)
        {
            return right;
        }
        if (right.Length == 0)
        {
            return left;
        }
        if (left.Height > right.Height + 1)
        {
            var Leftˉbranch = (Branchˉnode)left;
            return Balance(
                Leftˉbranch.Left,
                Concat(Leftˉbranch.Right, right));
        }
        if (right.Height > left.Height + 1)
        {
            var Rightˉbranch = (Branchˉnode)right;
            return Balance(
                Concat(left, Rightˉbranch.Left),
                Rightˉbranch.Right);
        }

        return new Branchˉnode(left, right);
    }

    private static Runtimeˉbyteˉnode Balance(Runtimeˉbyteˉnode left, Runtimeˉbyteˉnode right)
    {
        if (left.Height > right.Height + 1)
        {
            var Leftˉbranch = (Branchˉnode)left;
            if (Leftˉbranch.Left.Height >= Leftˉbranch.Right.Height)
            {
                return new Branchˉnode(
                    Leftˉbranch.Left,
                    new Branchˉnode(Leftˉbranch.Right, right));
            }

            var Leftˉrightˉbranch = (Branchˉnode)Leftˉbranch.Right;
            return new Branchˉnode(
                new Branchˉnode(Leftˉbranch.Left, Leftˉrightˉbranch.Left),
                new Branchˉnode(Leftˉrightˉbranch.Right, right));
        }
        if (right.Height > left.Height + 1)
        {
            var Rightˉbranch = (Branchˉnode)right;
            if (Rightˉbranch.Right.Height >= Rightˉbranch.Left.Height)
            {
                return new Branchˉnode(
                    new Branchˉnode(left, Rightˉbranch.Left),
                    Rightˉbranch.Right);
            }

            var Rightˉleftˉbranch = (Branchˉnode)Rightˉbranch.Left;
            return new Branchˉnode(
                new Branchˉnode(left, Rightˉleftˉbranch.Left),
                new Branchˉnode(Rightˉleftˉbranch.Right, Rightˉbranch.Right));
        }

        return new Branchˉnode(left, right);
    }

    private sealed class Leafˉnode(
        ImmutableArray<byte> storage,
        int offset,
        int length) : Runtimeˉbyteˉnode
    {
        public ImmutableArray<byte> Storage { get; } = storage;

        public int Offset { get; } = offset;

        public override int Length { get; } = length;

        public override int Height => Length == 0 ? 0 : 1;

        public override byte Read(int offset)
        {
            if ((uint)offset >= (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            return Storage[Offset + offset];
        }

        protected override void Copyˉrangeˉto(Span<byte> destination, int sourceˉoffset, int length)
        {
            Storage.AsSpan(Offset + sourceˉoffset, length).CopyTo(destination);
        }
    }

    private sealed class Branchˉnode : Runtimeˉbyteˉnode
    {
        public Branchˉnode(Runtimeˉbyteˉnode left, Runtimeˉbyteˉnode right)
        {
            if (left.Length == 0 || right.Length == 0)
            {
                throw new ArgumentException("A byte branch requires two non-empty children.");
            }

            Left = left;
            Right = right;
            Length = checked(left.Length + right.Length);
            Height = checked(Math.Max(left.Height, right.Height) + 1);
        }

        public Runtimeˉbyteˉnode Left { get; }

        public Runtimeˉbyteˉnode Right { get; }

        public override int Length { get; }

        public override int Height { get; }

        public override byte Read(int offset)
        {
            if ((uint)offset >= (uint)Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            return offset < Left.Length
                ? Left.Read(offset)
                : Right.Read(offset - Left.Length);
        }

        protected override void Copyˉrangeˉto(Span<byte> destination, int sourceˉoffset, int length)
        {
            if (length == 0)
            {
                return;
            }
            if (sourceˉoffset >= Left.Length)
            {
                Right.Copyˉto(destination, sourceˉoffset - Left.Length, length);
                return;
            }

            var Leftˉlength = Math.Min(length, Left.Length - sourceˉoffset);
            Left.Copyˉto(destination, sourceˉoffset, Leftˉlength);
            if (Leftˉlength < length)
            {
                Right.Copyˉto(destination, 0, length - Leftˉlength, Leftˉlength);
            }
        }
    }
}
