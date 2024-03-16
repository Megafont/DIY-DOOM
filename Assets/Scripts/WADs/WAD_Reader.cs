using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Analytics;

using DIY_DOOM.Maps;


namespace DIY_DOOM.WADs
{
    public class WAD_Reader
    {
        public byte[] Read2Bytes(byte[] wadData, int offset)
        {
            byte[] bytes2 = new byte[2];

            Array.Copy(wadData, offset, bytes2, 0, 2);

            return bytes2;
        }

        public byte[] Read4Bytes(byte[] wadData, int offset)
        {
            byte[] bytes4 = new byte[4];

            Array.Copy(wadData, offset, bytes4, 0, 4);

            return bytes4;
        }

        public byte[] Read8Bytes(byte[] wadData, int offset)
        {
            byte[] bytes8 = new byte[8];

            Array.Copy(wadData, offset, bytes8, 0, 8);

            return bytes8;
        }



        public WAD_Header ReadHeaderData(byte[] wadData, int offset)
        {
            WAD_Header header = new WAD_Header();

            header.WAD_Type = System.Text.Encoding.UTF8.GetString(Read4Bytes(wadData, offset));
            header.DirectoryCount = BitConverter.ToInt32(Read4Bytes(wadData, offset + 4));
            header.DirectoryOffset = BitConverter.ToInt32(Read4Bytes(wadData, offset + 8));

            return header;
        }

        public Directory ReadDirectoryData(byte[] wadData, int offset)
        {
            Directory directory = new Directory();

            directory.LumpOffset = BitConverter.ToInt32(Read4Bytes(wadData, offset));
            directory.LumpSize = BitConverter.ToInt32(Read4Bytes(wadData, offset + 4));

            directory.LumpName = System.Text.Encoding.UTF8.GetString(Read8Bytes(wadData, offset + 8)).Trim();
            directory.LumpName = directory.LumpName.Trim('\0'); // This is needed, as unused characters at the end of the lump name are all set to the null character. We need to remove those unnecessary chars from the end of the string.

            return directory;
        }

        public Vector2 ReadVertexData(byte[] wadData, int offset)
        {
            Vector2 vertex = new Vector2(BitConverter.ToInt16(Read2Bytes(wadData, offset)),
                                         BitConverter.ToInt16(Read2Bytes(wadData, offset + 2)));

            return vertex;
        }

        public LineDef ReadLineDefData(byte[] wadData, int offset)
        {
            LineDef lineDef = new LineDef();

            lineDef.StartVertex = BitConverter.ToUInt16(Read2Bytes(wadData, offset));
            lineDef.EndVertex = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 2));
            lineDef.Flags = (LineDefFlags)BitConverter.ToUInt16(Read2Bytes(wadData, offset + 4));
            lineDef.LineType = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 6));
            lineDef.SectorTag = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 8));
            lineDef.RightSideDef = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 10));
            lineDef.LeftSideDef = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 12));

            return lineDef;
        }

        public ThingDef ReadThingData(byte[] wadData, int offset)
        {
            ThingDef thing = new ThingDef();

            Vector2 position = new Vector2();
            position.x = BitConverter.ToInt16(Read2Bytes(wadData, offset));
            position.y = BitConverter.ToInt16(Read2Bytes(wadData, offset + 2));
            thing.Position = position;

            thing.Angle = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 4));
            thing.Type = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 6));
            thing.Flags = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 8));

            return thing;
        }

        public NodeDef ReadNodeData(byte[] wadData, int offset)
        {
            NodeDef node = new NodeDef();

            Vector2 v = new Vector2();
            v.x = BitConverter.ToInt16(Read2Bytes(wadData, offset));
            v.y = BitConverter.ToInt16(Read2Bytes(wadData, offset + 2));
            node.PartitionStart = v;

            v = new Vector2();
            v.x = BitConverter.ToInt16(Read2Bytes(wadData, offset + 4));
            v.y = BitConverter.ToInt16(Read2Bytes(wadData, offset + 6));
            node.DeltaToPartitionEnd = v;


            int RightBox_Top = BitConverter.ToInt16(Read2Bytes(wadData, offset + 8));
            int RightBox_Bottom = BitConverter.ToInt16(Read2Bytes(wadData, offset + 10));
            int RightBox_Left = BitConverter.ToInt16(Read2Bytes(wadData, offset + 12));
            int RightBox_Right = BitConverter.ToInt16(Read2Bytes(wadData, offset + 14));

            node.RightBox_BottomLeft = new Vector2(RightBox_Left, RightBox_Bottom);
            node.RightBox_TopRight = new Vector2(RightBox_Right, RightBox_Top);

            int LeftBox_Top = BitConverter.ToInt16(Read2Bytes(wadData, offset + 16));
            int LeftBox_Bottom = BitConverter.ToInt16(Read2Bytes(wadData, offset + 18));
            int LeftBox_Left = BitConverter.ToInt16(Read2Bytes(wadData, offset + 20));
            int LeftBox_Right = BitConverter.ToInt16(Read2Bytes(wadData, offset + 22));

            node.LeftBox_BottomLeft = new Vector2(LeftBox_Left, LeftBox_Bottom);
            node.LeftBox_TopRight = new Vector2(LeftBox_Right, LeftBox_Top);


            node.RightChildID = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 24));
            node.LeftChildID = BitConverter.ToUInt16(Read2Bytes(wadData, offset + 26));

            return node;
        }
    }
}