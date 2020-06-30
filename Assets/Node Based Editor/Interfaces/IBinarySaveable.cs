using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NodeEditor.FileIO
{
    public interface IBinarySaveable
    {
        void BinarySave(BinaryWriter writer);
    }
}