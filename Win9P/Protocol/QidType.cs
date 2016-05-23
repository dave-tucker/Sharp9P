namespace Win9P.Protocol
{
    public enum QidType
    {
        QTDIR = 0x80, // type bit for directories
        QTAPPEND = 0x40, // type bit for append only files
        QTEXCL = 0x20, // type bit for exclusive use files
        QTMOUNT = 0x10, // type bit for mounted channel
        QTAUTH = 0x08, // type bit for authentication file
        QTTMP = 0x04, // type bit for not-backed-up file
        QTFILE = 0x00 // plain file
    }
}