namespace Win9P
{
    public static class Constants
    {
        internal const int BIT8SZ = 1;
        internal const int BIT16SZ = 2;
        internal const int BIT32SZ = 4;
        internal const int BIT64SZ = 8;
        internal const int QIDSZ = BIT8SZ + BIT32SZ + BIT64SZ;
        internal const int MAXWELEM = 16;
        internal const int HeaderOffset = 7;

        internal const ushort NoTag = 0;
        public const uint NoFid = 0;

        public const uint DMDIR = 0x80000000;
        public const uint DMAPPEND = 0x40000000;
        public const uint DMEXCL = 0x20000000;
        public const uint DMMOUNT = 0x10000000;
        public const uint DMAUTH = 0x08000000;
        public const uint DMTMP = 0x04000000;
        public const uint DMNONE = 0xFC000000;
        public const uint DMREAD = 0x4;
        public const uint DMWRITE = 0x2;
        public const uint DMEXEC = 0x1;
        public const byte OREAD = 0x00;
        public const byte OWRITE = 0x01;
        public const byte ORDWR = 0x02;
        public const byte OEXEC = 0x03;
        public const byte OTRUNC = 0x10;
        public const byte OCEXEC = 0x10;
        public const byte ORCLOSE = 0x10;

        public const uint DefaultMsize = 16384;
        public const string DefaultVersion = "9P2000";
        public const uint RootFid = 1;
    }
}