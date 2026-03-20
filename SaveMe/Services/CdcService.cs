public class CdcService
{
    const int normalChunkSize = 4096; // IF YOU CHANGE THIS, UPDATE THE MASK
    const uint mask = 0xFFF;
    const int minChunkSize = normalChunkSize / 2; 
    const int maxChunkSize = normalChunkSize * 2; 

    private static readonly uint[] GearTable =
    [
        0xA4B2C3D1, 0x5F7E8A9C, 0xE2F1D5C4, 0x3B4A7F9E, 0x8C9D1E2F, 0x76B5C4E3, 0xC1D7E8F2, 0x4A5B6D9F,
        0x92E3F1A7, 0x6F7D8C5B, 0xD4E2F7A9, 0x1C3E4F8B, 0xB5C6D8E4, 0x7A8B9FD2, 0xE9F0C1B3, 0x2D4E5F7C,
        0x9A3B4C5E, 0x67D8E9F4, 0xC2F1A8D3, 0x4B5A6E7F, 0x8D9E2F3C, 0x5F6D7E8A, 0xD5E4F9B2, 0x1E2F4D7B,
        0xA3B4C5D6, 0x7C8D9EA5, 0xE1F7C8B4, 0x3A4B5E6F, 0x96A7B8C9, 0x6D7F8E9D, 0xC4D5E6F7, 0x2B3C4D5E,
        0x8F9DA1B2, 0x74859FCB, 0xCFDFE5F8, 0x4A5D6E7C, 0x9BBD2D3E, 0x5A6D7C8F, 0xD9E8F7A6, 0x0F1E2D3C,
        0xA1B2C3D4, 0x7E8F9DA8, 0xDFECFDB9, 0x3C4D5E6F, 0x94A5B6C7, 0x6F7E8D9C, 0xCBD2E3F4, 0x2A3B4C5D,
        0x8D9EA0B1, 0x7F8C9DA9, 0xD8E9FAB3, 0x4E5F6E7D, 0x9CAD1E2F, 0x5D6E7F8A, 0xDEEFF8A7, 0x1D2E3F4C,
        0xA4B5C6D7, 0x7D8E9FA6, 0xDCEDFEB5, 0x3B4C5D6E, 0x92A3B4C5, 0x6D7E8F9A, 0xC8D9EAF3, 0x2F3E4F5C,
        0x8C9DA1B0, 0x7B8C9DA8, 0xD5E6F7A4, 0x4C5D6E7F, 0x9DBE2F3E, 0x5E6F7E81, 0xD7E8F9A2, 0x1C2D3E4F,
        0xA2B3C4D5, 0x7C8D9EA7, 0xDEEFD0B8, 0x3A4B5C6D, 0x91A2B3C4, 0x6C7D8E9F, 0xC9DAEAF5, 0x2E3F4E5B,
        0x8B9CA0AF, 0x7A8B9CA5, 0xD4E5F6A3, 0x4B5C6D7E, 0x9CBD2E3F, 0x5D6E7D80, 0xD6E7F8A1, 0x1B2C3D4E,
        0xA1B2C3D4, 0x7B8C9DA6, 0xDDEECFB7, 0x393A5B6C, 0x90A1B2C3, 0x6B7C8D9E, 0xC8D9E8F4, 0x2D3E4D5A,
        0x8A9B9FAE, 0x7989A9A4, 0xD3E4F5A2, 0x4A5B6C7D, 0x9BBC2D3E, 0x5C6D7C7F, 0xD5E6F7A0, 0x1A2B3C4D,
        0xA0B1C2D3, 0x7A8B9CA5, 0xDCEDC0B6, 0x383950BB, 0x8F9A9BC2, 0x6A7B8C9D, 0xC7D8E7F3, 0x2C3D4C59,
        0x898A9EAD, 0x7888A8A3, 0xD2E3F4A1, 0x495A6B7C, 0x9ABB2C3D, 0x5B6C7B7E, 0xD4E5F69F, 0x191A3B4C,
        0x9FA0C1D2, 0x798A9BA4, 0xDBECB0B5, 0x37385A5F, 0x8E99A9C1, 0x697A8B9C, 0xC6D7E6F2, 0x2B3C4B58,
        0x87898DAC, 0x7787A7A2, 0xD1E2F3A0, 0x48596A7B, 0x99AA2B3C, 0x5A6B7A7D, 0xD3E4F59E, 0x1718394A,
        0x9D9EBFD0, 0x7788A8A2, 0xDAADB0B4, 0x35365A5E, 0x8C97A7BF, 0x67689A9A, 0xC4D5E4F0, 0x293A4A56,
        0x84828ECA, 0x7684A4B8, 0xCEDFF09C, 0x46576879, 0x99A9B2BB, 0x586A78CE, 0xD1E2F3AD, 0x161930BA,
        0x9B9DBECE, 0x798A87A0, 0xD9ACB1B9, 0x35365C5E, 0x8C97A7BF, 0x67689A9A, 0xC4D5E4F0, 0x293A4A56,
        0x83818DCA, 0x7582A3B8, 0xCCDEEF9C, 0x45566779, 0x98A8B1BA, 0x576977CD, 0xD0E1F2AC, 0x15183949,
        0x9A9CBDCD, 0x787986A0, 0xD8ABB0B8, 0x34355D5E, 0x8B96A6BF, 0x666799A9, 0xC3D4E3EF, 0x283A4955,
        0x82808CCA, 0x7481A2B7, 0xCBDDEE9B, 0x44556678, 0x97A7B0B9, 0x566876CC, 0xCFE0F1AB, 0x14173848,
        0x999BBCCC, 0x777885A9, 0xD7AAF1B7, 0x33345C5D, 0x8A95A5BE, 0x656798A8, 0xC2D3E2EE, 0x273949D4,
        0x817F8BC9, 0x7380A1B6, 0xCADCED9A, 0x43545577, 0x96A6AFBB, 0x556775CB, 0xCEDFF0AA, 0x13163747,
        0x989ABBC3, 0x767984A3, 0xD6A9B0B6, 0x32335C5C, 0x8994A4BE, 0x646697A7, 0xC1D2E1ED, 0x262848D3,
        0x807E8AB0, 0x727FA0B5, 0xC9DBEC99, 0x42535476, 0x95A5AEBA, 0x546674CA, 0xCDDEEFA9, 0x12153646,
        0x9799B8B2, 0x757883A2, 0xD5A8AFB5, 0x31325B5B, 0x8A93A4BC, 0x636596A6, 0xC0D1E0ED, 0x252748D2,
        0x7F7D89AE, 0x727E9FB4, 0xC9DAEB98, 0x41525475, 0x94A4ADB9, 0x546573C9, 0xCCDDEFA8, 0x11153645,
        0x9798B7B1, 0x757982A1, 0xD4A8AEB4, 0x30315A5A, 0x8992A3BB, 0x626495A5, 0xBFD0DFEC, 0x242647D1,
        0x7E7C88AD, 0x717D9EB3, 0xC8D9EA97, 0x40515374, 0x93A3ACB8, 0x536472C8, 0xCBDCEEA7, 0x10143544,
        0x9697B6B0, 0x747881A0, 0xD3A7ADB3, 0x2F305959, 0x8891A2BA, 0x616394A4, 0xBECFDEEB, 0x232645D0
    ];

    public List<byte[]> ChunkData(byte[] data)
    {
        List<byte[]> chunks = new List<byte[]>();
        int offset = 0;

        while (offset < data.Length)
        {
            int chunkStart = offset;
            int chunkEnd = Math.Min(offset + maxChunkSize, data.Length);

            // Minimum chunk size
            if (chunkEnd - chunkStart < minChunkSize)
            {
                chunkEnd = Math.Min(chunkStart + minChunkSize, data.Length);
            }

            // Find boundary using rolling hash
            int boundary = FindBoundary(data, chunkStart, chunkEnd);

            // If no boundary found, use maxChunkSize
            if (boundary == -1)
            {
                boundary = chunkEnd;
            }

            // Extract chunk
            byte[] chunk = new byte[boundary - chunkStart];
            Array.Copy(data, chunkStart, chunk, 0, chunk.Length);
            chunks.Add(chunk);

            offset = boundary;
        }

        return chunks;
    }

    private static int FindBoundary(byte[] data, int start, int end)
    {
        uint hash = 0;
        int minPos = start + minChunkSize;

        for (int i = start; i < end; i++)
        {
            // Rolling hash
            hash = ((hash << 1) + GearTable[data[i]]) & 0xFFFFFFFF;

            // Check if we've reached minimum size and if hash matches boundary condition
            if (i >= minPos && (hash & mask) == 0)
            {
                return i + 1;
            }
        }

        // Nothing found
        return -1;
    }

    public static string CalculateChunkFingerprint(byte[] chunk)
    {
        byte[] hash = System.Security.Cryptography.SHA256.HashData(chunk);
        return Convert.ToBase64String(hash);
    }
}
