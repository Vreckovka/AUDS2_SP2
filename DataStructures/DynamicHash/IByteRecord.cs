using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures.DynamicHash
{
    public interface IByteRecord
    {
        byte[] ToByteArray();
        void FromByteArray(byte[] byteArray);
        int GetSizeOfByteArray();
    }
}
